using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
	class NextVersionManifest {

		public class Rootobject {
			public string Version { get; set; }
			public string EstimatedRelease { get; set; }
			public string ReleaseDateConfirmed { get; set; }
			public string[] NewFeatures { get; set; }
		}

	}
}
