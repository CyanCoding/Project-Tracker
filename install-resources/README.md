# Install-resources information
This folder is directly accessed by the Project Tracker program and its installer.
Here's an easy list of what each program accesses to keep in mind **before** you
change anything.

### Project Tracker
* `version.json` - to check for a new version of the program and see release information.

### Project Tracker Installer
* `version.txt` - Pull the latest version without needing the JSON module libraries.
* `Project Tracker.zip` - The zip file with all of the components for the program.

The unzipped folder is what's inside the `Project Tracker.zip` file.
## Updating procedure:
When updating, this is the procedure you need to follow:

1. Double check you've changed the version in `MainWindow.xaml.cs`.
2. Build the release `.exe` program.
3. Copy all files from the release folder to the Project Tracker folder here.
4. Zip the folder. Make sure you zip the contents and not the folder so that it ends up as Project Tracker/ and not Project Tracker/Project Tracker.
5. Update `version.json`.
6. Change the `version.txt` version.
7. Change `next-version.json`.

After that the files are ready to be installed on the user's system and you can create a 
new release!
