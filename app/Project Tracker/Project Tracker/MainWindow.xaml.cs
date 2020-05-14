﻿using Newtonsoft.Json;
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

namespace Project_Tracker {

	public partial class MainWindow : Window {
		private int rowsAdded = 0; // We use a global variable so the AddRow function knows what id of a row to edit
		private int rowSelectionID = 0; // We use this to identify which row is currently selected
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
		private readonly string CURRENT_VERSION = "0.6"; // IF YOU CHANGE THIS, ALSO CHANGE IT IN UpdateWindow.xaml.cs

		public MainWindow() {
			InitializeComponent();
			Startup();
		}

		private void AddRow(string title, int warnings, int features, int comments, string time, string percent) {
			addedProgram = true;

			// rowsAdded++;
			rowsAdded = listTable.RowGroups[0].Rows.Count - 1;

			Dispatcher.Invoke(() =>
			{
				listTable.RowGroups[0].Rows.Add(new TableRow());
				TableRow newRow = listTable.RowGroups[0].Rows[rowsAdded];

				if (rowsAdded == 1) { // It's the first row so we add the selection to it
					newRow.Background = new SolidColorBrush(selectionColor);
					rowSelectionID++;
				}

				if (rowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}

				newRow.FontSize = 16;
				newRow.FontFamily = textFont;
				newRow.Cells.Add(new TableCell(new Paragraph(new Run(" " + title))));
				newRow.Cells.Add(new TableCell(new Paragraph(new Run(" " + warnings.ToString()))));
				newRow.Cells.Add(new TableCell(new Paragraph(new Run(" " + features.ToString()))));
				newRow.Cells.Add(new TableCell(new Paragraph(new Run(" " + comments.ToString()))));
				newRow.Cells.Add(new TableCell(new Paragraph(new Run(" " + time))));
				newRow.Cells.Add(new TableCell(new Paragraph(new Run(" " + percent + "%"))));

				tableValues.Add(title);

				rowsAdded = listTable.RowGroups[0].Rows.Count - 1;
			});
		}

		private void SelectionChange(bool isUp) {
			this.Dispatcher.Invoke(() =>
			{
				if (rowSelectionID < 0) {
					rowSelectionID = 0;
				}
				if (isUp == true && rowSelectionID != 0) { // REMEMBER: Going "up" actually brings the selection ID lower.
					TableRow selectedRow = listTable.RowGroups[0].Rows[rowSelectionID];
					selectedRow.Background = new SolidColorBrush(selectionColor);

					if ((rowSelectionID - 1) % 2 == 0) {
						TableRow previouslySelectedRow = listTable.RowGroups[0].Rows[rowSelectionID + 1];
						previouslySelectedRow.Background = new SolidColorBrush(sortColor);
					}
					else {
						TableRow previouslySelectedRow = listTable.RowGroups[0].Rows[rowSelectionID + 1];
						previouslySelectedRow.Background = Brushes.White;
					}
				}
				else if (isUp == false) {
					if (rowSelectionID < rowsAdded) {
						TableRow selectedRow = listTable.RowGroups[0].Rows[rowSelectionID];
						selectedRow.Background = new SolidColorBrush(selectionColor);

						if (rowSelectionID != 0) {
							if ((rowSelectionID - 1) % 2 == 0) {
								TableRow previouslySelectedRow = listTable.RowGroups[0].Rows[rowSelectionID - 1];
								previouslySelectedRow.Background = new SolidColorBrush(sortColor);
							}
							else {
								TableRow previouslySelectedRow = listTable.RowGroups[0].Rows[rowSelectionID - 1];
								previouslySelectedRow.Background = Brushes.White;
							}
						}
					}
					else {
						rowSelectionID--;
					}
				}
			});
		}

		/*
		 * Updates the table by adding any new files to the table
		 */

		private void UpdateTable() {
			while (true) {
				try {
					if (Directory.Exists(DATA_DIRECTORY)) { // Check if data directory exists and read files
						string[] files = Directory.GetFiles(DATA_DIRECTORY, pathExtension, SearchOption.AllDirectories);

						foreach (string path in files) {
							if (!filesRead.Contains(path)) {
								// Convert each json file to a table row
								using (StreamReader reader = new StreamReader(path)) {
									string json = reader.ReadToEnd();

									dynamic array = JsonConvert.DeserializeObject(json);

									MainTableManifest.Rootobject mainTable = JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

									AddRow(mainTable.Title, mainTable.Errors.Length, mainTable.Features.Length, mainTable.Comments.Length, mainTable.Duration, mainTable.Percent);
									filesRead.Add(path);
									// rowsAdded++;
								}
							}
						}
					}
					else { // Data directory doesn't exist so we create it
						Directory.CreateDirectory(DATA_DIRECTORY);
					}

					Thread.Sleep(10);
					filesUpdate++;

					if (filesUpdate == 50) {
						filesUpdate = 0;
						for (int i = 0; i < rowsAdded - 1; i++) {
							using (StreamReader reader = new StreamReader(filesRead[i])) {
								string json = reader.ReadToEnd();

								dynamic array = JsonConvert.DeserializeObject(json);

								MainTableManifest.Rootobject mainTable = JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

								Dispatcher.Invoke(new Action(() =>
								{
									// TODO: See if we can optimize this repetitive code
									// Title
									listTable.RowGroups[0].Rows[i + 1].Cells.RemoveRange(0, 1);
									listTable.RowGroups[0].Rows[i + 1].Cells.Insert(0, new TableCell(new Paragraph(new Run(" " + mainTable.Title))));
									// Errors
									listTable.RowGroups[0].Rows[i + 1].Cells.RemoveRange(1, 1);
									listTable.RowGroups[0].Rows[i + 1].Cells.Insert(1, new TableCell(new Paragraph(new Run(" " + mainTable.Errors.Length))));
									// Features
									listTable.RowGroups[0].Rows[i + 1].Cells.RemoveRange(2, 1);
									listTable.RowGroups[0].Rows[i + 1].Cells.Insert(2, new TableCell(new Paragraph(new Run(" " + mainTable.Features.Length))));
									// Comments
									listTable.RowGroups[0].Rows[i + 1].Cells.RemoveRange(3, 1);
									listTable.RowGroups[0].Rows[i + 1].Cells.Insert(3, new TableCell(new Paragraph(new Run(" " + mainTable.Comments.Length))));
									// Duration
									listTable.RowGroups[0].Rows[i + 1].Cells.RemoveRange(4, 1);
									listTable.RowGroups[0].Rows[i + 1].Cells.Insert(4, new TableCell(new Paragraph(new Run(" " + mainTable.Duration))));
									// Percent
									listTable.RowGroups[0].Rows[i + 1].Cells.RemoveRange(5, 1);
									listTable.RowGroups[0].Rows[i + 1].Cells.Insert(5, new TableCell(new Paragraph(new Run(" " + mainTable.Percent + "%"))));
								}));
							}
						}
					}
				}
				catch (IOException) { // File is being used by something else so just wait until we can do it
					Thread.Sleep(100);
				}
			}
		}

		private void Startup() {
			// Init the table and add the first row (the title row that's already in the XAML that we don't want to edit)
			listTable.RowGroups.Add(new TableRowGroup());
			listTable.RowGroups[0].Rows.Add(new TableRow());

			scrollView.Focus();

			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker")) {
				Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker");
			}
			if (!Directory.Exists(DATA_DIRECTORY)) {
				Directory.CreateDirectory(DATA_DIRECTORY);
			}

			if (firstRun) {
				try {
					string[] files = Directory.GetFiles(DATA_DIRECTORY, pathExtension, SearchOption.AllDirectories);
					foreach (string path in files) {
						if (!filesRead.Contains(path)) {
							// Convert each json file to a table row
							using (StreamReader reader = new StreamReader(path)) {
								string json = reader.ReadToEnd();

								dynamic array = JsonConvert.DeserializeObject(json);

								MainTableManifest.Rootobject mainTable = JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

								AddRow(mainTable.Title, mainTable.Errors.Length, mainTable.Features.Length, mainTable.Comments.Length, mainTable.Duration, mainTable.Percent);
								filesRead.Add(path);
								// rowsAdded++;
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

		private void ShowUpdate(object sender, AsyncCompletedEventArgs e) {
			using (StreamReader reader = new StreamReader(VERSION_INFO)) {
				string json = reader.ReadToEnd();

				dynamic array = JsonConvert.DeserializeObject(json);

				UpdateManifest.Rootobject update = JsonConvert.DeserializeObject<UpdateManifest.Rootobject>(json);

				if (float.Parse(CURRENT_VERSION) < float.Parse(update.Version)) { // Update is available
					UpdateWindow updateWindow = new UpdateWindow();
					updateWindow.Show();
				}
			}
		}

		private void KeyPress(object sender, System.Windows.Input.KeyEventArgs e) {
			if (e.Key == Key.Down) {
				rowSelectionID++;

				SelectionChange(false);
			}
			else if (e.Key == Key.Up) {
				if (rowSelectionID > 1) {
					rowSelectionID--;
				}
				SelectionChange(true);
			}
		}

		private void AddProgram(object sender, MouseButtonEventArgs e) {
			AddNewProgram newProgram = new AddNewProgram();
			newProgram.Show();
		}

		private void EditProgram(object sender, MouseButtonEventArgs e) {
			if (addedProgram == true) {
				Passthrough.EditingFile = filesRead[rowSelectionID - 1];

				EditProgram editProgram = new EditProgram();
				editProgram.Show();
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			tableUpdateThread.Abort();
			System.Windows.Application.Current.Shutdown(); // If we don't do this, the AddNewProgram window doesn't close and the program keeps running in the bg
		}
	}
}