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

        public static string ReadEncryptedFile(string path) {
            string json = File.ReadAllText(path);
            return Cryptography.Decrypt(json, Globals.ENCRYPTION_GUID);
        }
    }
}
