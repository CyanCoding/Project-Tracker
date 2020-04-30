using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace Project_Tracker_Installer {
	class Uninstaller {
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

        private void RemoveRegistry(string REGISTRY, string SHORTCUT_LOCATION) {
            Registry.CurrentUser.DeleteSubKey(REGISTRY);
            File.Delete(SHORTCUT_LOCATION);
        }
	}
}
