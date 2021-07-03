using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker.Manifests {
    class ExportManifest {
        public class Rootobject {
            public string Version { get; set; }
            public Settings Settings { get; set; }
            public Project[] Projects { get; set; }
        }

        public class Settings {
            public int LastSelectedIndex { get; set; }
            public bool DisplayingCompleted { get; set; }
            public bool ForceClose { get; set; }
        }

        public class Project {
            public string Title { get; set; }
            public string[] Tasks { get; set; }
            public string[] TaskData { get; set; }
            public string[] TaskIdentifier { get; set; }
            public object[] LinesOfCodeFiles { get; set; }
            public string FolderLocation { get; set; }
            public string Duration { get; set; }
            public string DateCreated { get; set; }
            public long TasksMade { get; set; }
            public long TasksCompleted { get; set; }
            public string Icon { get; set; }
            public string Percent { get; set; }
        }

    }
}
