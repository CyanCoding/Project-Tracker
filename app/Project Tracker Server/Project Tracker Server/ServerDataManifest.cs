using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker_Server {
	class ServerDataManifest {
		// Make sure to set ints to longs!
		public class Rootobject {
			public long AmountOpened { get; set; }
			public string[] DaysOpened { get; set; }
			public long ProjectsCount { get; set; }
			public long YearlyOpens { get; set; }
			public long MonthlyOpens { get; set; }
			public long WeeklyOpens { get; set; }
			public long CurrentlyOpenSessions { get; set; }
		}

	}
}
