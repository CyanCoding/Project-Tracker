using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace Project_Tracker_Installer {
	class Uninstaller {
        readonly string LOG_PATH = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install-log.txt";

        private void LogData(string data) {
            File.AppendAllText(LOG_PATH, data + "\n");
        }

        /// <summary>
        /// Main uninstall function.
        /// Prompts user to delete data and deletes every other file.
        /// </summary>
        /// <param name="DATA_DIRECTORY_PATH">The directory where the user's data lies.</param>
        /// <param name="INSTALLER_PATH">The path of the installer to not delete.</param>
        /// <param name="INSTALL_DIRECTORY">The path of the install program directory.</param>
        /// <param name="REGISTRY">The title of the registry entry to remove.</param>
        /// <param name="SHORTCUT_LOCATION">The path of the shortcut to delete.</param>
		public void Uninstall(string DATA_DIRECTORY_PATH, string INSTALLER_PATH, string BASE_DIRECTORY, string LOG_PATH, string REGISTRY = "", string SHORTCUT_LOCATION = "") {
            try {
                LogData("[Uninstaller]: Running...");
                if (Directory.Exists(DATA_DIRECTORY_PATH)) {
                    LogData("[Uninstaller]: Asking user if they want to delete project data");
                    var deleteData = MessageBox.Show("Do you wish to delete your projects data?", "Project Tracker Installer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (deleteData == DialogResult.Yes) {
                        LogData("[Uninstaller]: Deleting project data in " + DATA_DIRECTORY_PATH);
                        try {
                            Directory.Delete(DATA_DIRECTORY_PATH, true);
                        }
                        catch (Exception e) { // Probably couldn't delete the directory for some reason
                            LogData("[Uninstaller ERROR]: Error while deleting directory data: " + e);
                        }
                    }
                }

                DirectoryInfo di = new DirectoryInfo(BASE_DIRECTORY);

                foreach (FileInfo file in di.GetFiles()) {
                    string hi = file.FullName; // TODO: This doesn't match either because file.FullName returns a different appdata folder than the on we're using.
                    if (file.FullName != INSTALLER_PATH && file.FullName != LOG_PATH) {
                        file.Delete();
                        LogData("[Uninstaller]: Deleting file " + file);
                    }
                }
                foreach (DirectoryInfo dir in di.GetDirectories()) {
                    if (dir.FullName != DATA_DIRECTORY_PATH) {
                        dir.Delete(true);
                        LogData("[Uninstaller]: Deleting directory " + dir);
                    }
                }

                if (REGISTRY != "") {
                    LogData("[Uninstaller]: Registry is not blank, so we delete it");
                    RemoveRegistry(REGISTRY, SHORTCUT_LOCATION);
                }
            }
            catch (Exception e) {
                LogData("[Uninstaller ERROR]: " + e);
            }
        }

        /// <summary>
        /// Deletes the registry value for the program.
        /// </summary>
        /// <param name="REGISTRY">The title of the registry entry to remove.</param>
        /// <param name="SHORTCUT_LOCATION">The path of the shortcut to delete.</param>
        private void RemoveRegistry(string REGISTRY, string SHORTCUT_LOCATION) {
            try {
                Registry.CurrentUser.DeleteSubKey(REGISTRY);
                File.Delete(SHORTCUT_LOCATION);
            }
            catch (Exception e) {
                LogData("[Uninstaller(Registry) ERROR]: " + e);
            }
        }
	}
}
