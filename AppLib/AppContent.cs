using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace AppLib
{
    // AppContent is an App with the actual .zip file content
    public class AppContent : AppManifest
    {
        // Use FileStream to read and write file
        public byte[] Archive { get; set; }

        public void EncryptArchive()
        {
            if (Key == null) return;
            if (Archive == null) return;
            try
            {
                Archive =  StringCipher.Encrypt(Archive, Key);
            }
            catch { }
        }

        public static AppContent Cast(AppManifest app)
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
