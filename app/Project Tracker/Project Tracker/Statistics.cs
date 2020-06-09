using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
	class Statistics {
		public static int CountLines(string[] files) {
			int linesOfCode = 0;

			foreach (string file in files) {
				linesOfCode += File.ReadLines(file).Count();
			}

			return linesOfCode;
		}

		
	}
}
