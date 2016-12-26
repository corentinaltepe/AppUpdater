using AppLib;
using System.Net.Http;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AppUpdaterClient
{
    public class AppUpdater : INotifyPropertyChanged
    {
        #region Properties
        // Address and port of the server hosting AppUpdaterService
        public string ServerAddress { get; set; }

        // Info about the Application the AppUpdater is embedded to
        public App CurrentApp { get; set; }

        // If a newer application is available, its info is loaded in this object
        private App newerApp;
        public App NewerApp
        {
            get { return newerApp; }
            set
            {
                newerApp = value;
                OnPropertyChanged("NewerApp");
            }
        }

        // True if the newer app was downloaded and properly checked.
        public bool isUpdateDownloaded;
        public bool IsUpdateDownloaded 
        {
            get { return isUpdateDownloaded; }
            set
            {
                isUpdateDownloaded = value;
                OnPropertyChanged("IsUpdateDownloaded");
            }
        }

        // Filename (and path) to the newly downloaded app
        public string DownloadedFilename { get; set; }

        private double progress = 0;
        /// <summary>
        /// Progress of the download of the App. From 0.0 (%) to 100.0 (%)
        /// </summary>
        public double Progress
        {
            get { return progress; }
            set
            {
                // Max / Min
                double val = value;
                if (val > 100.0) val = 100;
                else if (val < 0.0) val = 0.0;

                // Assign value
                if (progress != val)
                {
                    progress = val;
                    OnProgressReport("Progress");
                    OnPropertyChanged("Progress");
                }
            }
        }

        public long downloadedSize = 0;
        /// <summary>
        /// Quantity of bytes downloaded of the app.
        /// Note: there can be more bytes downloaded than advertized because
        /// the quantity of advertize filesize is not encrypted while the
        /// received bytes are encrypted.
        /// TODO: advertize the size of the encrypted file.
        /// </summary>
        public long DownloadedSize
        {
            get
            {
                return downloadedSize;
            }
            set
            {
                if (downloadedSize != value)
                {
                    downloadedSize = value;
                    OnDownloadedSizeReport("DownloadedSize");
                }
            }
        }
        #endregion



        #region Constructors
        public AppUpdater(string serverAddress)
        {
            this.ServerAddress = serverAddress;
            this.CurrentApp = ReadAppXML();
        }
        public AppUpdater(string serverAddress, string appFilename)
        {
            this.ServerAddress = serverAddress;
            this.CurrentApp = ReadAppXML(appFilename);
        }
        public AppUpdater(string serverAddress, App currentApp)
        {
            this.ServerAddress = serverAddress;
            this.CurrentApp = currentApp;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Loads the app described in XML file.
        /// By default, returns the app contained in App.xml.
        /// Throws exception if no file is found.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static App ReadAppXML(string filename = "App.xml")
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(App));

            // Check App.xml exists
            string pathtoAppXml = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            pathtoAppXml = Path.Combine(pathtoAppXml, filename);
            if (!File.Exists(pathtoAppXml))
                throw new FileNotFoundException("File: " + filename + 
                    " not found. Make sure to create one containing the information about your application.");

            // Create a new file stream for reading the XML file
            FileStream ReadFileStream = new FileStream(pathtoAppXml, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Load the object saved above by using the Deserialize function
            App app = (App)SerializerObj.Deserialize(ReadFileStream);

            // Cleanup
            ReadFileStream.Close();

            return app;
        }

        /// <summary>
        /// Check on the server if a newer version is available for the given app
        /// If newer app is available, assign it to this.NewerApp
        /// </summary>
        /// <param name="callback">Callback: true if the server has replied with a newer version
        /// available. False otherwise.</param>
        public void CheckNewerVersionAvailableAsync(Action<bool> callback)
        {
            // Encrypt the AppID

            var client = new RestClient(ServerAddress);
            var request = new RestRequest("/Apps/", Method.POST);
            request.AddParameter("id", CurrentApp.EncryptedId());
            
            // Long call (potentially)
            var task = client.ExecuteAsync<App>(request, res =>
            {
                // Function has ended - return whether a newer application was found, or not
                bool hasNewerApp = HandleServerResponseForNewerApp(res);
                callback(hasNewerApp);
            });
            
            //callback(HandleServerResponseForNewerApp(response));
        }

        /// <summary>
        /// If a newer app is available, start downloading
        /// It can take a very long time to execute since the client is downloading a file from
        /// the server.
        /// Uses System.Net.Http.HttpClient instead of RestSharp to report progress.
        /// </summary>
        /// <param name="callback"></param>
        public async Task DownloadAsync(Action<bool> callback)
        {
            if (NewerApp == null) return;

            // Your original code.
            HttpClientHandler aHandler = new HttpClientHandler();
            aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            HttpClient aClient = new HttpClient(aHandler);
            aClient.DefaultRequestHeaders.ExpectContinue = false;
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, ServerAddress + "/Apps/");
            string content = "id=" + CurrentApp.EncryptedId() + "&action=download";
            message.Content = new StringContent(content);
            HttpResponseMessage response = await aClient.SendAsync(message,
                HttpCompletionOption.ResponseHeadersRead); // Important! ResponseHeadersRead.
            
            // New code.
            Stream stream = await response.Content.ReadAsStreamAsync();
            MemoryStream memStream = new MemoryStream();

            // Start reading the stream
            var res = stream.CopyToAsync(memStream);

            // While reading the stream
            while (true)
            {
                // Report progress
                this.DownloadedSize = memStream.Length;
                this.Progress = 100.0 * (double)memStream.Length / (double)NewerApp.Filesize;

                // Leave if no new data was read
                if (res.IsCompleted)
                {
                    // Report progress one last time
                    this.DownloadedSize = memStream.Length;
                    this.Progress = 100.0 * (double)memStream.Length / (double)NewerApp.Filesize;
                    
                    break;
                }
                Thread.Sleep(100);
            }

            // Get the bytes from the memory stream
            byte[] responseContent = new byte[memStream.Length];
            memStream.Position = 0;
            memStream.Read(responseContent, 0, responseContent.Length);
            
            // Function has ended - return whether the app was donwloaded
            // properly and verified, or not
            callback(HandleResponseToDownloadRequest(responseContent));
        }

        /// <summary>
        /// Reads the response from the API. If a newer version is available,
        /// stores the information into this.NewerApp.
        /// Returns true if the server has replied with a newer version available.
        /// Returns false otherwise (errors, no app available, id wrong, etc.)
        /// </summary>
        /// <param name="response">Byte array of response from the API</param>
        /// <returns>True if a newer version is available</returns>
        private bool HandleServerResponseForNewerApp(IRestResponse<App> response)
        {
            if (response.Data == null) return false;
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) return false;
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return false;


            App serverAppInfo = response.Data;
            
            // If the app info was found on the server
            if (serverAppInfo != null)
            {
                // If the app on the server is more recent (Version higher)
                if (serverAppInfo.Version > this.CurrentApp.Version)
                {
                    // the notify by placing the info in thi.NewerApp
                    this.NewerApp = serverAppInfo;
                    return true;
                }
            }

            return false;
        }

        private bool HandleResponseToDownloadRequest(byte[] response)
        {
            if (response == null || response.Length == 0)
                return false;
            
            try
            {
                // Decrypt the bytes using the key of the app
                byte[] decryptedFile = StringCipher.Decrypt(response, CurrentApp.Key);

                // Verify validity of the file (size and hash)
                if (decryptedFile.Length != NewerApp.Filesize) return false;
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] hash = mySHA256.ComputeHash(decryptedFile);
                if (!StringHex.ToHexStr(hash).Equals(NewerApp.Sha256)) return false;

                // Download the file to TMP folder
                string filename = System.IO.Path.GetTempPath() + "update_" + Guid.NewGuid().ToString("D") + ".zip";
                File.WriteAllBytes(filename, decryptedFile);

                // Verify the file was written properly
                if (!File.Exists(filename)) return false;
                FileInfo info = new FileInfo(filename);
                if (info.Length != NewerApp.Filesize) return false;

                // Notify app of the newly downloaded app
                DownloadedFilename = filename;
                IsUpdateDownloaded = true;

                // Application properly downloaded and verified. Ready
                // to call Bootloader anytime.
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void InstallUpdate()
        {
            InstallUpdate(false);
        }
        public void InstallUpdate(bool showBootloader)
        {
            // Run a few checks on the validity and existence of the file
            if (!IsUpdateDownloaded) return;
            if (!File.Exists(DownloadedFilename)) return;
            
            FileInfo info = new FileInfo(DownloadedFilename);
            if (!info.Extension.Equals(".zip")) return;

            // Start Bootloader
            string arguments = DownloadedFilename;
            if (showBootloader) arguments = "-show " + arguments;
            arguments += " " + Process.GetCurrentProcess().Id.ToString();
            Process.Start("Bootloader.exe", arguments);

            // May not be clean. Kills the current process in which the client is embedded (the app)
            Process.GetCurrentProcess().Kill();
        }
        
        /// <summary>
        /// Remove the downloaded file from the tmp folder, if any.
        /// Return true if file properly deleted. Returns False if
        /// no file was found or could not delete the file.
        /// </summary>
        /// <returns></returns>
        public bool RemoveAppFile()
        {
            // Verify the file exists
            if (!File.Exists(DownloadedFilename)) return false;
            FileInfo info = new FileInfo(DownloadedFilename);
            if (!info.Extension.Equals(".zip")) return false;

            // Delete
            File.Delete(DownloadedFilename);

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

        /// <summary>
        /// Fired when the progress of the download of the app file
        /// has updated (more bytes received).
        /// </summary>
        public event PropertyChangedEventHandler ProgressReport;
        protected void OnProgressReport(string name)
        {
            PropertyChangedEventHandler handler = ProgressReport;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        /// <summary>
        /// Fired when the progress of the download of the app file
        /// has updated (more bytes received).
        /// </summary>
        public event PropertyChangedEventHandler DownloadedSizeReport;
        protected void OnDownloadedSizeReport(string name)
        {
            PropertyChangedEventHandler handler = DownloadedSizeReport;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        #endregion
    }
}
