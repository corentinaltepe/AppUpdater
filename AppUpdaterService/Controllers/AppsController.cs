using AppLib;
using AppUpdaterService.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace AppUpdaterService.Controllers
{
    public class AppsController : ApiController
    {
        private static string DEBUG_PATH = "C:/Users/Corentin/Desktop/samplesFolder/";
        private static string RELEASE_PATH = "";

        #region HTTP
        // GET: api/Apps
        public AppList Get()
        {
            return ReadListOfApps();
        }

        // GET: api/Apps/5
        public App Get(int id)
        {
            return FindAppById(id);
        }

        // POST: api/Apps
        public void Post(HttpRequestMessage request)
        {
            var message = request.Content.ReadAsStringAsync().Result;
            if (message == null) return;

            RequestParser parser = new RequestParser(message);

            // For now, do nothing
        }

        // POST: api/Apps/id
        public void Post(int id, HttpRequestMessage request)
        {
            var message = request.Content.ReadAsStringAsync().Result;
            if (message == null) return;

            RequestParser parser = new RequestParser(message);

            if(parser.Items.Count > 0)
            {
                if(parser.Items[0].Key.Equals("action"))
                {
                    switch(parser.Items[0].Value)
                    {
                        case "download":
                            // Get the .zip if exists and send it back
                            break;
                    }
                }
            }
            // For now, do nothing
        }

        // PUT: api/Apps/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Apps/5
        public void Delete(int id)
        {
        }

        #endregion

        private AppList ReadListOfApps()
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(AppList));

            // Create a new file stream for reading the XML file
            FileStream ReadFileStream = new FileStream(HttpRuntime.AppDomainAppPath + "/App_Data/AppsList.xml",
                                                        FileMode.Open, FileAccess.Read, FileShare.Read);

            // Load the object saved above by using the Deserialize function
            AppList apps = (AppList)SerializerObj.Deserialize(ReadFileStream);

            // Cleanup
            ReadFileStream.Close();
            
            return apps;
        }
       
        private App FindAppById(int id)
        {
            AppList apps = ReadListOfApps();
            foreach (App app in apps.Items)
                if (app.Id.Equals(Convert.ToString(id)))
                    return app;

            return null;
        }

    }
}
