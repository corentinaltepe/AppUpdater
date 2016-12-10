using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AppUpdaterService.Models
{
    public class App
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionStr { get; set; }
        public int Version { get; set; }
        public bool Supported { get; set; }

        public static App ReadXML(XmlTextReader reader)
        {
            App app = new App();

            while (reader.Read() && 
                    !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "App"))
            {
                if(reader.NodeType == XmlNodeType.Element)
                {
                    switch(reader.Name)
                    {
                        case "Id":
                            reader.Read();
                            app.Id = reader.Value;
                            break;

                        case "Name":
                            reader.Read();
                            app.Name = reader.Value;
                            break;

                        case "VersionStr":
                            reader.Read();
                            app.VersionStr = reader.Value;
                            break;

                        case "Version":
                            reader.Read();
                            app.Version = XmlConvert.ToInt32(reader.Value);
                            break;

                        case "Supported":
                            reader.Read();
                            app.Supported = XmlConvert.ToBoolean(reader.Value);
                            break;

                        default:
                            // Do something else, then break;
                            break;
                    }
                }
            }

            return app;
        }
    }
}