using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_Tracker {
    class Folder {
        // WARNING: READONLY VARIABLES. IF YOU CHANGE THESE, CHANGE IN ALL OTHER FILES.
        private static readonly string INITIAL_FILE_DIALOG_DIRECTORY = Environment.SpecialFolder.MyDocuments.ToString();

        public static string SelectFolder() {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.InitialDirectory = INITIAL_FILE_DIALOG_DIRECTORY;
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
