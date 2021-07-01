using Microsoft.WindowsAPICodePack.Dialogs;
using Project_Tracker.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_Tracker {
    class Folder {
        public static string SelectFolder() {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.InitialDirectory = Globals.INITIAL_FILE_DIALOG_DIRECTORY;
            folderDialog.IsFolderPicker = true;

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok) {
                return folderDialog.FileName;
            }
            else {
                return "";
            }
        }
    }
}
