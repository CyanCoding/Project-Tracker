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
		private int errorsRowsAdded = 0;
		private int featuresRowsAdded = 0;
		private int commentsRowsAdded = 0;
		private bool isStopwatchRunning = false;
		private Thread timerThread;
		private Thread readThread;
		private long lastSecond = 0;
		private long currentSecond = 0;
		private bool isTablesGenerated = false;
		private int rowSelectionID = 0; // We use this to identify which row is currently selected
		private int oldRowSelectionID = 0; // We use this to identify which row was last selected

		private Stopwatch stopwatch;

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly Color greenStopwatch = Color.FromRgb(42, 112, 35);
		private readonly Color completedColor = Color.FromRgb(108, 219, 86);
		private readonly Color redStopwatch = Color.FromRgb(156, 9, 9);
		private readonly Color selectionColor = Color.FromRgb(84, 207, 255);
		private readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		public EditProgram() {
			InitializeComponent();

			Startup();
		}

		/// <summary>
		/// Adds a row to a certain table.
		/// </summary>
		/// <param name="table">The table to add an item to.</param>
		/// <param name="rowsAddedValue">The table number we're adding to (errors = 0, features = 1, comments = 2)</param>
		/// <param name="value">The text of the row.</param>
		/// <param name="index">The index where the value is in the array.</param>
		/// <param name="dataValues">The array of data.</param>
		private void AddRow(Table table, int rowsAddedValue, string value, int index, List<string> dataValues) {
			//if (value == "") {
			//	return;
			//}

			table.RowGroups[0].Rows.Add(new TableRow());
			TableRow newRow = null;

			if (rowsAddedValue == 0) {
				newRow = table.RowGroups[0].Rows[errorsRowsAdded];
				errorsRowsAdded++;
			}
			else if (rowsAddedValue == 1) {
				newRow = table.RowGroups[0].Rows[featuresRowsAdded];
				featuresRowsAdded++;
			}
			else if (rowsAddedValue == 2) {
				newRow = table.RowGroups[0].Rows[commentsRowsAdded];
				commentsRowsAdded++;
			}

			if (dataValues[index] == "1") {
				newRow.Background = new SolidColorBrush(completedColor);
			}

			newRow.FontSize = 16;
			newRow.FontFamily = textFont;
			newRow.Cells.Add(new TableCell(new Paragraph(new Run(value))));
		}

		/// <summary>
		/// Changes the selection on the current table.
		/// </summary>
		/// <param name="oldPos">The position the selection was.</param>
		/// <param name="newPos">The position the selection is going to.</param>
		/// <param name="rowsAdded">The count of rows in this table.</param>
		/// <param name="table">The table we're editing.</param>
		/// <param name="dataValues">The data values of the table we're editing.</param>
		private void SelectionChange(int oldPos, int newPos, int rowsAdded, Table table, List<string> dataValues) {
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
					checkmarkButton.ToolTip = "Mark as incomplete";
					checkmarkButton.Text = "\xE73A";
				}
				else { // Unchecked checkmark
					checkmarkButton.ToolTip = "Mark as completed";
					checkmarkButton.Text = "\xE739";
				}

				if (oldPos != newPos) { // We don't want to draw over the already selected one
					TableRow previouslySelectedRow = table.RowGroups[0].Rows[oldPos];
					// Set the color of the old selected item
					if (dataValues[oldPos] == "0") { // Normal
						previouslySelectedRow.Background = Brushes.White;
					}
					else if (dataValues[oldPos] == "1") { // Complete
						previouslySelectedRow.Background = new SolidColorBrush(completedColor);
					}
				}
			}
			else if (rowsAdded == 1) {
				TableRow selectedRow = table.RowGroups[0].Rows[0];
				selectedRow.Background = new SolidColorBrush(selectionColor);

				rowSelectionID = 0;
			}
		}

		/// <summary>
		/// Resets the selection for the table and remakes all rows.
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
				errorsRowsAdded = 0;
			}
			else if (rowsAddedValue == 1) {
				featuresRowsAdded = 0;
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
						row.Background = new SolidColorBrush(completedColor);
					}
				}
				catch (ArgumentOutOfRangeException) { // This would occur if we just deleted something and the array is one smaller
					table.RowGroups[0].Rows[index].Cells.RemoveRange(0, 1);
					break;
				}
			}

			SelectionChange(0, 0, totalRows, table, dataValues);
		}

		/// <summary>
		/// A thread that keeps track of duration since the stopwtach button was pressed.
		/// </summary>
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

		/// <summary>
		/// Calculates the percentage of compeletion.
		/// </summary>
		private void CalculatePercentage() {
			long totalItems = errorsData.Count + featuresData.Count + commentsData.Count;
			long totalCheckedItems = 0;

			foreach (string data in errorsData) {
				if (data == "1") {
					totalCheckedItems++;
				}
			}
			foreach (string data in featuresData) {
				if (data == "1") {
					totalCheckedItems++;
				}
			}
			foreach (string data in commentsData) {
				if (data == "1") {
					totalCheckedItems++;
				}
			}

			long percent;
			try {
				percent = (totalCheckedItems * 100) / totalItems;
			}
			catch (DivideByZeroException) {
				percent = 0;
			}
			percentComplete = percent.ToString();
			if (percent <= 9) {
				percentComplete = "0" + percent.ToString();
				// Would convert 9% to 09%
			}

			this.Dispatcher.Invoke(() =>
			{
				if (Passthrough.Title != "") {
					this.Title = Passthrough.Title + " - " + percentComplete + "%";
				}
				else {
					this.Title = projectTitle + " - " + percentComplete + "%";
				}
			});
			Save();
		}

		/// <summary>
		/// Updates values from editing file.
		/// </summary>
		private void Read() {
			while (Passthrough.IsAdding) {
				Thread.Sleep(1000);

				if (Passthrough.IsDeleting) {
					Passthrough.IsAdding = false;

					isStopwatchRunning = false;

					// Exit out of all threads forcefully
					if (timerThread.IsAlive) {
						timerThread.Abort();
					}

					this.Dispatcher.Invoke(() =>
					{
						this.Hide();
						this.Close();
					});
					break;
				}


				CalculatePercentage();
				string json = File.ReadAllText(editingFile);
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


				this.Dispatcher.Invoke(() =>
				{
					if (switchLabels.SelectedIndex == 0) { // Errors
						errorsRowsAdded = 0;
						try {
							for (int i = 0; i < errors.Count; i++) {
								errorTable.RowGroups[0].Rows[i].Cells.RemoveRange(0, 1);
							}
						}
						catch (ArgumentOutOfRangeException) {
							// They just added a new value
							SelectionChange(0, 0, errorsRowsAdded, errorTable, errorsData);
						}
						int index = 0;
						foreach (string value in errors) {
							AddRow(errorTable, 0, value, index, errorsData);
							index++;
						}
						SelectionChange(rowSelectionID, rowSelectionID, errorsRowsAdded, errorTable, errorsData);
					}
					else if (switchLabels.SelectedIndex == 1) { // Features
						featuresRowsAdded = 0;
						try {
							for (int i = 0; i < features.Count; i++) {
								featureTable.RowGroups[0].Rows[i].Cells.RemoveRange(0, 1);
							}
						}
						catch (ArgumentOutOfRangeException) {
							// They just added a new value
							SelectionChange(0, 0, featuresRowsAdded, featureTable, featuresData);
						}
						int index = 0;
						foreach (string value in features) {
							AddRow(featureTable, 1, value, index, featuresData);
							index++;
						}
						SelectionChange(rowSelectionID, rowSelectionID, featuresRowsAdded, featureTable, featuresData);
					}
					else if (switchLabels.SelectedIndex == 2) { // Comments
						commentsRowsAdded = 0;
						try {
							for (int i = 0; i < comments.Count; i++) {
								commentTable.RowGroups[0].Rows[i].Cells.RemoveRange(0, 1);
							}
						}
						catch (ArgumentOutOfRangeException) {
							// They just added a new value
							SelectionChange(0, 0, commentsRowsAdded, commentTable, commentsData);
						}
						int index = 0;
						foreach (string value in comments) {
							AddRow(commentTable, 2, value, index, commentsData);
							index++;
						}
						SelectionChange(rowSelectionID, rowSelectionID, commentsRowsAdded, commentTable, commentsData);
					}
				});
				
			}
		}

		/// <summary>
		/// Writes the changes to the editing file.
		/// </summary>
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

						if (Passthrough.Title != "") {
							if (Passthrough.Title != projectTitle) {
								projectTitle = Passthrough.Title; // Sync up values after changing title from details window
							}
						}
						else { // Set title for passthrough
							Passthrough.Title = projectTitle;
						}

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

					File.WriteAllText(editingFile, sw.ToString());
					sb.Clear();
					sw.Close();

					break;
				}
				catch (ObjectDisposedException) {

				}
			}
		}

		/// <summary>
		/// Startup function that runs when code execution begins.
		/// </summary>
		private void Startup() {
			stopwatch = new Stopwatch();

			editingFile = Passthrough.EditingFile;

			Passthrough.Title = "";

			string json = File.ReadAllText(editingFile);
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
			ResetSelection(errorTable, errors, errorsData, 0, errorsRowsAdded);
			ResetSelection(featureTable, features, featuresData, 1, featuresRowsAdded);
			ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);

			errorScrollView.Focus();
			isTablesGenerated = true;
			durationLabel.Text = values.Duration;

			try {
				if (errorsData[0] == "1") { // Checked checkbox
					checkmarkButton.ToolTip = "Mark as incomplete";
					checkmarkButton.Text = "\xE73A";
				}
				else { // Unchecked checkbox
					checkmarkButton.ToolTip = "Mark as completed";
					checkmarkButton.Text = "\xE739";
				}
			}
			catch (ArgumentOutOfRangeException) { // No value there so uncheck it
				checkmarkButton.ToolTip = "Mark as completed";
				checkmarkButton.Text = "\xE739";
			}

			timerThread = new Thread(StopwatchTimer);
			timerThread.Start();

			CalculatePercentage();
		}

		/// <summary>
		/// Stopwatch start/stop function that runs when the stopwatch button is pressed.
		/// </summary>
		private void Stopwatch(object sender, MouseButtonEventArgs e) {
			if (isStopwatchRunning) {
				stopwatchButton.Foreground = new SolidColorBrush(redStopwatch);
				isStopwatchRunning = false;
				lastSecond = 0;
				currentSecond = 0;

				stopwatch.Stop();
				stopwatch.Reset();

				Save();
			}
			else {
				stopwatchButton.Foreground = new SolidColorBrush(greenStopwatch);
				isStopwatchRunning = true;
				stopwatch.Start();
			}
		}

		/// <summary>
		/// Launches the EditProgramDetails window to edit the program title.
		/// </summary>
		private void Edit(object sender, MouseButtonEventArgs e) {
			Passthrough.Title = projectTitle;
			Passthrough.Errors = errors.ToArray();
			Passthrough.ErrorsData = errorsData.ToArray();
			Passthrough.Features = features.ToArray();
			Passthrough.FeaturesData = featuresData.ToArray();
			Passthrough.Comments = comments.ToArray();
			Passthrough.CommentsData = commentsData.ToArray();
			Passthrough.Duration = duration;
			Passthrough.Percent = percentComplete;
			Passthrough.EditingFile = editingFile;
			Passthrough.SelectedIndex = switchLabels.SelectedIndex;

			Passthrough.IsAdding = true;
			readThread = new Thread(Read);
			readThread.Start();

			EditProgramDetails details = new EditProgramDetails();
			details.Show();
		}

		/// <summary>
		/// Aborts threads and closes the window.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			MainWindow main = new MainWindow();
			main.filesRead.Remove(editingFile);

			isStopwatchRunning = false;

			if (timerThread.IsAlive) {
				timerThread.Abort();
			}
			try {
				if (readThread.IsAlive) {
					readThread.Abort();
				}
			}
			catch (NullReferenceException) {
				// We haven't tried to create a new item
			}
		}

		/// <summary>
		/// Takes key inputs to change the selection on the current table.
		/// </summary>
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
						SelectionChange(oldRowSelectionID, rowSelectionID, errorsRowsAdded, errorTable, errorsData);
					}
					else if (switchLabels.SelectedIndex == 1) { // Feature table is selected
						SelectionChange(oldRowSelectionID, rowSelectionID, featuresRowsAdded, featureTable, featuresData);
					}
					else { // Comment table is selected
						SelectionChange(oldRowSelectionID, rowSelectionID, commentsRowsAdded, commentTable, commentsData);
					}
					oldRowSelectionID = rowSelectionID;
				}
			}
		}

		/// <summary>
		/// Refreshes table data when the comboBox selection is changed.
		/// </summary>
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			// The selection "changes" (is automatically set) when the window
			// is first opened, but if we try to make changes to one of the
			// elements that hasn't been initialized or added rows to it yet
			// then it'll obviously return an error. So we wait until AFTER
			// the tables have been initialized, aka after startup.
			if (isTablesGenerated) {
				ResetSelection(errorTable, errors, errorsData, 0, errorsRowsAdded);
				ResetSelection(featureTable, features, featuresData, 1, featuresRowsAdded);
				ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);

				if (switchLabels.SelectedIndex == 0) { // Errors
					errorScrollView.Visibility = Visibility.Visible;
					featureScrollView.Visibility = Visibility.Hidden;
					commentScrollView.Visibility = Visibility.Hidden;

					try {
						if (errorsData[0] == "1") { // Checked checkbox
							checkmarkButton.ToolTip = "Mark as incomplete";
							checkmarkButton.Text = "\xE73A";
						}
						else { // Unchecked checkbox
							checkmarkButton.ToolTip = "Mark as completed";
							checkmarkButton.Text = "\xE739";
						}
					}
					catch (ArgumentOutOfRangeException) { // No value there so uncheck it
						checkmarkButton.ToolTip = "Mark as completed";
						checkmarkButton.Text = "\xE739";
					}

					errorScrollView.Focus();
				}
				else if (switchLabels.SelectedIndex == 1) { // Features
					errorScrollView.Visibility = Visibility.Hidden;
					featureScrollView.Visibility = Visibility.Visible;
					commentScrollView.Visibility = Visibility.Hidden;

					try {
						if (featuresData[0] == "1") { // Checked checkbox
							checkmarkButton.ToolTip = "Mark as incomplete";
							checkmarkButton.Text = "\xE73A";
						}
						else { // Unchecked checkbox
							checkmarkButton.ToolTip = "Mark as completed";
							checkmarkButton.Text = "\xE739";
						}
					}
					catch (ArgumentOutOfRangeException) { // No value there so uncheck it
						checkmarkButton.ToolTip = "Mark as completed";
						checkmarkButton.Text = "\xE739";
					}

					featureScrollView.Focus();
				}
				else { // Comments
					errorScrollView.Visibility = Visibility.Hidden;
					featureScrollView.Visibility = Visibility.Hidden;
					commentScrollView.Visibility = Visibility.Visible;

					try {
						if (commentsData[0] == "1") { // Checked checkbox
							checkmarkButton.ToolTip = "Mark as incomplete";
							checkmarkButton.Text = "\xE73A";
						}
						else { // Unchecked checkbox
							checkmarkButton.ToolTip = "Mark as completed";
							checkmarkButton.Text = "\xE739";
						}
					}
					catch (ArgumentOutOfRangeException) { // No value there so uncheck it
						checkmarkButton.ToolTip = "Mark as completed";
						checkmarkButton.Text = "\xE739";
					}

					commentScrollView.Focus();
				}
			}
		}

		/// <summary>
		/// Deletes an item from the current table.
		/// </summary>
		private void RemoveValue(object sender, MouseButtonEventArgs e) {
			try {
				if (switchLabels.SelectedIndex == 0) { // Errors
					SelectionChange(oldRowSelectionID, 0, errorsRowsAdded, errorTable, errorsData);
					errors.RemoveAt(rowSelectionID);
					errorsData.RemoveAt(rowSelectionID);
					ResetSelection(errorTable, errors, errorsData, 0, errorsRowsAdded);

					if (errors.Count == 0) {
						checkmarkButton.ToolTip = "Mark as completed";
						checkmarkButton.Text = "\xE739";
					}
				}
				else if (switchLabels.SelectedIndex == 1) { // Features
					SelectionChange(oldRowSelectionID, 0, featuresRowsAdded, featureTable, featuresData);
					features.RemoveAt(rowSelectionID);
					featuresData.RemoveAt(rowSelectionID);
					ResetSelection(featureTable, features, featuresData, 1, featuresRowsAdded);

					if (features.Count == 0) {
						checkmarkButton.ToolTip = "Mark as completed";
						checkmarkButton.Text = "\xE739";
					}
				}
				else if (switchLabels.SelectedIndex == 2) { // Comments
					SelectionChange(oldRowSelectionID, 0, commentsRowsAdded, commentTable, commentsData);
					comments.RemoveAt(rowSelectionID);
					commentsData.RemoveAt(rowSelectionID);
					ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);

					if (comments.Count == 0) {
						checkmarkButton.ToolTip = "Mark as completed";
						checkmarkButton.Text = "\xE739";
					}
				}
			}
			catch (ArgumentOutOfRangeException) { // They tried to press the delete button when there was no items in it
				return;
			}
			CalculatePercentage();
		}

		/// <summary>
		/// Launches AddNewItem to add a new item to the current table.
		/// </summary>
		private void AddValue(object sender, MouseButtonEventArgs e) {
			if (switchLabels.SelectedIndex == 0) {
				errors.Add("");
				errorsData.Add("0");
			}
			else if (switchLabels.SelectedIndex == 1) {
				features.Add("");
				featuresData.Add("0");
			}
			else if (switchLabels.SelectedIndex == 2) {
				comments.Add("");
				commentsData.Add("0");
			}

			Passthrough.Title = projectTitle;
			Passthrough.Errors = errors.ToArray();
			Passthrough.ErrorsData = errorsData.ToArray();
			Passthrough.Features = features.ToArray();
			Passthrough.FeaturesData = featuresData.ToArray();
			Passthrough.Comments = comments.ToArray();
			Passthrough.CommentsData = commentsData.ToArray();
			Passthrough.Duration = duration;
			Passthrough.Percent = percentComplete;
			Passthrough.EditingFile = editingFile;
			Passthrough.SelectedIndex = switchLabels.SelectedIndex;

			Passthrough.IsAdding = true;
			readThread = new Thread(Read);
			readThread.Start();

			AddNewItem newItem = new AddNewItem();			
			newItem.Show();
		}

		/// <summary>
		/// Checks off the selected item.
		/// </summary>
		private void CheckOff(object sender, MouseButtonEventArgs e) {
			try {
				if (switchLabels.SelectedIndex == 0) { // Errors
					if (errorsData[rowSelectionID] == "0") { // Not checked, check it!
						errorsData[rowSelectionID] = "1";
						ResetSelection(errorTable, errors, errorsData, 0, errorsRowsAdded);

						if (errorsRowsAdded == 1) {
							checkmarkButton.ToolTip = "Mark as incomplete";
							checkmarkButton.Text = "\xE73A";
						}
					}
					else { // Uncheck it
						errorsData[rowSelectionID] = "0";
						ResetSelection(errorTable, errors, errorsData, 0, errorsRowsAdded);

						if (errorsRowsAdded == 1) {
							checkmarkButton.ToolTip = "Mark as completed";
							checkmarkButton.Text = "\xE739";
						}
					}
				}
				else if (switchLabels.SelectedIndex == 1) { // Features
					if (featuresData[rowSelectionID] == "0") {
						featuresData[rowSelectionID] = "1";
						ResetSelection(featureTable, features, featuresData, 1, featuresRowsAdded);

						if (featuresRowsAdded == 1) {
							checkmarkButton.ToolTip = "Mark as incomplete";
							checkmarkButton.Text = "\xE73A";
						}
					}
					else {
						featuresData[rowSelectionID] = "0";
						ResetSelection(featureTable, features, featuresData, 1, featuresRowsAdded);

						if (featuresRowsAdded == 1) {
							checkmarkButton.ToolTip = "Mark as completed";
							checkmarkButton.Text = "\xE739";
						}
					}
				}
				else if (switchLabels.SelectedIndex == 2) { // Comments
					if (commentsData[rowSelectionID] == "0") {
						commentsData[rowSelectionID] = "1";
						ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);

						if (commentsRowsAdded == 1) {
							checkmarkButton.ToolTip = "Mark as incomplete";
							checkmarkButton.Text = "\xE73A";
						}
					}
					else {
						commentsData[rowSelectionID] = "0";
						ResetSelection(commentTable, comments, commentsData, 2, commentsRowsAdded);
						if (commentsRowsAdded == 1) {
							checkmarkButton.ToolTip = "Mark as completed";
							checkmarkButton.Text = "\xE739";
						}
					}
				}
			}
			catch (ArgumentOutOfRangeException) {
				// Happens if they try to check nothing
				return;
			}
			CalculatePercentage();
		}
	}
}