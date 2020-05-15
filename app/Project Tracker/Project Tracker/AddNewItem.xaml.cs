using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Project_Tracker {

	/// <summary>
	/// Interaction logic for AddNewItem.xaml
	/// </summary>
	public partial class AddNewItem : Window {

		public AddNewItem() {
			InitializeComponent();

			inputBox.Focus();
		}

		/// <summary>
		/// Adds the item to the current table when the return key is pressed.
		/// </summary>
		private void KeyPress(object sender, KeyEventArgs e) {
			if (e.Key == Key.Return) {
				if (inputBox.Text != "") {
					if (Passthrough.SelectedIndex == 0) {
						Passthrough.Errors[Passthrough.Errors.Length - 1] = inputBox.Text;
					}
					else if (Passthrough.SelectedIndex == 1) {
						Passthrough.Features[Passthrough.Features.Length - 1] = inputBox.Text;
					}
					else if (Passthrough.SelectedIndex == 2) {
						Passthrough.Comments[Passthrough.Comments.Length - 1] = inputBox.Text;
					}
					Save();

					this.Hide();
				}
				else {
					warning.Visibility = Visibility.Visible;
				}
			}
		}

		/// <summary>
		/// Adds the item to the current table when the finish button is pressed.
		/// </summary>
		private void finishButton_Click(object sender, RoutedEventArgs e) {
			if (inputBox.Text != "") {
				if (Passthrough.SelectedIndex == 0) {
					Passthrough.Errors[Passthrough.Errors.Length - 1] = inputBox.Text;
				}
				else if (Passthrough.SelectedIndex == 1) {
					Passthrough.Features[Passthrough.Features.Length - 1] = inputBox.Text;
				}
				else if (Passthrough.SelectedIndex == 2) {
					Passthrough.Comments[Passthrough.Comments.Length - 1] = inputBox.Text;
				}
				Save();

				this.Hide();
			}
			else {
				warning.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Cancels the operation and hides the window.
		/// </summary>
		private void cancelButton_Click(object sender, RoutedEventArgs e) {
			Passthrough.IsAdding = false;
			this.Hide();
		}

		/// <summary>
		/// Saves the item to the editing file.
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

			Passthrough.IsAdding = true;
		}
	}
}