using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker_Server {
	class Program {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES TOO INCLUDING CLIENT
		private static readonly int PORT = 2784;
		private static readonly string FILE_SAVE_DIRECTORY =
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			+ "/Project Tracker Data";
		private readonly static int ID_LENGTH = 30;

		/// <summary>
		/// Creates a new id of numbers with the length of ID_LENGTH.
		/// </summary>
		/// <returns>Returns the new id.</returns>
		private static string CreateNewID() {
			string id = "id";
			Random r = new Random();

			for (int i = 0; i < ID_LENGTH; i++) {
				id += r.Next(0, 9).ToString();
			}

			if (id == "id") {
				return "null";
			}
			else {
				return id;
			}
		}

		private static string GetLocalIP() {
			IPHostEntry host;
			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					return ip.ToString();
				}
			}
			return "";
		}

		/// <summary>
		/// Gets the ID from the data transmitted.
		/// </summary>
		/// <param name="data">The data that was transmitted.</param>
		/// <returns>Returns the username.</returns>
		private static string GetUserID(string data) {
			string json = data;
			DataTransferManifest.Rootobject dataValues =
				JsonConvert.DeserializeObject<DataTransferManifest.Rootobject>(json);

			return dataValues.UserID;
		}

		static void Main(string[] args) {
			if (!Directory.Exists(FILE_SAVE_DIRECTORY)) {
				Directory.CreateDirectory(FILE_SAVE_DIRECTORY);
			}

			IPAddress ip = IPAddress.Parse(GetLocalIP());
			IPEndPoint localEndPoint = new IPEndPoint(ip, PORT);

			Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(localEndPoint);
			server.Listen(100);

			Console.WriteLine("Created endpoint: " + localEndPoint);

			while (true) {
				try {
					Console.WriteLine("Waiting for a connection...");

					Socket client = server.Accept();

					// Take up to 5mb of file. Should only be a few kb though
					byte[] buffer = new byte[1024 * 10000];

					// Receive bytes for file
					int bytesReceived = client.Receive(buffer);

					char[] chars = new char[bytesReceived];
					Decoder decoder = Encoding.UTF8.GetDecoder();
					int charLength = decoder.GetChars(buffer, 0, bytesReceived, chars, 0);
					string fileData = new string(chars);

					string username = GetUserID(fileData);
					Console.WriteLine("Received info from " + username);

					if (username.Length < ID_LENGTH) {
						username = CreateNewID();
					}

					File.WriteAllText(FILE_SAVE_DIRECTORY + "\\" + username + ".json", fileData);

					client.Close();
					Console.WriteLine("Closed the server");
				}
				catch (Exception e) {
					Console.WriteLine("We had the following error: " + e);
				}
			}
		}
	}
}
