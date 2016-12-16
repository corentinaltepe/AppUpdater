using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace AppLib
{
    public class App
    {
        public string Id { get; set; }
        public string Key { get; set; }

        public string Name { get; set; }
        public string VersionStr { get; set; }
        public int Version { get; set; }
        public bool Supported { get; set; }

        // Name of the .exe to be started once the update has finished
        public string ProcessName { get; set; }

        public string Sha256 { get; set; }
        public string Filename { get; set; }
        public DateTime PublishDate { get; set; }
        public int Filesize { get; set; }

        // Called before sending the App info to the API client
        public App RemoveSensitiveInfo()
        {
            this.Id = null;
            this.Key = null;

            return this;
        }

        public string DecryptedId(string encryptedId)
        {
            try
            { return StringCipher.Decrypt(encryptedId, this.Key); }
            catch { return null; }
        }
        public string EncryptedId()
        {
            if (Key == null) return null;
            if (Id == null) return null;
            try
            {
                return StringCipher.Encrypt(Id, Key);
            }
            catch { return null; }
        }

        public string ToXML()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }
        public static App FromXML(string s)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(App));
            using (StringReader textReader = new StringReader(s))
            {
                return (App)(xmlSerializer.Deserialize(textReader));
            }
        }

    }
}