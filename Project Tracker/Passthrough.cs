using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
	class Passthrough {
		// We use this to tell EditProgram what file MainWindow is editing when the user
		// Presses the edit button
		public static string EditingFile { get; set; }
	}
}
