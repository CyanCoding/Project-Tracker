using System.Windows.Documents;

namespace Project_Tracker {

	internal class Passthrough {

		// We use this to tell EditProgram what file MainWindow is editing when the
		// user presses the edit button
		public static string EditingFile { get; set; }

		// We have to pass the information from EditProgram to AddNewItem
		// because we need to save from AddNewItem
		public static int SelectedIndex { get; set; }

		public static string Title { get; set; }
		public static string[] Errors { get; set; }
		public static string[] ErrorsData { get; set; }
		public static string[] Features { get; set; }
		public static string[] FeaturesData { get; set; }
		public static string[] Comments { get; set; }
		public static string[] CommentsData { get; set; }
		public static string Duration { get; set; }
		public static string Percent { get; set; }

		// We use this to tell whether the add program window is open
		// This is just used for optimization to not run the read thread
		// If we're not about to add something
		public static bool IsAdding { get; set; }

		// Whether we're deleting the edit program file or not
		public static bool IsDeleting { get; set; }
	}
}