using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_Tracker {
	class BackgroundProcesses {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE CHANGE IN OTHER FILES AS WELL
		private readonly static string DLL_APPDATA_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll";
		private readonly static string TEMP_SERVER_VERSION_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/temp-server-version.dll";


		// Server Communcation DLL
		private readonly static string USUAL_SERVER_DLL_LOCATION = @"C:\Program Files\Project Tracker\Server Communcation DLL.dll";
		private readonly static string SERVER_DLL_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/Server Communication DLL.dll";
		private readonly static string SECONDARY_SERVER_DLL_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/Secondary Server Communication DLL.dll";
		private readonly static string SERVER_DLL_VERSION_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/server-communication-dll-version.txt";
		private readonly static string SERVER_DLL_VERSION_URL =
			"https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version-info/server-communication-dll-version.txt";
		private readonly static string SERVER_DLL_URL =
			"https://github.com/CyanCoding/Project-Tracker/raw/master/install-resources/Project%20Tracker/Server%20Communication%20DLL.dll";

		private static double version = 0.1;


		private static double ServerVersion(string url) {
			try {
				WebClient client = new WebClient();
				client.DownloadFile(new Uri(url), TEMP_SERVER_VERSION_LOCATION);
			}
			catch (WebException) {
				return 0.0;
			}

			string fileVersionText = File.ReadAllText(TEMP_SERVER_VERSION_LOCATION);

			return Convert.ToDouble(fileVersionText);
		}

		/// <summary>
		/// Gets the latest version and updates if necessary.
		/// </summary>
		/// <param name="versionFile">The file of the current dll version.</param>
		/// <param name="url">The url of the version download.</param>
		/// <param name="dllUrl">The url of the dll we're checking.</param>
		/// <param name="secondaryDllFile">The secondary file for the dll we're checking.</param>
		private static void ReadVersion(string versionFile, string url, string dllUrl, string dllFile) {
			if (!File.Exists(versionFile)) {
				version = ServerVersion(url);
				File.Move(TEMP_SERVER_VERSION_LOCATION, versionFile);
			}
			else {
				string fileVersionText = File.ReadAllText(versionFile);
				version = Convert.ToDouble(fileVersionText);

				double latestVersion = ServerVersion(url);

				if (latestVersion > version) { // An update is available
					File.Delete(versionFile);
					File.Move(TEMP_SERVER_VERSION_LOCATION, versionFile);

					// Download the secondary file
					try {
						WebClient client = new WebClient();
						client.DownloadFile(new Uri(dllUrl), dllFile);
					}
					catch (WebException) {

					}
				}
			}

			File.Delete(TEMP_SERVER_VERSION_LOCATION);
		}

		public static void DataReporting() {
			if (!Directory.Exists(DLL_APPDATA_LOCATION)) {
				Directory.CreateDirectory(DLL_APPDATA_LOCATION);
			}
			
			if (!File.Exists(SERVER_DLL_LOCATION)) { // Server dll isn't here (probably old version)
				if (File.Exists(USUAL_SERVER_DLL_LOCATION)) { // Server dll was there where it was installed so just move it
					File.Copy(USUAL_SERVER_DLL_LOCATION, SERVER_DLL_LOCATION);
				}
				else { // The dll isn't there for some reason so we download it
					try {
						WebClient client = new WebClient();
						client.DownloadFile(new Uri(SERVER_DLL_URL), SERVER_DLL_LOCATION);
					}
					catch (WebException) {

					}
				}
			}

			// If there is an updated dll, move it to be the current one
			if (File.Exists(SECONDARY_SERVER_DLL_LOCATION)) {
				File.Delete(SERVER_DLL_LOCATION);
				File.Move(SECONDARY_SERVER_DLL_LOCATION, SERVER_DLL_LOCATION);
			}

			ReadVersion(SERVER_DLL_VERSION_LOCATION, SERVER_DLL_VERSION_URL, SERVER_DLL_URL, SERVER_DLL_LOCATION);

			// Add Server Communication DLL
			Assembly assembly = Assembly.LoadFrom(SERVER_DLL_LOCATION);
			Type type = assembly.GetType("Server_Communication_DLL.SetData");
			object instance = Activator.CreateInstance(type);

			MethodInfo[] methods = type.GetMethods();
			// 0: SetActivity
			// 1: Refresh

			methods[0].Invoke(instance, new object[] {true});			

			while (true) {
				Thread.Sleep(60000);

				methods[1].Invoke(instance, new object[] { 1 });				
			}
		}
	}
}
