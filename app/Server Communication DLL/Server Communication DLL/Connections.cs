using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_Communication_DLL {
	/*
	 * Ideas
	 * 
	 * Pass a variable to the function that lets it know whether the program is open or closed rn
	 * Track how many times they've logged on in the last month
	 * Track how many times they've logged on in the last week
	 * Track how many times they've logged on in the last year
	 * 
	 * 
	 */
	public class Connections {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES TOO INCLUDING SERVER
		private static readonly int PORT = 2784;
		private static string SERVER_IP = "192.168.1.158";
		private readonly static string DATA_COLLECTION_FILE =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			+ "/Project Tracker/data-collection.json";
		private readonly static string SERVER_ADDRESS = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/dll/server-address.txt";
		private readonly static string SERVER_ADDRESS_URL =
			"https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/server-address.txt";

		public static bool CreateConnection() {
			// Get the address to go to
			try {
				WebClient client = new WebClient();
				client.DownloadFile(new Uri(SERVER_ADDRESS_URL), SERVER_ADDRESS);

				// SERVER_IP = File.ReadAllText(SERVER_ADDRESS);
				File.Delete(SERVER_ADDRESS);
			}
			catch (WebException) {
				return false;
			}

			// Get IP Address and create endpoint
			IPAddress ip = IPAddress.Parse(SERVER_IP);
			IPEndPoint serverEndPoint = new IPEndPoint(ip, PORT);

			Socket sender = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			try {
				FileManager.PrepareDataTransfer();

				// Does this get the file or the file title lmao
				byte[] dataFile = File.ReadAllBytes(DATA_COLLECTION_FILE);
				byte[] userIDByteArray = Encoding.ASCII.GetBytes(FileManager.userID);

				sender.Connect(serverEndPoint);
				sender.Send(dataFile);
				sender.Close();

				FileManager.ResetFile();

				return true;
			}
			catch (ArgumentNullException) {
				// Sender endpoint is null
				Console.WriteLine("null exception");
				return false;
			}
			catch (SocketException e) {
				// An error occurred when connecting
				Console.WriteLine("sock exception: " + e);
				return false;
			}
			catch (ObjectDisposedException) {
				// The socket was closed
				Console.WriteLine("closed exception");
				return true;
			}
		}
	}
}
