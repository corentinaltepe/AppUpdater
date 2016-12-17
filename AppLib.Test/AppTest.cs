using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AppLib.Test
{
    [TestClass]
    public class AppTest
    {
        [TestMethod]
        public void XMLParserTest()
        {
            // Parse an XML to an app
            string xml = File.ReadAllText("AppSample1.xml");
            App app = App.FromXML(xml);

            Assert.AreEqual(2016, app.PublishDate.Year);
            Assert.AreEqual(12, app.PublishDate.Month);
            Assert.AreEqual(17, app.PublishDate.Day);
            Assert.AreEqual(18, app.PublishDate.Hour);

            // TODO: more assertions
        }
    }
}
