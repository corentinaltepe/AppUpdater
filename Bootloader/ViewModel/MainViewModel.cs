﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;
using System.Timers;
using System.Reflection;

namespace Bootloader.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Files which should not be overwritten during update
        private static string[] BOOTLOADERFILES = { "Bootloader.exe",
                                                    "Bootloader.Test.dll",
                                                    "Bootloader.pdb",
                                                    "Bootloader.exe.config",
                                                    "Bootloader.vshost.exe",
                                                    "Bootloader.vshost.exe.config",
                                                    "Bootloader.vshost.exe.manifest",
                                                    "Bootloader.Test.pdb"};

        private static int TIMEOUT = 30000;

        #region Properties
        public string Title
        {
            get
            {
                if (currentApp != null && !String.IsNullOrEmpty(currentApp.Name))
                    return "Updating " + CurrentApp.Name;

                return "Application Update";
            }
        }
        private string filename;
        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                OnFilenameChanged();
                OnPropertyChanged("Filename");
            }
        }
        private AppManifest currentApp;
        public AppManifest CurrentApp
        {
            get { return currentApp; }
            set
            {
                currentApp = value;
                OnPropertyChanged("CurrentApp");

                // Notify a change of Title
                if(currentApp != null && !String.IsNullOrEmpty(currentApp.Name))
                    OnPropertyChanged("Title");
            }
        }
        private AppManifest newerApp;
        public AppManifest NewerApp
        {
            get { return newerApp; }
            set
            {
                newerApp = value;
                OnPropertyChanged("NewerApp");
            }
        }

        // Not triggering events because not necessary
        public Process callingProcess;
        public Process CallingProcess
        {
            get { return callingProcess; }
            set
            {
                callingProcess = value;

                // Wait for the calling process to be killed
                if (callingProcess != null)
                {
                    callingProcess.EnableRaisingEvents = true;
                    //callingProcess.Disposed += CallingProcess_Exited;
                    callingProcess.Exited += CallingProcess_Exited;
                }

                IsProcessGiven = true;
            }
        }
        public bool IsProcessGiven { get; set; }

        // The stopwatch is used to kill the bootloader if the update has not been
        // initiated within a certain time limit
        private Timer Timer { get; set; }

        // Used to log when developing
        private Logger Logger;
        #endregion

        #region Constructors
        public MainViewModel()
        {
            // Start the Timer
            this.Timer = new Timer(TIMEOUT);
            this.Timer.Elapsed += Timer_Elapsed;
            this.Timer.Start();

            // Log start of the app
            Logger = new Logger();
            Logger.LogLine("Bootloader start");

            // Read the App.xml at the local folder of the bootloader.exe
            ReadCurrentApp();
        }

        
        #endregion

        #region Methods
        private void OnFilenameChanged()
        {
            try
            {
                // Read and assign newer app
                this.NewerApp = ReadApp(this.Filename);
            }
            catch
            {
                // TODO: display error message
            }
        }

        private void ReadCurrentApp()
        {
            try
            {
                // Read and assign newer app
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string xml = File.ReadAllText(path + @"\App.xml");
                this.CurrentApp = AppManifest.FromXML(xml);
            }
            catch
            {
                // TODO: display error message
            }
        }

        public void UpdateApplication()
        {
            // Do nothing is everything is not ready for the update
            if (!IsReadyForAppUpdate()) return;

            // Stop the timer, which could interrupt the update during its process
            this.Timer.Stop();

            Logger.LogLine("Bootloader ready for the update.");

            // Run the update
            try
            {
                // Open the zip
                ZipArchive archive = ZipFile.OpenRead(Filename);
                Logger.LogLine("Zip opened");

                // Overwrite every file
                string path = AppDomain.CurrentDomain.BaseDirectory;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!IsNonReplaceableFile(entry.Name))
                    {
                        string fullpath = path + @"\" + entry.FullName;
                        Logger.LogLine(path + @"\" + entry.FullName);

                        // If file
                        if (!String.IsNullOrEmpty(entry.Name))
                            entry.ExtractToFile(fullpath, true);

                        // If folder
                        else
                        {
                            // Create directory
                            if (!Directory.Exists(fullpath))
                                Directory.CreateDirectory(fullpath);
                        }
                    }
                    else if (IsBootloaderFile(entry.Name))
                    {
                        // Update the bootloader if higher version number
                        UpdateBootloader(entry);
                    }
                }

                archive.Dispose();
                Logger.LogLine("End of copy");
            }
            catch (Exception e)
            {
                File.WriteAllText("log.txt", e.Message + Environment.NewLine + e.StackTrace.ToString());
                Console.WriteLine(e.Message);
                Logger.LogLine("Error: "+ e.Message + Environment.NewLine + e.StackTrace.ToString());
            }

            // Start the Application
            Process.Start(NewerApp.ProcessName + ".exe");

            // Delete the tmp file
            File.Delete(Filename);

            // Kill the bootloader process
            Process.GetCurrentProcess().Kill();
        }

        private void UpdateBootloader(ZipArchiveEntry entry)
        {
            // Copy the file to tmp folder
            string filename = System.IO.Path.GetTempPath() + entry.Name;
            entry.ExtractToFile(filename, true);

            // Read the assembly version number
            Version version = AssemblyName.GetAssemblyName(filename).Version;
            Logger.LogLine("New bootloader version: " + version);

            // Get current assembly version
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Logger.LogLine("Current bootloaderversion: " + currentVersion);

            // Compare
            if (currentVersion.CompareTo(version) < 0)
            {
                // Copy the bootloader to "Bootloader_update.exe"
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string fullpath = path + @"\Bootloader_update.exe";

                entry.ExtractToFile(fullpath, true);
                Logger.LogLine(fullpath);
            }

            // Delete the tmp file Bootloader.exe
            File.Delete(filename);
        }
        #endregion

        #region Functions
        private AppManifest ReadApp(string filepath)
        {
            // Do verification on file existence and extension
            if (String.IsNullOrEmpty(filepath)) return null;
            if (!File.Exists(filepath)) return null;
            FileInfo info = new FileInfo(filepath);
            if (!info.Extension.Equals(".zip")) return null;
            
            // Open the zip
            ZipArchive archive = ZipFile.OpenRead(filepath);

            // Verify it contains an App.xml
            if (!ContainsEntry(archive, "App.xml")) return null;

            // Read the App.xml
            AppManifest app = ReadApp(GetEntry(archive, "App.xml"));
            if (app == null) return null;

            // Release resources
            archive.Dispose();
                
            return app;
        }

        private bool ContainsEntry(ZipArchive zip, string filename)
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
                if (entry.Name.Equals(filename)) return true;

            return false;
        }

        private ZipArchiveEntry GetEntry(ZipArchive zip, string filename)
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
                if (entry.Name.Equals(filename)) return entry;

            return null;
        }
        
        private AppManifest ReadApp(ZipArchiveEntry file)
        {
            using (var stream = file.Open())
            using (var reader = new StreamReader(stream))
            {
                // Read the text
                string xml = reader.ReadToEnd();

                // Deserialize
                return AppManifest.FromXML(xml);
            }
        }
        
        // Verifies that all required information was gathered before initiating an update
        private bool IsReadyForAppUpdate()
        {
            // Do verification on file existence and extension
            if (String.IsNullOrEmpty(Filename)) return false;
            if (!File.Exists(Filename)) return false;
            FileInfo info = new FileInfo(Filename);
            if (!info.Extension.Equals(".zip")) return false;

            if (CurrentApp == null) return false;
            if (String.IsNullOrEmpty(CurrentApp.Id)) return false;
            if (String.IsNullOrEmpty(CurrentApp.Key)) return false;

            if (NewerApp == null) return false;
            if (String.IsNullOrEmpty(NewerApp.Id)) return false;
            if (String.IsNullOrEmpty(CurrentApp.Key)) return false;

            if (NewerApp.Id != CurrentApp.Id) return false;
            if (NewerApp.Key != CurrentApp.Key) return false;

            // The calling process must be given and must have alread exited
            if (!IsProcessGiven) return false;
            if (CallingProcess != null && !CallingProcess.HasExited) return false;

            return true;
        }
        
        private bool IsNonReplaceableFile(string filename)
        {
            foreach (string file in BOOTLOADERFILES)
                if (filename.Equals(file)) return true;

            return false;
        }
        private bool IsBootloaderFile(string filename)
        {
            if (filename.Equals(BOOTLOADERFILES[0]))
                return true;

            return false;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void CallingProcess_Exited(object sender, EventArgs e)
        {
            // Now that the process has exited, everything should be ready for an update
            UpdateApplication();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // If this line is reach, the bootloading has lasted too long
            // and yet not initiated. Kill the bootloader.
            Process.GetCurrentProcess().Kill();
        }

        #endregion
    }
}
