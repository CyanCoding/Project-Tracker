using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Project_Tracker {

	/// <summary>
	/// Interaction logic for UpdateWindow.xaml
	/// </summary>
	public partial class UpdateWindow : Window {

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly string VERSION_INFO = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.json";

		private readonly string INSTALLER_PATH = @"C:\Program Files\Project Tracker\Project Tracker Installer.exe";
		private readonly string VERSION_FILE = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/update.txt";
		private readonly string CURRENT_VERSION = "2.1";
		private readonly Color sortColor = Color.FromRgb(228, 233, 235);
		private readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		private int updateRowsAdded = 0;
		private int fixRowsAdded = 0;

		public UpdateWindow() {
			InitializeComponent();

			Startup();
		}

		/// <summary>
		/// Adds a row to the provided table.
		/// </summary>
		/// <param name="value">The value to add to the table.</param>
		/// <param name="table">The table to add a value to.</param>
		/// <param name="tableCount">The id of the table (0 = updates, 1 = fixes)</param>
		private void AddRow(string value, Table table, int tableCount) {
			table.RowGroups[0].Rows.Add(new TableRow());

			TableRow newRow = null;

			// For some reason the tables put in like really weird spacing if rows are
			// longer than one line. I coded my own kind of "text-wrap" property. Basically
			// it waits for 58 characters and then finds the next space. At that point it
			// inserts a new line. It seems to solve the issue, though I'm sure there could
			// be some kind of string that might break this (like a really long word or
			// something I'm not sure. A temporary fix. I suggest making a better method
			// soon, but it's not a priority.
			int index = -1;
			bool isLookingForSpace = false;

			foreach (char c in value) {
				index++;

				if (c == ' ' && isLookingForSpace) {
					value = value.Remove(index, 1);
					value = value.Insert(index, "\n");
					isLookingForSpace = false;
				}
				if (index % 58 == 0 && index > 57) {
					isLookingForSpace = true;
				}
			}

			if (tableCount == 0) { // Update table
				newRow = table.RowGroups[0].Rows[updateRowsAdded];

				updateRowsAdded++;

				if (updateRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}
			else if (tableCount == 1) { // Error table
				newRow = table.RowGroups[0].Rows[fixRowsAdded];

				fixRowsAdded++;

				if (fixRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}

			newRow.FontSize = 16;
			newRow.FontFamily = textFont;
			newRow.Cells.Add(new TableCell(new Paragraph(new Run(value))));
		}

		/// <summary>
		/// Startup function that runs when code execution begins.
		/// </summary>
		private void Startup() {
			if (File.Exists(VERSION_FILE)) {
				File.Delete(VERSION_FILE);
			}

			try {
				using (StreamReader reader = new StreamReader(VERSION_INFO)) {
					string json = reader.ReadToEnd();

					dynamic array = JsonConvert.DeserializeObject(json);

					UpdateManifest.Rootobject update = JsonConvert.DeserializeObject<UpdateManifest.Rootobject>(json);

					versionTitle.Text = "Version " + CURRENT_VERSION + " → " + update.Version;
					featureTitle.Text = "New features (" + update.Updates.Length + "):";
					fixTitle.Text = "Fixes (" + update.Fixes.Length + "):";

					foreach (string feature in update.Updates) {
						AddRow(feature, featureTable, 0);
					}
					foreach (string fix in update.Fixes) {
						AddRow(fix, fixTable, 1);
					}

					string[] versionLines = { update.Version };
					File.WriteAllLines(VERSION_FILE, versionLines);
				}
			}
			catch (IOException) { // Can't access the file for some reason
				versionTitle.Text = "An error occurred while updating";
				updateButton.Visibility = Visibility.Hidden;
				Height = 125;
			}
		}

		/// <summary>
		/// Launches the installer when the update button is pressed.
		/// </summary>
		private void Button_Click(object sender, RoutedEventArgs e) {
			ProcessStartInfo start = new ProcessStartInfo {
				// Enter in the command line arguments, everything you would enter after the executable name itself
				Arguments = "update",
				// Enter the executable to run, including the complete path
				FileName = INSTALLER_PATH,
				// Do you want to show a console window?
				WindowStyle = ProcessWindowStyle.Normal,
				CreateNoWindow = true
			};
			Process.Start(start);
			Environment.Exit(0);
		}
	}
}