using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace AppLib
{
    // AppContent is an App with the actual .zip file content
    public class AppContent : App
    {
        // Use FileStream to read and write file
        public byte[] Archive { get; set; }

        public static AppContent Cast(App app)
        {
            AppContent appContent = new AppContent();

            appContent.Filename = app.Filename;
            appContent.Id = app.Id;
            appContent.Key = app.Key;
            appContent.Name = app.Name;
            appContent.PublishDate = app.PublishDate;
            appContent.Supported = app.Supported;
            appContent.Version = app.Version;
            appContent.VersionStr = app.VersionStr;
            appContent.Filesize = app.Filesize;
            appContent.Sha256 = app.Sha256;

            return appContent;
        }

    }
}
