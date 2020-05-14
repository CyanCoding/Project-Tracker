using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Diagnostics;

namespace Project_Tracker {

	/// <summary>
	/// Interaction logic for AddNewItem.xaml
	/// </summary>
	public partial class EditProgramDetails : Window {
		readonly string PROGRAM_PATH = @"C:\Program Files\Project Tracker\Project Tracker.exe";

		public EditProgramDetails() {
			InitializeComponent();

			this.Title = "Edit " + Passthrough.Title;
			inputBox.Text = Passthrough.Title;
			inputBox.Focus();
			inputBox.CaretIndex = inputBox.Text.Length;
		}

		private void KeyPress(object sender, System.Windows.Input.KeyEventArgs e) {
			if (e.Key == Key.Return) {
				if (inputBox.Text != "") {
					Passthrough.Title = inputBox.Text;
					Save();

					Passthrough.IsAdding = false;
					this.Hide();
				}
				else {
					warning.Visibility = Visibility.Visible;
				}
			}
		}

		private void finishButton_Click(object sender, RoutedEventArgs e) {
			if (inputBox.Text != "") {
				Passthrough.Title = inputBox.Text;
				Save();

				Passthrough.IsAdding = false;
				this.Hide();
			}
			else {
				warning.Visibility = Visibility.Visible;
			}
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e) {
			Passthrough.IsAdding = false;
			this.Hide();
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
						js.WriteValue(Passthrough.Title);

						js.WritePropertyName("Errors");
						js.WriteStartArray();
						foreach (string error in Passthrough.Errors) {
							js.WriteValue(error);
						}
						js.WriteEnd();

						js.WritePropertyName("ErrorsData");
						js.WriteStartArray();
						foreach (string data in Passthrough.ErrorsData) {
							js.WriteValue(data);
						}
						js.WriteEnd();

						js.WritePropertyName("Features");
						js.WriteStartArray();
						foreach (string feature in Passthrough.Features) {
							js.WriteValue(feature);
						}
						js.WriteEnd();

						js.WritePropertyName("FeaturesData");
						js.WriteStartArray();
						foreach (string data in Passthrough.FeaturesData) {
							js.WriteValue(data);
						}
						js.WriteEnd();

						js.WritePropertyName("Comments");
						js.WriteStartArray();
						foreach (string comment in Passthrough.Comments) {
							js.WriteValue(comment);
						}
						js.WriteEnd();

						js.WritePropertyName("CommentsData");
						js.WriteStartArray();
						foreach (string data in Passthrough.CommentsData) {
							js.WriteValue(data);
						}
						js.WriteEnd();

						js.WritePropertyName("Duration");
						js.WriteValue(Passthrough.Duration);

						js.WritePropertyName("Percent");
						js.WriteValue(Passthrough.Percent);

						js.WriteEndObject();
					}

					File.WriteAllText(Passthrough.EditingFile, sw.ToString());
					sb.Clear();
					sw.Close();

					break;
				}
				catch (ObjectDisposedException) {

				}
				catch (IOException) {
					Thread.Sleep(100);
				}
			}
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e) {
			var deleteData = System.Windows.Forms.MessageBox.Show("Are you sure you wish to delete " + Passthrough.Title + "?", "Confirm project deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (deleteData == System.Windows.Forms.DialogResult.Yes) {
				Passthrough.IsDeleting = true;
				this.Close();
				File.Delete(Passthrough.EditingFile);

				ProcessStartInfo start = new ProcessStartInfo {
					// Enter the executable to run, including the complete path
					FileName = PROGRAM_PATH,
					// Do you want to show a console window?
					WindowStyle = ProcessWindowStyle.Normal,
					CreateNoWindow = true
				};
				Process.Start(start);
				Environment.Exit(0);
			}
		}
	}
}