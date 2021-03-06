﻿using AppLib;
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
        private static string DEBUG_PATH = "C:/inetpub/wwwroot/App_Data/Applications/";

        #region Properties
        private string appDomainAppVirtualPath = "";
        public string AppDomainAppPath
        {
            get {
                if (string.IsNullOrEmpty(appDomainAppVirtualPath))
                    return HttpContext.Current.Server.MapPath("~");
                else
                    return appDomainAppVirtualPath;
            }
            set { appDomainAppVirtualPath = value; }
        }
        #endregion
        
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

        /// <summary>
        /// Called on a POST api/Apps
        /// Returns the info of the App if id matches an app in the list,
        /// returns the App file if 'action' key says 'download'. Error
        /// message otherwise.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult Post(HttpRequestMessage request)
        {
            var message = request.Content.ReadAsStringAsync().Result;
            if (message == null) return BadRequest("action key missing");

            // Read the message, expect an "id" key
            RequestParser parser = new RequestParser(message);
            string encryptedId = parser.FindValue("id");
            if (encryptedId == null) return BadRequest("ID missing or value is null");

            // Find the app information
            AppManifest app = FindLatestApp(encryptedId);
            if (app == null) return BadRequest("App not found. No corresponding ID.");

            // Read the message, expect an "action" key
            parser = new RequestParser(message);
            string action = parser.FindValue("action");
            if (action == null)
                return Ok(app.RemoveSensitiveInfo());
            
            switch(action)
            {
                case "download":
                    AppContent appContent = FindAppContentByApp(app);
                    if(appContent == null) return BadRequest("App not found. File in system is not correct (hash, filesize or filename).");

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
            FileStream ReadFileStream = new FileStream(AppDomainAppPath + "App_Data/AppsList.xml",
                                                        FileMode.Open, FileAccess.Read, FileShare.Read);

            // Load the object saved above by using the Deserialize function
            AppList apps = (AppList)SerializerObj.Deserialize(ReadFileStream);

            // Cleanup
            ReadFileStream.Close();
            
            return apps;
        }

        /// <summary>
        /// Goes through each app in AppsList.xml until the pair Key/Id
        /// matches encryptedId.
        /// </summary>
        /// <param name="encrypted">Id of the app, encrypted with its own key.</param>
        /// <returns>Decrypted ID of the App if any found. Null otherwise.</returns>
        private string FindAppDecryptedId(string encryptedId)
        {
            AppList apps = ReadListOfApps();
            foreach (AppManifest app in apps.Items)
            {
                try
                {
                    // Decrypt the ID with the App's key
                    string decryptedId = app.DecryptedId(encryptedId);

                    if (app.Id.Equals(decryptedId))
                        return app.Id;
                }
                catch { return null; }
            }

            return null;
        }

        /// <summary>
        /// Find all apps with the given (encrypted) Id. First finds 1 app with the
        /// matching pair ID/Key, then returns all apps with the same Id. If not found,
        /// returns null.
        /// </summary>
        /// <param name="encryptedId">Id of the app, encrypted with its own key.</param>
        /// <returns>Decrypted ID of the App if any found. Null otherwise.</param>
        /// <returns>List of apps with the given (encrypted) Id.</returns>
        private List<AppManifest> FindAppsByEncryptedId(string encryptedId)
        {
            string decryptedId = FindAppDecryptedId(encryptedId);
            if (decryptedId == null) return null;

            List<AppManifest> selectedApps = new List<AppManifest>();
            AppList apps = ReadListOfApps();
            foreach (AppManifest app in apps.Items)
            {
                if (app.Id.Equals(decryptedId))
                    selectedApps.Add(app);
            }

            return selectedApps;
        }

        private AppManifest FindLatestApp(string encryptedId)
        {
            // Return the app with encrypteId with highest version number
            var res = FindAppsByEncryptedId(encryptedId);
            if (res == null) return null;

            return res.OrderByDescending(i => i.Version).FirstOrDefault();
        }

        private AppContent FindAppContentByApp(AppManifest app)
        {
            if (app == null) return null;

            // Create an AppContent containing the ZipArchive designated
            AppContent myApp = AppContent.Cast(app);     // Cast

            // Find the file
#if DEBUG
            string filename = DEBUG_PATH + app.Filename;
#else
            string filename = AppDomainAppPath + "App_Data/Applications/" + app.Filename;
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
            if (!StringHex.ToHexStr(hash).ToLower().Equals(app.Sha256.ToLower())) return null;

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
