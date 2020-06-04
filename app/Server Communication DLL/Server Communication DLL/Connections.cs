using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_Communication_DLL {
	public class Connections {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES TOO INCLUDING SERVER
		private static readonly int PORT = 2784;
		private static readonly string SERVER_IP = "192.168.1.206";
		private readonly static string DATA_COLLECTION_FILE =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			+ "/Project Tracker/data-collection.json";

		public static bool CreateConnection() {
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
