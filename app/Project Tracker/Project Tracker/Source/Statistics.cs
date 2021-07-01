using Microsoft.Win32;
using Project_Tracker.Resources;
using System;
using System.IO;
using System.Linq;

namespace Project_Tracker {

    internal class Statistics {

        /// <summary>
        /// Counts the lines for each provided file.
        /// </summary>
        /// <param name="files">The array of files to count.</param>
        /// <returns>Returns the number of total lines.</returns>
        public static int CountLines(string[] files) {
            int linesOfCode = 0;

            foreach (string file in files) {
                linesOfCode += File.ReadLines(file).Count();
            }

            return linesOfCode;
        }

        /// <summary>
        /// Creates the current date in mm/dd/yyyy form.
        /// </summary>
        /// <returns>Returns the date.</returns>
        public static string CreationDate() {
            DateTime today = DateTime.Now;

            return today.ToString("MM/dd/yyyy");
        }

        /// <summary>
        /// Opens a file dialog and gets an array of the selected files.
        /// </summary>
        /// <returns>Returns the array of selected files.</returns>
        public static string[] GetFiles() {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Globals.INITIAL_FILE_DIALOG_DIRECTORY;
            fileDialog.Title = "Select Code Files to Count";
            fileDialog.Multiselect = true;

            fileDialog.ShowDialog();

            return fileDialog.FileNames;
        }
    }
}