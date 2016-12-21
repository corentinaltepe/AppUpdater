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
            
            Assert.AreEqual("0cac65551f91a4068955bfbefef01b0f65e8ca73261b05ded3d5b7d62e6c3dddbf169a10e604639ae59bc09ba8b2d8751d0ba0e4910f44b5022d6e1a1da2f5794df9d5e25bc1d3f17dc38ac6d09405f369a57a14098b12b1ceeb5b3232684516",
                            app.EncryptedId());

            // TODO: more assertions
        }
    }
}
