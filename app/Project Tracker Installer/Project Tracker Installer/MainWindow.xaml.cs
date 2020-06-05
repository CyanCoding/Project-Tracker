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
using System.Windows.Shell;

#pragma warning disable CS0168


namespace Project_Tracker_Installer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        bool uninstall = false; // This is true if the program is already installed on the device
        bool retrying = false; // Used to determine whether the install button is actually being used as a retry button
        bool finishedDownloading = false;
       
        readonly string PROGRAM_TITLE = "Project Tracker";
        string PROGRAM_VERSION = "calculating version...";
        readonly string PROGRAM_PATH = @"C:\Program Files\Project Tracker\Project Tracker.exe";
        readonly string VERSION_PATH = @"C:\Program Files\Project Tracker\version.txt";
        readonly string VERSION_DOWNLOAD_LINK = "https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version.txt";
        readonly string DATA_DIRECTORY_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker\data";
        readonly string INSTALLER_PATH = @"C:\Program Files\Project Tracker\Project Tracker Installer.exe";
        readonly string INSTALL_DIRECTORY = @"C:\Program Files\Project Tracker";
        readonly string VERSION_FILE = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker\update.txt";
        readonly string REGISTRY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Project Tracker";
        readonly string ONLINE_PROGRAM_LINK = "https://github.com/CyanCoding/Project-Tracker/raw/master/install-resources/Project%20Tracker.zip";
        readonly string ZIP_PATH = @"C:\Program Files\Project Tracker\Project Tracker.zip";
        readonly string ICON_PATH = @"C:\Program Files\Project Tracker\logo.ico";
        readonly string SHORTCUT_LOCATION = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Project Tracker.lnk";

        public MainWindow() {
            InitializeComponent();
            startup();
        }

        /// <summary>
        /// Retrieves the latest available version from the server.
        /// </summary>
        private void GetVersion(object sender, AsyncCompletedEventArgs e) {
            try {
                PROGRAM_VERSION = File.ReadAllText(VERSION_PATH);
            }
            catch (Exception eg) {
                return;
            }

            File.Delete(VERSION_PATH);
            Dispatcher.Invoke(new Action(() => {
                if (!uninstall) {
                    if (subTitle.Content.ToString() != "Couldn't install at this time" && subTitle.Content.ToString() != ("Please close every " + PROGRAM_TITLE + " Installer before continuing.")) {
                        subTitle.Content = "Version: " + PROGRAM_VERSION;
                    }
                    
                }
                
            }));
            installButton.IsEnabled = true;
            reinstallButton.IsEnabled = true;

            if (subTitle.Content.ToString()== "Version: ") {
                FinalResult("An error occured while installing");
            }
        }

        /// <summary>
        /// Startup method, runs when code execution begins.
        /// </summary>
        void startup() {
            subTitle.Content = "Version: " + PROGRAM_VERSION;

            // Copies the current file to the Program Data folder. Code execution doesn't pass this if it's not already there
            if ((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Project Tracker Installer.exe") != INSTALLER_PATH) {
                Process process = Process.GetCurrentProcess();
                string currentLocation = process.MainModule.FileName;

                if (!File.Exists(INSTALLER_PATH)) { // An installer file exists
                    try {
                        if (!Directory.Exists(INSTALL_DIRECTORY)) {
                            Directory.CreateDirectory(INSTALL_DIRECTORY);
                        }

                        // Copy this program over to the new location
                        File.Copy(currentLocation, INSTALLER_PATH);
                        
                        File.SetAttributes(INSTALLER_PATH, FileAttributes.Normal);
                    }
                    catch (UnauthorizedAccessException) { // For some reason we can't take the file hmmf
                        FinalResult("Couldn't install at this time (034)");
                        return;
                    }
                }
                else { // No installer file exists
                    try {
                        // Delete older installer and copy this one there
                        File.SetAttributes(INSTALLER_PATH, FileAttributes.Normal);
                        File.Delete(INSTALLER_PATH);
                        File.Copy(currentLocation, INSTALLER_PATH);
                        File.SetAttributes(INSTALLER_PATH, FileAttributes.Normal);
                    }
                    catch (UnauthorizedAccessException) {
                        FinalResult("Couldn't install at this time (042)");
                        return;
                    }
                }
                // Start the new installer
                ProcessStartInfo start = new ProcessStartInfo {
                    // Enter the executable to run, including the complete path
                    FileName = INSTALLER_PATH,
                    // Do you want to show a console window?
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = true
                };
                Process.Start(start);
                this.Close();

                return;

            }
            else { // We're in the right installation location but we need the version first for registry.
                try {
                    installButton.IsEnabled = false;
                    reinstallButton.IsEnabled = false;
                    WebClient client = new WebClient();
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(GetVersion);
                    client.DownloadFileAsync(new Uri(VERSION_DOWNLOAD_LINK), VERSION_PATH);
                }
                catch (WebException) {
                    subTitle.Visibility = Visibility.Hidden; // Couldn't get version
                }
            }

            try {
                installButton.IsEnabled = false;
                reinstallButton.IsEnabled = false;
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(GetVersion);
                client.DownloadFileAsync(new Uri(VERSION_DOWNLOAD_LINK), VERSION_PATH);
            }
            catch (WebException) {
                subTitle.Visibility = Visibility.Hidden; // Couldn't get version
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

         /// <summary>
         /// Launches the program from PROGRAM_PATH.
         /// </summary>
        private void LaunchButtonClick(object sender, RoutedEventArgs e) {
            ProcessStartInfo start = new ProcessStartInfo {
                // Enter the executable to run, including the complete path
                FileName = PROGRAM_PATH,
                // Do you want to show a console window?
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = true
            };
            Process.Start(start);
            Environment.Exit(0);
        }

        /// <summary>
        /// Runs when the user clicks on the reinstall button.
        /// Uninstalls and then installs the program.
        /// </summary>
        private void ReinstallButtonClick(object sender, RoutedEventArgs e) {
            reinstallButton.Visibility = Visibility.Hidden;
            installButton.Visibility = Visibility.Hidden;
            subTitle.Content = "Installing program...";

            Uninstaller uninstaller = new Uninstaller();
            uninstaller.Uninstall(DATA_DIRECTORY_PATH, INSTALLER_PATH, INSTALL_DIRECTORY, REGISTRY, SHORTCUT_LOCATION);

            InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);

            try {
                Registry.CurrentUser.DeleteSubKey(REGISTRY);
            }
            catch (ArgumentException) { // Registry doesn't exist

			}
        }

         /// <summary>
         /// Runs when the user clicks on the install button.
         /// If the program is already installed, it becomes an uninstall button.
         /// </summary>
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

                    mainTitle.Content = "Installing program...";

                    Installer installer = new Installer();
                    InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);         
                }
                else { // Update
                    // Hide/show components
                    copyright.Visibility = Visibility.Hidden;
                    installButton.Visibility = Visibility.Hidden;
                    subTitle.Visibility = Visibility.Visible;

                    mainTitle.Content = "Updating program...";

                    foreach (string f in Directory.GetFiles(INSTALL_DIRECTORY)) { // Delete old files
                        if (f != INSTALLER_PATH) {
                            File.Delete(f);
                        }
                    }

                    InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);
                    try {
                        Registry.CurrentUser.DeleteSubKey(REGISTRY);
                    }
                    catch (ArgumentException) { // Registry doesn't exist

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

        /// <summary>
        /// Installs the latest Project Tracker program to the user's computer.
        /// </summary>
        /// <param name="PROGRAM_PATH">The path of the unzipped program.</param>
        /// <param name="INSTALL_DIRECTORY">The directory to install to.</param>
        /// <param name="ONLINE_PROGRAM_LINK">The URL to the program download.</param>
        /// <param name="ZIP_PATH">The zip path to download to and extract from.</param>
        /// <returns></returns>
        public void InstallProgram(string PROGRAM_PATH, string INSTALL_DIRECTORY, string ONLINE_PROGRAM_LINK, string ZIP_PATH) {
            if (File.Exists(PROGRAM_PATH)) {
                File.Delete(PROGRAM_PATH);
            }
            if (File.Exists(ZIP_PATH)) {
                File.Delete(ZIP_PATH);
            }

            try {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(CurrentlyDownloading);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadComplete);
                client.DownloadFileAsync(new Uri(ONLINE_PROGRAM_LINK), ZIP_PATH);

                subTitle.Content = "Starting download...";
            }
            catch (WebException) {
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
        }

        /// <summary>
        /// Runs while the program is downloading.
        /// </summary>
        private void CurrentlyDownloading(object sender, DownloadProgressChangedEventArgs e) {
            double bytesReceived = e.BytesReceived;
            double totalBytes = e.TotalBytesToReceive;

            string suffixA = "b";
            string suffixB = "b";

            if (bytesReceived > 1024) {
                bytesReceived /= 1024;
                suffixA = "kb";
            }
            if (bytesReceived > 1024) {
                bytesReceived /= 1024;
                suffixA = "mb";
            }
            if (bytesReceived > 1024) {
                bytesReceived /= 1024;
                suffixA = "gb";
            }
            if (bytesReceived > 1024) {
                bytesReceived /= 1024;
                suffixA = "tb";
            }

            if (totalBytes > 1024) {
                totalBytes /= 1024;
                suffixB = "kb";
            }
            if (totalBytes > 1024) {
                totalBytes /= 1024;
                suffixB = "mb";
            }
            if (totalBytes > 1024) { // I'm going to be really worried if my java installer needs either of these last two
                totalBytes /= 1024;
                suffixB = "gb";
            }
            if (totalBytes > 1024) {
                totalBytes /= 1024;
                suffixB = "tb";
            }

            bytesReceived = Math.Round(bytesReceived, 2);
            totalBytes = Math.Round(totalBytes, 2);

            string stringBytesReceived = bytesReceived.ToString();
            string stringTotalBytes = totalBytes.ToString();

            string[] splitBytesReceived = stringBytesReceived.Split('.');
            try {
                if (splitBytesReceived[1].Length == 0) {
                    stringBytesReceived += ".00";
                }
                else if (splitBytesReceived[1].Length == 1) {
                    stringBytesReceived += "0";
                }
            }
            catch (IndexOutOfRangeException eg) {
                stringBytesReceived += ".00";
            }


            string[] splitTotalBytes = stringTotalBytes.Split('.');
            try {
                if (splitTotalBytes[1].Length == 0) {
                    stringTotalBytes += ".00";
                }
                else if (splitTotalBytes[1].Length == 1) {
                    stringTotalBytes += "0";
                }
            }
            catch (IndexOutOfRangeException eg) {
                stringTotalBytes += ".00";
            }

            Dispatcher.Invoke(new Action(() => {
                mainTitle.Content = "Downloading program...";
                installBar.Visibility = Visibility.Visible;
                installBar.IsIndeterminate = false;

                subTitle.Content = stringBytesReceived + suffixA + " / " + stringTotalBytes + suffixB;
                installBar.Minimum = 0;
                installBar.Maximum = e.TotalBytesToReceive;
                installBar.Value = e.BytesReceived;
            }));

            if (e.BytesReceived == e.TotalBytesToReceive) {
                finishedDownloading = true;
			}
        }


        /// <summary>
        /// Runs after the download has finished.
        /// </summary>
        private void DownloadComplete(object sender, AsyncCompletedEventArgs e) {
            if (finishedDownloading) {
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        mainTitle.Content = "Installing program...";
                        subTitle.Content = "";

                        installBar.Visibility = Visibility.Visible;
                        installBar.IsIndeterminate = true;

                        ZipFile.ExtractToDirectory(ZIP_PATH, INSTALL_DIRECTORY);
                        File.Delete(ZIP_PATH);

                        Installer installer = new Installer();
                        installer.CreateUninstaller(REGISTRY, PROGRAM_TITLE, PROGRAM_VERSION, ICON_PATH, PROGRAM_PATH, INSTALL_DIRECTORY, SHORTCUT_LOCATION);


                        if (File.Exists(ZIP_PATH)) {
                            mainTitle.Content = "Couldn't update at this time";
                            subTitle.Content = "Please close every " + PROGRAM_TITLE + " Installer before continuing.";
                            subTitle.Visibility = Visibility.Visible;

                            retrying = true;
                            installButton.Visibility = Visibility.Visible;
                            installButton.Margin = new Thickness(0, 0, 0, 30);
                            installButton.Content = "Retry";

                            installBar.Visibility = Visibility.Hidden;
                        }
                        else {
                            if (mainTitle.Content.ToString() == "Updating program...") {
                                FinalResult("Project Tracker has been updated!");
                            }
                        }

                        FinalResult("Project Tracker has been installed!");
                    }));
                });
                thread.Start();
            }
        }

        /// <summary>
        /// A final display presented to the user after code execution has finished.
        /// Used to display the success or failure of the installation.
        /// </summary>
        /// <param name="main">The main title text.</param>
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
                else if (main == "Couldn't install at this time (034)" || main == "Couldn't install at this time (042)" || main == "Couldn't install at this time (092)") {
                    subTitle.Content = "Please close every " + PROGRAM_TITLE + " Installer before continuing.";
                    installButton.Visibility = Visibility.Hidden;
                }

            }));
        }

        /// <summary>
        /// Runs before window is closed after the user closes the program.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e) {
            Environment.Exit(0);
        }
    }
}
