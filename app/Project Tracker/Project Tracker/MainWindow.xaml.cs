﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Project_Tracker {

	public partial class MainWindow : UWPHost.Window {
		private int filesUpdate = 0;
		private bool addedProgram = false; // We do this so that we can only open a program if there is one to prevent an error
		private List<string> tableValues = new List<string>();
		public List<string> filesRead = new List<string>();
		private int itemsAdded = 0; // The amount of items added to the scrollviewer
		private bool isCompletedTasksShown = false;
		private bool isSwitchingAnimationRunning = false; // Keeps the user from double clicking the animation
		private int selectedIndex = 0;
		

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly Color selectionColor = Color.FromRgb(84, 207, 255);
		private readonly Color itemColor = Color.FromRgb(60, 60, 60);
		private readonly Color labelTextColor = Color.FromRgb(255, 255, 255);
		private readonly Color sortColor = Color.FromRgb(228, 233, 235);
		private readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");
		private readonly string pathExtension = "*.json";
		private readonly string DATA_DIRECTORY = 
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) 
			+ "/Project Tracker/data";
		private readonly string VERSION_MANIFEST_URL = 
			"https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version.json";
		private readonly string VERSION_INFO = Environment.GetFolderPath
			(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.json";
		private readonly string CURRENT_VERSION = "1.5"; // IF YOU CHANGE THIS, ALSO CHANGE IT IN UpdateWindow.xaml.cs
		private readonly string APPDATA_DIRECTORY =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			+ "/Project Tracker";
		private readonly string SETTINGS_FILE =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			+ "/Project Tracker/settings.json";

		public MainWindow() {
			InitializeComponent();
			Startup();
		}


		/// <summary>
		/// Loads an item into the scrollviewer.
		/// </summary>
		/// <param name="text">The text value of the item.</param>
		/// <param name="type">The type of the item (0 = error, 1 = feature, 2 = comment).</param>
		private void LoadValues(string text, int type) {
			// The grid holds all of the values
			Grid grid = new Grid();
			grid.Margin = new Thickness(0, itemsAdded * 65, 0, 0);
			grid.VerticalAlignment = VerticalAlignment.Top;
			grid.HorizontalAlignment = HorizontalAlignment.Center;

			ColumnDefinition column1 = new ColumnDefinition();
			column1.Width = new GridLength(50, GridUnitType.Pixel);
			ColumnDefinition column2 = new ColumnDefinition();
			column2.Width = new GridLength(620, GridUnitType.Pixel);
			ColumnDefinition column3 = new ColumnDefinition();
			column3.Width = new GridLength(50, GridUnitType.Pixel);

			grid.ColumnDefinitions.Add(column1);
			grid.ColumnDefinitions.Add(column2);
			grid.ColumnDefinitions.Add(column3);

			// The border is a curved rectangle behind all the other objects
			Border border = new Border();
			border.Width = 725;
			border.Height = 60;
			border.CornerRadius = new CornerRadius(10);
			border.Background = new SolidColorBrush(itemColor);
			border.BorderThickness = new Thickness(2);
			border.HorizontalAlignment = HorizontalAlignment.Left;
			border.VerticalAlignment = VerticalAlignment.Top;
			border.Margin = new Thickness(0, 0, -5, 0);
			border.SetValue(Grid.ColumnSpanProperty, 4);

			// The canvas item for the checkmark
			Canvas checkmarkCanvas = new Canvas();
			checkmarkCanvas.HorizontalAlignment = HorizontalAlignment.Center;
			checkmarkCanvas.VerticalAlignment = VerticalAlignment.Center;
			checkmarkCanvas.Height = 40;
			checkmarkCanvas.Width = 40;
			checkmarkCanvas.SetValue(Grid.ColumnProperty, 0);
			checkmarkCanvas.Margin = new Thickness(5, 10, 5, 10);

			// The checkmark image
			Image checkmarkImage = new Image();
			checkmarkImage.Height = 40;
			checkmarkImage.Width = 40;
			Canvas.SetLeft(checkmarkImage, 10);
			checkmarkImage.Cursor = Cursors.Hand;
			checkmarkImage.Style = (Style)TryFindResource("checkbox");

			checkmarkCanvas.Children.Add(checkmarkImage);

			// The canvas item for the item type (error/feature/comment)
			Canvas typeCanvas = new Canvas();
			typeCanvas.HorizontalAlignment = HorizontalAlignment.Center;
			typeCanvas.VerticalAlignment = VerticalAlignment.Center;
			typeCanvas.Height = 30;
			typeCanvas.Width = 30;
			typeCanvas.SetValue(Grid.ColumnProperty, 2);

			// The type image
			Image typeImage = new Image();
			typeImage.Height = 30;
			typeImage.Width = 30;

			if (type == 0) {
				typeImage.Source = (ImageSource)TryFindResource("errorDrawingImage");
				border.BorderBrush = new SolidColorBrush(Colors.IndianRed);
			}
			else if (type == 1) {
				typeImage.Source = (ImageSource)TryFindResource("featureDrawingImage");
				border.BorderBrush = new SolidColorBrush(Colors.Yellow);
			}
			else if (type == 2) {
				typeImage.Source = (ImageSource)TryFindResource("commentDrawingImage");
				border.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
			}
			else {
				typeImage.Source = (ImageSource)TryFindResource("noIcon");
				border.BorderBrush = new SolidColorBrush(Colors.White);
			}

			typeCanvas.Children.Add(typeImage);

			// The label displaying the content of the item
			Label label = new Label();
			label.Content = text;
			label.Foreground = new SolidColorBrush(labelTextColor);
			label.Margin = new Thickness(80, 6, 20, 0);
			label.FontSize = 24;

			border.Child = label;
			grid.Children.Add(border);
			grid.Children.Add(checkmarkCanvas);
			grid.Children.Add(typeCanvas);
			scrollviewerGrid.Children.Add(grid);
			
			itemsAdded++;
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
					border1.Visibility = Visibility.Visible;
					break;
				case 2:
					image2.Source = (ImageSource)TryFindResource(icon);
					name2.Content = title;
					percent2.Content = percent + "%";
					border2.Visibility = Visibility.Visible;
					break;
				case 3:
					image3.Source = (ImageSource)TryFindResource(icon);
					name3.Content = title;
					percent3.Content = percent + "%";
					border3.Visibility = Visibility.Visible;
					break;
				case 4:
					image4.Source = (ImageSource)TryFindResource(icon);
					name4.Content = title;
					percent4.Content = percent + "%";
					border4.Visibility = Visibility.Visible;
					break;
				case 5:
					image5.Source = (ImageSource)TryFindResource(icon);
					name5.Content = title;
					percent5.Content = percent + "%";
					border5.Visibility = Visibility.Visible;
					break;
				case 6:
					image6.Source = (ImageSource)TryFindResource(icon);
					name6.Content = title;
					percent6.Content = percent + "%";
					border6.Visibility = Visibility.Visible;
					break;
				case 7:
					image7.Source = (ImageSource)TryFindResource(icon);
					name7.Content = title;
					percent7.Content = percent + "%";
					border7.Visibility = Visibility.Visible;
					break;
				case 8:
					image8.Source = (ImageSource)TryFindResource(icon);
					name8.Content = title;
					percent8.Content = percent + "%";
					border8.Visibility = Visibility.Visible;
					break;
				case 9:
					image9.Source = (ImageSource)TryFindResource(icon);
					name9.Content = title;
					percent9.Content = percent + "%";
					border9.Visibility = Visibility.Visible;
					break;
				case 10:
					image10.Source = (ImageSource)TryFindResource(icon);
					name10.Content = title;
					percent10.Content = percent + "%";
					border10.Visibility = Visibility.Visible;
					break;
			}
		}

		/// <summary>
		/// Runs when the update file has been downloaded. 
		/// Notifies the user of an update if one is available.
		/// </summary>
		private void Update(object sender, AsyncCompletedEventArgs e) {
			string json = File.ReadAllText(VERSION_INFO);

			UpdateManifest.Rootobject update =
				JsonConvert.DeserializeObject<UpdateManifest.Rootobject>(json);

			if (float.Parse(CURRENT_VERSION) < float.Parse(update.Version)) { // Update is available
				Thread thread = new Thread(() => {
					Thread.Sleep(5000);
					Dispatcher.Invoke(new Action(() => {
						updateGrid.Visibility = Visibility.Visible;

						DoubleAnimation animation = new DoubleAnimation();
						animation.From = 0;
						animation.To = 130;
						animation.Duration = TimeSpan.FromSeconds(0.2);

						updateGrid.BeginAnimation(HeightProperty, animation);
					}));
				});
				thread.Start();
			}
		}

		/// <summary>
		/// Highlights the selected project based on selectedIndex.
		/// </summary>
		private void SetSelectedProject() {
			border1.Style = (Style)TryFindResource("hoverOver");
			border2.Style = (Style)TryFindResource("hoverOver");
			border3.Style = (Style)TryFindResource("hoverOver");
			border4.Style = (Style)TryFindResource("hoverOver");
			border5.Style = (Style)TryFindResource("hoverOver");
			border6.Style = (Style)TryFindResource("hoverOver");
			border7.Style = (Style)TryFindResource("hoverOver");
			border8.Style = (Style)TryFindResource("hoverOver");
			border9.Style = (Style)TryFindResource("hoverOver");
			border10.Style = (Style)TryFindResource("hoverOver");

			scrollviewerGrid.Children.Clear();
			itemsAdded = 0;

			if (selectedIndex == 1) {
				border1.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 2) {
				border2.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 3) {
				border3.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 4) {
				border4.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 5) {
				border5.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 6) {
				border6.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 7) {
				border7.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 8) {
				border8.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 9) {
				border9.Style = (Style)TryFindResource("solidOver");
			}
			else if (selectedIndex == 10) {
				border10.Style = (Style)TryFindResource("solidOver");
			}
			else { // selectedIndex isn't a valid number
				selectedIndex = 0;
			}

			// Read the values from this project
			if (selectedIndex != 0) {
				noProjectsGrid.Visibility = Visibility.Hidden;
				displayingImage.Visibility = Visibility.Visible;
				displayingTitle.Visibility = Visibility.Visible;
				scrollviewerGrid.Visibility = Visibility.Visible;
				completeGrid.Visibility = Visibility.Visible;

				string json = File.ReadAllText(filesRead[selectedIndex - 1]);
				MainTableManifest.Rootobject projectInfo =
					JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

				// Set values
				displayingTitle.Content = projectInfo.Title;
				displayingImage.Source = (ImageSource)TryFindResource(projectInfo.Icon);

				for (int i = 0; i < projectInfo.Errors.Length; i++) {
					if (projectInfo.ErrorsData[i] == "0") { // It's not a completed task
						LoadValues(projectInfo.Errors[i], 0);
					}
				}
				for (int i = 0; i < projectInfo.Features.Length; i++) {
					if (projectInfo.FeaturesData[i] == "0") { // It's not a completed task
						LoadValues(projectInfo.Features[i], 1);
					}
				}
				for (int i = 0; i < projectInfo.Comments.Length; i++) {
					if (projectInfo.CommentsData[i] == "0") { // It's not a completed task
						LoadValues(projectInfo.Comments[i], 2);
					}
				}
			}
		}

		/// <summary>
		/// Startup function that runs when code execution starts.
		/// </summary>
		private void Startup() {
			this.Height = 815;
			this.Width = 1200;
			// Item positioning

			if (!Directory.Exists(APPDATA_DIRECTORY)) { // Create AppData directory
				Directory.CreateDirectory(APPDATA_DIRECTORY);
			}
			if (!Directory.Exists(DATA_DIRECTORY)) { // Create AppData/Data directory
				Directory.CreateDirectory(DATA_DIRECTORY);
			}

			if (!File.Exists(SETTINGS_FILE)) { // Settings file doesn't exist. Create it
				File.Create(SETTINGS_FILE);

				StringBuilder sb = new StringBuilder();
				StringWriter sw = new StringWriter(sb);

				using (JsonWriter js = new JsonTextWriter(sw)) {
					js.Formatting = Formatting.Indented;

					js.WriteStartObject();

					// LastSelectedIndex
					js.WritePropertyName("LastSelectedIndex");
					js.WriteValue("1");

					// DisplayingCompleted
					js.WritePropertyName("DisplayingCompleted");
					js.WriteValue("false");

					js.WriteEndObject();
				}

				File.WriteAllText(SETTINGS_FILE, sw.ToString());
				sb.Clear();
				sw.Close();
			}
			else { // Settings file exists, read from it
				string json = File.ReadAllText(SETTINGS_FILE);
				SettingsManifest.Rootobject settings =
					JsonConvert.DeserializeObject<SettingsManifest.Rootobject>(json);

				if (settings.LastSelectedIndex != null) {
					try {
						selectedIndex = Int32.Parse(settings.LastSelectedIndex);
					}
					catch (FormatException) { // The index isn't a number for some reason, someone probably tampered with the file
						selectedIndex = 1;
					}
				}
				if (settings.DisplayingCompleted != null) {
					if (settings.DisplayingCompleted == "true") {
						isCompletedTasksShown = true;
						completedTaskSwitchLabel.Content = "Completed tasks";
					}
				}
			}

			versionLabel.Content = "Version " + CURRENT_VERSION;

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

				if (files.Length == 0) {
					selectedIndex = 0;
				}
			}
			catch (IOException) {
			}

			SetSelectedProject();


			if (File.Exists(VERSION_INFO)) {
				File.Delete(VERSION_INFO);
			}

			// Download latest version information
			try {
				WebClient client = new WebClient();
				client.DownloadFileCompleted += new AsyncCompletedEventHandler(Update);
				client.DownloadFileAsync(new Uri(VERSION_MANIFEST_URL), VERSION_INFO);
			}
			catch (WebException) {
				// Couldn't download update file. Possible their wifi isn't working
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
		/// Closes threads and shuts down the program.
		/// </summary>
		private void Window_Closing(object sender, CancelEventArgs e) {
			Application.Current.Shutdown(); 
			// If we don't do this, the AddNewProgram window doesn't close and the program keeps running in the bg
		}

		/// <summary>
		/// Runs when the user presses the update button when an update is available.
		/// </summary>
		private void UpdateButtonPressed(object sender, MouseButtonEventArgs e) {
			Thread thread = new Thread(() => {
				Dispatcher.Invoke(new Action(() => {
					updateGrid.Visibility = Visibility.Visible;

					DoubleAnimation animation = new DoubleAnimation();
					animation.From = 130;
					animation.To = 0;
					animation.Duration = TimeSpan.FromSeconds(0.2);

					updateGrid.BeginAnimation(HeightProperty, animation);
				}));
			});
			thread.Start();

			UpdateWindow updateWindow = new UpdateWindow();
			updateWindow.Show();
		}

		/// <summary>
		/// Runs when the user chooses to ignore a shown update.
		/// </summary>
		private void IgnoreUpdate(object sender, MouseButtonEventArgs e) {
			Thread thread = new Thread(() => {		
				Dispatcher.Invoke(new Action(() => {
					updateGrid.Visibility = Visibility.Visible;

					DoubleAnimation animation = new DoubleAnimation();
					animation.From = 130;
					animation.To = 0;
					animation.Duration = TimeSpan.FromSeconds(0.2);

					updateGrid.BeginAnimation(HeightProperty, animation);
				}));
			});
			thread.Start();
		}

		/// <summary>
		/// A function that displays the completed vs incompleted tasks.
		/// </summary>
		private void SwitchCategory(object sender, MouseButtonEventArgs e) {
			// ANIMATION
			if (!isSwitchingAnimationRunning) {
				isSwitchingAnimationRunning = true;
				Dispatcher.Invoke(new Action(() => {
					ThicknessAnimation animation = new ThicknessAnimation();

					animation.From = new Thickness(0, 0, 0, 0);
					animation.To = new Thickness(50, 0, 0, 0);
					animation.Duration = TimeSpan.FromSeconds(0.2);

					switchButtonLabel.BeginAnimation(MarginProperty, animation);
				}));

				Thread thread = new Thread(() => {
					Thread.Sleep(200);
					Dispatcher.Invoke(new Action(() => {
						switchButtonLabel.Margin = new Thickness(-50, 0, 0, 0);
						ThicknessAnimation animation = new ThicknessAnimation();

						animation.From = new Thickness(-50, 0, 0, 0);
						animation.To = new Thickness(0, 0, 0, 0);
						animation.Duration = TimeSpan.FromSeconds(0.2);

						switchButtonLabel.BeginAnimation(MarginProperty, animation);
						isSwitchingAnimationRunning = false;
					}));
				});
				thread.Start();
			}



			if (!isCompletedTasksShown) {
				isCompletedTasksShown = true;
				completedTaskSwitchLabel.Content = "Completed tasks";

			}
			else {
				isCompletedTasksShown = false;
				completedTaskSwitchLabel.Content = "Incomplete tasks";
			}


			// Save everything to the settings file
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			using (JsonWriter js = new JsonTextWriter(sw)) {
				js.Formatting = Formatting.Indented;

				js.WriteStartObject();

				// LastSelectedIndex
				js.WritePropertyName("LastSelectedIndex");
				js.WriteValue(selectedIndex);

				// DisplayingCompleted
				js.WritePropertyName("DisplayingCompleted");
				if (isCompletedTasksShown) {
					js.WriteValue("true");
				}
				else {
					js.WriteValue("false");
				}

				js.WriteEndObject();
			}

			File.WriteAllText(SETTINGS_FILE, sw.ToString());
			sb.Clear();
			sw.Close();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
			
		}

		private void border1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 1;
			SetSelectedProject();
		}

		private void border2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 2;
			SetSelectedProject();
		}

		private void border3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 3;
			SetSelectedProject();
		}

		private void border4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 4;
			SetSelectedProject();
		}

		private void border5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 5;
			SetSelectedProject();
		}

		private void border6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 6;
			SetSelectedProject();
		}

		private void border7_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 7;
			SetSelectedProject();
		}

		private void border8_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 8;
			SetSelectedProject();
		}

		private void border9_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 9;
			SetSelectedProject();
		}

		private void border10_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			selectedIndex = 10;
			SetSelectedProject();
		}
	}
}