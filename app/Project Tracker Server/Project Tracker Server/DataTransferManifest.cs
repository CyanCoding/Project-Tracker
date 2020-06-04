using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker_Server {
	class DataTransferManifest {
		public class Rootobject {
			public int AmountOpened { get; set; }
			public string[] DaysOpened { get; set; }
			public string[] TimesOpened { get; set; }
			public int ProjectsCount { get; set; }
			public string UserID { get; set; }
		}
	}
}
