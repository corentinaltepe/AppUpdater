using AppLib;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        

        #region Constructors
        public AppUpdater(string serverAddress)
        {
            this.ServerAddress = serverAddress;
            this.CurrentApp = ReadAppXML();
        }

        #endregion

        #region Methods
        private App ReadAppXML()
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(App));

            // Check App.xml exists
            string pathtoAppXml = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            pathtoAppXml = Path.Combine(pathtoAppXml, "App.xml");
            if (!File.Exists(pathtoAppXml))
                throw new FileNotFoundException("File App.xml not found. Make sure to create one containing the information about your application.");

            // Create a new file stream for reading the XML file
            FileStream ReadFileStream = new FileStream(pathtoAppXml, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Load the object saved above by using the Deserialize function
            App app = (App)SerializerObj.Deserialize(ReadFileStream);

            // Cleanup
            ReadFileStream.Close();

            return app;
        }
        
        // Check on the server if a newer version is available for the given app
        // If newer app is available, assign it to this.NewerApp
        public void CheckNewerVersionAvailable()
        {
            // Encrypt the AppID

            var client = new RestClient(ServerAddress);
            var request = new RestRequest("/Apps/", Method.POST);
            request.AddParameter("id", CurrentApp.EncryptedId());

            var asyncHandle = client.ExecuteAsync<App>(request, response => {
                HandleServerResponse(response);
            });

        }

        // If a newer app is available, start downloading
        // It can take a very long time to execute since the client is downloading a file from
        // the server
        public void Download()
        {
            if (NewerApp == null) return;
            
            var client = new RestClient(ServerAddress);
            var request = new RestRequest("/Apps/", Method.POST);
            request.AddParameter("id", CurrentApp.EncryptedId());
            request.AddParameter("action", "download");
            
            var asyncHandle = client.ExecuteAsync<App>(request, response => {
                HandleResponseToDownloadRequest(response);
            });
        }

        private void HandleServerResponse(IRestResponse<App> response)
        {
            if (response.Data == null) return;
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) return;
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return;


            App serverAppInfo = response.Data;
            
            // If the app info was found on the server
            if (serverAppInfo != null)
            {
                // If the app on the server is more recent (Version higher)
                if (serverAppInfo.Version > this.CurrentApp.Version)
                    // the notify by placing the info in thi.NewerApp
                    this.NewerApp = serverAppInfo;
            }
        }

        private void HandleResponseToDownloadRequest(IRestResponse<App> response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) return;
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return;
            if (!response.ContentType.Equals("application/octet-stream")) return;
            
            try
            {
                // Verify validity of the file (size and hash)
                if (response.RawBytes.Length != NewerApp.Filesize) return;
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] hash = mySHA256.ComputeHash(response.RawBytes);
                if (!StringHex.ToHexStr(hash).Equals(NewerApp.Sha256)) return;

                // Download the file to TMP folder
                string tempPath = System.IO.Path.GetTempPath();
                File.WriteAllBytes(tempPath + "appUpdaterClientNewerSoftware.zip", response.RawBytes);

                // Verify the file was written properly
                if (!File.Exists(tempPath + "appUpdaterClientNewerSoftware.zip")) return;
                FileInfo info = new FileInfo(tempPath + "appUpdaterClientNewerSoftware.zip");
                if (info.Length != NewerApp.Filesize) return;

                // TODO: continue here
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
