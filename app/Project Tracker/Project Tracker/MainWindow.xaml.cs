using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Project_Tracker {

	public partial class MainWindow : Window {
		private int rowsAdded = 0; // We use a global variable so the AddRow function knows what id of a row to edit
		private int rowSelectionID = 0; // We use this to identify which row is currently selected
		private int oldRowSelectionID = 1; // We use this to identify which row was last selected
		private int filesUpdate = 0;
		private bool addedProgram = false; // We do this so that we can only open a program if there is one to prevent an error
		private static bool firstRun = true; // We use this to not call startup stuff more than once when MainWindow is called.
		private List<string> tableValues = new List<string>();
		public List<string> filesRead = new List<string>();
		private Thread tableUpdateThread;
		

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly Color selectionColor = Color.FromRgb(84, 207, 255);

		private readonly Color sortColor = Color.FromRgb(228, 233, 235);
		private readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");
		private readonly string pathExtension = "*.json";
		private readonly string DATA_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/data";
		private readonly string VERSION_MANIFEST_URL = "https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version.json";
		private readonly string VERSION_INFO = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.json";
		private readonly string CURRENT_VERSION = "1.2"; // IF YOU CHANGE THIS, ALSO CHANGE IT IN UpdateWindow.xaml.cs

		public MainWindow() {
			InitializeComponent();
			Startup();
		}

		/// <summary>
		/// Updates the table by adding any new files to the table.
		/// </summary>
		private void UpdateTable() {

		}

		/// <summary>
		/// Startup function that runs when code execution starts.
		/// </summary>
		private void Startup() {
			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker")) {
				Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker");
			}
			if (!Directory.Exists(DATA_DIRECTORY)) {
				Directory.CreateDirectory(DATA_DIRECTORY);
			}

			versionLabel.Content = "Version " + CURRENT_VERSION;

			if (firstRun) {
				try {
					string[] files = Directory.GetFiles(DATA_DIRECTORY, pathExtension, SearchOption.AllDirectories);
					foreach (string path in files) {
						if (!filesRead.Contains(path)) {
							// Convert each json file to a table row
							string json = File.ReadAllText(path);
							MainTableManifest.Rootobject mainTable = JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

							name1.Content = mainTable.Title;
							percent1.Content = mainTable.Percent + "%";
							if (mainTable.Icon == "cplusplus") {
								image1.Source = (ImageSource)TryFindResource("cplusplusDrawingImage");
							}
							if (mainTable.Icon == null) { // Handle them not having that item in the file (probably just updated)

							}

							filesRead.Add(path);
						}
					}
				}
				catch (IOException) {
				}

				if (File.Exists(VERSION_INFO)) {
					File.Delete(VERSION_INFO);
				}

				try {
					WebClient client = new WebClient();
					client.DownloadFileCompleted += new AsyncCompletedEventHandler(ShowUpdate);
					client.DownloadFileAsync(new Uri(VERSION_MANIFEST_URL), VERSION_INFO);
				}
				catch (WebException) {
					// Couldn't download update file. Possible their wifi isn't working
				}

				firstRun = false;
			}

			tableUpdateThread = new Thread(UpdateTable); // Start the background resize thread
			tableUpdateThread.Start();
		}

		/// <summary>
		/// Runs UpdateWindow when an update is detected.
		/// </summary>
		private void ShowUpdate(object sender, AsyncCompletedEventArgs e) {
			string json = File.ReadAllText(VERSION_INFO);

			UpdateManifest.Rootobject update = JsonConvert.DeserializeObject<UpdateManifest.Rootobject>(json);

			if (float.Parse(CURRENT_VERSION) < float.Parse(update.Version)) { // Update is available
				UpdateWindow updateWindow = new UpdateWindow();
				updateWindow.Show();
			}
		}

		/// <summary>
		/// Runs when a key is pressed.
		/// Controls selection of main table.
		/// </summary>
		private void KeyPress(object sender, KeyEventArgs e) {

		}

		/// <summary>
		/// Launches AddProgram.
		/// </summary>
		private void AddProgram(object sender, MouseButtonEventArgs e) {
			AddNewProgram newProgram = new AddNewProgram();
			newProgram.Show();
		}

		/// <summary>
		/// Launches EditProgram.
		/// </summary>
		private void EditProgram(object sender, MouseButtonEventArgs e) {
			if (addedProgram == true) {
				Passthrough.EditingFile = filesRead[rowSelectionID - 1];

				EditProgram editProgram = new EditProgram();
				editProgram.Show();
			}
		}

		/// <summary>
		/// Closes threads and shuts down the program.
		/// </summary>
		private void Window_Closing(object sender, CancelEventArgs e) {
			tableUpdateThread.Abort();
			Application.Current.Shutdown(); // If we don't do this, the AddNewProgram window doesn't close and the program keeps running in the bg
		}
	}
}