using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;

#pragma warning disable CS0168


namespace Project_Tracker_Installer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        bool uninstall = false;
        bool retrying = false; // Used to determine whether the install button is actually being used as a retry button

        readonly string PROGRAM_TITLE = "Project Tracker";
        string PROGRAM_VERSION = "calculating version...";
        readonly string PROGRAM_PATH = @"C:\Program Files\Project Tracker\Project Tracker.exe";
        readonly string VERSION_PATH = @"C:\Program Files\Project Tracker\version.txt";
        readonly string VERSION_DOWNLOAD_LINK = "http://cyancoding-server.000webhostapp.com/Project%20Tracker/version.txt";
        readonly string DATA_DIRECTORY_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker\data";
        readonly string INSTALLER_PATH = @"C:\Program Files\Project Tracker\Project Tracker Installer.exe";
        readonly string INSTALL_DIRECTORY = @"C:\Program Files\Project Tracker";
        readonly string VERSION_FILE = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker\update.txt";
        readonly string REGISTRY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Project Tracker";
        readonly string ONLINE_PROGRAM_LINK = "http://cyancoding-server.000webhostapp.com/Project%20Tracker/Project%20Tracker.zip";
        readonly string ZIP_PATH = @"C:\Program Files\Project Tracker\Project Tracker.zip";
        readonly string ICON_PATH = @"C:\Program Files\Project Tracker\logo.ico";
        readonly string SHORTCUT_LOCATION = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Project Tracker.lnk";

        public MainWindow() {
            InitializeComponent();
            startup();
        }

        private void GetVersion(object sender, AsyncCompletedEventArgs e) {
            try {
                PROGRAM_VERSION = File.ReadAllText(VERSION_PATH);
            }
            catch (Exception) {
                FinalResult("Couldn't install at this time");
                return;
            }

            File.Delete(VERSION_PATH);
            Dispatcher.Invoke(new Action(() => {
                subTitle.Content = "Version: " + PROGRAM_VERSION;
            }));
            installButton.IsEnabled = true;
        }

        void startup() {
            subTitle.Content = "Version: " + PROGRAM_VERSION;

            // Copies the current file to the Program Data folder. Code execution doesn't pass this if it's not already there
            if ((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Project Tracker Installer.exe") != INSTALLER_PATH) {
                string currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Project Tracker Installer.exe";

                if (!File.Exists(INSTALLER_PATH)) {
                    try {
                        if (!Directory.Exists(INSTALL_DIRECTORY)) {
                            Directory.CreateDirectory(INSTALL_DIRECTORY);
                        }

                        File.Copy(currentLocation, INSTALLER_PATH);
                        File.SetAttributes(INSTALLER_PATH, FileAttributes.Normal);
                    }
                    catch (UnauthorizedAccessException) { // For some reason we can't take the file hmmf
                        FinalResult("Couldn't install at this time");
                        return;
                    }
                }
                else {
                    try {
                        File.SetAttributes(INSTALLER_PATH, FileAttributes.Normal);
                        File.Delete(INSTALLER_PATH);
                        File.Copy(currentLocation, INSTALLER_PATH);
                        File.SetAttributes(INSTALLER_PATH, FileAttributes.Normal);
                    }
                    catch (UnauthorizedAccessException) {
                        FinalResult("Couldn't install at this time");
                        return;
                    }
                }
                // Start the new installer
                ProcessStartInfo start = new ProcessStartInfo {
                    // Enter the executable to run, including the complete path
                    FileName = INSTALLER_PATH,
                    // Do you want to show a console window?
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(start);
                this.Hide();

                return;

            }

            if (!File.Exists(VERSION_FILE)) { // It's not an update
                subTitle.Content = "Version: " + PROGRAM_VERSION;

                if (File.Exists(PROGRAM_PATH)) { // This if statement decides whether we're installing or uninstalling the program
                    uninstall = true;
                    installButton.Content = "Uninstall";
                    subTitle.Content = "Project Tracker is already installed on your device";
                    subTitle.Visibility = Visibility.Visible;
                    reinstallButton.Visibility = Visibility.Visible;

                    installButton.Margin = new Thickness(152, 0, 0, 30);
                    reinstallButton.Margin = new Thickness(288, 0, 0, 30);
                    installButton.HorizontalAlignment = HorizontalAlignment.Left;
                    reinstallButton.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else { // We're just installing. Keep everything as normal
                    try {
                        installButton.IsEnabled = false;
                        WebClient client = new WebClient();
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(GetVersion);
                        client.DownloadFileAsync(new Uri(VERSION_DOWNLOAD_LINK), VERSION_PATH);
                    }
                    catch (WebException) {
                        subTitle.Visibility = Visibility.Hidden; // Couldn't get version
                    }
                }
            }
            else { // It is an update
                string[] updateLines = File.ReadAllLines(VERSION_FILE);
                PROGRAM_VERSION = updateLines[0];

                subTitle.Content = "Version: " + PROGRAM_VERSION;

                installButton.Content = "Update";
                mainTitle.Content = "Update Project Tracker";

                File.Delete(VERSION_FILE);
            }
        }

        /*
         * When the user clicks to open the Projet Tracker program
         */
        private void LaunchButtonClick(object sender, RoutedEventArgs e) {
            ProcessStartInfo start = new ProcessStartInfo {
                // Enter the executable to run, including the complete path
                FileName = PROGRAM_PATH,
                // Do you want to show a console window?
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            Process.Start(start);
            Environment.Exit(0);
        }


        /* When the user clicks the reinstall button
        * This runs the same install button procedure, just deletes the contents of the program first and sets uninstall to false
        */
        private void ReinstallButtonClick(object sender, RoutedEventArgs e) {
            reinstallButton.Visibility = Visibility.Hidden;
            installButton.Visibility = Visibility.Hidden;
            subTitle.Content = "Installing";

            Uninstaller uninstaller = new Uninstaller();
            uninstaller.Uninstall(DATA_DIRECTORY_PATH, INSTALLER_PATH, INSTALL_DIRECTORY);

            Installer installer = new Installer();

            bool success = installer.InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);

            if (!success) {
                Dispatcher.Invoke(new Action(() => {
                    mainTitle.Content = "Unable to connect to the internet";
                    subTitle.Visibility = Visibility.Hidden;

                    retrying = true;
                    installButton.Visibility = Visibility.Visible;
                    installButton.Margin = new Thickness(0, 0, 0, 30);
                    installButton.Content = "Retry";

                    installBar.Visibility = Visibility.Hidden;
                }));
            }
            else {
                installer.CreateUninstaller(REGISTRY, PROGRAM_TITLE, PROGRAM_VERSION, ICON_PATH, PROGRAM_PATH, INSTALL_DIRECTORY, SHORTCUT_LOCATION);
            }

            FinalResult("Project Tracker has been installed!");
        }
        /*
         * When the user clicks on the install button
         */
        private void InstallButtonClick(object sender, RoutedEventArgs e) {
            if (retrying == true) {
                installButton.Content = "Install";
                retrying = false;
                subTitle.Content = "";
            }

            if (!uninstall) {
                if (installButton.Content.ToString() == "Click to Install") {
                    // Hide/show components
                    copyright.Visibility = Visibility.Hidden;
                    installButton.Visibility = Visibility.Hidden;
                    subTitle.Visibility = Visibility.Visible;
                    installBar.Visibility = Visibility.Visible;

                    mainTitle.Content = "Installing program...";

                    Installer installer = new Installer();
                    bool success = installer.InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);

                    if (!success) {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            mainTitle.Content = "Unable to connect to the internet";
                            subTitle.Visibility = Visibility.Hidden;

                            retrying = true;
                            installButton.Visibility = Visibility.Visible;
                            installButton.Margin = new Thickness(0, 0, 0, 30);
                            installButton.Content = "Retry";

                            installBar.Visibility = Visibility.Hidden;
                        }));
                    }
                    else {
                        installer.CreateUninstaller(REGISTRY, PROGRAM_TITLE, PROGRAM_VERSION, ICON_PATH, PROGRAM_PATH, INSTALL_DIRECTORY, SHORTCUT_LOCATION);
                    }

                    FinalResult("Project Tracker has been installed!");                
                }
                else { // Update
                    // Hide/show components
                    copyright.Visibility = Visibility.Hidden;
                    installButton.Visibility = Visibility.Hidden;
                    subTitle.Visibility = Visibility.Visible;
                    installBar.Visibility = Visibility.Visible;

                    mainTitle.Content = "Updating program...";

                    foreach (string f in Directory.GetFiles(INSTALL_DIRECTORY)) { // Delete old files
                        if (f != INSTALLER_PATH) {
                            File.Delete(f);
                        }
                    }

                    Installer installer = new Installer();
                    bool success = installer.InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);

                    if (!success) {
                        Dispatcher.Invoke(new Action(() => {
                            mainTitle.Content = "Unable to connect to the internet";
                            subTitle.Visibility = Visibility.Hidden;

                            retrying = true;
                            installButton.Visibility = Visibility.Visible;
                            installButton.Margin = new Thickness(0, 0, 0, 30);
                            installButton.Content = "Retry";

                            installBar.Visibility = Visibility.Hidden;
                        }));
                    }
                    else {
                        Registry.CurrentUser.DeleteSubKey(REGISTRY);
                        installer.CreateUninstaller(REGISTRY, PROGRAM_TITLE, PROGRAM_VERSION, ICON_PATH, PROGRAM_PATH, INSTALL_DIRECTORY, SHORTCUT_LOCATION);
                    }

                    if (File.Exists(ZIP_PATH)) {
                        Dispatcher.Invoke(new Action(() => {
                            mainTitle.Content = "Couldn't update at this time";
                            subTitle.Content = "Please close every " + PROGRAM_TITLE + " Installer before continuing.";
                            subTitle.Visibility = Visibility.Visible;

                            retrying = true;
                            installButton.Visibility = Visibility.Visible;
                            installButton.Margin = new Thickness(0, 0, 0, 30);
                            installButton.Content = "Retry";

                            installBar.Visibility = Visibility.Hidden;
                        }));
                    }
                    else {
                        FinalResult("Project Tracker has been updated!");
                    }
                }
            }
            else { // Uninstall == true
                Dispatcher.Invoke(new Action(() => {
                    installButton.Visibility = Visibility.Hidden;
                    reinstallButton.Visibility = Visibility.Hidden;
                    subTitle.Visibility = Visibility.Hidden;
                    mainTitle.Content = "Uninstalling";
                    installBar.Visibility = Visibility.Hidden;

                    Uninstaller uninstaller = new Uninstaller();
                    uninstaller.Uninstall(DATA_DIRECTORY_PATH, INSTALLER_PATH, INSTALL_DIRECTORY, REGISTRY, SHORTCUT_LOCATION);

                    FinalResult("Successfully uninstalled!");
                }));

            }

        }

        public void FinalResult(string main) {
            Dispatcher.Invoke(new Action(() => {
                installBar.Visibility = Visibility.Hidden;
                subTitle.Content = "You may close this at any time.";
                subTitle.Visibility = Visibility.Visible;
                mainTitle.Content = main;
                copyright.Visibility = Visibility.Visible;

                if (main == "An error occured while installing") {
                    subTitle.Content = "Make sure you have a good internet connection and try again.";

                    retrying = true;
                    installButton.Visibility = Visibility.Visible;
                    installButton.Margin = new Thickness(0, 0, 0, 30);
                    installButton.Content = "Retry";

                }
                else if (main == "Project Tracker has been installed!" || main == "Project Tracker has been updated!") {
                    launchButton.Visibility = Visibility.Visible;
                }
                else if (main == "Administer priveleges are required") {
                    subTitle.Content = "Please launch the program again with the necessary priveleges to install.";
                    installButton.Visibility = Visibility.Hidden;
                }
                else if (main == "Couldn't install at this time") {
                    subTitle.Content = "Please close every " + PROGRAM_TITLE + " Installer before continuing.";
                    installButton.Visibility = Visibility.Hidden;
                }

            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            Environment.Exit(0);
        }
    }
}