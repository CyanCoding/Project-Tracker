namespace Project_Tracker {

	internal class MainTableManifest {
		/*
		 * Hey this is the manifest class file for reading each variable
		 * From the individual json files for each project the user creates.
		 *
		 * Here's an explanation of each variable:
		 *
		 * Title: the title of the program: "Key Statistics"
		 * Errors: an array of all of the errors: "error 1", "error 2"
		 * ErrorData: an array of each error's completion (0 = incomplete, 1 = complete, 2 = deleted): "0", "2"
		 * Features: an array of all of the features: "feature 1", "feature 2"
		 * FeatureData: see ErrorData except for features
		 * Comments: an array of all of the comments: "comment 1", "comment 2"
		 * CommentsData: see ErrorData except for comments
		 * Duration: a string of the current runtime on the project: "00:00:19"
		 * Icon: the icon for the program: "javascript"
		 * Percent: a string of the current percent completion: "09"
		 */


		public class Rootobject {
			public string Title { get; set; }
			public string[] Errors { get; set; }
			public string[] ErrorsData { get; set; }
			public string[] Features { get; set; }
			public string[] FeaturesData { get; set; }
			public string[] Comments { get; set; }
			public string[] CommentsData { get; set; }
			public string Duration { get; set; }
			public string Icon { get; set; }
			public string Percent { get; set; }
		}
	}
}