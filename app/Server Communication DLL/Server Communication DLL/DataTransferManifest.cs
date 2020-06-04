using System;
using System.Collections.Generic;
using System.Text;

namespace Server_Communication_DLL {
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
