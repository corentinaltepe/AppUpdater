using AppUpdaterService.Models;
using AppUpdaterService.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace AppUpdaterService.Controllers
{
    public class AppsController : ApiController
    {
        #region HTTP
        // GET: api/Apps
        public IEnumerable<App> Get()
        {
            return ReadListOfApps();
        }

        // GET: api/Apps/5
        public App Get(int id)
        {
            List<App> apps = ReadListOfApps();
            foreach (App app in apps)
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

        private List<App> ReadListOfApps()
        {
            List<App> apps = new List<App>();

            XmlTextReader reader = new XmlTextReader(HttpRuntime.AppDomainAppPath+"/App_Data/AppsList.xml");
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element && reader.Name == "App")
                {
                    apps.Add(App.ReadXML(reader));
                }
            }

            return apps;
        }


    }
}
