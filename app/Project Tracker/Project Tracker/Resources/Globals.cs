using System;
using System.Windows.Media;

namespace Project_Tracker.Resources {

    /// <summary>
    /// All readonly global variables used throughout the Project Tracker
    /// Don't even think about changing these during runtime punk
    /// 
    /// Project tracker data files end in *.pt while other files like settings/version
    /// end in *.ptsd (project tracker settings data)
    /// </summary>
    class Globals {
        // The passcode used for encrypting and decrypting files
        public static readonly string ENCRYPTION_GUID = "FCABA3D4-E5A6-4F14-A41B-4DEEF5AA60B2";

        // The file extension for project tracker files
        public static readonly string projectDataExtension = "*.pt";

        // Generic public color values
        public static readonly Color itemColor = Color.FromRgb(60, 60, 60);
        public static readonly Color labelTextColor = Color.FromRgb(255, 255, 255);

        #region Version
        public static readonly bool IS_BETA = false;

        // IF YOU CHANGE THIS, ALSO CHANGE IT IN UpdateWindow.xaml.cs
        public static readonly string CURRENT_VERSION = "2.4";

        // The next version info
        public static readonly string NEXT_VERSION_INFO = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/next-version.ptsd";

        // The current version info
        public static readonly string VERSION_INFO = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.ptsd";

        // The URL for downloading the current version
        public static readonly string VERSION_MANIFEST_URL =
            "https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version-info/version.json";

        // The URL for downloading the next version
        public static readonly string NEXT_VERSION_MANIFEST_URL =
            "https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version-info/next-version.json";

        #endregion

        #region Directories/paths

        #endregion
        public static readonly string APPDATA_DIRECTORY =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + "/Project Tracker";        

        public static readonly string DATA_DIRECTORY =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + "/Project Tracker/data";

        public static readonly string SETTINGS_FILE =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + "/Project Tracker/settings.ptsd";
    }
}
