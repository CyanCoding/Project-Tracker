using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
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
		private readonly string DATA_DIRECTORY = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/data";
		private readonly string VERSION_MANIFEST_URL = 
			"https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version.json";
		private readonly string VERSION_INFO = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.json";
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
		/// Checks each file value for null values.
		/// </summary>
		/// <returns>True if no null values, otherwise false.</returns>
		private bool CheckValues(string title, string[] errors, string[] errorsData,
			string[] features, string[] featuresData, string[] comments, 
			string[] commentsData, string duration, string icon, string percent) {


			if (title == null || errors == null || errorsData == null || features == null
				|| featuresData == null || comments == null || commentsData == null
				|| duration == null || icon == null || percent == null) { 
				// Check all strings for null values
				return false;
			}

			return true;
		}

		/// <summary>
		/// Loads a project to the window.
		/// </summary>
		/// <param name="title">The project title.</param>
		/// <param name="errors">The project errors.</param>
		/// <param name="errorsData">The project error data.</param>
		/// <param name="features">The project features.</param>
		/// <param name="featuresData">The project feature data.</param>
		/// <param name="comments">The project comments.</param>
		/// <param name="commentsData">The project comments data.</param>
		/// <param name="duration">The project duration.</param>
		/// <param name="icon">The project icon.</param>
		/// <param name="percent">The project percent complete.</param>
		/// <param name="index">The project index number.</param>
		private void AddProjectToWindow(string title, string[] errors, string[] errorsData,
			string[] features, string[] featuresData, string[] comments,
			string[] commentsData, string duration, string icon, string percent, int index) {

			switch (index) {
				case 1:
					image1.Source = (ImageSource)TryFindResource(icon);
					name1.Content = title;
					percent1.Content = percent + "%";
					break;
				case 2:
					image2.Source = (ImageSource)TryFindResource(icon);
					name2.Content = title;
					percent2.Content = percent + "%";
					break;
				case 3:
					image3.Source = (ImageSource)TryFindResource(icon);
					name3.Content = title;
					percent3.Content = percent + "%";
					break;
				case 4:
					image4.Source = (ImageSource)TryFindResource(icon);
					name4.Content = title;
					percent4.Content = percent + "%";
					break;
				case 5:
					image5.Source = (ImageSource)TryFindResource(icon);
					name5.Content = title;
					percent5.Content = percent + "%";
					break;
				case 6:
					image6.Source = (ImageSource)TryFindResource(icon);
					name6.Content = title;
					percent6.Content = percent + "%";
					break;
				case 7:
					image7.Source = (ImageSource)TryFindResource(icon);
					name7.Content = title;
					percent7.Content = percent + "%";
					break;
				case 8:
					image8.Source = (ImageSource)TryFindResource(icon);
					name8.Content = title;
					percent8.Content = percent + "%";
					break;
				case 9:
					image9.Source = (ImageSource)TryFindResource(icon);
					name9.Content = title;
					percent9.Content = percent + "%";
					break;
				case 10:
					image10.Source = (ImageSource)TryFindResource(icon);
					name10.Content = title;
					percent10.Content = percent + "%";
					break;
			}
		}

		/// <summary>
		/// Startup function that runs when code execution starts.
		/// </summary>
		private void Startup() {
			if (!Directory.Exists(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker")) {

				Directory.CreateDirectory(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker");
			}
			if (!Directory.Exists(DATA_DIRECTORY)) {
				Directory.CreateDirectory(DATA_DIRECTORY);
			}

			versionLabel.Content = "Version " + CURRENT_VERSION;

			if (firstRun) {
				try {
					string[] files = Directory.GetFiles(DATA_DIRECTORY, 
						pathExtension, SearchOption.AllDirectories);

					int index = 0;
					foreach (string path in files) {
						if (!filesRead.Contains(path)) {
							// Convert each json file to a table row
							string json = File.ReadAllText(path);
							MainTableManifest.Rootobject mainTable = 
								JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

							bool fileHasAllValues = CheckValues(
								mainTable.Title, mainTable.Errors, mainTable.ErrorsData,
								mainTable.Features, mainTable.FeaturesData, mainTable.Comments,
								mainTable.CommentsData, mainTable.Duration, mainTable.Icon,
								mainTable.Percent
								);

							if (!fileHasAllValues) {
								// This is an older file with improper values
								// We need to migrate it to a new version.
								string notice = "";
								if (mainTable.Title != null) {
									notice = "The " + mainTable.Title + " project is missing data that" +
										" is necessary to load it. This is likely due to having upgraded" +
										" recently to a new version of the Project Tracker. Do you want the" +
										" Project Tracker to atempt to automatically fix the project?";
								}
								else {
									notice = "The " + path + " project file is missing data that is" +
										" necessary to load it. This is likely due to having upgraded" +
										" recently to a new version of the Project Tracker. Do you want" +
										" the Project Tracker to attempt to automatically fix the project" +
										" file?";
								}

								var migrateData = System.Windows.Forms.MessageBox.Show(
									notice, 
									"Missing File Information", 
									System.Windows.Forms.MessageBoxButtons.YesNo, 
									System.Windows.Forms.MessageBoxIcon.Question);

								if (migrateData == System.Windows.Forms.DialogResult.Yes) {
									StringBuilder sb = new StringBuilder();
									StringWriter sw = new StringWriter(sb);

									using (JsonWriter js = new JsonTextWriter(sw)) {
										js.Formatting = Formatting.Indented;

										js.WriteStartObject();

										// Title
										js.WritePropertyName("Title");
										if (mainTable.Title != null) {
											js.WriteValue(mainTable.Title);
										}
										else {
											js.WriteValue("Untitled project");
										}

										// Errors
										js.WritePropertyName("Errors");
										js.WriteStartArray();
										if (mainTable.Errors != null) {
											foreach (string error in mainTable.Errors) {
												js.WriteValue(error);
											}
										}
										js.WriteEnd();

										// ErrorsData
										js.WritePropertyName("ErrorsData");
										js.WriteStartArray();
										if (mainTable.ErrorsData != null) {
											foreach (string data in mainTable.ErrorsData) {
												js.WriteValue(data);
											}
										}
										js.WriteEnd();

										// Features
										js.WritePropertyName("Features");
										js.WriteStartArray();
										if (mainTable.Features != null) {
											foreach (string feature in mainTable.Features) {
												js.WriteValue(feature);
											}
										}
										js.WriteEnd();
										js.WritePropertyName("FeaturesData");

										// FeaturesData
										js.WriteStartArray();
										if (mainTable.FeaturesData != null) {
											foreach (string data in mainTable.FeaturesData) {
												js.WriteValue(data);
											}
										}
										js.WriteEnd();

										// Comments
										js.WritePropertyName("Comments");
										js.WriteStartArray();
										if (mainTable.Comments != null) {
											foreach (string comment in mainTable.Comments) {
												js.WriteValue(comment);
											}
										}
										js.WriteEnd();

										// CommentsData
										js.WritePropertyName("CommentsData");
										js.WriteStartArray();
										if (mainTable.CommentsData != null) {
											foreach (string data in mainTable.CommentsData) {
												js.WriteValue(data);
											}
										}
										js.WriteEnd();

										// Duration
										js.WritePropertyName("Duration");
										if (mainTable.Duration != null) {
											js.WriteValue(mainTable.Duration);
										}
										else {
											js.WriteValue("00:00:00");
										}

										// Icon
										js.WritePropertyName("Icon");
										if (mainTable.Icon != null) {
											js.WriteValue(mainTable.Icon);
										}
										else {
											js.WriteValue("noIcon");
										}

										// Percent
										js.WritePropertyName("Percent");
										if (mainTable.Percent != null) {
											js.WriteValue(mainTable.Percent);
										}
										else {
											js.WriteValue("00");
										}
										

										js.WriteEndObject();
									}

									File.WriteAllText(path, sw.ToString());
									sb.Clear();
									sw.Close();

									fileHasAllValues = true;
									// Reread all the data back again
									json = File.ReadAllText(path);
									mainTable =
										JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

									System.Windows.Forms.MessageBox.Show(
										"Your data has been migrated! The project has been loaded.",
										"Project Migrated",
										System.Windows.Forms.MessageBoxButtons.OK,
										System.Windows.Forms.MessageBoxIcon.Information);


								}
								else { // They chose not to migrate the data
									System.Windows.Forms.MessageBox.Show(
										"Your data has not been migrated. You cannot load this project.",
										"Project Skipped",
										System.Windows.Forms.MessageBoxButtons.OK,
										System.Windows.Forms.MessageBoxIcon.Information);
								}
							}

							if (fileHasAllValues) {
								// Every value in the manifest table is covered
								// Meaning that this is an updated file
								index++;
								if (index > 10) {
									System.Windows.Forms.MessageBox.Show(
									"Some projects have been left unloaded because you have reached the" +
									" 10 project limit. Please finish projects to view more.",
									"Some Projects Left Unloaded",
									System.Windows.Forms.MessageBoxButtons.OK,
									System.Windows.Forms.MessageBoxIcon.Information);
									break;
								}

								AddProjectToWindow(mainTable.Title, mainTable.Errors, mainTable.ErrorsData,
									mainTable.Features, mainTable.FeaturesData, mainTable.Comments,
									mainTable.CommentsData, mainTable.Duration, mainTable.Icon,
									mainTable.Percent, index);
								filesRead.Add(path);
							}
							
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

			UpdateManifest.Rootobject update = 
				JsonConvert.DeserializeObject<UpdateManifest.Rootobject>(json);

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
			Application.Current.Shutdown(); 
			// If we don't do this, the AddNewProgram window doesn't close and the program keeps running in the bg
		}
	}
}