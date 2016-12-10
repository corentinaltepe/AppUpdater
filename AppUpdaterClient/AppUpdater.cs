using AppUpdaterService.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUpdaterClient
{
    public class AppUpdater
    {
        public string ServerAddress { get; set; }
        public string AppId { get; set; }
        public int CurrentVersion { get; set; }
        public App NewerApp { get; set; }

        public AppUpdater()
        {

        }

        // Check on the server if a newer version is available for the given app
        // If newer app is available, assign it to this.NewerApp
        public void CheckNewerVersionAvailable()
        {
            var client = new RestClient(ServerAddress);
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("Apps/", Method.GET);
            
            var asyncHandle = client.ExecuteAsync<App>(request, response => {
                Console.WriteLine(response.Data.Name);
            });
        }
    }
}
