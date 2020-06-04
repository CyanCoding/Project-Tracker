using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server_Communication_DLL {
	class FileManager {
		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly static string DATA_COLLECTION_FILE =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			+ "/Project Tracker/data-collection.json";
		private readonly static int ID_LENGTH = 30;

		// Manifest variables
		private static int amountOpened = 0;
		private static string[] daysOpened = {
			"Monday: 0",
			"Tuesday: 0",
			"Wednesday: 0",
			"Thursday: 0",
			"Friday: 0",
			"Saturday: 0",
			"Sunday: 0"
		};
		private static List<string> timesOpened = new List<string>();
		private static int projectsCount = 0;
		public static string userID = "null";

		/// <summary>
		/// Confirms that the file has all of the data and sets the collection
		/// variables to match the data that it pulls from the collection file.
		/// </summary>
		private static void ConfirmHasData() {
			if (!File.Exists(DATA_COLLECTION_FILE)) {
				throw new IOException("Data collection file does not exist");
			}

			try {
				string json = File.ReadAllText(DATA_COLLECTION_FILE);
				DataTransferManifest.Rootobject dataValues =
					JsonConvert.DeserializeObject<DataTransferManifest.Rootobject>(json);

				amountOpened = dataValues.AmountOpened;
				daysOpened = dataValues.DaysOpened;
				for (int i = 0; i < dataValues.TimesOpened.Length; i++) {
					timesOpened.Add(dataValues.TimesOpened[i]);
				}
				projectsCount = dataValues.ProjectsCount;
				userID = dataValues.UserID;
			}
			catch (IOException) {
				CreateNewData();
			}
			catch (NullReferenceException) {
				// This would probably be triggered if one of the values wasn't present
				// in the file or was invalid for some reason.
				// CreateNewData rewrites over values basically but keeps present ones.
				CreateNewData();
			}
		}

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

		/// <summary>
		/// Writes to the file with new values or existing old ones pulled from
		/// the collection file.
		/// </summary>
		private static void CreateNewData() {
			if (File.Exists(DATA_COLLECTION_FILE)) {
				File.Delete(DATA_COLLECTION_FILE);
			}

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);

			using (JsonWriter js = new JsonTextWriter(sw)) {
				js.Formatting = Formatting.Indented;

				js.WriteStartObject();

				js.WritePropertyName("AmountOpened");
				js.WriteValue(amountOpened);

				js.WritePropertyName("DaysOpened");
				js.WriteStartArray();
				foreach (string day in daysOpened) {
					js.WriteValue(day);
				}
				js.WriteEnd();

				js.WritePropertyName("TimesOpened");
				js.WriteStartArray();
				foreach (string time in timesOpened) {
					js.WriteValue(time);
				}
				js.WriteEnd();

				js.WritePropertyName("ProjectsCount");
				js.WriteValue(projectsCount);

				js.WritePropertyName("UserID");
				if (userID.Length < ID_LENGTH) {
					js.WriteValue(CreateNewID());
				}

				js.WriteEndObject();
			}

			File.WriteAllText(DATA_COLLECTION_FILE, sw.ToString());
			sb.Clear();
			sw.Close();
		}

		/// <summary>
		/// Makes sure the data file is complete.
		/// </summary>
		public static void PrepareDataTransfer() {
			try {
				ConfirmHasData();
			}
			catch (IOException) {
				// The file didn't exist
				CreateNewData();
			}
		}
	}
}
