﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AppLib
{
    [XmlRoot("Apps")]
    public class AppList
    {
        public AppList() { Items = new List<App>(); }
        [XmlElement("App")]
        public List<App> Items { get; set; }

        public App FindAppByID(string id)
        {
            foreach (App app in Items)
                if (app.Id.Equals(id))
                    return app;

            return null;
        }
    }
}