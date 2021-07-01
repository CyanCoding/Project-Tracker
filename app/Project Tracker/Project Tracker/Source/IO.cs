using Newtonsoft.Json;
using Project_Tracker.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Tracker.Source {
    class IO {
        /// <summary>
        /// Creates a new encrypted project file.
        /// </summary>
        /// <param name="projectTitle">The title of the new project.</param>
        /// <param name="DATA_DIRECTORY">The directory for storing data files in.</param>
        public static void CreateNewProject(string projectTitle, string DATA_DIRECTORY) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter js = new JsonTextWriter(sw)) {
                js.Formatting = Formatting.Indented;

                js.WriteStartObject();

                // Title
                js.WritePropertyName("Title");
                js.WriteValue(projectTitle);

                // The name of each task
                js.WritePropertyName("Tasks");
                js.WriteStartArray();
                js.WriteEnd();

                // The data for each task (0 = incomplete, 1 = complete)
                js.WritePropertyName("TaskData");
                js.WriteStartArray();
                js.WriteEnd();

                // The identifer for each task (0 = error, 1 = feature, 2 = comment)
                js.WritePropertyName("TaskIdentifier");
                js.WriteStartArray();
                js.WriteEnd();

                // The lines of code files
                js.WritePropertyName("LinesOfCodeFiles");
                js.WriteStartArray();
                js.WriteEnd();

                // The folder location for the project
                js.WritePropertyName("FolderLocation");
                js.WriteValue("");

                // Duration
                js.WritePropertyName("Duration");
                js.WriteValue("00:00:00");

                // Date Created
                js.WritePropertyName("DateCreated");
                js.WriteValue(Statistics.CreationDate());

                // Tasks made
                js.WritePropertyName("TasksMade");
                js.WriteValue(0);

                // Tasks completed
                js.WritePropertyName("TasksCompleted");
                js.WriteValue(0);

                // Icon
                js.WritePropertyName("Icon");
                js.WriteValue("noIcon");

                // Percent
                js.WritePropertyName("Percent");
                js.WriteValue("00");

                js.WriteEndObject();
            }

            string encryptedData = Cryptography.Encrypt(sw.ToString(), Globals.ENCRYPTION_GUID);

            // Attempts to save the file using a unique file name
            try {
                if (!File.Exists(DATA_DIRECTORY + "/" + projectTitle + ".json")) {
                    File.WriteAllText(DATA_DIRECTORY + "/" + projectTitle + ".json",
                        encryptedData);
                }
                else {
                    int index = 0;
                    while (true) {
                        if (!File.Exists(DATA_DIRECTORY + "/" + projectTitle + " (" + index + ").json")) {
                            File.WriteAllText(DATA_DIRECTORY + "/" + projectTitle + " (" + index + ").json",
                                encryptedData);
                            break;
                        }
                        else {
                            index++;
                            continue;
                        }
                    }
                }
            }
            catch (ArgumentException) { // Path is an invalid name
                int index = 0;
                while (true) {
                    if (!File.Exists("project " + index + ".json")) {
                        File.WriteAllText("project " + index + ".json", encryptedData);
                        break;
                    }
                    else {
                        index++;
                        continue;
                    }
                }
            }

            sb.Clear();
            sw.Close();
        }

        public static void AttemptFixProject(MainTableManifest.Rootobject mainTable, string path) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter js = new JsonTextWriter(sw)) {
                js.Formatting = Formatting.Indented;

                js.WriteStartObject();

                // Title
                js.WritePropertyName("Title");
                if (mainTable.Title != null) {
                    js.WriteValue(mainTable.Title);
                }
                else {
                    js.WriteValue("Untitled project");
                }

                // The name of each task
                js.WritePropertyName("Tasks");
                js.WriteStartArray();
                if (mainTable.Errors != null) {
                    foreach (string error in mainTable.Errors) {
                        js.WriteValue(error);
                    }
                }
                if (mainTable.Features != null) {
                    foreach (string feature in mainTable.Features) {
                        js.WriteValue(feature);
                    }
                }
                if (mainTable.Comments != null) {
                    foreach (string comment in mainTable.Comments) {
                        js.WriteValue(comment);
                    }
                }
                if (mainTable.Tasks != null) {
                    foreach (string task in mainTable.Tasks) {
                        js.WriteValue(task);
                    }
                }
                js.WriteEnd();

                // The data for each task (0 = incomplete, 1 = complete)
                js.WritePropertyName("TaskData");
                js.WriteStartArray();
                if (mainTable.ErrorsData != null) {
                    foreach (string data in mainTable.ErrorsData) {
                        js.WriteValue(data);
                    }
                }
                if (mainTable.FeaturesData != null) {
                    foreach (string data in mainTable.FeaturesData) {
                        js.WriteValue(data);
                    }
                }
                if (mainTable.CommentsData != null) {
                    foreach (string data in mainTable.CommentsData) {
                        js.WriteValue(data);
                    }
                }
                if (mainTable.TaskData != null) {
                    foreach (string data in mainTable.TaskData) {
                        js.WriteValue(data);
                    }
                }
                js.WriteEnd();

                // The identifer for each task (0 = error, 1 = feature, 2 = comment)
                js.WritePropertyName("TaskIdentifier");
                js.WriteStartArray();
                if (mainTable.Errors != null) {
                    for (int i = 0; i < mainTable.Errors.Length; i++) {
                        js.WriteValue("0");
                    }
                }
                if (mainTable.Features != null) {
                    for (int i = 0; i < mainTable.Features.Length; i++) {
                        js.WriteValue("1");
                    }
                }
                if (mainTable.Comments != null) {
                    for (int i = 0; i < mainTable.Comments.Length; i++) {
                        js.WriteValue("2");
                    }
                }
                if (mainTable.TaskIdentifier != null) {
                    foreach (string identifier in mainTable.TaskIdentifier) {
                        js.WriteValue(identifier);
                    }
                }
                js.WriteEnd();

                // Lines of code files
                js.WritePropertyName("LinesOfCodeFiles");
                js.WriteStartArray();
                if (mainTable.LinesOfCodeFiles != null) {
                    foreach (string file in mainTable.LinesOfCodeFiles) {
                        js.WriteValue(file);
                    }
                }
                js.WriteEnd();

                // Folder location
                js.WritePropertyName("FolderLocation");
                if (mainTable.FolderLocation != null) {
                    js.WriteValue(mainTable.FolderLocation);
                }
                else {
                    js.WriteValue("");
                }

                // Duration
                js.WritePropertyName("Duration");
                if (mainTable.Duration != null) {
                    js.WriteValue(mainTable.Duration);
                }
                else {
                    js.WriteValue("00:00:00");
                }

                // Date created
                js.WritePropertyName("DateCreated");
                if (mainTable.DateCreated != null) {
                    js.WriteValue(mainTable.DateCreated);
                }
                else {
                    js.WriteValue(Statistics.CreationDate());
                }

                // Tasks made
                js.WritePropertyName("TasksMade");
                if (mainTable.TasksMade != 0) {
                    js.WriteValue(mainTable.TasksMade);
                }
                else {
                    js.WriteValue(0);
                }

                // Tasks completed
                js.WritePropertyName("TasksCompleted");
                if (mainTable.TasksMade != 0) {
                    js.WriteValue(mainTable.TasksCompleted);
                }
                else {
                    js.WriteValue(0);
                }

                // Icon
                js.WritePropertyName("Icon");
                if (mainTable.Icon != null) {
                    js.WriteValue(mainTable.Icon);
                }
                else {
                    js.WriteValue("noIcon");
                }

                // Percent
                js.WritePropertyName("Percent");
                if (mainTable.Percent != null) {
                    js.WriteValue(mainTable.Percent);
                }
                else {
                    js.WriteValue("00");
                }

                js.WriteEndObject();
            }

            string fileData = Cryptography.Encrypt(sw.ToString(), Globals.ENCRYPTION_GUID);
            File.WriteAllText(path, fileData);
            sb.Clear();
            sw.Close();
        }

        /// <summary>
        /// Saves the project at the provided path.
        /// </summary>
        /// <param name="filePath">The path to save to.</param>
        public static void Save(string filePath, string title, List<string> tasks, List<string> taskData, List<string> taskIdentifier, List<string> linesOfCodeFiles, string folderLocation, string dateCreated, long tasksMade, long tasksCompleted, string percent, string duration, string icon) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter js = new JsonTextWriter(sw)) {
                js.Formatting = Formatting.Indented;

                js.WriteStartObject();

                js.WritePropertyName("Title");
                js.WriteValue(title);

                js.WritePropertyName("Tasks");
                js.WriteStartArray();
                foreach (string task in tasks) {
                    js.WriteValue(task);
                }
                js.WriteEnd();

                js.WritePropertyName("TaskData");
                js.WriteStartArray();
                foreach (string data in taskData) {
                    js.WriteValue(data);
                }
                js.WriteEnd();

                js.WritePropertyName("TaskIdentifier");
                js.WriteStartArray();
                foreach (string identifier in taskIdentifier) {
                    js.WriteValue(identifier);
                }
                js.WriteEnd();

                js.WritePropertyName("LinesOfCodeFiles");
                js.WriteStartArray();
                foreach (string file in linesOfCodeFiles) {
                    js.WriteValue(file);
                }
                js.WriteEnd();

                js.WritePropertyName("FolderLocation");
                js.WriteValue(folderLocation);

                js.WritePropertyName("Duration");
                js.WriteValue(duration);

                js.WritePropertyName("DateCreated");
                js.WriteValue(dateCreated);

                js.WritePropertyName("TasksMade");
                js.WriteValue(tasksMade);

                js.WritePropertyName("TasksCompleted");
                js.WriteValue(tasksCompleted);

                js.WritePropertyName("Icon");
                js.WriteValue(icon);
                js.WritePropertyName("Percent");
                js.WriteValue(percent);

                js.WriteEndObject();
            }

            string fileData = Cryptography.Encrypt(sw.ToString(), Globals.ENCRYPTION_GUID);
            File.WriteAllText(filePath, fileData);
            sb.Clear();
            sw.Close();
        }

        public static void SaveSettings(int selectedIndex, bool isCompletedTasksShown) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter js = new JsonTextWriter(sw)) {
                js.Formatting = Formatting.Indented;

                js.WriteStartObject();

                // LastSelectedIndex
                js.WritePropertyName("LastSelectedIndex");
                js.WriteValue(selectedIndex);

                // DisplayingCompleted
                js.WritePropertyName("DisplayingCompleted");
                if (isCompletedTasksShown) {
                    js.WriteValue(true);
                }
                else {
                    js.WriteValue(false);
                }

                // ForceClose
                js.WritePropertyName("ForceClose");
                js.WriteValue(false);

                js.WriteEndObject();
            }

            string fileData = Cryptography.Encrypt(sw.ToString(), Globals.ENCRYPTION_GUID);
            File.WriteAllText(Globals.SETTINGS_FILE, fileData);
            sb.Clear();
            sw.Close();
        }

        /// <summary>
        /// Reads the data from a path and returns the unencrypted text
        /// </summary>
        /// <param name="path">The path to read</param>
        /// <returns>The unencrypted file</returns>
        public static string ReadEncryptedFile(string path) {
            string json = File.ReadAllText(path);
            return Cryptography.Decrypt(json, Globals.ENCRYPTION_GUID);
        }
    }
}
