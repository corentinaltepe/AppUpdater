using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;

namespace Bootloader.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
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

                UpdateApplication();
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

                UpdateApplication();
            }
        }

        // Not triggering events because not necessary
        private int callingProcessId;
        public int CallingProcessId
        {
            get { return callingProcessId; }
            set
            {
                callingProcessId = value;
                IsProcessIdGiven = true;
            }
        }
        public bool IsProcessIdGiven { get; set; }
        #endregion

        #region Constructors
        public MainViewModel()
        {
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

        private void UpdateApplication()
        {
            // Do nothing is everything is not ready for the update
            if (!IsReadyForAppUpdate()) return;

            try
            {
                // Open the zip
                ZipArchive archive = ZipFile.OpenRead(Filename);

                // Overwrite every file
                string path = AppDomain.CurrentDomain.BaseDirectory;
                foreach (ZipArchiveEntry entry in archive.Entries)
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
            catch (Exception e)
            { Console.WriteLine(e.Message); }
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

            // Wait for process to be killed if process ID is given
            if(IsProcessIdGiven)
            {
                try
                {
                    Process process = Process.GetProcessById(CallingProcessId);

                    // If a process is found, then it was not killed
                    return false;
                }
                catch { }
            }

            return true;
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

        #endregion
    }
}
