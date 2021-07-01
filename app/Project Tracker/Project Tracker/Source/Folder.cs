using Microsoft.WindowsAPICodePack.Dialogs;
using Project_Tracker.Resources;

namespace Project_Tracker {

    internal class Folder {

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