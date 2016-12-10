using AppUpdaterService.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppUpdaterClient
{
    public class AppUpdater : INotifyPropertyChanged
    {
        public string ServerAddress { get; set; }
        public string AppId { get; set; }
        public int CurrentVersion { get; set; }

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

        public AppUpdater()
        { }

        public AppUpdater(string serverAddress, string appId, int currentVersion)
        {
            this.ServerAddress = serverAddress;
            this.AppId = appId;
            this.CurrentVersion = currentVersion;
        }


        // Check on the server if a newer version is available for the given app
        // If newer app is available, assign it to this.NewerApp
        public void CheckNewerVersionAvailable()
        {
            var client = new RestClient(ServerAddress);
            var request = new RestRequest("/Apps/"+AppId, Method.GET);
            
            var asyncHandle = client.ExecuteAsync<App>(request, response => {
                HandleServerResponse(response);
                //Console.WriteLine(response.Data.Name);
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
                if (serverAppInfo.Version > this.CurrentVersion)
                    // the notify by placing the info in thi.NewerApp
                    this.NewerApp = serverAppInfo;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
