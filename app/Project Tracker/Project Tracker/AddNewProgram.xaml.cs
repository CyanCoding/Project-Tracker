﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Project_Tracker {

	/// <summary>
	/// Interaction logic for AddNewProgram.xaml
	/// </summary>
	public partial class AddNewProgram : Window {
		private string projectTitle;
		private List<string> errors = new List<string>();
		private List<string> features = new List<string>();
		private List<string> comments = new List<string>();
		private string duration;

		// WARNING: READONLY VALUES. IF YOU CHANGE THESE, CHANGE IN OTHER FILES AS WELL
		private readonly string DATA_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/data";

		// readonly Color selectionColor = Color.FromRgb(84, 207, 255);
		private readonly Color sortColor = Color.FromRgb(228, 233, 235);

		private readonly FontFamily textFont = new FontFamily("Microsoft Sans Serif");

		private int errorsRowsAdded = 0; // We use a global variable so the AddRow function knows what id of a row to edit
		private int featuresRowsAdded = 0;
		private int commentsRowsAdded = 0;
		// int rowSelectionID = 0; // We use this to identify which row is currently selected

		private bool[] durationSuccess = { false, false, false };

		/*
		 * Current progression through the form
		 *
		 * 0: Project title
		 * 1: Duration
		 * 2: Errors
		 * 3: Features
		 * 4: Comments
		 * 5: Review
		 */
		private int level = 0;

		public AddNewProgram() {
			InitializeComponent();
			projectTitleInputBox.Focus();
		}

		/// <summary>
		/// Moves the form from the project title to the project duration.
		/// </summary>
		private void FirstForward() {
			if (projectTitleInputBox.Text != "") {
				level = 1;

				projectTitle = projectTitleInputBox.Text;
				projectTitleInputBox.Visibility = Visibility.Hidden;
				projectTitleWarning.Visibility = Visibility.Hidden;

				// Set up next components
				cancelButton.Content = "Back";
				projectTitleText.Text = "Duration";
				projectHourInputBox.Visibility = Visibility.Visible;
				projectMinuteInputBox.Visibility = Visibility.Visible;
				projectSecondInputBox.Visibility = Visibility.Visible;
				spacer1.Visibility = Visibility.Visible;
				spacer2.Visibility = Visibility.Visible;
				projectHourInputBox.Focus();
			}
			else { // They haven't inputted a title
				projectTitleWarning.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Moves the form from the project duration to the error list.
		/// </summary>
		private void SecondForward() {
			durationSuccess[0] = CheckValid(projectHourInputBox.Text, "hour");
			durationSuccess[1] = CheckValid(projectMinuteInputBox.Text, "minute");
			durationSuccess[2] = CheckValid(projectSecondInputBox.Text, "second");

			if (durationSuccess[0] && durationSuccess[1] && durationSuccess[2]) { // All are valid numbers!
				level = 2;

				if (projectHourInputBox.Text.Length == 1) {
					projectHourInputBox.Text = "0" + projectHourInputBox.Text;
				}
				if (projectMinuteInputBox.Text.Length == 1) {
					projectMinuteInputBox.Text = "0" + projectMinuteInputBox.Text;
				}
				if (projectSecondInputBox.Text.Length == 1) {
					projectSecondInputBox.Text = "0" + projectSecondInputBox.Text;
				}
				duration = projectHourInputBox.Text + ":" + projectMinuteInputBox.Text + ":" + projectSecondInputBox.Text;

				projectTitleWarning.Visibility = Visibility.Hidden;
				projectTitleText.Text = "Errors";
				projectHourInputBox.Visibility = Visibility.Hidden;
				projectMinuteInputBox.Visibility = Visibility.Hidden;
				projectSecondInputBox.Visibility = Visibility.Hidden;
				spacer1.Visibility = Visibility.Hidden;
				spacer2.Visibility = Visibility.Hidden;

				// Set up the next components
				errorScrollView.Visibility = Visibility.Visible;
				projectErrorInputBox.Visibility = Visibility.Visible;
				errorAddButton.Visibility = Visibility.Visible;
				this.Height = 350;
				projectErrorInputBox.Focus();
			}
		}

		/// <summary>
		/// Moves the form from the error list to the feature list.
		/// </summary>
		private void ThirdForward() {
			level = 3;
			projectTitleText.Text = "Features";

			projectErrorInputBox.Visibility = Visibility.Hidden;
			errorScrollView.Visibility = Visibility.Hidden;

			// Set up the next components
			featureScrollView.Visibility = Visibility.Visible;
			projectFeatureInputBox.Visibility = Visibility.Visible;
			projectFeatureInputBox.Focus();
		}

		/// <summary>
		/// Moves the form from the feature list to the comment list.
		/// </summary>
		private void FourthForward() {
			level = 4;
			projectTitleText.Text = "Comments";

			projectFeatureInputBox.Visibility = Visibility.Hidden;
			featureScrollView.Visibility = Visibility.Hidden;

			// Set up the next components
			commentScrollView.Visibility = Visibility.Visible;
			projectCommentInputBox.Visibility = Visibility.Visible;
			projectCommentInputBox.Focus();
		}

		/// <summary>
		/// Moves the form from the comment list to the final result
		/// </summary>
		private void FifthForward() {
			level = 5;
			projectTitleText.Text = "Percent complete";

			commentScrollView.Visibility = Visibility.Hidden;
			projectCommentInputBox.Visibility = Visibility.Hidden;
			errorAddButton.Visibility = Visibility.Hidden;

			// Set up the next components
			nextButton.Content = "Finish";
			projectTitleText.Text = projectTitle;
			this.Height = 250;

			durationResult.Text = "Duration: " + duration;
			errorResult.Text = "Errors: " + errors.Count;
			featureResult.Text = "Features: " + features.Count;
			commentResult.Text = "Comments: " + comments.Count;

			durationResult.Visibility = Visibility.Visible;
			errorResult.Visibility = Visibility.Visible;
			featureResult.Visibility = Visibility.Visible;
			commentResult.Visibility = Visibility.Visible;
		}

		/// <summary>
		/// Finalizes the project and writes the files to the data folder.
		/// </summary>
		private void Finish() { // Creating file for new project
			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker")) {
				Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker");
			}
			if (!Directory.Exists(DATA_DIRECTORY)) {
				Directory.CreateDirectory(DATA_DIRECTORY);
			}

			string path = DATA_DIRECTORY + "/" + projectTitle + ".json";

			int value = 1;

			while (true) {
				if (File.Exists(path)) {
					path = DATA_DIRECTORY + "/" + projectTitle + " (" + value.ToString() + ").json"; // "data/BFPC (2).json"
					value++;
				}
				else {
					break;
				}
			}

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
				foreach (string error in errors) {
					js.WriteValue("0");
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
				foreach (string feature in features) {
					js.WriteValue("0");
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
				foreach (string comment in comments) {
					js.WriteValue("0");
				}
				js.WriteEnd();

				js.WritePropertyName("Duration");
				js.WriteValue(duration);
				js.WritePropertyName("Percent");
				js.WriteValue("00");

				js.WriteEndObject();
			}

			File.WriteAllText(path, sw.ToString());
			sb.Clear();
			sw.Close();

			this.Hide();
		}

		/*
		 * Checks the validitiy of the duration values to make sure they are real numbers.
		 */

		/// <summary>
		/// Checks to make sure that numbers are valid.
		/// </summary>
		/// <param name="value">The string of the number to test.</param>
		/// <param name="identifier">The name for that number.</param>
		/// <returns>Returns true if a valid number and false if not.</returns>
		private bool CheckValid(string value, string identifier) {
			try {
				Int32.Parse(value);
				return true;
			}
			catch (FormatException) {
				projectTitleWarning.Text = "The " + identifier + " value is invalid.";
				projectTitleWarning.Visibility = Visibility.Visible;
				return false;
			}
		}

		/// <summary>
		/// Advances to the next item in the form.
		/// </summary>
		private void nextButton_Click(object sender, RoutedEventArgs e) {
			switch (level) {
				case 0:
					FirstForward();
					break;

				case 1:
					SecondForward();
					break;

				case 2:
					ThirdForward();
					break;

				case 3:
					FourthForward();
					break;

				case 4:
					FifthForward();
					break;

				case 5:
					Finish();
					break;
			}
		}

		/// <summary>
		/// Adds a row to the current table.
		/// </summary>
		/// <param name="value">The value to add to the table.</param>
		/// <param name="table">The table to add to.</param>
		/// <param name="tableCount">Which table we're adding to (0 = error, 1 = feature, 2 = comment).</param>
		private void AddRow(string value, Table table, int tableCount) {
			table.RowGroups[0].Rows.Add(new TableRow());

			TableRow newRow = null;

			if (tableCount == 0) { // Error table
				newRow = table.RowGroups[0].Rows[errorsRowsAdded];

				errorsRowsAdded++;

				if (errorsRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
					newRow.Background = new SolidColorBrush(sortColor);
				}
			}
			else if (tableCount == 1) { // Error table
				newRow = table.RowGroups[0].Rows[featuresRowsAdded];

				featuresRowsAdded++;

				if (featuresRowsAdded % 2 == 0) { // Every other, change the color for readability purposes
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

		/// <summary>
		/// Checks the table input box value and adds a row if valid.
		/// </summary>
		private void AddValue(object sender, MouseButtonEventArgs e) {
			if (level == 2) { // Error table
				if (projectErrorInputBox.Text != "") {
					errors.Add(projectErrorInputBox.Text);
					AddRow(projectErrorInputBox.Text, errorTable, 0);
					projectErrorInputBox.Text = "";
				}
			}
			else if (level == 3) { // Feature table
				if (projectFeatureInputBox.Text != "") {
					features.Add(projectFeatureInputBox.Text);
					AddRow(projectFeatureInputBox.Text, featureTable, 1);
					projectFeatureInputBox.Text = "";
				}
			}
			else if (level == 4) { // Feature table
				if (projectCommentInputBox.Text != "") {
					comments.Add(projectCommentInputBox.Text);
					AddRow(projectCommentInputBox.Text, commentTable, 2);
					projectCommentInputBox.Text = "";
				}
			}
		}

		/// <summary>
		/// Advances through the thread or adds to the table depending on current view.
		/// </summary>
		private void KeyPress(object sender, KeyEventArgs e) {
			if (e.Key == Key.Return) {
				if (level == 0) { // Title page
					FirstForward();
				}
				else if (level == 1) { // Duration page
					SecondForward();
				}
				else if (level == 2) { // Errors page
					if (projectErrorInputBox.Text != "") {
						errors.Add(projectErrorInputBox.Text);
						AddRow(projectErrorInputBox.Text, errorTable, 0);
						projectErrorInputBox.Text = "";
					}
				}
				else if (level == 3) { // Features page
					if (e.Key == Key.Return) {
						if (projectFeatureInputBox.Text != "") {
							features.Add(projectFeatureInputBox.Text);
							AddRow(projectFeatureInputBox.Text, featureTable, 1);
							projectFeatureInputBox.Text = "";
						}
					}
				}
				else if (level == 4) { // Comments page
					if (projectCommentInputBox.Text != "") {
						comments.Add(projectCommentInputBox.Text);
						AddRow(projectCommentInputBox.Text, commentTable, 2);
						projectCommentInputBox.Text = "";
					}
				}
				else if (level == 5) { // Review page
					Finish();
				}
			}

		}

		/// <summary>
		/// Moves the form from the project duration to the project title. 
		/// </summary>
		private void FirstBack() {
			level = 0;

			cancelButton.Content = "Cancel";
			projectTitleText.Text = "Project title";
			projectTitleInputBox.Visibility = Visibility.Visible;

			// Hide old elements
			projectHourInputBox.Visibility = Visibility.Hidden;
			projectMinuteInputBox.Visibility = Visibility.Hidden;
			projectSecondInputBox.Visibility = Visibility.Hidden;
			spacer1.Visibility = Visibility.Hidden;
			spacer2.Visibility = Visibility.Hidden;
			projectTitleInputBox.Focus();
		}

		/// <summary>
		/// Moves the form from the error list to the project duration.
		/// </summary>
		private void SecondBack() {
			level = 1;

			this.Height = 190;
			projectTitleText.Text = "Duration";
			projectHourInputBox.Visibility = Visibility.Visible;
			projectMinuteInputBox.Visibility = Visibility.Visible;
			projectSecondInputBox.Visibility = Visibility.Visible;
			spacer1.Visibility = Visibility.Visible;
			spacer2.Visibility = Visibility.Visible;
			projectHourInputBox.Focus();

			// Hide old elements
			errorScrollView.Visibility = Visibility.Hidden;
			projectErrorInputBox.Visibility = Visibility.Hidden;
			errorAddButton.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Moves the form from the feature list to the error list.
		/// </summary>
		private void ThirdBack() {
			level = 2;

			projectTitleText.Text = "Errors";
			projectErrorInputBox.Visibility = Visibility.Visible;
			errorScrollView.Visibility = Visibility.Visible;
			projectErrorInputBox.Focus();

			// Hide old elements
			featureScrollView.Visibility = Visibility.Hidden;
			projectFeatureInputBox.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Moves the form from the comment list to the feature list.
		/// </summary>
		private void FourthBack() {
			level = 3;

			projectTitleText.Text = "Features";
			featureScrollView.Visibility = Visibility.Visible;
			projectFeatureInputBox.Visibility = Visibility.Visible;
			projectFeatureInputBox.Focus();

			// Hide old elements
			commentScrollView.Visibility = Visibility.Hidden;
			projectCommentInputBox.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Moves the form from the final window to the comment list.
		/// </summary>
		private void FifthBack() {
			level = 4;

			this.Height = 350;
			projectTitleText.Text = "Comments";
			commentScrollView.Visibility = Visibility.Visible;
			projectCommentInputBox.Visibility = Visibility.Visible;
			errorAddButton.Visibility = Visibility.Visible;
			projectCommentInputBox.Focus();
			nextButton.Content = "Next";

			// Hide old elements
			durationResult.Visibility = Visibility.Hidden;
			errorResult.Visibility = Visibility.Hidden;
			featureResult.Visibility = Visibility.Hidden;
			commentResult.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Moves the form in a reverse order when the back/cancel button is pressed.
		/// </summary>
		private void cancelButton_Click(object sender, RoutedEventArgs e) {
			switch (level) {
				case 0:
					this.Hide();
					break;

				case 1:
					FirstBack();
					break;

				case 2:
					SecondBack();
					break;

				case 3:
					ThirdBack();
					break;

				case 4:
					FourthBack();
					break;

				case 5:
					FifthBack();
					break;
			}
		}
	}
}