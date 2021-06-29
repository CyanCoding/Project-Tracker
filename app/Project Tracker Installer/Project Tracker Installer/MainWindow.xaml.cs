using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows;

namespace Project_Tracker_Installer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private bool finishedDownloading = false;
        private bool uninstall = false; // This is true if the program is already installed on the device
        private bool retrying = false; // Used to determine whether the install button is actually being used as a retry button

        private int oldMegabyteSize = 0; // Used for logging purpose on the download. We only want to log when the MB changes, since bytes change like a million times.

        private readonly string PROGRAM_TITLE = "Project Tracker";
        private string PROGRAM_VERSION = "2.4";

        private readonly string PROGRAM_PATH = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install/Project Tracker.exe";

        private readonly string VERSION_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/version.txt";
        private readonly string VERSION_DOWNLOAD_LINK = "https://raw.githubusercontent.com/CyanCoding/Project-Tracker/master/install-resources/version-info/version.txt";

        private readonly string SETTINGS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker\settings.json";

        private readonly string DATA_DIRECTORY_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker\data";

        private readonly string BASE_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Project Tracker";

        private readonly string INSTALLER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install/Project Tracker Installer.exe";
        private readonly string INSTALL_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install";

        private readonly string REGISTRY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Project Tracker";
        private readonly string ONLINE_PROGRAM_LINK = "https://github.com/CyanCoding/Project-Tracker/raw/master/install-resources/Project%20Tracker.zip";
        private readonly string ZIP_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install/Project Tracker.zip";
        private readonly string ICON_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install/logo.ico";
        private readonly string SHORTCUT_LOCATION = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Project Tracker.lnk";

        private readonly string LOG_PATH = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData) + "/Project Tracker/install-log.txt";

        public MainWindow() {
            InitializeComponent();
            startup();
        }

        private void LogData(string data) {
            File.AppendAllText(LOG_PATH, data + "\n");
        }

        /// <summary>
        /// Retrieves the latest available version from the server.
        /// </summary>
        private void GetVersionFromDownload(object sender, AsyncCompletedEventArgs e) {
            try {
                PROGRAM_VERSION = File.ReadAllText(VERSION_PATH);
            }
            catch (Exception eg) {
                LogData("[Main]: FAILED TO READ VERSION FILE");
                return;
            }

            File.Delete(VERSION_PATH);
            Dispatcher.Invoke(new Action(() => {
                subTitle.Content = "Version: " + PROGRAM_VERSION;
            }));

            LogData("[Main]: Received version info: " + PROGRAM_VERSION);
        }

        /// <summary>
        /// Attempts to contact the server to retrieve the version file.
        /// </summary>
        private void GetOnlineVersion() {
            LogData("[Main]: Getting online version...");
            try {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(GetVersionFromDownload);
                client.DownloadFileAsync(new Uri(VERSION_DOWNLOAD_LINK), VERSION_PATH);
            }
            catch (WebException) {
                LogData("[Main]: FAILED TO RECEIVE ONLINE VERSION");
                subTitle.Visibility = Visibility.Hidden; // Couldn't get version
            }
        }

        /// <summary>
        /// Startup method, runs when code execution begins.
        /// </summary>
        private void startup() {
            Directory.CreateDirectory(INSTALL_DIRECTORY);
            LogData("[Main]: Started the installer");
            subTitle.Content = "Version: " + PROGRAM_VERSION;

            Process process = Process.GetCurrentProcess();
            string currentLocation = process.MainModule.FileName;

            // When the user first opens the program, we duplicate this program
            // to the install directory and run that program.
            // The user's program ends execution here.
            if (currentLocation != INSTALLER_PATH) {
                try {
                    // Create the install directories
                    Directory.CreateDirectory(INSTALL_DIRECTORY);
                    LogData("[Main]: Created the install directory");

                    // Copy the installer
                    File.Copy(currentLocation, INSTALLER_PATH);
                    LogData("[Main]: Copied the installer to the install location");

                    // Start the new program, since we need to install from
                    // the uninstall location.
                    ProcessStartInfo start = new ProcessStartInfo {
                        FileName = INSTALLER_PATH,
                        WindowStyle = ProcessWindowStyle.Normal,
                        CreateNoWindow = true // No console window
                    };
                    Process.Start(start);
                    LogData("[Main]: Started the remote installer, closing this one.");

                    this.Close();
                    Environment.Exit(0);
                }
                catch (Exception) {
                    // This occurs if the file already exists, which is a non-issue
                    Console.WriteLine("Encountered an error while copying (304)");
                }
            }

            // ================================================================
            // EVERYTHING FROM HERE BELOW IS RUN IN THE LOCAL INSTALL DIRECTORY
            // ================================================================
            GetOnlineVersion(); // Attempts to retrieve online version or hides subtitle if not available

            if (File.Exists(PROGRAM_PATH)) { // Program exists, so we uninstall
                LogData("[Main]: The program file already exists, so we're uninstalling");
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
        }

        /// <summary>
        /// Launches the program from PROGRAM_PATH.
        /// </summary>
        private void LaunchButtonClick(object sender, RoutedEventArgs e) {
            LogData("[Main]: Launch button pressed. Aborting installer and launching app.");
            ProcessStartInfo start = new ProcessStartInfo {
                FileName = PROGRAM_PATH,
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
            LogData("[Main]: Reinstall button pressed.");
            reinstallButton.Visibility = Visibility.Hidden;
            installButton.Visibility = Visibility.Hidden;
            subTitle.Content = "Installing program...";

            LogData("[Main]: Uninstalling program...");
            Uninstaller uninstaller = new Uninstaller();
            //uninstaller.RequestProgramShutdown(SETTINGS_PATH);
            uninstaller.Uninstall(DATA_DIRECTORY_PATH, INSTALL_DIRECTORY, BASE_DIRECTORY, REGISTRY, SHORTCUT_LOCATION);

            LogData("[Main]: Uninstalled program! Installing...");
            InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);
            LogData("[Main]: Reinstalled program!");

            //try {
            //    Registry.CurrentUser.DeleteSubKey(REGISTRY);
            //    LogData("[Main]: Deleted the old registry");
            //}
            //catch (ArgumentException) { // Registry doesn't exist
            //    LogData("[Main]: ERROR: Tried to remove a registry that doesn't exist!");
            //}
        }

        /// <summary>
        /// Runs when the user clicks on the install button.
        /// If the program is already installed, it becomes an uninstall button.
        /// </summary>
        private void InstallButtonClick(object sender, RoutedEventArgs e) {
            LogData("[Main]: Install/uninstall button pressed");
            if (retrying == true) {
                installButton.Content = "Install";
                retrying = false;
                subTitle.Content = "";
            }

            if (!uninstall) { // We're installing the program normally
                // Hide/show components
                copyright.Visibility = Visibility.Hidden;
                installButton.Visibility = Visibility.Hidden;
                subTitle.Visibility = Visibility.Visible;

                mainTitle.Content = "Installing program...";
                LogData("[Main]: Installing program...");
                InstallProgram(PROGRAM_PATH, INSTALL_DIRECTORY, ONLINE_PROGRAM_LINK, ZIP_PATH);
                LogData("[Main]: Finished installing!");
            }
            else { // Uninstall == true
                Dispatcher.Invoke(new Action(() => {
                    installButton.Visibility = Visibility.Hidden;
                    reinstallButton.Visibility = Visibility.Hidden;
                    subTitle.Visibility = Visibility.Hidden;
                    mainTitle.Content = "Uninstalling";
                    installBar.Visibility = Visibility.Hidden;

                    LogData("[Main]: Uninstalling program...");
                    Uninstaller uninstaller = new Uninstaller();
                    //uninstaller.RequestProgramShutdown(SETTINGS_PATH);
                    uninstaller.Uninstall(DATA_DIRECTORY_PATH, INSTALL_DIRECTORY, BASE_DIRECTORY, REGISTRY, SHORTCUT_LOCATION);

                    LogData("[Main]: Successfully uninstalled!");
                    FinalResult("Successfully uninstalled!", "You can close this uninstaller whenever.");
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
                LogData("[Main]: Downloading ZIP program file.");
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(CurrentlyDownloading);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadComplete);
                client.DownloadFileAsync(new Uri(ONLINE_PROGRAM_LINK), ZIP_PATH);

                subTitle.Content = "Starting download...";
            }
            catch (WebException) {
                LogData("[Main]: FAILED to download ZIP program file (most likely an internet issue)");
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

            // Only update log every megabyte
            if (Double.Parse(stringBytesReceived) - 1 > oldMegabyteSize && suffixA == "mb") {
                LogData("[Main]: Downloading: " + stringBytesReceived + suffixA + " / " + stringTotalBytes + suffixB);
                oldMegabyteSize = (int)Double.Parse(stringBytesReceived);
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
                LogData("[Main]: Finished downloading!");
                // We create a cool thread to extract the program and unzip file contents
                Thread thread = new Thread(() => {
                    Dispatcher.Invoke(new Action(() => {
                        LogData("[Main]: Beginning installation...");
                        mainTitle.Content = "Installing program...";
                        subTitle.Content = "";

                        installBar.Visibility = Visibility.Visible;
                        installBar.IsIndeterminate = true;

                        LogData("[Main]: Unzipping ZIP file");
                        ZipFile.ExtractToDirectory(ZIP_PATH, INSTALL_DIRECTORY);
                        File.Delete(ZIP_PATH);
                        LogData("[Main]: Unzipped ZIP file. Installing...");

                        Installer installer = new Installer();
                        installer.CreateUninstaller(REGISTRY, PROGRAM_TITLE, PROGRAM_VERSION, ICON_PATH, PROGRAM_PATH, INSTALL_DIRECTORY, SHORTCUT_LOCATION);
                        LogData("[Main]: Successfully installed!");

                        if (File.Exists(ZIP_PATH)) { // An issue occurred while unzipping
                            LogData("[Main]: FAILED while unzipping!");
                            FinalResult("Couldn't install at this time", "Please close every " + PROGRAM_TITLE + " Installer before continuing.");
                            mainTitle.Content = "Couldn't install at this time";
                            subTitle.Content = "Please close every " + PROGRAM_TITLE + " Installer before continuing.";
                            subTitle.Visibility = Visibility.Visible;

                            retrying = true;
                            installButton.Visibility = Visibility.Visible;
                            installButton.Margin = new Thickness(0, 0, 0, 30);
                            installButton.Content = "Retry";

                            installBar.Visibility = Visibility.Hidden;
                            return;
                        }

                        FinalResult("Project Tracker has been installed!", "You can close this installer whenever.");
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
        public void FinalResult(string main, string subtitle) {
            LogData("[Main]: Installer has finished.");
            Dispatcher.Invoke(new Action(() => {
                installBar.Visibility = Visibility.Hidden;
                subTitle.Content = subtitle;
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
                else if (main == "Project Tracker has been installed!") {
                    launchButton.Visibility = Visibility.Visible;
                }
            }));
        }
    }
}