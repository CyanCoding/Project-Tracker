﻿namespace Server_Communication_DLL {
	class DataTransferManifest {
		// Don't forget to convert ints to longs!
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
