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
		public void Uninstall(string DATA_DIRECTORY, string INSTALLER_DIRECTORY, string BASE_DIRECTORY, string REGISTRY = "", string SHORTCUT_LOCATION = "") {
            try {
                LogData("[Uninstaller]: Running...");
                if (Directory.Exists(DATA_DIRECTORY)) {
                    LogData("[Uninstaller]: Asking user if they want to delete project data");
                    var deleteData = MessageBox.Show("Do you wish to delete your projects data?", "Project Tracker Installer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (deleteData == DialogResult.Yes) {
                        LogData("[Uninstaller]: Deleting project data in " + DATA_DIRECTORY);
                        try {
                            Directory.Delete(DATA_DIRECTORY, true);
                        }
                        catch (Exception e) { // Probably couldn't delete the directory for some reason
                            LogData("[Uninstaller ERROR]: Error while deleting directory data: " + e);
                        }
                    }
                }

                // Removes the files inside the install folder except for the installer
                DirectoryInfo installerDir = new DirectoryInfo(INSTALLER_DIRECTORY);

                foreach (FileInfo file in installerDir.GetFiles()) {
                    if (file.Name != "Project Tracker Installer.exe") {
                        file.Delete();
                        LogData("[Uninstaller]: Deleting file " + file);
                    }
                }

                DirectoryInfo dir = new DirectoryInfo(BASE_DIRECTORY);

                // Removes all the base files
                foreach (FileInfo file in dir.GetFiles()) {
                    if (file.Name != "install-log.txt") {
                        file.Delete();
                        LogData("[Uninstaller]: Deleting file " + file);
                    }
                }

                // Removes every folder except the install folder and data folder (which should've already been removed)
                foreach (DirectoryInfo i in dir.GetDirectories()) {
                    if (i.Name != "data" && i.Name != "install") {
                        i.Delete(true);
                        LogData("[Uninstaller]: Deleting directory " + dir);
                    }
                }

                if (REGISTRY != "") {
                    LogData("[Uninstaller]: Registry is not blank, so we delete it");
                    RemoveRegistry(REGISTRY, SHORTCUT_LOCATION);
                }
            }
            catch (Exception e) {
                // TODO: We're having issues uninstalling because the other program is still running
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
