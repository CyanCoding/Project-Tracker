using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker_Server {
	class ClientDataManifest {
		// Make sure to set ints to longs!
		public class Rootobject {
			public long AmountOpened { get; set; }
			public string[] DaysOpened { get; set; }
			public long ProjectsCount { get; set; }
			public string UserID { get; set; }
			public bool IsOpen { get; set; }
			public long YearlyOpens { get; set; }
			public long MonthlyOpens { get; set; }
			public long WeeklyOpens { get; set; }
		}

	}
}
