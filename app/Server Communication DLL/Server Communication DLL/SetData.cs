using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server_Communication_DLL {
	public class SetData {
		public static void SetActivity(bool isActive) {
			FileManager.PrepareDataTransfer();

			FileManager.isOpen = isActive;

			if (isActive) {
				FileManager.amountOpened++;
				FileManager.yearlyOpens++;
				FileManager.monthlyOpens++;
				FileManager.weeklyOpens++;

				DayOfWeek day = DateTime.Today.DayOfWeek;

				if (day.ToString() == "Monday") {
					FileManager.daysOpened[0] = Regex.Replace(FileManager.daysOpened[0], @"\s+", "");

					string[] sides = FileManager.daysOpened[0].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[0] = rebuiltString;
				}
				else if (day.ToString() == "Tuesday") {
					FileManager.daysOpened[1] = Regex.Replace(FileManager.daysOpened[1], @"\s+", "");

					string[] sides = FileManager.daysOpened[1].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[1] = rebuiltString;
				}
				else if (day.ToString() == "Wednesday") {
					FileManager.daysOpened[2] = Regex.Replace(FileManager.daysOpened[2], @"\s+", "");

					string[] sides = FileManager.daysOpened[2].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[2] = rebuiltString;
				}
				else if (day.ToString() == "Thursday") {
					FileManager.daysOpened[3] = Regex.Replace(FileManager.daysOpened[3], @"\s+", "");

					string[] sides = FileManager.daysOpened[3].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[3] = rebuiltString;
				}
				else if (day.ToString() == "Friday") {
					FileManager.daysOpened[4] = Regex.Replace(FileManager.daysOpened[4], @"\s+", "");

					string[] sides = FileManager.daysOpened[4].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[4] = rebuiltString;
				}
				else if (day.ToString() == "Saturday") {
					FileManager.daysOpened[5] = Regex.Replace(FileManager.daysOpened[5], @"\s+", "");

					string[] sides = FileManager.daysOpened[5].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[5] = rebuiltString;
				}
				else if (day.ToString() == "Sunday") {
					FileManager.daysOpened[6] = Regex.Replace(FileManager.daysOpened[6], @"\s+", "");

					string[] sides = FileManager.daysOpened[6].Split(':');
					long value = Int64.Parse(sides[1]);

					value++;

					string rebuiltString = sides[0] + ": " + value;
					FileManager.daysOpened[6] = rebuiltString;
				}
			}

			FileManager.SaveData();
			Connections.CreateConnection();
		}

		public static void Refresh(int a) {
			Connections.CreateConnection();
			// Add something here about how long the user has been active
		}
	}
}
