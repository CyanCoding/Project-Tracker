using Microsoft.Win32;
using System;
using System.IO.Compression;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using IWshRuntimeLibrary;

namespace Project_Tracker_Installer {
	class Installer {
        /// <summary>
        /// Installs the latest Project Tracker program to the user's computer.
        /// </summary>
        /// <param name="PROGRAM_PATH">The path of the unzipped program.</param>
        /// <param name="INSTALL_DIRECTORY">The directory to install to.</param>
        /// <param name="ONLINE_PROGRAM_LINK">The URL to the program download.</param>
        /// <param name="ZIP_PATH">The zip path to download to and extract from.</param>
        /// <returns></returns>
        public bool InstallProgram(string PROGRAM_PATH, string INSTALL_DIRECTORY, string ONLINE_PROGRAM_LINK, string ZIP_PATH) {
            if (System.IO.File.Exists(PROGRAM_PATH)) {
                System.IO.File.Delete(PROGRAM_PATH);
            }
            if (System.IO.File.Exists(ZIP_PATH)) {
                System.IO.File.Delete(ZIP_PATH);
            }

            try {
                WebClient client = new WebClient();
                client.DownloadFile(new Uri(ONLINE_PROGRAM_LINK), ZIP_PATH);

                ZipFile.ExtractToDirectory(ZIP_PATH, INSTALL_DIRECTORY);
                System.IO.File.Delete(ZIP_PATH);
                return true;
            }
            catch (WebException) {
                return false;
            }

            
        }

        /// <summary>
        /// Creates a program uninstall registry and a shortcut.
        /// </summary>
        /// <param name="REGISTRY">The title of the registry entry to create.</param>
        /// <param name="PROGRAM_TITLE">The title of the program.</param>
        /// <param name="PROGRAM_VERSION">The version of the program we're installing.</param>
        /// <param name="ICON_PATH">The path to the icon.</param>
        /// <param name="PROGRAM_PATH">The path to the installed program.</param>
        /// <param name="INSTALL_DIRECTORY">The directory of the installed program.</param>
        /// <param name="SHORTCUT_LOCATION">The path for the shortcut.</param>
        public void CreateUninstaller(string REGISTRY, string PROGRAM_TITLE, string PROGRAM_VERSION, string ICON_PATH, string PROGRAM_PATH, string INSTALL_DIRECTORY, string SHORTCUT_LOCATION) {
            RegistryKey keyCheck = Registry.CurrentUser.OpenSubKey(REGISTRY);

            if (keyCheck != null) {
                keyCheck.DeleteSubKey(REGISTRY);
            }

            RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY);

            Assembly asm = GetType().Assembly;
            string exe = "\"" + asm.CodeBase.Substring(8).Replace("/", "\\\\") + "\"";

            //storing the values  
            key.SetValue("DisplayName", PROGRAM_TITLE);
            key.SetValue("DisplayVersion", PROGRAM_VERSION);
            key.SetValue("Version", PROGRAM_VERSION);
            key.SetValue("Publisher", "CyanCoding");
            key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
            key.SetValue("DisplayIcon", ICON_PATH);
            key.SetValue("UninstallString", exe + " /uninstallprompt");

            key.Close();

            // Create .lnk (shortcut file) in start menu so the user can open it via the start menu
            WshShell wsh = new WshShell();
            IWshShortcut shortcut = wsh.CreateShortcut(SHORTCUT_LOCATION) as IWshShortcut;
            shortcut.Arguments = "";
            shortcut.TargetPath = PROGRAM_PATH;
            // not sure about what this is for
            shortcut.WindowStyle = 1;
            shortcut.Description = "Shortcut to Project Tracker";
            shortcut.WorkingDirectory = INSTALL_DIRECTORY;
            shortcut.IconLocation = ICON_PATH;
            shortcut.Save();
        }
	}
}
