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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Project_Tracker {

	/// <summary>
	/// Interaction logic for EditProgram.xaml
	/// </summary>
	public partial class EditProgram : Window {
		private string projectTitle;

		// We use a list for the items instead of an array because we have to remove values if the user deletes an item
		private List<string> errors = new List<string>();

		private List<string> errorsData = new List<string>();
		private List<string> features = new List<string>();
		private List<string> featuresData = new List<string>();
		private List<string> comments = new List<string>();
		private List<string> commentsData = new List<string>();
		private string duration;
		private string percentComplete;

		private string editingFile;
		private int errorRowsAdded = 0;
		private int featureRowsAdded = 0;
		private int commentsRowsAdded = 0;
		private bool isStopwatchRunning = false;
		private Thread timerThread;
		private long lastSecond = 0;
		private long currentSecond = 0;
		private bool isTablesGenerated = false;
		private int rowSelectionID = 0; // We use this to identify which row is currently selected
		private int oldRowSelectionID = 0; // We use this to identify which row was last selected

		private Stopwatch stopwatch;

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly Color sortColor = Color.FromRgb(228, 233, 235);

		private readonly Color greenStopwatch = Color.FromRgb(42, 112, 35);
		private readonly Color redStopwatch = Color.FromRgb(156, 9, 9);
		private readonly Color selectionColor = Color.FromRgb(84, 207, 255);
		private readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		public EditProgram() {
			InitializeComponent();

			Startup();
		}

		// TODO: You can change values from the function. No need to repeat the code. Optimize NOW
		private void AddRow(Table table, int rowsAddedValue, string value, int index, List<string> dataValues) {
			table.RowGroups[0].Rows.Add(new TableRow());
			TableRow newRow = null;

			if (rowsAddedValue == 0) {
				newRow = table.RowGroups[0].Rows[errorRowsAdded];
				errorRowsAdded++;
			}
			else if (rowsAddedValue == 1) {
				newRow = table.RowGroups[0].Rows[featureRowsAdded];
				featureRowsAdded++;
			}
			else if (rowsAddedValue == 2) {
				newRow = table.RowGroups[0].Rows[commentsRowsAdded];
				commentsRowsAdded++;
			}

			if (dataValues[index] == "1") {
				newRow.Background = new SolidColorBrush(greenStopwatch);
			}

			newRow.FontSize = 16;
			newRow.FontFamily = textFont;
			newRow.Cells.Add(new TableCell(new Paragraph(new Run(value))));
		}

		private void SelectionChange(int oldPos, int newPos, int rowsAdded, Table table, List<string> dataValues) {
			this.Dispatcher.Invoke(() =>
			{
				if (newPos < 0) {
					newPos = 0;
				}

				if (newPos > oldPos && newPos > rowsAdded - 1) { // Moved down and newPos is too big
					rowSelectionID--;
					return;
				}

				if (rowsAdded > 1) {
					// Set the color of the newly selected item
					TableRow selectedRow = table.RowGroups[0].Rows[newPos];
					selectedRow.Background = new SolidColorBrush(selectionColor);

					if (dataValues[newPos] == "1") { // Checked checkmark
						checkmarkButton.Text = "\xE73A";
					}
					else { // Unchecked checkmark
						checkmarkButton.Text = "\xE739";
					}

					if (oldPos != newPos) { // We don't want to draw over the already selected one
						TableRow previouslySelectedRow = table.RowGroups[0].Rows[oldPos];
						// Set the color of the old selected item
						if (dataValues[oldPos] == "0") { // Normal
							previouslySelectedRow.Background = Brushes.White;
						}
						else if (dataValues[oldPos] == "1") { // Complete
							previouslySelectedRow.Background = new SolidColorBrush(greenStopwatch);
						}
					}
				}
			});
		}

		/// <summary>
		/// Resets the selection for the table. This might occur if
		/// you changed tables or changed the value of a table element.
		/// We need to reset the table to refresh the view.
		/// </summary>
		/// <param name="table">The table to edit.</param>
		/// <param name="values">The value of each item for when we re-add elements.</param>
		/// <param name="dataValues">The data value of each item (e.g. 0 for normal, 1 for completed, 2 for deleted).</param>
		/// <param name="rowsAddedValue">Whether we're changing the error (0), feature (1), or comments (2).</param>
		/// <param name="totalRows">The total amount of rows for that table.</param>
		private void ResetSelection(Table table, List<string> values, List<string> dataValues, int rowsAddedValue, int totalRows) {
			if (totalRows == 0) { // Shouldn't do anything if there's nothing to do it to
				return;
			}

			rowSelectionID = 0;
			oldRowSelectionID = 0;

			for (int i = 0; i < values.Count; i++) {
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

			// For some reason the values get a bit messed up so this attempts to redraw over every
			// element and try and fix it.
			for (int i = 0; i < totalRows; i++) {
				TableRow row = table.RowGroups[0].Rows[i];

				try {
					if (dataValues[i] == "0") { // Normal
						row.Background = Brushes.White;
					}
					else if (dataValues[i] == "1") { // Complete
						row.Background = new SolidColorBrush(greenStopwatch);
					}
				}
				catch (ArgumentOutOfRangeException) { // This would occur if we just deleted something and the array is one smaller
					table.RowGroups[0].Rows[index].Cells.RemoveRange(0, 1);
					break;
				}
			}
			SelectionChange(0, 0, totalRows, table, dataValues);
		}

		private void StopwatchTimer() {
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
							Dispatcher.Invoke(new Action(() =>
							{
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

							Save();
						}
						catch (TaskCanceledException) { // Happens when the user tries to close the program
							isStopwatchRunning = false;
							timerThread.Abort();
						}
					}
				}
			}
		}

		private void Save() {
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
				catch (ObjectDisposedException) {
				}
				catch (IOException) {
					Save();
					Thread.Sleep(1000);
				}
			}

			sb.Clear();
			sw.Close();
		}

		private void Startup() {
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
				errors = values.Errors.ToList();
				errorsData = values.ErrorsData.ToList();
				features = values.Features.ToList();
				featuresData = values.FeaturesData.ToList();
				comments = values.Comments.ToList();
				commentsData = values.CommentsData.ToList();
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
				ResetSelection(errorTable, errors, errorsData, 0, errorRowsAdded);
				ResetSelection(featureTable, features, featuresData, 1, featureRowsAdded);
				ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);

				errorScrollView.Focus();

				isTablesGenerated = true;

				this.Title = values.Title + " - " + values.Percent + "%";
				durationLabel.Text = values.Duration;

				timerThread = new Thread(StopwatchTimer);
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
			CheckBox box;
			StackPanel panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical };
			for (int i = 0; i < errors.Count; i++) {
				box = new CheckBox();
				box.Content = errors[i];
				box.VerticalAlignment = VerticalAlignment.Top;
				box.HorizontalAlignment = HorizontalAlignment.Left;
				box.Margin = new Thickness(10, i * 50, 50, 50);

				panel.Children.Add(box);
			}
			this.Content = panel;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			MainWindow main = new MainWindow();
			main.filesRead.Remove(editingFile);

			isStopwatchRunning = false;

			if (timerThread.IsAlive) {
				timerThread.Abort();
			}
		}

		private void KeyPress(object sender, KeyEventArgs e) {
			// Remember that combo box values can be changed by the arrow keys.
			// We don't want to change the table selection when we're doing this.
			if (!errorSelection.IsFocused && !featureSelection.IsFocused && !commentSelection.IsFocused) {
				if (e.Key == Key.Down || e.Key == Key.Up) { // Either arrow is pressed
					if (e.Key == Key.Down) {
						rowSelectionID++;
					}
					else if (e.Key == Key.Up && rowSelectionID > 0) {
						rowSelectionID--;
					}
					if (switchLabels.SelectedIndex == 0) { // Error table is selected
						SelectionChange(oldRowSelectionID, rowSelectionID, errorRowsAdded, errorTable, errorsData);
					}
					else if (switchLabels.SelectedIndex == 1) { // Feature table is selected
						SelectionChange(oldRowSelectionID, rowSelectionID, featureRowsAdded, featureTable, featuresData);
					}
					else { // Comment table is selected
						SelectionChange(oldRowSelectionID, rowSelectionID, commentsRowsAdded, commentTable, commentsData);
					}
					oldRowSelectionID = rowSelectionID;
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
				ResetSelection(errorTable, errors, errorsData, 0, errorRowsAdded);
				ResetSelection(featureTable, features, featuresData, 1, featureRowsAdded);
				ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);

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

		// TODO: We need to change the SelectionChange function to change the selection before resetting. Do this by creating params like int newIndex, int oldIndex.
		private void RemoveValue(object sender, MouseButtonEventArgs e) {
			try {
				if (switchLabels.SelectedIndex == 0) { // Errors
					SelectionChange(oldRowSelectionID, 0, errorRowsAdded, errorTable, errorsData);
					errors.RemoveAt(rowSelectionID);
					errorsData.RemoveAt(rowSelectionID);
					ResetSelection(errorTable, errors, errorsData, 0, errorRowsAdded);
				}
				else if (switchLabels.SelectedIndex == 1) { // Features
					SelectionChange(oldRowSelectionID, 0, featureRowsAdded, featureTable, featuresData);
					features.RemoveAt(rowSelectionID);
					featuresData.RemoveAt(rowSelectionID);
					ResetSelection(featureTable, features, featuresData, 1, featureRowsAdded);
				}
				else if (switchLabels.SelectedIndex == 2) { // Comments
					SelectionChange(oldRowSelectionID, 0, commentsRowsAdded, commentTable, commentsData);
					comments.RemoveAt(rowSelectionID);
					commentsData.RemoveAt(rowSelectionID);
					ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);
				}
			}
			catch (ArgumentOutOfRangeException) { // They tried to press the delete button when there was no items in it
				return;
			}

			Save();
		}

		private void AddValue(object sender, MouseButtonEventArgs e) {
		}

		private void CheckOff(object sender, MouseButtonEventArgs e) {
		}
	}
}