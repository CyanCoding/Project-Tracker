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
		string[] features;
		string[] comments;
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

		Stopwatch stopwatch;

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		readonly Color sortColor = Color.FromRgb(228, 233, 235);
		readonly Color greenStopwatch = Color.FromRgb(42, 112, 35);
		readonly Color redStopwatch = Color.FromRgb(156, 9, 9);
		readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		public EditProgram() {
			InitializeComponent();

			startup();
		}

		private void AddRow(Table table, int tableCount, string value) {
			table.RowGroups[0].Rows.Add(new TableRow());

			TableRow newRow = null;

			if (tableCount == 0) { // Error table
				newRow = table.RowGroups[0].Rows[errorRowsAdded];

				errorRowsAdded++;

				if (errorRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}
			else if (tableCount == 1) { // Error table
				newRow = table.RowGroups[0].Rows[featureRowsAdded];

				featureRowsAdded++;

				if (featureRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}
			else { // Comments table
				newRow = table.RowGroups[0].Rows[commentsRowsAdded];

				commentsRowsAdded++;

				if (commentsRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}

			newRow.FontSize = 16;
			newRow.FontFamily = textFont;
			newRow.Cells.Add(new TableCell(new Paragraph(new Run(value))));
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
				projectTitle = values.title;
				errors = values.errors;
				features = values.features;
				comments = values.comments;
				duration = values.duration;
				percentComplete = values.percent;

				// Calculate table pos
				int individualSize = ((int)this.Width - 40) / 3;
				int commentDistance = (int)this.Width - individualSize - 20;
				int featureDistance = (int)this.Width - (individualSize + individualSize + 40);

				// Set pos of tables and their labels
				commentScrollView.Margin = new Thickness(commentDistance, 140, 10, 60);
				featureScrollView.Margin = new Thickness(featureDistance, 140, individualSize + 10, 60);
				errorScrollView.Margin = new Thickness(10, 140, individualSize + individualSize + 30, 60);
				errorLabel.Margin = new Thickness(10, 100, 0, 0);
				featureLabel.Margin = new Thickness(featureDistance, 100, 0, 0);
				commentLabel.Margin = new Thickness(commentDistance, 100, 0, 0);

				// Add values to tables
				foreach (string error in values.errors) {
					AddRow(errorTable, 0, error);
				}
				foreach (string feature in values.features) {
					AddRow(featureTable, 1, feature);
				}
				foreach (string comment in values.comments) {
					AddRow(commentTable, 2, comment);
				}
				this.Title = values.title + " - " + values.percent + "%";
				durationLabel.Text = values.duration;

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

		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
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

			MainWindow main = new MainWindow();
			foreach (string file in main.filesRead) {
				Console.WriteLine(file);
			}
			Console.WriteLine(editingFile);
			main.filesRead.Remove(editingFile);

			isStopwatchRunning = false;
			timerThread.Abort();
		}
	}
}
