using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;
using System.Timers;

namespace Bootloader.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Files which should not be overwritten during update
        private static string[] BOOTLOADERFILES = { "AppLib.dll",
                                                    "Bootloader.exe",
                                                    "Bootloader.Test.dll",
                                                    "AppLib.pdb",
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
        private AppLib.App currentApp;
        public AppLib.App CurrentApp
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
        private AppLib.App newerApp;
        public AppLib.App NewerApp
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
        
        #endregion

        #region Constructors
        public MainViewModel()
        {
            // Start the Timer
            this.Timer = new Timer(TIMEOUT);
            this.Timer.Elapsed += Timer_Elapsed;
            this.Timer.Start();

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
                this.CurrentApp = AppLib.App.FromXML(xml);
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

            // Run the update
            try
            {
                // Open the zip
                ZipArchive archive = ZipFile.OpenRead(Filename);

                // Overwrite every file
                string path = AppDomain.CurrentDomain.BaseDirectory;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!IsBootloaderFile(entry.Name))
                    {
                        string fullpath = path + @"\" + entry.FullName;

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
                }

                archive.Dispose();
            }
            catch (Exception e)
            { Console.WriteLine(e.Message); }

            // Start the Application
            Process.Start(NewerApp.ProcessName + ".exe");

            // Delete the tmp file
            File.Delete(Filename);

            // Kill the bootloader process
            Process.GetCurrentProcess().Kill();
        }
        #endregion

        #region Functions
        private AppLib.App ReadApp(string filepath)
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
            AppLib.App app = ReadApp(GetEntry(archive, "App.xml"));
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
        
        private AppLib.App ReadApp(ZipArchiveEntry file)
        {
            using (var stream = file.Open())
            using (var reader = new StreamReader(stream))
            {
                // Read the text
                string xml = reader.ReadToEnd();

                // Deserialize
                return AppLib.App.FromXML(xml);
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
        
        private bool IsBootloaderFile(string filename)
        {
            foreach (string file in BOOTLOADERFILES)
                if (filename.Equals(file)) return true;

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
