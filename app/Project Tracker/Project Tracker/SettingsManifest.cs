
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
	class SettingsManifest {

		/*
		 * Here's a list of what each setting menas:
		 * 
		 * 1. LastSelectedIndex: The index of the program last selected. If the user
		 * selects the second program, we want that and not the first to display
		 * next time the program launches. NOTE: first program is index 1 not 0.
		 * By default this is 1.
		 * 
		 * 2. DisplayingCompleted: If the user had been displaying the completed
		 * tab, we want to display that again. By default this is false.
		 */
		public class Rootobject {
			public string LastSelectedIndex { get; set; }
			public string DisplayingCompleted { get; set; }
		}

	}
}
