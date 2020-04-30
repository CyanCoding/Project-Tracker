using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
	class MainTableManifest {

		public class Rootobject {
			public string title { get; set; }
			public string[] errors { get; set; }
			public string[] features { get; set; }
			public string[] comments { get; set; }
			public string duration { get; set; }
			public string percent { get; set; }
		}

	}
}
