using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Project_Tracker_Server {
	class Program {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES TOO INCLUDING CLIENT
		private static readonly int PORT = 2784;
		private static readonly string FILE_SAVE_DIRECTORY =
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			+ "/Project Tracker Data";
		private static readonly string DATA_FILE =
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			+ "/Project Tracker Data/data.json";
		private readonly static int ID_LENGTH = 30;

		private static bool viewConnectionDetails = false;
		private static bool viewStatistics = false;
		private static bool resetWeek = false;
		private static bool speedUpStatistics = false;
		private static string lastMonth = "";

		private static string[] resetDaysOpened = {
			"Monday: 0",
			"Tuesday: 0",
			"Wednesday: 0",
			"Thursday: 0",
			"Friday: 0",
			"Saturday: 0",
			"Sunday: 0"
		};

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
			ClientDataManifest.Rootobject dataValues =
				JsonConvert.DeserializeObject<ClientDataManifest.Rootobject>(json);

			return dataValues.UserID;
		}

		private static void CopyData(string origin) {
			string clientJson = File.ReadAllText(origin);
			ClientDataManifest.Rootobject clientDataValues =
				JsonConvert.DeserializeObject<ClientDataManifest.Rootobject>(clientJson);

			string serverJson = File.ReadAllText(DATA_FILE);
			ServerDataManifest.Rootobject serverDataValues =
				JsonConvert.DeserializeObject<ServerDataManifest.Rootobject>(serverJson);

			serverDataValues.AmountOpened += clientDataValues.AmountOpened;

			for (int i = 0; i < serverDataValues.DaysOpened.Length; i++) {
				clientDataValues.DaysOpened[i] = Regex.Replace(clientDataValues.DaysOpened[i], @"\s+", "");

				string[] clientSides = clientDataValues.DaysOpened[i].Split(':');
				long clientValue = Int64.Parse(clientSides[1]);

				serverDataValues.DaysOpened[i] = Regex.Replace(serverDataValues.DaysOpened[i], @"\s+", "");

				string[] serverSides = serverDataValues.DaysOpened[i].Split(':');
				long serverValue = Int64.Parse(serverSides[1]);

				serverValue += clientValue;

				string rebuiltString = serverSides[0] + ": " + serverValue;
				serverDataValues.DaysOpened[i] = rebuiltString;
			}

			serverDataValues.ProjectsCount += clientDataValues.ProjectsCount;

			serverDataValues.YearlyOpens += clientDataValues.YearlyOpens;
			serverDataValues.MonthlyOpens += clientDataValues.MonthlyOpens;
			serverDataValues.WeeklyOpens += clientDataValues.WeeklyOpens;
			
			if (clientDataValues.IsOpen) {
				serverDataValues.CurrentlyOpenSessions++;
			}
			else {
				serverDataValues.CurrentlyOpenSessions--;
			}

			// Write new values
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter js = new JsonTextWriter(sw)) {
				js.Formatting = Formatting.Indented;

				js.WriteStartObject();

				js.WritePropertyName("AmountOpened");
				js.WriteValue(serverDataValues.AmountOpened);

				js.WritePropertyName("DaysOpened");
				js.WriteStartArray();
				foreach (string day in serverDataValues.DaysOpened) {
					js.WriteValue(day);
				}
				js.WriteEnd();

				js.WritePropertyName("ProjectsCount");
				js.WriteValue(serverDataValues.ProjectsCount);

				js.WritePropertyName("YearlyOpens");
				js.WriteValue(serverDataValues.YearlyOpens);

				js.WritePropertyName("MonthlyOpens");
				js.WriteValue(serverDataValues.MonthlyOpens);

				js.WritePropertyName("WeeklyOpens");
				js.WriteValue(serverDataValues.WeeklyOpens);

				js.WritePropertyName("CurrentlyOpenSessions");
				js.WriteValue(serverDataValues.CurrentlyOpenSessions);

				js.WriteEndObject();
			}

			File.WriteAllText(DATA_FILE, sw.ToString());
			sb.Clear();
			sw.Close();
		}

		/// <summary>
		/// The thread for connections
		/// </summary>
		private static void BackgroundLisener() {
			IPAddress ip = IPAddress.Parse(GetLocalIP());
			IPEndPoint localEndPoint = new IPEndPoint(ip, PORT);

			Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(localEndPoint);
			server.Listen(100);

			while (true) {
				try {
					if (viewConnectionDetails) {
						Console.WriteLine("Waiting for a connection...");
					}
					
					Socket client = server.Accept();

					// Take up to 5mb of file. Should only be a few kb though
					byte[] buffer = new byte[1024 * 10000];

					// Receive bytes for file
					int bytesReceived = client.Receive(buffer);
					client.Close();

					char[] chars = new char[bytesReceived];
					Decoder decoder = Encoding.UTF8.GetDecoder();
					int charLength = decoder.GetChars(buffer, 0, bytesReceived, chars, 0);
					string fileData = new string(chars);

					string username = GetUserID(fileData);
					if (viewConnectionDetails) {
						Console.WriteLine("Received info from " + username);
					}

					if (username.Length < ID_LENGTH) {
						username = CreateNewID();
					}

					string savePath = FILE_SAVE_DIRECTORY + "\\" + username + ".json";
					File.WriteAllText(savePath, fileData);

					CopyData(savePath);
					File.Delete(savePath);

					if (viewConnectionDetails) {
						Console.WriteLine("Closed the server");
					}
				}
				catch (Exception e) {
					if (viewConnectionDetails) {
						Console.WriteLine("We had the following error: " + e);
					}
				}
			}
		}

		/// <summary>
		/// The thread for managing the server via the console.
		/// </summary>
		private static void BackgroundConsoleHelper() {
			while (true) {
				ConsoleColor green = ConsoleColor.Green;
				ConsoleColor red = ConsoleColor.Red;
				ConsoleColor blue = ConsoleColor.Blue;
				ConsoleColor yellow = ConsoleColor.Yellow;

				Console.ForegroundColor = green;
				Console.Clear();
				Console.WriteLine("Server's IP address: " + GetLocalIP());
				Console.WriteLine("\nWelcome to the Project Tracker server console!");
				Console.WriteLine("Available actions: ");
				
				Console.ForegroundColor = yellow;
				Console.WriteLine("1. View connection details");

				Console.ForegroundColor = blue;
				Console.WriteLine("2. View statistics");

				Console.ForegroundColor = red;
				Console.WriteLine("3. Stop the server console");

				Console.ForegroundColor = green;
				Console.Write("\n\nPlease input the number of the action of your choice > ");

				int action = 0;

				while (true) {
					ConsoleKeyInfo key = Console.ReadKey();
					if (key.Key == ConsoleKey.D1) {
						action = 1;
						break;
					}
					else if (key.Key == ConsoleKey.D2) {
						action = 2;
						break;
					}
					else if (key.Key == ConsoleKey.D3) {
						action = 3;
						break;
					}
				}

				if (action == 1) {
					// Display connection details
					Console.Clear();
					Console.ForegroundColor = yellow;
					Console.WriteLine("You're currently viewing connection details.");
					Console.WriteLine("Press E to go back to the main console menu.\n");
					viewConnectionDetails = true;

					while (viewConnectionDetails) {
						ConsoleKeyInfo key = Console.ReadKey();
						if (key.Key == ConsoleKey.E) {
							viewConnectionDetails = false;
						}
					}
				}
				else if (action == 2) {
					// Display statistics
					Console.Clear();
					Console.ForegroundColor = blue;
					Console.WriteLine("You're currently viewing statistics.");
					Console.WriteLine("Press E to go back to the main console menu.\n");
					viewStatistics = true;

					while (viewStatistics) {
						ConsoleKeyInfo key = Console.ReadKey();
						if (key.Key == ConsoleKey.E) {
							viewStatistics = false;
						}
						else if (key.Key == ConsoleKey.S) {
							if (speedUpStatistics) {
								speedUpStatistics = false;
							}
							else {
								speedUpStatistics = true;
							}
						}
					}
				}
				else if (action == 3) {
					// Close the console
					Environment.Exit(0);
				}
			}
		}

		private static void BackgroundStatistics() {
			int resetTimer = 600;
			long lastMinuteSessions = -1;
			while (true) {
				resetTimer -= 5;

				string serverJson = File.ReadAllText(DATA_FILE);
				ServerDataManifest.Rootobject serverDataValues =
					JsonConvert.DeserializeObject<ServerDataManifest.Rootobject>(serverJson);

				DayOfWeek weekDay = DateTime.Today.DayOfWeek;
				DateTime date = DateTime.Now;

				// Reset weekly statistics
				if (weekDay.ToString() == "Monday" && resetWeek == false) {
					resetWeek = true;

					serverDataValues.DaysOpened = resetDaysOpened;
					serverDataValues.WeeklyOpens = 0;
				}
				else if (weekDay.ToString() != "Monday") {
					resetWeek = false;
				}

				// Reset monthly statistics
				if (date.Month.ToString() != lastMonth) {
					lastMonth = date.Month.ToString();
					serverDataValues.MonthlyOpens = 0;
				}

				// Reset yearly statistics
				if (date.DayOfYear == 1) {
					serverDataValues.YearlyOpens = 0;
				}

				// Reset active statistics
				if (resetTimer == 0) {
					lastMinuteSessions = serverDataValues.CurrentlyOpenSessions;
					serverDataValues.CurrentlyOpenSessions = 0;
				}

				if (serverDataValues.CurrentlyOpenSessions < 0) {
					serverDataValues.CurrentlyOpenSessions = 0;
				}

				if (viewStatistics) {
					Console.Clear();
					Console.WriteLine("You're currently viewing statistics.");
					Console.WriteLine("Press E to go back to the main console menu.");
					Console.WriteLine("Press S to speed up/slow down statistics.");

					int secondsUntilReset = resetTimer;
					int minutesUntilReset = 0;

					while (secondsUntilReset > 59) {
						minutesUntilReset++;
						secondsUntilReset -= 60;
					}

					Console.WriteLine("Time until open sessions reset: {0}m {1}s\n", minutesUntilReset, secondsUntilReset);
					Console.WriteLine("Currently open sessions: " + serverDataValues.CurrentlyOpenSessions);
					if (lastMinuteSessions == -1) {
						Console.WriteLine("Last 10m open sessions: waiting for data...");
					}
					else {
						Console.WriteLine("Last 10m open sessions: " + lastMinuteSessions);
					}
					

					Console.WriteLine("\nTotal opens: " + serverDataValues.AmountOpened);
					Console.WriteLine("Total projects: " + serverDataValues.ProjectsCount);
					Console.WriteLine("Opens this year: " + serverDataValues.YearlyOpens);
					Console.WriteLine("Opens this month: " + serverDataValues.MonthlyOpens);
					Console.WriteLine("Opens this week: " + serverDataValues.WeeklyOpens);

					Console.WriteLine("-----------------");
					foreach (string day in serverDataValues.DaysOpened) {
						Console.WriteLine(day);
					}
				}

				// Write new values
				StringBuilder sb = new StringBuilder();
				StringWriter sw = new StringWriter(sb);

				using (JsonWriter js = new JsonTextWriter(sw)) {
					js.Formatting = Formatting.Indented;

					js.WriteStartObject();

					js.WritePropertyName("AmountOpened");
					js.WriteValue(serverDataValues.AmountOpened);

					js.WritePropertyName("DaysOpened");
					js.WriteStartArray();
					foreach (string day in serverDataValues.DaysOpened) {
						js.WriteValue(day);
					}
					js.WriteEnd();

					js.WritePropertyName("ProjectsCount");
					js.WriteValue(serverDataValues.ProjectsCount);

					js.WritePropertyName("YearlyOpens");
					js.WriteValue(serverDataValues.YearlyOpens);

					js.WritePropertyName("MonthlyOpens");
					js.WriteValue(serverDataValues.MonthlyOpens);

					js.WritePropertyName("WeeklyOpens");
					js.WriteValue(serverDataValues.WeeklyOpens);

					js.WritePropertyName("CurrentlyOpenSessions");
					js.WriteValue(serverDataValues.CurrentlyOpenSessions);

					js.WriteEndObject();
				}

				File.WriteAllText(DATA_FILE, sw.ToString());
				sb.Clear();
				sw.Close();

				if (resetTimer == 0) {
					resetTimer = 600;
				}

				if (speedUpStatistics) {
					Thread.Sleep(1000);
				}
				else {
					Thread.Sleep(5000);
				}
			}
		}

		static void Main(string[] args) {
			if (!Directory.Exists(FILE_SAVE_DIRECTORY)) {
				Directory.CreateDirectory(FILE_SAVE_DIRECTORY);
			}
			if (!File.Exists(DATA_FILE)) {
				// Write new values
				StringBuilder sb = new StringBuilder();
				StringWriter sw = new StringWriter(sb);

				using (JsonWriter js = new JsonTextWriter(sw)) {
					js.Formatting = Formatting.Indented;

					js.WriteStartObject();

					js.WritePropertyName("AmountOpened");
					js.WriteValue(0);

					js.WritePropertyName("DaysOpened");
					js.WriteStartArray();
					js.WriteValue("Monday: 0");
					js.WriteValue("Tuesday: 0");
					js.WriteValue("Wednesday: 0");
					js.WriteValue("Thursday: 0");
					js.WriteValue("Friday: 0");
					js.WriteValue("Saturday: 0");
					js.WriteValue("Sunday: 0");
					js.WriteEnd();

					js.WritePropertyName("ProjectsCount");
					js.WriteValue(0);

					js.WritePropertyName("YearlyOpens");
					js.WriteValue(0);

					js.WritePropertyName("MonthlyOpens");
					js.WriteValue(0);

					js.WritePropertyName("WeeklyOpens");
					js.WriteValue(0);

					js.WritePropertyName("CurrentlyOpenSessions");
					js.WriteValue(0);

					js.WriteEndObject();
				}

				File.WriteAllText(DATA_FILE, sw.ToString());
				sb.Clear();
				sw.Close();
			}

			DateTime date = DateTime.Now;
			lastMonth = date.Month.ToString();

			Thread backgroundListener = new Thread(BackgroundLisener);
			backgroundListener.Start();

			Thread backgroundConsoleHelper = new Thread(BackgroundConsoleHelper);
			backgroundConsoleHelper.Start();

			Thread backgroundStatistics = new Thread(BackgroundStatistics);
			backgroundStatistics.Start();
		}
	}
}
