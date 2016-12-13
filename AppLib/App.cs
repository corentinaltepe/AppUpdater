using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace AppLib
{
    public class App
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionStr { get; set; }
        public int Version { get; set; }
        public bool Supported { get; set; }

        public string Hash_sha256 { get; set; }
        public string Filename { get; set; }
        public DateTime PublishDate { get; set; }

    }
}