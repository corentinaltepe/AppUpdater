using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppUpdaterService.Models
{
    public class App
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionStr { get; set; }
        public int Version { get; set; }
        public bool Supported { get; set; }
    }
}