namespace Project_Tracker {

    internal class UpdateManifest {

        public class Rootobject {
            public string Version { get; set; }
            public object[] Updates { get; set; }
            public object[] Fixes { get; set; }
        }
    }
}