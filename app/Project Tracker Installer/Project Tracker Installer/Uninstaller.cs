using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace Project_Tracker_Installer {
	class Uninstaller {
        /// <summary>
        /// Main uninstall function.
        /// Prompts user to delete data and deletes every other file.
        /// </summary>
        /// <param name="DATA_DIRECTORY_PATH">The directory where the user's data lies.</param>
        /// <param name="INSTALLER_PATH">The path of the installer to not delete.</param>
        /// <param name="INSTALL_DIRECTORY">The path of the install program directory.</param>
        /// <param name="REGISTRY">The title of the registry entry to remove.</param>
        /// <param name="SHORTCUT_LOCATION">The path of the shortcut to delete.</param>
		public void Uninstall(string DATA_DIRECTORY_PATH, string INSTALLER_PATH, string INSTALL_DIRECTORY, string REGISTRY = "", string SHORTCUT_LOCATION = "") {
            if (Directory.Exists(DATA_DIRECTORY_PATH)) {
                var deleteData = MessageBox.Show("Do you wish to delete your projects data?", "Project Tracker Installer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (deleteData == DialogResult.Yes) {
                    try {
                        Directory.Delete(DATA_DIRECTORY_PATH, true);
                    }
                    catch (Exception) { // Probably couldn't delete the directory for some reason

                    }
                }
            }
            foreach (string f in Directory.GetFiles(INSTALL_DIRECTORY)) {
                if (f != INSTALLER_PATH) {
                    File.Delete(f);
                }
            }

            if (REGISTRY != "") {
                RemoveRegistry(REGISTRY, SHORTCUT_LOCATION);
            }

        }

        /// <summary>
        /// Deletes the registry value for the program.
        /// </summary>
        /// <param name="REGISTRY">The title of the registry entry to remove.</param>
        /// <param name="SHORTCUT_LOCATION">The path of the shortcut to delete.</param>
        private void RemoveRegistry(string REGISTRY, string SHORTCUT_LOCATION) {
            Registry.CurrentUser.DeleteSubKey(REGISTRY);
            File.Delete(SHORTCUT_LOCATION);
        }
	}
}
