using System;
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
        /*[XmlElement("App")]*/
        public List<AppManifest> Items { get; set; }

        public AppList()
        {
            Items = new List<AppManifest>();
        }

        public AppManifest FindAppByID(string id)
        {
            foreach (AppManifest app in Items)
                if (app.Id.Equals(id))
                    return app;

            return null;
        }
    }
}
