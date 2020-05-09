using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_Tracker {
	/// <summary>
	/// Interaction logic for EditProgram.xaml
	/// </summary>
	public partial class EditProgram : Window {

		string projectTitle;
		string[] errors;
		string[] errorsData;
		string[] features;
		string[] featuresData;
		string[] comments;
		string[] commentsData;
		string duration;
		string percentComplete;

		string editingFile;
		int errorRowsAdded = 0;
		int featureRowsAdded = 0;
		int commentsRowsAdded = 0;
		bool isStopwatchRunning = false;
		Thread timerThread;
		long lastSecond = 0;
		long currentSecond = 0;
		bool isTablesGenerated = false;
		int rowSelectionID = 0; // We use this to identify which row is currently selected

		Stopwatch stopwatch;

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		readonly Color sortColor = Color.FromRgb(228, 233, 235);
		readonly Color greenStopwatch = Color.FromRgb(42, 112, 35);
		readonly Color redStopwatch = Color.FromRgb(156, 9, 9);
		readonly Color selectionColor = Color.FromRgb(84, 207, 255);
		readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		public EditProgram() {
			InitializeComponent();

			startup();
		}
		// TODO: You can change values from the function. No need to repeat the code. Optimize NOW
		private void AddRow(Table table, int rowsAddedValue, string value, int index, string[] dataValues) {
			table.RowGroups[0].Rows.Add(new TableRow());

			TableRow newRow = null;

			if (rowsAddedValue == 0) {
				newRow = table.RowGroups[0].Rows[errorRowsAdded];
				errorRowsAdded++;

				if (errorRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}
			else if (rowsAddedValue == 1) {
				newRow = table.RowGroups[0].Rows[featureRowsAdded];
				featureRowsAdded++;

				if (featureRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}
			else if (rowsAddedValue == 2) {
				newRow = table.RowGroups[0].Rows[commentsRowsAdded];
				commentsRowsAdded++;

				if (commentsRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}

			if (dataValues[index] == "1") {
				newRow.Background = new SolidColorBrush(greenStopwatch);
			}
			else if (dataValues[index] == "2") {
				newRow.Background = new SolidColorBrush(redStopwatch);
			}

			newRow.FontSize = 16;
			newRow.FontFamily = textFont;
			newRow.Cells.Add(new TableCell(new Paragraph(new Run(value))));
		}

		private void SelectionChange(bool isUp, Table table, int rowsAdded, string[] dataValues) {
			this.Dispatcher.Invoke(() => {
				if (rowSelectionID < 0) {
					rowSelectionID = 0;
				}
				if (isUp == true && rowsAdded > 1) { // REMEMBER: Going "up" actually brings the selection ID lower.
					TableRow selectedRow = table.RowGroups[0].Rows[rowSelectionID];
					selectedRow.Background = new SolidColorBrush(selectionColor);

					if (dataValues[rowSelectionID + 1] == "0") {
						if (rowSelectionID % 2 == 0) {
							TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID + 1];
							previouslySelectedRow.Background = new SolidColorBrush(sortColor);
						}
						else {
							TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID + 1];
							previouslySelectedRow.Background = Brushes.White;
						}
					}
					else if (dataValues[rowSelectionID + 1] == "1") {
						TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID + 1];
						previouslySelectedRow.Background = new SolidColorBrush(greenStopwatch);
					}
					else if (dataValues[rowSelectionID + 1] == "2") {
						TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID + 1];
						previouslySelectedRow.Background = new SolidColorBrush(redStopwatch);
					}

				}
				else if (isUp == false) {
					if (rowSelectionID < rowsAdded) {
						TableRow selectedRow = table.RowGroups[0].Rows[rowSelectionID];
						selectedRow.Background = new SolidColorBrush(selectionColor);

						if (rowSelectionID != 0) {

							if (dataValues[rowSelectionID - 1] == "0") {
								if (rowSelectionID % 2 == 0) {
									TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID - 1];
									previouslySelectedRow.Background = new SolidColorBrush(sortColor);
								}
								else {
									TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID - 1];
									previouslySelectedRow.Background = Brushes.White;
								}
							}
							else if (dataValues[rowSelectionID - 1] == "1") {
								TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID - 1];
								previouslySelectedRow.Background = new SolidColorBrush(greenStopwatch);
							}
							else if (dataValues[rowSelectionID - 1] == "2") {
								TableRow previouslySelectedRow = table.RowGroups[0].Rows[rowSelectionID - 1];
								previouslySelectedRow.Background = new SolidColorBrush(redStopwatch);
							}
						}
					}
					else {
						rowSelectionID--;
					}
				}
			});
		}

		private void ResetSelection(Table table, string[] values, string[] dataValues, int rowsAddedValue) {
			for (int i = 0; i < values.Length; i++) {
				table.RowGroups[0].Rows[i].Cells.RemoveRange(0, 1);
			}
			if (rowsAddedValue == 0) {
				errorRowsAdded = 0;
			}
			else if (rowsAddedValue == 1) {
				featureRowsAdded = 0;
			}
			else if (rowsAddedValue == 2) {
				commentsRowsAdded = 0;
			}

			int index = 0;
			foreach (string value in values) {
				AddRow(table, rowsAddedValue, value, index, dataValues);
				index++;
			}

			TableRow selectedRow = table.RowGroups[0].Rows[0];
			selectedRow.Background = new SolidColorBrush(selectionColor);
			rowSelectionID = 0;
		}

		private void stopwatchTimer() {
			while (true) {
				// I believe this is a more accurate way of counting. It creates a stopwatch
				// And updates it for each second rather than sleeping for a thousand milliseconds
				Thread.Sleep(50);
				while (isStopwatchRunning) {
					Thread.Sleep(50);

					currentSecond = stopwatch.ElapsedMilliseconds / 1000;

					if (currentSecond > lastSecond) {
						lastSecond = currentSecond;
						try {
							Dispatcher.Invoke(new Action(() => {
								string[] durationCut = durationLabel.Text.Split(':');
								int hours = Int32.Parse(durationCut[0]);
								int minutes = Int32.Parse(durationCut[1]);
								int seconds = Int32.Parse(durationCut[2]);

								seconds++;
								if (seconds == 60) {
									minutes++;
									seconds = 0;
								}
								if (minutes == 60) {
									hours++;
									minutes = 0;
								}

								string reparsedDuration = "";

								if (hours.ToString().Length == 1) {
									reparsedDuration += "0" + hours.ToString();
								}
								else {
									reparsedDuration += hours.ToString();
								}

								reparsedDuration += ":";

								if (minutes.ToString().Length == 1) {
									reparsedDuration += "0" + minutes.ToString();
								}
								else {
									reparsedDuration += minutes.ToString();
								}

								reparsedDuration += ":";

								if (seconds.ToString().Length == 1) {
									reparsedDuration += "0" + seconds.ToString();
								}
								else {
									reparsedDuration += seconds.ToString();
								}

								duration = reparsedDuration;
								durationLabel.Text = reparsedDuration;
							}));

							if (lastSecond % 10 == 0) {
								StringBuilder sb = new StringBuilder();
								StringWriter sw = new StringWriter(sb);

								using (JsonWriter js = new JsonTextWriter(sw)) {
									js.Formatting = Formatting.Indented;

									js.WriteStartObject();

									js.WritePropertyName("Title");
									js.WriteValue(projectTitle);

									js.WritePropertyName("Errors");
									js.WriteStartArray();
									foreach (string error in errors) {
										js.WriteValue(error);
									}
									js.WriteEnd();

									js.WritePropertyName("ErrorsData");
									js.WriteStartArray();
									foreach (string data in errorsData) {
										js.WriteValue(data);
									}
									js.WriteEnd();

									js.WritePropertyName("Features");
									js.WriteStartArray();
									foreach (string feature in features) {
										js.WriteValue(feature);
									}
									js.WriteEnd();

									js.WritePropertyName("FeaturesData");
									js.WriteStartArray();
									foreach (string data in featuresData) {
										js.WriteValue(data);
									}
									js.WriteEnd();

									js.WritePropertyName("Comments");
									js.WriteStartArray();
									foreach (string comment in comments) {
										js.WriteValue(comment);
									}
									js.WriteEnd();

									js.WritePropertyName("CommentsData");
									js.WriteStartArray();
									foreach (string data in commentsData) {
										js.WriteValue(data);
									}
									js.WriteEnd();

									js.WritePropertyName("Duration");
									js.WriteValue(duration);

									js.WritePropertyName("Percent");
									js.WriteValue(percentComplete);

									js.WriteEndObject();
								}
								using (StreamWriter writer = File.CreateText(editingFile)) {
									writer.WriteLine(sb.ToString());
								}
							}
						}
						catch (TaskCanceledException) { // Happens when the user tries to close the program
							isStopwatchRunning = false;
							timerThread.Abort();
						}
					}
				}
			}
		}

		private void startup() {
			stopwatch = new Stopwatch();

			editingFile = Passthrough.EditingFile;

			// Convert each json file to a table row
			using (StreamReader reader = new StreamReader(editingFile)) {
				// Read JSON
				string json = reader.ReadToEnd();
				dynamic array = JsonConvert.DeserializeObject(json);
				MainTableManifest.Rootobject values = JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

				// Set values for saving feature to rewrite
				projectTitle = values.Title;
				errors = values.Errors;
				errorsData = values.ErrorsData;
				features = values.Features;
				featuresData = values.FeaturesData;
				comments = values.Comments;
				commentsData = values.CommentsData;
				duration = values.Duration;
				percentComplete = values.Percent;

				int index = 0;
				// Add values to tables
				foreach (string error in values.Errors) {
					AddRow(errorTable, 0, error, index, errorsData);
					index++;
				}

				index = 0;
				foreach (string feature in values.Features) {
					AddRow(featureTable, 1, feature, index, featuresData);
					index++;
				}

				index = 0;
				foreach (string comment in values.Comments) {
					AddRow(commentTable, 2, comment, index, commentsData);
					index++;
				}
				ResetSelection(errorTable, errors, errorsData, 0);
				ResetSelection(featureTable, features, featuresData, 1);
				ResetSelection(commentTable, comments, commentsData, 2);

				errorScrollView.Focus();

				isTablesGenerated = true;

				this.Title = values.Title + " - " + values.Percent + "%";
				durationLabel.Text = values.Duration;

				timerThread = new Thread(stopwatchTimer);
				timerThread.Start();
			}
		}

		private void Stopwatch(object sender, MouseButtonEventArgs e) {
			if (isStopwatchRunning) {
				stopwatchButton.Foreground = new SolidColorBrush(redStopwatch);
				isStopwatchRunning = false;
				lastSecond = 0;
				currentSecond = 0;

				stopwatch.Stop();
				stopwatch.Reset();

				// Save
				StringBuilder sb = new StringBuilder();
				StringWriter sw = new StringWriter(sb);

				using (JsonWriter js = new JsonTextWriter(sw)) {
					js.Formatting = Formatting.Indented;

					js.WriteStartObject();

					js.WritePropertyName("Title");
					js.WriteValue(projectTitle);
					js.WritePropertyName("Errors");
					js.WriteStartArray();
					foreach (string error in errors) {
						js.WriteValue(error);
					}
					js.WriteEnd();
					js.WritePropertyName("Features");
					js.WriteStartArray();
					foreach (string feature in features) {
						js.WriteValue(feature);
					}
					js.WriteEnd();
					js.WritePropertyName("Comments");
					js.WriteStartArray();
					foreach (string comment in comments) {
						js.WriteValue(comment);
					}
					js.WriteEnd();
					js.WritePropertyName("Duration");
					js.WriteValue(duration);
					js.WritePropertyName("Percent");
					js.WriteValue(percentComplete);

					js.WriteEndObject();
				}
				using (StreamWriter writer = File.CreateText(editingFile)) {
					writer.WriteLine(sb.ToString());
				}
			}
			else {
				stopwatchButton.Foreground = new SolidColorBrush(greenStopwatch);
				isStopwatchRunning = true;
				stopwatch.Start();
			}
		}

		private void Edit(object sender, MouseButtonEventArgs e) {
			// Hide everything
			durationLabel.Visibility = Visibility.Hidden;
			commentScrollView.Visibility = Visibility.Hidden;
			errorScrollView.Visibility = Visibility.Hidden;
			featureScrollView.Visibility = Visibility.Hidden;

			// Create new checkboxes
			System.Windows.Controls.CheckBox box;
			StackPanel panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical };
			for (int i = 0; i < errors.Length; i++) {
				box = new System.Windows.Controls.CheckBox();
				box.Content = errors[i];
				box.VerticalAlignment = VerticalAlignment.Top;
				box.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
				box.Margin = new Thickness(10, i * 50, 50, 50);

				panel.Children.Add(box);
			}
			this.Content = panel;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			// Save
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);


			// Make 10 attempts to save the file.
			for (int i = 0; i < 10; i++) {
				try {
					using (JsonWriter js = new JsonTextWriter(sw)) {
						js.Formatting = Formatting.Indented;

						js.WriteStartObject();

						js.WritePropertyName("Title");
						js.WriteValue(projectTitle);

						js.WritePropertyName("Errors");
						js.WriteStartArray();
						foreach (string error in errors) {
							js.WriteValue(error);
						}
						js.WriteEnd();

						js.WritePropertyName("ErrorsData");
						js.WriteStartArray();
						foreach (string data in errorsData) {
							js.WriteValue(data);
						}
						js.WriteEnd();

						js.WritePropertyName("Features");
						js.WriteStartArray();
						foreach (string feature in features) {
							js.WriteValue(feature);
						}
						js.WriteEnd();

						js.WritePropertyName("FeaturesData");
						js.WriteStartArray();
						foreach (string data in featuresData) {
							js.WriteValue(data);
						}
						js.WriteEnd();

						js.WritePropertyName("Comments");
						js.WriteStartArray();
						foreach (string comment in comments) {
							js.WriteValue(comment);
						}
						js.WriteEnd();

						js.WritePropertyName("CommentsData");
						js.WriteStartArray();
						foreach (string data in commentsData) {
							js.WriteValue(data);
						}
						js.WriteEnd();

						js.WritePropertyName("Duration");
						js.WriteValue(duration);

						js.WritePropertyName("Percent");
						js.WriteValue(percentComplete);

						js.WriteEndObject();
					}
					using (StreamWriter writer = File.CreateText(editingFile)) {
						writer.WriteLine(sb.ToString());
					}
					break;
				}
				catch (IOException) {
					Thread.Sleep(1000);
				}
			}



			MainWindow main = new MainWindow();

			main.filesRead.Remove(editingFile);

			isStopwatchRunning = false;

			if (timerThread.IsAlive) {
				timerThread.Abort();
			}
		}

		private void KeyPress(object sender, System.Windows.Input.KeyEventArgs e) {
			// Remember that combo box values can be changed by the arrow keys.
			// We don't want to change the table selection when we're doing this.
			if (!errorSelection.IsFocused && !featureSelection.IsFocused && !commentSelection.IsFocused) {
				if (e.Key == Key.Down) {
					rowSelectionID++;

					if (switchLabels.SelectedIndex == 0) { // Error table is selected
						SelectionChange(false, errorTable, errorRowsAdded, errorsData);
					}
					else if (switchLabels.SelectedIndex == 1) { // Feature table is selected
						SelectionChange(false, featureTable, featureRowsAdded, featuresData);
					}
					else { // Comment table is selected
						SelectionChange(false, commentTable, commentsRowsAdded, commentsData);
					}

				}
				else if (e.Key == Key.Up) {
					if (rowSelectionID > 0) {
						rowSelectionID--;
					}

					if (switchLabels.SelectedIndex == 0) { // Error table is selected
						SelectionChange(true, errorTable, errorRowsAdded, errorsData);
					}
					else if (switchLabels.SelectedIndex == 1) { // Feature table is selected
						SelectionChange(true, featureTable, featureRowsAdded, featuresData);
					}
					else { // Comment table is selected
						SelectionChange(true, commentTable, commentsRowsAdded, commentsData);
					}
				}
			}
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			// The selection "changes" (is automatically set) when the window
			// is first opened, but if we try to make changes to one of the
			// elements that hasn't been initialized or added rows to it yet
			// then it'll obviously return an error. So we wait until AFTER
			// the tables have been initialized, aka after startup.
			if (isTablesGenerated) {
				ResetSelection(errorTable, errors, errorsData, 0);
				ResetSelection(featureTable, features, featuresData, 1);
				ResetSelection(commentTable, comments, commentsData, 2);


				if (switchLabels.SelectedIndex == 0) { // Errors
					errorScrollView.Visibility = Visibility.Visible;
					featureScrollView.Visibility = Visibility.Hidden;
					commentScrollView.Visibility = Visibility.Hidden;

					errorScrollView.Focus();
				}
				else if (switchLabels.SelectedIndex == 1) { // Features
					errorScrollView.Visibility = Visibility.Hidden;
					featureScrollView.Visibility = Visibility.Visible;
					commentScrollView.Visibility = Visibility.Hidden;

					featureScrollView.Focus();
				}
				else { // Comments
					errorScrollView.Visibility = Visibility.Hidden;
					featureScrollView.Visibility = Visibility.Hidden;
					commentScrollView.Visibility = Visibility.Visible;

					commentScrollView.Focus();
				}
			}
		}

		private void RemoveValue(object sender, MouseButtonEventArgs e) {

		}
		private void AddValue(object sender, MouseButtonEventArgs e) {

		}
		private void CheckOff(object sender, MouseButtonEventArgs e) {

		}
	}
}
