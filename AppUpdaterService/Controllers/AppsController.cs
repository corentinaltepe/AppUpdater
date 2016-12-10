using AppUpdaterService.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppUpdaterService.Controllers
{
    public class AppsController : ApiController
    {
        // GET: api/Apps
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Apps/5
        public string Get(int id)
        {
            return "value";
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
    }
}
