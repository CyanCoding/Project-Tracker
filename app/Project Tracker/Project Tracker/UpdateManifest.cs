
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
	class UpdateManifest {

		public class Rootobject {
			public string Version { get; set; }
			public object[] Updates { get; set; }
			public object[] Fixes { get; set; }
		}

	}
}
