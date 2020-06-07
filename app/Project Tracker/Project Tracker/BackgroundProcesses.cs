using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_Tracker {
	class BackgroundProcesses {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE CHANGE IN OTHER FILES AS WELL
		private readonly static string TEMP_SERVER_VERSION_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/temp-server-version.dll";
		private readonly static string SERVER_DLL_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/Server Communication DLL.dll";
		private readonly static string SERVER_DLL_VERSION_LOCATION = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/server-communication-dll-version.txt";

		private static double version = 0.1;


		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string dllToLoad);
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);


		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void SetActivity(bool isActive);
		private delegate void Refresh();


		private static double ServerVersion(string url) {
			try {
				WebClient client = new WebClient();
				client.DownloadFile(new Uri(url), TEMP_SERVER_VERSION_LOCATION);
			}
			catch (WebException) {
				return 0.0;
			}

			return 0.0;
		}

		/// <summary>
		/// Reads the DLL version file and sets the version variable.
		/// </summary>
		/// <param name="versionFile">The version file to read from.</param>
		/// <returns>True if an update is necessary, false otherwise.</returns>
		private static void ReadVersion(string versionFile) {
			if (!File.Exists(versionFile)) {
				ServerVersion();
			}
			string fileVersionText = File.ReadAllText(versionFile);
			version = Convert.ToDouble(fileVersionText);



		}

		public static void DataReporting() {
			// Add Server Communication DLL
			IntPtr serverDll = BackgroundProcesses.LoadLibrary(SERVER_DLL_LOCATION);
			IntPtr serverDllSetActivityFunction = BackgroundProcesses.GetProcAddress(serverDll, "SetActivity");
			IntPtr serverDllRefreshFunction = BackgroundProcesses.GetProcAddress(serverDll, "Refresh");

			SetActivity activity = (SetActivity)Marshal.GetDelegateForFunctionPointer(serverDllSetActivityFunction, typeof(SetActivity));
			Refresh refresh = (Refresh)Marshal.GetDelegateForFunctionPointer(serverDllRefreshFunction, typeof(Refresh));

			activity(true); // Set activity to true for this program

			ReadVersion(SERVER_DLL_VERSION_LOCATION);

			while (true) {
				refresh();

				Thread.Sleep(60000);
			}
		}
	}
}
