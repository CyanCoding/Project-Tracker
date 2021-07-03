using Newtonsoft.Json;
using Project_Tracker.Resources;
using Project_Tracker.Source;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Project_Tracker {

    public partial class MainWindow : Window {
        public List<string> filesRead = new List<string>();

        //private Thread backgroundThread;

        private int addingType = 0;
        private int selectedIndex = 0;
        private int itemIndex = 0; // We need this to figure out the index of the item
        private int itemsAdded = 0; // The amount of items added to the scrollviewer
        private int renameProjectClicks = 0; // When we click on the rename project button it activates the "you click the window so stop renaming" so we use an index to make sure that doesn't happen

        private bool isChangingTitle = false;
        private bool isCompletedTasksShown = false;
        private bool isIconSelecting = false;
        private bool isOverallSettingsOpen = false;
        private bool isSettingsOpen = false;
        private bool isSettingsWindowDisplaying = false;
        private bool updateResponse = false;
        private bool isSwitchingAnimationRunning = false; // Keeps the user from double clicking the animation
        private bool isTypeSelecting = false;

        // All these variables relate to the projects and are used for saving/loading
        private List<string> tasks = new List<string>();

        private List<string> taskData = new List<string>();
        private List<string> taskIdentifier = new List<string>(); // The type of item we're adding (0 = error, 1 = feature, 2 = comment)
        private List<string> linesOfCodeFiles = new List<string>();
        private string folderLocation;
        private string dateCreated;
        private long tasksMade;
        private long tasksCompleted;
        private string percent; // Each project's data - Used for saving
        private string title;
        private string duration;
        private string icon;

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

        public MainWindow() {
            InitializeComponent();
            Startup();
        }

        /// <summary>
        /// When we click away from the add item TextBox.
        /// </summary>
        private void addItemTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (addItemTextBox.Text == "") { // Clear the textbox if they click out
                addItemTextBox.Text = "Add something to the project";
                Keyboard.ClearFocus();
            }
        }

        /// <summary>
        /// When we click on the add new item TextBox.
        /// </summary>
        private void AddNewItemTextBoxClick(object sender, MouseButtonEventArgs e) {
            if (addItemTextBox.Text == "Add something to the project") {
                addItemTextBox.Text = "";
            }

            if (isTypeSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isTypeSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    itemTypeSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
            if (isIconSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isIconSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 250;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    iconSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        private void AddProjectClick(object sender, MouseButtonEventArgs e) {
            Window_MouseDown(sender, e);

            if (addProjectTextBox.Text == "Create a new project") {
                addProjectTextBox.Text = "";
            }
        }

        private void AddProjectTextBoxLostFocus(object sender, RoutedEventArgs e) {
            if (addProjectTextBox.Text == "") { // Clear the textbox if they click out
                addProjectTextBox.Text = "Create a new project";
                Keyboard.ClearFocus();
            }
        }

        /// <summary>
        /// Loads a project into the border window.
        /// </summary>
        /// <param name="projectTitle">The title of the adding project.</param>
        /// <param name="projectIcon">The icon for the adding project.</param>
        /// <param name="projectPercent">The percent of the adding project.</param>
        /// <param name="index">The index of the adding project.</param>
        private void AddProjectToWindow(string projectTitle, string projectIcon,
            string projectPercent, int index) {
            switch (index) {
                case 1:
                    if (projectIcon == "noIcon") {
                        image1.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image1.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name1.Content = projectTitle;
                    percent1.Content = projectPercent + "%";
                    border1.Visibility = Visibility.Visible;
                    break;

                case 2:
                    if (projectIcon == "noIcon") {
                        image2.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image2.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name2.Content = projectTitle;
                    percent2.Content = projectPercent + "%";
                    border2.Visibility = Visibility.Visible;
                    break;

                case 3:
                    if (projectIcon == "noIcon") {
                        image3.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image3.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name3.Content = projectTitle;
                    percent3.Content = projectPercent + "%";
                    border3.Visibility = Visibility.Visible;
                    break;

                case 4:
                    if (projectIcon == "noIcon") {
                        image4.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image4.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name4.Content = projectTitle;
                    percent4.Content = projectPercent + "%";
                    border4.Visibility = Visibility.Visible;
                    break;

                case 5:
                    if (projectIcon == "noIcon") {
                        image5.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image5.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name5.Content = projectTitle;
                    percent5.Content = projectPercent + "%";
                    border5.Visibility = Visibility.Visible;
                    break;

                case 6:
                    if (projectIcon == "noIcon") {
                        image6.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image6.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name6.Content = projectTitle;
                    percent6.Content = projectPercent + "%";
                    border6.Visibility = Visibility.Visible;
                    break;

                case 7:
                    if (projectIcon == "noIcon") {
                        image7.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image7.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name7.Content = projectTitle;
                    percent7.Content = projectPercent + "%";
                    border7.Visibility = Visibility.Visible;
                    break;

                case 8:
                    if (projectIcon == "noIcon") {
                        image8.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image8.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name8.Content = projectTitle;
                    percent8.Content = projectPercent + "%";
                    border8.Visibility = Visibility.Visible;
                    break;

                case 9:
                    if (projectIcon == "noIcon") {
                        image9.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image9.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name9.Content = projectTitle;
                    percent9.Content = projectPercent + "%";
                    border9.Visibility = Visibility.Visible;
                    break;

                case 10:
                    if (projectIcon == "noIcon") {
                        image10.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                    }
                    else {
                        image10.Source = (ImageSource)TryFindResource(projectIcon);
                    }

                    name10.Content = projectTitle;
                    percent10.Content = projectPercent + "%";
                    border10.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Calculates the percentage complete for the project.
        /// </summary>
        private void CalculatePercentage() {
            int completed = 0;
            int incomplete = 0;

            for (int i = 0; i < taskData.Count; i++) {
                if (taskData[i] == "0") {
                    incomplete++;
                }
                else if (taskData[i] == "1") {
                    completed++;
                }
            }

            int totalAmountOfItems = completed + incomplete;
            int percentComplete;

            try {
                percentComplete = (completed * 100) / totalAmountOfItems;
            }
            catch (DivideByZeroException) {
                percentComplete = 0;
            }

            if (percentComplete <= 9) {
                percent = "0" + percentComplete;
            }
            else {
                percent = percentComplete.ToString();
            }

            // Set the selected index to display the new percent
            if (selectedIndex == 1) {
                percent1.Content = percent + "%";
            }
            else if (selectedIndex == 2) {
                percent2.Content = percent + "%";
            }
            else if (selectedIndex == 3) {
                percent3.Content = percent + "%";
            }
            else if (selectedIndex == 4) {
                percent4.Content = percent + "%";
            }
            else if (selectedIndex == 5) {
                percent5.Content = percent + "%";
            }
            else if (selectedIndex == 6) {
                percent6.Content = percent + "%";
            }
            else if (selectedIndex == 7) {
                percent7.Content = percent + "%";
            }
            else if (selectedIndex == 8) {
                percent8.Content = percent + "%";
            }
            else if (selectedIndex == 9) {
                percent9.Content = percent + "%";
            }
            else if (selectedIndex == 10) {
                percent10.Content = percent + "%";
            }

            IO.Save(filesRead[selectedIndex - 1], title, tasks, taskData, taskIdentifier, linesOfCodeFiles, folderLocation, dateCreated, tasksMade, tasksCompleted, percent, duration, icon);
            SetSelectedProject();
        }

        private void changeTitleTextBox_LostFocus(object sender, RoutedEventArgs e) {
            isChangingTitle = false;

            changeTitleTextBox.Text = title;

            changeTitleBorder.Visibility = Visibility.Hidden;
            displayingTitle.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// When we check off a task.
        /// </summary>
        private void CheckmarkPressed(object sender, MouseButtonEventArgs e) {
            Image callingImage = (Image)sender;

            string name = callingImage.Name;
            name = name.Remove(0, 1); // Removes the t from the name (e.g. t5 -> 5)

            if (taskData[Int32.Parse(name) - 1] == "1") { // It's already checked
                taskData[Int32.Parse(name) - 1] = "0";
                tasksCompleted--;
            }
            else { // It's not checked
                taskData[Int32.Parse(name) - 1] = "1";
                tasksCompleted++;
            }

            CalculatePercentage();
        }

        /// <summary>
        /// Checks each value for null values.
        /// </summary>
        /// <param name="projectTitle">The project title.</param>
        /// <param name="projectTasks">The project tasks array.</param>
        /// <param name="projectData">The project tasksData array.</param>
        /// <param name="projectIdentifier">The project taskIdentifier array.</param>
        /// <param name="projectLinesOfCodeFiles">The project lines of code file array.</param>
        /// <param name="projectFolderLocation">The project's folder location.</param>
        /// <param name="projectDuration">The project's duration.</param>
        /// <param name="projectDateCreated">The project's date created.</param>
        /// <param name="projectIcon">The project's icon.</param>
        /// <param name="projectPercent">The project's percent.</param>
        /// <returns></returns>
        private bool CheckValues(string projectTitle, string[] projectTasks,
            string[] projectTaskData, string[] projectIdentifier,
            string[] projectLinesOfCodeFiles, string projectFolderLocation,
            string projectDuration, string projectDateCreated, long projectTasksMade,
            long projectTasksCompleted, string projectIcon, string projectPercent) {
            if (projectTitle == null || projectTasks == null || projectTaskData == null ||
                projectIdentifier == null || projectLinesOfCodeFiles == null ||
                projectFolderLocation == null || projectDuration == null ||
                projectDateCreated == null || projectTasksMade == null ||
                projectTasksCompleted == null || projectIcon == null ||
                projectPercent == null) {
                // Check all strings for null values
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        private void CreateNewProject(string title = "") {
            Thread thread = new Thread(() => {
                BackgroundProcesses.ReportProject();
            });
            thread.Start();

            string projectTitle = title;
            if (projectTitle == "") {
                projectTitle = addProjectTextBox.Text;
            }

            IO.CreateNewProject(projectTitle, Globals.DATA_DIRECTORY);

            addProjectTextBox.Text = "Create a new project";
            Keyboard.ClearFocus();
            LoadFiles();
            SetSelectedProject();
        }
        /// <summary>
        /// When the user clicks to delete the project.
        /// </summary>
        private void DeleteProjectButtonPressed(object sender, MouseButtonEventArgs e) {
            var deleteProject = System.Windows.Forms.MessageBox.Show(
                "Are you sure you want to delete the " +
                title + " project?\nThis cannot be undone",
                "Confirm Project Deletion",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Question);

            if (deleteProject == System.Windows.Forms.DialogResult.Yes) { // They want to delete the project
                Dispatcher.Invoke(new Action(() => {
                    isSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    settingsBorder.BeginAnimation(HeightProperty, animation);
                }));

                folderImage.Visibility = Visibility.Hidden;

                addItemTextBox.Text = "Add something to the project";
                Keyboard.ClearFocus();

                File.Delete(filesRead[selectedIndex - 1]);

                selectedIndex = 0;
                IO.SaveSettings(selectedIndex, isCompletedTasksShown);
                LoadFiles();

                SetSelectedProject();
            }
        }

        /// <summary>
        /// Waits for 15 seconds before allowing the user to check for an update again.
        /// </summary>
        /// <returns></returns>
        private async Task DisableUpdateButton() {
            for (int i = 15; i >= 0; i--) {
                updateButton1.Content = i.ToString();
                await Task.Delay(1000);
            }
            if (updateResponse) {
                updateButton1.Content = "Check for an update";
                updateButton1.IsEnabled = true;
            }
            else {
                updateButton1.Content = "Please wait...";
                while (!updateResponse) {
                    await Task.Delay(1000);
                }
                updateButton1.Content = "Check for an update";
                updateButton1.IsEnabled = true;
            }
        }

        /// <summary>
        /// When the user clicks on the icon of the program.
        /// We want to open the change icon dialog.
        /// </summary>
        private void displayingImage_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (!isIconSelecting) { // Display icon selector window
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        isIconSelecting = true;

                        iconSelectBorder.Visibility = Visibility.Visible;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = 0;
                        animation.To = 250;
                        animation.Duration = TimeSpan.FromSeconds(0.2);

                        iconSelectBorder.BeginAnimation(HeightProperty, animation);
                    }));
                });
                thread.Start();
            }
        }

        private void folderImage_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (folderLocation == "" || !Directory.Exists(folderLocation)) {
                folderLocation = Folder.SelectFolder();

                IO.Save(filesRead[selectedIndex - 1], title, tasks, taskData, taskIdentifier, linesOfCodeFiles, folderLocation, dateCreated, tasksMade, tasksCompleted, percent, duration, icon);
            }
            else {
                Process.Start(folderLocation);
            }
        }

        private void folderLocationResetButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            folderLocation = Folder.SelectFolder();
            IO.Save(filesRead[selectedIndex - 1], title, tasks, taskData, taskIdentifier, linesOfCodeFiles, folderLocation, dateCreated, tasksMade, tasksCompleted, percent, duration, icon);

            folderLocationLabel.Content = "Folder location: " + folderLocation;

            if (folderLocation != "" && Directory.Exists(folderLocation)) {
                folderLocationResetButton.Content = "Reset folder location";
            }
        }

        /// <summary>
        /// When we click away from the icon selection border.
        /// </summary>
        private void iconSelectBorder_LostFocus(object sender, RoutedEventArgs e) {
            if (isIconSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isIconSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 250;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    iconSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        /// <summary>
        /// Runs when the user chooses to ignore a shown update.
        /// </summary>
        private void IgnoreUpdate(object sender, MouseButtonEventArgs e) {
            Thread thread = new Thread(() => {
                Dispatcher.Invoke(new Action(() => {
                    updateGrid.Visibility = Visibility.Visible;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 130;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    updateGrid.BeginAnimation(HeightProperty, animation);
                }));
            });
            thread.Start();
        }

        /// <summary>
        /// When we click away from the item selection border.
        /// </summary>
        private void itemTypeSelectBorder_LostFocus(object sender, RoutedEventArgs e) {
            if (isTypeSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isTypeSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.1);

                    itemTypeSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        /// <summary>
        /// Runs when a key is pressed.
        /// Controls selection of main table.
        /// </summary>
        private void KeyPress(object sender, KeyEventArgs e) {
            // Add a new item
            if (e.Key == Key.Return && addItemTextBox.Text != "" &&
                addItemTextBox.Text != "Add something to the project" &&
                addItemTextBox.IsFocused) {
                tasks.Add(addItemTextBox.Text);
                taskData.Add("0");
                taskIdentifier.Add(addingType.ToString());

                // We need to switch the category from complete to incomplete
                if (isCompletedTasksShown) {
                    SwitchCategory(sender, null);
                }

                addItemTextBox.Text = "";

                tasksMade++;

                CalculatePercentage();
            }
            // Add a new project
            else if (e.Key == Key.Return && addProjectTextBox.Text != "" &&
                addProjectTextBox.Text != "Create a new project" &&
                addProjectTextBox.IsFocused) {
                CreateNewProject();
            }
            else if (e.Key == Key.Return && isChangingTitle) {
                isChangingTitle = false;

                title = changeTitleTextBox.Text;

                changeTitleBorder.Visibility = Visibility.Hidden;
                displayingTitle.Visibility = Visibility.Visible;

                if (title.Length > 25) {
                    displayingTitle.Content = title.Substring(0, 22) + "...";
                }
                else {
                    displayingTitle.Content = title;
                }

                IO.Save(filesRead[selectedIndex - 1], title, tasks, taskData, taskIdentifier, linesOfCodeFiles, folderLocation, dateCreated, tasksMade, tasksCompleted, percent, duration, icon);
                LoadFiles();
            }
        }

        /// <summary>
        /// Loads each project.
        /// Used during startup or if the projects change.
        /// </summary>
        private void LoadFiles() {
            // Reset all the projects
            border1.Visibility = Visibility.Hidden;
            image1.Source = (ImageSource)TryFindResource("noIcon");
            name1.Content = "";
            percent1.Content = "";

            border2.Visibility = Visibility.Hidden;
            image2.Source = (ImageSource)TryFindResource("noIcon");
            name2.Content = "";
            percent2.Content = "";

            border3.Visibility = Visibility.Hidden;
            image3.Source = (ImageSource)TryFindResource("noIcon");
            name3.Content = "";
            percent3.Content = "";

            border4.Visibility = Visibility.Hidden;
            image4.Source = (ImageSource)TryFindResource("noIcon");
            name4.Content = "";
            percent4.Content = "";

            border5.Visibility = Visibility.Hidden;
            image5.Source = (ImageSource)TryFindResource("noIcon");
            name5.Content = "";
            percent5.Content = "";

            border6.Visibility = Visibility.Hidden;
            image6.Source = (ImageSource)TryFindResource("noIcon");
            name6.Content = "";
            percent6.Content = "";

            border7.Visibility = Visibility.Hidden;
            image7.Source = (ImageSource)TryFindResource("noIcon");
            name7.Content = "";
            percent7.Content = "";

            border8.Visibility = Visibility.Hidden;
            image8.Source = (ImageSource)TryFindResource("noIcon");
            name8.Content = "";
            percent8.Content = "";

            border9.Visibility = Visibility.Hidden;
            image9.Source = (ImageSource)TryFindResource("noIcon");
            name9.Content = "";
            percent9.Content = "";

            border10.Visibility = Visibility.Hidden;
            image10.Source = (ImageSource)TryFindResource("noIcon");
            name10.Content = "";
            percent10.Content = "";

            filesRead.Clear();
            try {
                string[] files = Directory.GetFiles(Globals.DATA_DIRECTORY,
                    Globals.projectDataExtension, SearchOption.AllDirectories);

                int index = 0;
                foreach (string path in files) {
                    if (!filesRead.Contains(path)) {
                        // Convert each json file to a table row
                        string json = IO.ReadEncryptedFile(path);
                        MainTableManifest.Rootobject mainTable =
                            JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

                        bool fileHasAllValues = CheckValues(mainTable.Title, mainTable.Tasks,
                            mainTable.TaskData, mainTable.TaskIdentifier, mainTable.LinesOfCodeFiles,
                            mainTable.FolderLocation, mainTable.Duration, mainTable.DateCreated,
                            mainTable.TasksMade, mainTable.TasksCompleted, mainTable.Icon, mainTable.Percent);

                        if (!fileHasAllValues) {
                            // This is an older file with improper values
                            // We need to migrate it to a new version.
                            string notice = "";
                            if (mainTable.Title != null) {
                                notice = "The " + mainTable.Title + " project is missing data that" +
                                    " is necessary to load it. This is likely due to having upgraded" +
                                    " recently to a new version of the Project Tracker. Do you want the" +
                                    " Project Tracker to attempt to automatically fix the project?";
                            }
                            else {
                                notice = "The " + path + " project file is missing data that is" +
                                    " necessary to load it. This is likely due to having upgraded" +
                                    " recently to a new version of the Project Tracker. Do you want" +
                                    " the Project Tracker to attempt to automatically fix the project" +
                                    " file?";
                            }

                            var migrateData = System.Windows.Forms.MessageBox.Show(
                                notice,
                                "Missing File Information",
                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                System.Windows.Forms.MessageBoxIcon.Question);

                            if (migrateData == System.Windows.Forms.DialogResult.Yes) {
                                IO.AttemptFixProject(mainTable, path);

                                fileHasAllValues = true;
                                // Reread all the data back again
                                json = IO.ReadEncryptedFile(path);
                                mainTable =
                                    JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

                                System.Windows.Forms.MessageBox.Show(
                                    "Your data has been migrated! The project has been loaded.",
                                    "Project Migrated",
                                    System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Information);
                            }
                            else { // They chose not to migrate the data
                                System.Windows.Forms.MessageBox.Show(
                                    "Your data has not been migrated. You cannot load this project.",
                                    "Project Skipped",
                                    System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Information);
                            }
                        }

                        if (fileHasAllValues) {
                            // Every value in the manifest table is covered
                            // Meaning that this is an updated file
                            index++;
                            if (index > 10) {
                                System.Windows.Forms.MessageBox.Show(
                                "Some projects have been left unloaded because you have reached the" +
                                " 10 project limit. Please finish projects to view more.",
                                "Some Projects Left Unloaded",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                                break;
                            }

                            AddProjectToWindow(mainTable.Title, mainTable.Icon, mainTable.Percent, index);
                            filesRead.Add(path);
                        }
                    }
                }

                if (files.Length == 0) {
                    selectedIndex = 0;
                }
                else {
                    selectedIndex = 1;
                }
            }
            catch (IOException) {
            }
        }

        /// <summary>
        /// Loads an item into the scrollviewer.
        /// </summary>
        /// <param name="text">The text value of the item.</param>
        /// <param name="type">The type of the item (0 = error, 1 = feature, 2 = comment).</param>
        private void LoadValues(string text, int type) {
            // The grid holds all of the values
            Grid grid = new Grid();
            grid.Margin = new Thickness(0, itemsAdded * 65, 0, 0);
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.HorizontalAlignment = HorizontalAlignment.Center;

            grid.Name = "g" + itemsAdded;

            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(50, GridUnitType.Pixel);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(620, GridUnitType.Pixel);
            ColumnDefinition column3 = new ColumnDefinition();
            column3.Width = new GridLength(50, GridUnitType.Pixel);

            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);
            grid.ColumnDefinitions.Add(column3);

            // The border is a curved rectangle behind all the other objects
            Border border = new Border();
            border.Height = 60;
            border.CornerRadius = new CornerRadius(10);
            border.Background = new SolidColorBrush(Globals.itemColor);
            border.BorderThickness = new Thickness(2);
            border.HorizontalAlignment = HorizontalAlignment.Stretch;
            border.VerticalAlignment = VerticalAlignment.Top;
            border.Margin = new Thickness(0, 0, -5, 0);
            border.SetValue(Grid.ColumnSpanProperty, 4);

            // The canvas item for the checkmark
            Canvas checkmarkCanvas = new Canvas();
            checkmarkCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            checkmarkCanvas.VerticalAlignment = VerticalAlignment.Center;
            checkmarkCanvas.Height = 40;
            checkmarkCanvas.Width = 40;
            checkmarkCanvas.SetValue(Grid.ColumnProperty, 0);
            checkmarkCanvas.Margin = new Thickness(5, 10, 5, 10);

            // The checkmark image
            Image checkmarkImage = new Image();
            checkmarkImage.Name = "t" + itemIndex.ToString();
            checkmarkImage.Height = 40;
            checkmarkImage.Width = 40;
            Canvas.SetLeft(checkmarkImage, 10);
            checkmarkImage.Cursor = Cursors.Hand;
            checkmarkImage.MouseLeftButtonDown += CheckmarkPressed;

            if (!isCompletedTasksShown) {
                checkmarkImage.Style = (Style)TryFindResource("checkbox");
            }
            else {
                checkmarkImage.Style = (Style)TryFindResource("completedTask");
            }

            checkmarkCanvas.Children.Add(checkmarkImage);

            // The canvas item for the item type (error/feature/comment)
            Canvas typeCanvas = new Canvas();
            typeCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            typeCanvas.VerticalAlignment = VerticalAlignment.Center;
            typeCanvas.Height = 30;
            typeCanvas.Width = 30;
            typeCanvas.SetValue(Grid.ColumnProperty, 2);

            // The type image
            Image typeImage = new Image();
            typeImage.Height = 30;
            typeImage.Width = 30;

            if (type == 0) {
                typeImage.Source = (ImageSource)TryFindResource("errorDrawingImage");
                border.BorderBrush = new SolidColorBrush(Colors.IndianRed);
            }
            else if (type == 1) {
                typeImage.Source = (ImageSource)TryFindResource("featureDrawingImage");
                border.BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else if (type == 2) {
                typeImage.Source = (ImageSource)TryFindResource("commentDrawingImage");
                border.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else {
                typeImage.Source = (ImageSource)TryFindResource("noIcon");
                border.BorderBrush = new SolidColorBrush(Colors.White);
            }

            typeCanvas.Children.Add(typeImage);

            // The label displaying the content of the item
            Label label = new Label();
            label.Content = text;
            label.Foreground = new SolidColorBrush(Globals.labelTextColor);
            label.Margin = new Thickness(70, 6, 70, 0);
            label.FontSize = 24;

            border.Child = label;
            grid.Children.Add(border);
            grid.Children.Add(checkmarkCanvas);
            grid.Children.Add(typeCanvas);
            scrollviewerGrid.Children.Add(grid);

            itemsAdded++;
        }

        private void NewProjectMenuItem_Click(object sender, RoutedEventArgs e) {
            CreateNewProject("New project");
        }

        private void overallSettingsBorder_LostFocus(object sender, RoutedEventArgs e) {
            if (isOverallSettingsOpen) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isOverallSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 70;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    overallSettingsBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        private void OverallSettingsBorderMouseDown(object sender, MouseButtonEventArgs e) {
            if (!isOverallSettingsOpen) { // Display icon selector window
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        isOverallSettingsOpen = true;

                        overallSettingsBorder.Visibility = Visibility.Visible;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = 0;
                        animation.To = 70;
                        animation.Duration = TimeSpan.FromSeconds(0.2);

                        overallSettingsBorder.BeginAnimation(HeightProperty, animation);
                    }));
                });
                thread.Start();
            }
            else if (isOverallSettingsOpen) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isOverallSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 70;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    overallSettingsBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        private void ProjectStatisticsMouseDown(object sender, MouseButtonEventArgs e) {
            statisticsGrid.Visibility = Visibility.Visible;

            if (title.Length > 14) {
                displayingTitle.Content = title.Substring(0, 14) + "... Statistics";
            }
            else {
                displayingTitle.Content = title + " Statistics";
            }

            displayingImage.Source = (ImageSource)TryFindResource("graphDrawingImage");

            addItemBorder.Visibility = Visibility.Hidden;
            settingsImage.Visibility = Visibility.Hidden;
            folderImage.Visibility = Visibility.Hidden;
            scrollviewerGrid.Visibility = Visibility.Hidden;
            completeGrid.Visibility = Visibility.Hidden;
            noProjectsGrid.Visibility = Visibility.Hidden;

            creationDateLabel.Content = "Date created: " + dateCreated;
            tasksMadeLabel.Content = "Tasks created: " + tasksMade;
            tasksCompletedLabel.Content = "Tasks completed: " + tasksCompleted;
            statisticsDurationLabel.Content = "Duration: coming soon...";
            linesOfCodeLabel.Content = "Lines of code: " + String.Format("{0:#,###0}", Statistics.CountLines(linesOfCodeFiles.ToArray()));

            if (folderLocation != "" && Directory.Exists(folderLocation)) {
                folderLocationResetButton.Content = "Reset folder location";
                folderLocationLabel.Content = "Folder location: " + folderLocation;
            }
            else {
                folderLocationLabel.Content = "Folder location: not set";
                folderLocationResetButton.Content = "Set folder location";
            }
        }

        private void RenameProjectButtonPressed(object sender, MouseButtonEventArgs e) {
            isChangingTitle = true;
            renameProjectClicks = 0;
            changeTitleTextBox.Text = title;

            changeTitleBorder.Visibility = Visibility.Visible;
            displayingTitle.Visibility = Visibility.Hidden;
        }

        private void setCodeCountingButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            linesOfCodeFiles.Clear();

            foreach (string file in Statistics.GetFiles()) {
                linesOfCodeFiles.Add(file);
            }

            IO.Save(filesRead[selectedIndex - 1], title, tasks, taskData, taskIdentifier, linesOfCodeFiles, folderLocation, dateCreated, tasksMade, tasksCompleted, percent, duration, icon);

            linesOfCodeLabel.Content = "Lines of code: " + String.Format("{0:#,###0}", Statistics.CountLines(linesOfCodeFiles.ToArray()));
        }

        /// <summary>
        /// Change the icon for the project.
        /// </summary>
        /// <param name="selectedIcon">The icon to change to.</param>
        private void SetProjectIcon(string selectedIcon) {
            if (selectedIcon == "rustIcon") {
                displayingImage.Source = (ImageSource)TryFindResource("blackRustIcon");
            }
            else {
                displayingImage.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            icon = selectedIcon;

            IO.Save(filesRead[selectedIndex - 1], title, tasks, taskData, taskIdentifier, linesOfCodeFiles, folderLocation, dateCreated, tasksMade, tasksCompleted, percent, duration, icon);

            // Set the icon in the project borders
            if (selectedIndex == 1) {
                image1.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 2) {
                image2.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 3) {
                image3.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 4) {
                image4.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 5) {
                image5.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 6) {
                image6.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 7) {
                image7.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 8) {
                image8.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 9) {
                image9.Source = (ImageSource)TryFindResource(selectedIcon);
            }
            else if (selectedIndex == 10) {
                image10.Source = (ImageSource)TryFindResource(selectedIcon);
            }

            // Hide icon window
            Dispatcher.Invoke(new Action(() => {
                isIconSelecting = false;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 250;
                animation.To = 0;
                animation.Duration = TimeSpan.FromSeconds(0.2);

                iconSelectBorder.BeginAnimation(HeightProperty, animation);
            }));
        }

        /// <summary>
        /// Sets the variables for the project to allow easy saving.
        /// </summary>
        /// <param name="projectTitle">The project title.</param>
        /// <param name="projectTasks">The project tasks array.</param>
        /// <param name="projectTasksData">The project's task data array.</param>
        /// <param name="projectTasksIdentifier">The projet's task identification array.</param>
        /// <param name="projectLinesOfCodeFiles">The project's lines of code files array.</param>
        /// <param name="projectFolderLocation">The project's folder location.</param>
        /// <param name="projectDuration">The project's duration.</param>
        /// <param name="projectDateCreated">The project's creation date.</param>
        /// <param name="projectTasksMade">The project's amount of created tasks.</param>
        /// <param name="projectTasksCompleted">The project's amount of completed tasks.</param>
        /// <param name="projectIcon">The project's icon.</param>
        /// <param name="projectPercent">The project's percent complete.</param>
        private void SetProjectValues(string projectTitle, string[] projectTasks,
            string[] projectTasksData, string[] projectTasksIdentifier,
            string[] projectLinesOfCodeFiles, string projectFolderLocation,
            string projectDuration, string projectDateCreated, long projectTasksMade,
            long projectTasksCompleted, string projectIcon, string projectPercent) {
            title = projectTitle;

            tasks.Clear();
            foreach (string task in projectTasks) {
                tasks.Add(task);
            }
            taskData.Clear();
            foreach (string data in projectTasksData) {
                taskData.Add(data);
            }
            taskIdentifier.Clear();
            foreach (string identifier in projectTasksIdentifier) {
                taskIdentifier.Add(identifier);
            }

            linesOfCodeFiles.Clear();
            foreach (string file in projectLinesOfCodeFiles) {
                linesOfCodeFiles.Add(file);
            }

            folderLocation = projectFolderLocation;
            duration = projectDuration;
            dateCreated = projectDateCreated;
            tasksMade = projectTasksMade;
            tasksCompleted = projectTasksCompleted;
            icon = projectIcon;
            percent = projectPercent;
        }

        /// <summary>
        /// Highlights the selected project based on selectedIndex.
        /// </summary>
        private void SetSelectedProject() {
            if (selectedIndex != 0 && isSettingsWindowDisplaying) {
                isSettingsWindowDisplaying = false;
            }

            settingsGrid.Visibility = Visibility.Hidden;
            statisticsGrid.Visibility = Visibility.Hidden;

            border1.Style = (Style)TryFindResource("hoverOver");
            border2.Style = (Style)TryFindResource("hoverOver");
            border3.Style = (Style)TryFindResource("hoverOver");
            border4.Style = (Style)TryFindResource("hoverOver");
            border5.Style = (Style)TryFindResource("hoverOver");
            border6.Style = (Style)TryFindResource("hoverOver");
            border7.Style = (Style)TryFindResource("hoverOver");
            border8.Style = (Style)TryFindResource("hoverOver");
            border9.Style = (Style)TryFindResource("hoverOver");
            border10.Style = (Style)TryFindResource("hoverOver");

            scrollviewerGrid.Children.Clear();
            itemsAdded = 0;

            if (selectedIndex == 1) {
                border1.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 2) {
                border2.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 3) {
                border3.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 4) {
                border4.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 5) {
                border5.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 6) {
                border6.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 7) {
                border7.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 8) {
                border8.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 9) {
                border9.Style = (Style)TryFindResource("solidOver");
            }
            else if (selectedIndex == 10) {
                border10.Style = (Style)TryFindResource("solidOver");
            }
            else { // selectedIndex isn't a valid number
                selectedIndex = 0;
            }

            IO.SaveSettings(selectedIndex, isCompletedTasksShown);

            // Read the values from this project
            if (selectedIndex != 0) {
                noProjectsGrid.Visibility = Visibility.Hidden;
                displayingImage.Visibility = Visibility.Visible;
                displayingTitle.Visibility = Visibility.Visible;
                scrollviewerGrid.Visibility = Visibility.Visible;
                completeGrid.Visibility = Visibility.Visible;
                addItemBorder.Visibility = Visibility.Visible;
                settingsImage.Visibility = Visibility.Visible;
                folderImage.Visibility = Visibility.Visible;

                string json = IO.ReadEncryptedFile(filesRead[selectedIndex - 1]);
                MainTableManifest.Rootobject projectInfo =
                    JsonConvert.DeserializeObject<MainTableManifest.Rootobject>(json);

                SetProjectValues(projectInfo.Title, projectInfo.Tasks, projectInfo.TaskData,
                    projectInfo.TaskIdentifier, projectInfo.LinesOfCodeFiles,
                    projectInfo.FolderLocation, projectInfo.Duration, projectInfo.DateCreated,
                    projectInfo.TasksMade, projectInfo.TasksCompleted, projectInfo.Icon,
                    projectInfo.Percent);

                // Set values
                if (projectInfo.Title.Length > 25) {
                    displayingTitle.Content = projectInfo.Title.Substring(0, 22) + "...";
                }
                else {
                    displayingTitle.Content = projectInfo.Title;
                }

                if (projectInfo.Icon == "rustIcon") {
                    // Our default rust icon is white so we need to use the
                    // black one for the display image
                    displayingImage.Source = (ImageSource)TryFindResource("blackRustIcon");
                }
                else if (projectInfo.Icon == "noIcon") {
                    displayingImage.Source = (ImageSource)TryFindResource("checked_checkedboxDrawingImage");
                }
                else {
                    displayingImage.Source = (ImageSource)TryFindResource(projectInfo.Icon);
                }

                itemIndex = 0;
                for (int i = 0; i < projectInfo.Tasks.Length; i++) {
                    itemIndex++;
                    if (projectInfo.TaskData[i] == "0" && !isCompletedTasksShown) { // It's not a completed task
                        LoadValues(projectInfo.Tasks[i], Int32.Parse(taskIdentifier[i]));
                    }
                    else if (projectInfo.TaskData[i] == "1" && isCompletedTasksShown) {
                        LoadValues(projectInfo.Tasks[i], Int32.Parse(taskIdentifier[i]));
                    }
                }

                scrollviewerGrid.Width = this.Width - 450;

                for (int i = 0; i < scrollviewerGrid.Children.Count; i++) {
                    Grid grid = (Grid)scrollviewerGrid.Children[i];

                    grid.ColumnDefinitions.RemoveRange(0, 3);

                    ColumnDefinition column1 = new ColumnDefinition();
                    column1.Width = new GridLength(50, GridUnitType.Pixel);
                    ColumnDefinition column2 = new ColumnDefinition();
                    column2.Width = new GridLength(this.Width - 580, GridUnitType.Pixel);
                    ColumnDefinition column3 = new ColumnDefinition();
                    column3.Width = new GridLength(50, GridUnitType.Pixel);

                    grid.ColumnDefinitions.Add(column1);
                    grid.ColumnDefinitions.Add(column2);
                    grid.ColumnDefinitions.Add(column3);
                }

                if (itemsAdded == 0 && !isCompletedTasksShown) {
                    errorCanvas.Margin = new Thickness(0, 0, 120, 20);
                    featureCanvas.Margin = new Thickness(0, 0, 0, 20);
                    commentCanvas.Margin = new Thickness(0, 0, -120, 20);

                    noProjectsGrid.Visibility = Visibility.Visible;
                    secondLineLabel.Visibility = Visibility.Visible;
                    thirdLineLabel.Visibility = Visibility.Visible;
                    fourthLineLabel.Visibility = Visibility.Visible;

                    firstLineLabel.Visibility = Visibility.Hidden;
                    secondLineLabel.Content = "Everything is finished!";
                    thirdLineLabel.Content = "You don't have any";
                    fourthLineLabel.Content = "tasks for this project.";
                    fifthLineLabel.Visibility = Visibility.Hidden;
                }
                else if (itemsAdded == 0 && isCompletedTasksShown) {
                    errorCanvas.Margin = new Thickness(0, 0, 120, 20);
                    featureCanvas.Margin = new Thickness(0, 0, 0, 20);
                    commentCanvas.Margin = new Thickness(0, 0, -120, 20);

                    noProjectsGrid.Visibility = Visibility.Visible;
                    secondLineLabel.Visibility = Visibility.Visible;
                    thirdLineLabel.Visibility = Visibility.Visible;
                    fourthLineLabel.Visibility = Visibility.Visible;

                    firstLineLabel.Visibility = Visibility.Hidden;
                    secondLineLabel.Content = "There's nothing to see here!";
                    thirdLineLabel.Content = "You haven't completed";
                    fourthLineLabel.Content = "any tasks yet.";
                    fifthLineLabel.Visibility = Visibility.Hidden;
                }
            }
            else if (!isSettingsWindowDisplaying) { // There's no projects
                                                    // Hide all items
                displayingTitle.Visibility = Visibility.Hidden;
                displayingImage.Visibility = Visibility.Hidden;
                settingsImage.Visibility = Visibility.Hidden;
                addItemBorder.Visibility = Visibility.Hidden;
                scrollviewerGrid.Visibility = Visibility.Hidden;
                completeGrid.Visibility = Visibility.Hidden;

                // Show new items
                noProjectsGrid.Visibility = Visibility.Visible;
                firstLineLabel.Visibility = Visibility.Visible;
                secondLineLabel.Visibility = Visibility.Visible;
                thirdLineLabel.Visibility = Visibility.Visible;
                fourthLineLabel.Visibility = Visibility.Visible;
                fifthLineLabel.Visibility = Visibility.Visible;

                errorCanvas.Margin = new Thickness(0, 0, 120, 140);
                featureCanvas.Margin = new Thickness(0, 0, 0, 140);
                commentCanvas.Margin = new Thickness(0, 0, -120, 140);

                firstLineLabel.Content = "You haven't started a project yet.";
                secondLineLabel.Content = "Create your first one!";
                thirdLineLabel.Content = "With the Project Tracker, easily";
                fourthLineLabel.Content = "create, manage, track, and develop";
                fifthLineLabel.Content = "all of your programming projects.";
            }
        }

        private void settingsBorder_LostFocus(object sender, RoutedEventArgs e) {
            if (isSettingsOpen) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    settingsBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        private void SettingsButtonMouseDown(object sender, MouseButtonEventArgs e) {
            Window_MouseDown(sender, e);

            // Hide everything in the window
            noProjectsGrid.Visibility = Visibility.Hidden;
            settingsImage.Visibility = Visibility.Hidden;
            changeTitleBorder.Visibility = Visibility.Hidden;
            addItemBorder.Visibility = Visibility.Hidden;
            scrollviewerGrid.Visibility = Visibility.Hidden;
            completeGrid.Visibility = Visibility.Hidden;

            // Show everything
            displayingImage.Visibility = Visibility.Visible;
            displayingTitle.Visibility = Visibility.Visible;

            // Set values
            displayingImage.Source = (ImageSource)TryFindResource("settingsDrawingImage");
            displayingTitle.Content = "Settings";
            currentVersionLabel.Content = "Installed version: " + Globals.CURRENT_VERSION;

            isSettingsWindowDisplaying = true;
            selectedIndex = 0;
            SetSelectedProject();
            settingsGrid.Visibility = Visibility.Visible;
            folderImage.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// When the user clicks on the project's settings button.
        /// </summary>
        private void settingsImage_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (!isSettingsOpen) { // Display icon selector window
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        isSettingsOpen = true;

                        settingsBorder.Visibility = Visibility.Visible;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = 0;
                        animation.To = 210;
                        animation.Duration = TimeSpan.FromSeconds(0.2);

                        settingsBorder.BeginAnimation(HeightProperty, animation);
                    }));
                });
                thread.Start();
            }
            else if (isSettingsOpen) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    settingsBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        /// <summary>
        /// Startup function that runs when code execution starts.
        /// </summary>
        private void Startup() {
            if (!Directory.Exists(Globals.APPDATA_DIRECTORY)) { // Create AppData directory
                Directory.CreateDirectory(Globals.APPDATA_DIRECTORY);
            }
            if (!Directory.Exists(Globals.DATA_DIRECTORY)) { // Create AppData/Data directory
                Directory.CreateDirectory(Globals.DATA_DIRECTORY);
            }

            //backgroundThread = new Thread(BackgroundProcesses.DataReporting);
            //backgroundThread.Start();

            if (!File.Exists(Globals.SETTINGS_FILE)) { // Settings file doesn't exist. Create it
                IO.SaveSettings(1, false);
            }

            string json = IO.ReadEncryptedFile(Globals.SETTINGS_FILE);
            SettingsManifest.Rootobject settings =
                JsonConvert.DeserializeObject<SettingsManifest.Rootobject>(json);

            if (settings.LastSelectedIndex != null) {
                try {
                    selectedIndex = settings.LastSelectedIndex;
                }
                catch (FormatException) { // The index isn't a number for some reason, someone probably tampered with the file
                    selectedIndex = 1;
                }
            }
            if (settings.DisplayingCompleted != null) {
                if (settings.DisplayingCompleted == true) {
                    isCompletedTasksShown = true;
                    completedTaskSwitchLabel.Content = "Completed tasks";
                }
            }

            if (Globals.IS_BETA) {
                versionLabel.Content = "Version " + Globals.CURRENT_VERSION + " (beta)";
            }
            else {
                versionLabel.Content = "Version " + Globals.CURRENT_VERSION;
            }

            LoadFiles();
            SetSelectedProject();

            if (File.Exists(Globals.VERSION_INFO)) {
                File.Delete(Globals.VERSION_INFO);
            }
            if (File.Exists(Globals.NEXT_VERSION_INFO)) {
                File.Delete(Globals.NEXT_VERSION_INFO);
            }

            Thread thread1 = new Thread(() => {
                Dispatcher.Invoke(new Action(() => {
                    // Download latest version information
                    try {
                        WebClient client = new WebClient();
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(Update);
                        client.DownloadFileAsync(new Uri(Globals.VERSION_MANIFEST_URL), Globals.VERSION_INFO);
                    }
                    catch (WebException) {
                        // Couldn't download update file. Possible their wifi isn't working
                    }
                }));
            });
            thread1.Start();
        }

        /// <summary>
        /// A function that displays the completed vs incompleted tasks.
        /// </summary>
        private void SwitchCategory(object sender, MouseButtonEventArgs e) {
            // ANIMATION
            if (!isSwitchingAnimationRunning) {
                isSwitchingAnimationRunning = true;
                Dispatcher.Invoke(new Action(() => {
                    ThicknessAnimation animation = new ThicknessAnimation();

                    animation.From = new Thickness(0, 0, 0, 0);
                    animation.To = new Thickness(50, 0, 0, 0);
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    switchButtonLabel.BeginAnimation(MarginProperty, animation);
                }));

                Thread thread = new Thread(() => {
                    Thread.Sleep(200);
                    Dispatcher.Invoke(new Action(() => {
                        switchButtonLabel.Margin = new Thickness(-50, 0, 0, 0);
                        ThicknessAnimation animation = new ThicknessAnimation();

                        animation.From = new Thickness(-50, 0, 0, 0);
                        animation.To = new Thickness(0, 0, 0, 0);
                        animation.Duration = TimeSpan.FromSeconds(0.2);

                        switchButtonLabel.BeginAnimation(MarginProperty, animation);
                        isSwitchingAnimationRunning = false;
                    }));
                });
                thread.Start();
            }

            if (!isCompletedTasksShown) {
                isCompletedTasksShown = true;
                completedTaskSwitchLabel.Content = "Completed tasks";
            }
            else {
                isCompletedTasksShown = false;
                completedTaskSwitchLabel.Content = "Incomplete tasks";
            }

            SetSelectedProject(); // We use this to reset the scrollviewer grid
            IO.SaveSettings(selectedIndex, isCompletedTasksShown);
        }

        /// <summary>
        /// When the user clicks on the type image to change the type.
        /// </summary>
        private void TypeImagePressed(object sender, MouseButtonEventArgs e) {
            if (!isTypeSelecting) { // Display icon selector window
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        isTypeSelecting = true;

                        itemTypeSelectBorder.Visibility = Visibility.Visible;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = 0;
                        animation.To = 210;
                        animation.Duration = TimeSpan.FromSeconds(0.2);

                        itemTypeSelectBorder.BeginAnimation(HeightProperty, animation);
                    }));
                });
                thread.Start();
            }
            else if (isTypeSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isTypeSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    itemTypeSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        /// <summary>
        /// Runs when the update file has been downloaded.
        /// Notifies the user of an update if one is available.
        /// </summary>
        private void Update(object sender, AsyncCompletedEventArgs e) {
            // This File.ReadAllText() is FINE because version info isn't encrypted
            string json = File.ReadAllText(Globals.VERSION_INFO);

            if (json == "" && isSettingsWindowDisplaying) { // Didn't download the entire file properly
                System.Windows.Forms.MessageBox.Show("Please connect to the Internet to check for an update.",
                    "No Internet Connection",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                updateResponse = true;
                latestVersionLabel.Content = "Latest version: unavailable";
                return;
            }
            else if (json == "") {
                latestVersionLabel.Content = "Latest version: unavailable";
                updateResponse = true;
                return;
            }

            UpdateManifest.Rootobject update =
                JsonConvert.DeserializeObject<UpdateManifest.Rootobject>(json);

            latestVersionLabel.Visibility = Visibility.Visible;
            latestVersionLabel.Content = "Latest version: " + update.Version;

            if (float.Parse(Globals.CURRENT_VERSION) < float.Parse(update.Version)) { // Update is available
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        updateGrid.Visibility = Visibility.Visible;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = 0;
                        animation.To = 130;
                        animation.Duration = TimeSpan.FromSeconds(0.2);

                        updateGrid.BeginAnimation(HeightProperty, animation);
                    }));
                });
                thread.Start();
                updateResponse = true;
            }
            else if (isSettingsWindowDisplaying) {
                System.Windows.Forms.MessageBox.Show("You're running the latest version of the Project Tracker!",
                    "No New Update",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
                updateResponse = true;
            }
        }

        private void UpdateButtonMouseDown(object sender, MouseButtonEventArgs e) {
            updateButton1.IsEnabled = false;

            if (File.Exists(Globals.VERSION_INFO)) {
                File.Delete(Globals.VERSION_INFO);
            }
            if (File.Exists(Globals.NEXT_VERSION_INFO)) {
                File.Delete(Globals.NEXT_VERSION_INFO);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            DisableUpdateButton();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            // Download latest version information
            try {
                updateButton1.IsEnabled = false;
                updateResponse = false;

                WebClient updateClient = new WebClient();
                updateClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Update);
                updateClient.DownloadFileAsync(new Uri(Globals.VERSION_MANIFEST_URL), Globals.VERSION_INFO);
            }
            catch (WebException) {
                // Couldn't download update file. Possible their wifi isn't working
                updateResponse = true;
            }
        }

        /// <summary>
        /// Runs when the user presses the update button when an update is available.
        /// </summary>
        private void UpdateButtonPressed(object sender, MouseButtonEventArgs e) {
            Thread thread = new Thread(() => {
                Dispatcher.Invoke(new Action(() => {
                    updateGrid.Visibility = Visibility.Visible;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 130;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    updateGrid.BeginAnimation(HeightProperty, animation);
                }));
            });
            thread.Start();

            UpdateWindow updateWindow = new UpdateWindow();
            updateWindow.Show();
        }

        /// <summary>
        /// Closes threads and shuts down the program.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e) {
            //backgroundThread.Abort();
            //IO.DuplicateDecrypted(@"C:\Users\skyec\AppData\Local\Project Tracker\data\Well hello there!.pt");
        }

        /// <summary>
        /// When the user clicks their mouse in the window.
        /// Generally used to hide borders.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (isIconSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isIconSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 250;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    iconSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }

            if (isTypeSelecting) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isTypeSelecting = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    itemTypeSelectBorder.BeginAnimation(HeightProperty, animation);
                }));
            }

            if (isSettingsOpen) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 210;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    settingsBorder.BeginAnimation(HeightProperty, animation);
                }));
            }

            if (addItemTextBox.Text == "") { // Clear the textbox if they click out
                addItemTextBox.Text = "Add something to the project";
                Keyboard.ClearFocus();
            }

            if (addProjectTextBox.Text == "") { // Clear the textbox if they click out
                addProjectTextBox.Text = "Create a new project";
                Keyboard.ClearFocus();
            }

            if (isChangingTitle) {
                renameProjectClicks++;
                if (renameProjectClicks == 2) {
                    isChangingTitle = false;

                    changeTitleTextBox.Text = title;

                    changeTitleBorder.Visibility = Visibility.Hidden;
                    displayingTitle.Visibility = Visibility.Visible;
                }
            }

            if (isOverallSettingsOpen) { // Hide the icon selector window if they click out
                Dispatcher.Invoke(new Action(() => {
                    isOverallSettingsOpen = false;

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 70;
                    animation.To = 0;
                    animation.Duration = TimeSpan.FromSeconds(0.2);

                    overallSettingsBorder.BeginAnimation(HeightProperty, animation);
                }));
            }
        }

        /// <summary>
        /// When the user resizes the window.
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            blackRectangle.Height = this.Height + 5;
            addItemBorder.Width = this.Width - 450;
            changeTitleBorder.Width = this.Width - 700;
            folderLocationLabel.Width = this.Width - 420;

            scrollviewerGrid.Width = this.Width - 450;

            for (int i = 0; i < scrollviewerGrid.Children.Count; i++) {
                Grid grid = (Grid)scrollviewerGrid.Children[i];

                grid.ColumnDefinitions.RemoveRange(0, 3);

                ColumnDefinition column1 = new ColumnDefinition();
                column1.Width = new GridLength(50, GridUnitType.Pixel);
                ColumnDefinition column2 = new ColumnDefinition();
                column2.Width = new GridLength(this.Width - 580, GridUnitType.Pixel);
                ColumnDefinition column3 = new ColumnDefinition();
                column3.Width = new GridLength(50, GridUnitType.Pixel);

                grid.ColumnDefinitions.Add(column1);
                grid.ColumnDefinitions.Add(column2);
                grid.ColumnDefinitions.Add(column3);
            }
        }

        #region Border onclicks

        private void border1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 1;
            SetSelectedProject();
        }

        private void border10_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 10;
            SetSelectedProject();
        }

        private void border2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 2;
            SetSelectedProject();
        }

        private void border3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 3;
            SetSelectedProject();
        }

        private void border4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 4;
            SetSelectedProject();
        }

        private void border5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 5;
            SetSelectedProject();
        }

        private void border6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 6;
            SetSelectedProject();
        }

        private void border7_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 7;
            SetSelectedProject();
        }

        private void border8_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 8;
            SetSelectedProject();
        }

        private void border9_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            selectedIndex = 9;
            SetSelectedProject();
        }

        #endregion Border onclicks

        #region Icon clicks

        private void checkboxIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("checked_checkedboxDrawingImage");
        }

        private void cIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("cIcon");
        }

        private void cplusplusIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("cplusplusIcon");
        }

        private void csharpIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("csharpIcon");
        }

        private void dartIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("dartIcon");
        }

        private void githubIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("githubDrawingImage");
        }

        private void goIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("goIcon");
        }

        private void groovyIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("groovyIcon");
        }

        private void htmlIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("htmlIcon");
        }

        private void javaIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("javaIcon");
        }

        private void javascriptIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("javascriptIcon");
        }

        private void kotlinIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("kotlinIcon");
        }

        private void objectiveCIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("objective_cIcon");
        }

        private void perlIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("perlIcon");
        }

        private void phpIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("phpIcon");
        }

        private void pythonIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("pythonIcon");
        }

        private void rIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("rIcon");
        }

        private void rubyIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("rubyIcon");
        }

        private void rustIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("rustIcon");
        }

        private void sqlIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("sqlIcon");
        }

        private void swiftIconMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("swiftIcon");
        }

        private void visualBasicMouseDown(object sender, MouseButtonEventArgs e) {
            SetProjectIcon("visual_basicIcon");
        }

        #endregion Icon clicks

        #region Item selection presses

        /// <summary>
        /// When the user selects the comment type.
        /// </summary>
        private void CommentItemPressed(object sender, MouseButtonEventArgs e) {
            addingTypeImage.Source = (ImageSource)TryFindResource("commentDrawingImage");
            addingType = 2;
        }

        /// <summary>
        /// When the user selects the error type.
        /// </summary>
        private void ErrorItemPressed(object sender, MouseButtonEventArgs e) {
            addingTypeImage.Source = (ImageSource)TryFindResource("errorDrawingImage");
            addingType = 0;
        }

        /// <summary>
        /// When the user selects the feature type.
        /// </summary>
        private void FeatureItemPressed(object sender, MouseButtonEventArgs e) {
            addingTypeImage.Source = (ImageSource)TryFindResource("featureDrawingImage");
            addingType = 1;
        }

        #endregion Item selection presses

        private void ImportProjectsMenuItem_Click(object sender, RoutedEventArgs e) {

        }

        private void ExportProjectsMenuItem_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Docs folder
            dialog.Title = "Export project tracker data";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".ptex";
            dialog.FileName = "export";
            dialog.Filter = "Export files|*.ptex";

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) { // The user picked a file
                string filePicked = dialog.FileName;
            }

        }
    }
}