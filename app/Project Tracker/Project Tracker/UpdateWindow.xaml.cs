using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_Tracker {
	/// <summary>
	/// Interaction logic for UpdateWindow.xaml
	/// </summary>
	public partial class UpdateWindow : Window {

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		readonly string VERSION_INFO = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.json";
		readonly string INSTALLER_PATH = @"C:\Program Files\Project Tracker\Project Tracker Installer.exe";
		readonly string VERSION_FILE = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/update.txt";
		readonly string CURRENT_VERSION = "0.6";
		readonly Color sortColor = Color.FromRgb(228, 233, 235);
		readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		int updateRowsAdded = 0;
		int fixRowsAdded = 0;


		public UpdateWindow() {
			InitializeComponent();

			Startup();
		}

		private void AddRow(string value, Table table, int tableCount) {
			table.RowGroups[0].Rows.Add(new TableRow());

			TableRow newRow = null;

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

				/*if (featureRowsAdded == 1) { // It's the first row so we add the selection to it
					newRow.Background = new SolidColorBrush(selectionColor);
					rowSelectionID++;
				}*/

				if (fixRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}

			newRow.FontSize = 16;
			newRow.FontFamily = textFont;
			newRow.Cells.Add(new TableCell(new Paragraph(new Run(value))));
		}

		private void Startup() {
			if (File.Exists(VERSION_FILE)) {
				File.Delete(VERSION_FILE);
			}
			AddRow("hiiiiii iiiiiiiii iiiiiiii iiiiiiiiii iiiiiiiii iiiiiiiiii iiiiiiii iiiiii iiiiiii iiiiiiiiii iiiiiiii iiiiiiiiii iiiiiiii iiiiiiiiii iiiiiiiiii iiiiiiiiii iiiiii iiiiiiii iii i iiiiiiiii iiiiii iiiiiiiii iiiiiiiiiii iiiiiiiii iiiiii iiiiiii iiiiiii ii iiiiiiiiiii iiiii iiiiiii iiiiiiiii iiiiiiii iiiiiiii ii", featureTable, 0);
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
