﻿using AppUpdaterService.Models;
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
        #region HTTP
        // GET: api/Apps
        public AppList Get()
        {
            return ReadListOfApps();
        }

        // GET: api/Apps/5
        public App Get(int id)
        {
            AppList apps = ReadListOfApps();
            foreach (App app in apps.Items)
                if (app.Id.Equals(Convert.ToString(id)))
                    return app;

            return null;
        }

        // POST: api/Apps
        public void Post(HttpRequestMessage request)
        {
            var message = request.Content.ReadAsStringAsync().Result;
            if (message == null) return;

            RequestParser parser = new RequestParser(message);
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


    }
}