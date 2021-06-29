namespace Project_Tracker {
	class SettingsManifest {

		/*
         * Note: there is a duplicate of this file in the program. Make sure you change that one too.
         * 
		 * Here's a list of what each setting means:
		 * 
		 * 1. LastSelectedIndex: The index of the program last selected. If the user
		 * selects the second program, we want that and not the first to display
		 * next time the program launches. NOTE: first program is index 1 not 0.
		 * By default this is 1.
		 * 
		 * 2. DisplayingCompleted: If the user had been displaying the completed
		 * tab, we want to display that again. By default this is false.
		 * 
		 * 3. ForceClose: This should be false unless the program needs to exit immediately,
		 * like in the event of uninstallation.
		 */
		public class Rootobject {
			public int LastSelectedIndex { get; set; }
			public bool DisplayingCompleted { get; set; }
			public bool ForceClose { get; set; }
		}

	}
}
