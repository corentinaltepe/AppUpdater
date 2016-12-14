using AppLib;
using AppUpdaterService.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace AppUpdaterService.Controllers
{
    public class AppsController : ApiController
    {
        private static string DEBUG_PATH = "C:/inetpub/wwwroot/samplesFolder/";
        private static string RELEASE_PATH = "C:/inetpub/wwwroot/samplesFolder/";

        #region HTTP
        // GET: api/Apps
        public IHttpActionResult Get()
        {
            return BadRequest("List of Apps not open.");
        }

        // GET: api/Apps/duefubdvsbhei
        public IHttpActionResult Get(string id)
        {
            return BadRequest("List of Apps not open.");
        }
        
        // POST: api/Apps/id
        /* Returns the info of the App if id matches an app in the list,
         * Returns the App file if 'action' key says 'download'. */
        public IHttpActionResult Post(HttpRequestMessage request)
        {
            var message = request.Content.ReadAsStringAsync().Result;
            if (message == null) return BadRequest("action key missing");

            // Read the message, expect an "id" key
            RequestParser parser = new RequestParser(message);
            string id = parser.FindValue("id");
            if (id == null) return BadRequest("ID missing or value is null");

            // Find the app information
            var app = FindAppByEncryptedId(id);
            if (app == null)
                return BadRequest("App not found");

            // Read the message, expect an "action" key
            parser = new RequestParser(message);
            string action = parser.FindValue("action");
            if (action == null)
                return Ok(app.RemoveSensitiveInfo());
            
            switch(action)
            {
                case "download":
                    AppContent appContent = FindAppContentByApp(app);
                    if(appContent == null) return BadRequest("App not found");

                    // Encrypt ArchiveFile before sending it
                    appContent.EncryptArchive();

                    // Prepare to send the requested file as an octet-stream
                    var result = new HttpResponseMessage(HttpStatusCode.OK)
                    { Content = new ByteArrayContent(appContent.Archive) };
                    result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    { FileName = app.Filename};
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    return ResponseMessage(result);
                default:
                    return BadRequest("Action " + parser.Items[0].Value + " not recognized");
            }
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

        #region Functions
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

        private App FindAppByEncryptedId(string encryptedId)
        {
            AppList apps = ReadListOfApps();
            foreach (App app in apps.Items)
            {
                try
                {
                    // Decrypt the ID with the App's key
                    string decryptedId = app.DecryptedId(encryptedId);

                    if (app.Id.Equals(decryptedId))
                        return app;
                }
                catch { return null; }
            }
               

            return null;
        }

        private AppContent FindAppContentByApp(App app)
        {
            if (app == null) return null;

            // Create an AppContent containing the ZipArchive designated
            AppContent myApp = AppContent.Cast(app);     // Cast

            // Find the file
            string filename = RELEASE_PATH + app.Filename;
#if DEBUG
            filename = DEBUG_PATH + app.Filename;
#endif
            // Verify the file exists and is a .zip
            if (!File.Exists(filename)) return null;
            if (!Path.GetExtension(filename).Equals(".zip")) return null;

            // Read the zip file and load it to myApp.Archive
            byte[] file = ReadFile(filename);
            if (file == null) return null;

            // Verify validity of the file (size and hash)
            if (file.Length != app.Filesize) return null;
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hash = mySHA256.ComputeHash(file);
            if (!StringHex.ToHexStr(hash).Equals(app.Sha256)) return null;

            // The file was fully verified and proved valid
            myApp.Archive = file;
            return myApp;
        }

        private byte[] ReadFile(string filename)
        {
            using (FileStream fsSource = new FileStream(filename,
                FileMode.Open, FileAccess.Read))
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = bytes.Length;

                return bytes;
            }
        }
        
        #endregion


    }
}
