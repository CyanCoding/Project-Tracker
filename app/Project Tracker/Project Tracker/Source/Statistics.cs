using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker {
    class Statistics {
        // WARNING: READONLY VARIABLES. IF YOU CHANGE THESE, CHANGE IN ALL OTHER FILES.
        private static readonly string INITIAL_FILE_DIALOG_DIRECTORY = Environment.SpecialFolder.MyDocuments.ToString();

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
            fileDialog.InitialDirectory = INITIAL_FILE_DIALOG_DIRECTORY;
            fileDialog.Title = "Select Code Files to Count";
            fileDialog.Multiselect = true;

            fileDialog.ShowDialog();

            return fileDialog.FileNames;
        }
    }
}
