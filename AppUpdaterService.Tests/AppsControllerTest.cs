using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppUpdaterService.Controllers;
using AppLib;
using AppUpdaterService.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Text;

namespace AppUpdaterService.Tests
{
    [TestClass]
    public class AppsControllerTest
    {
        [TestMethod]
        public void PostTest1()
        {
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            var httpRouteData = new HttpRouteData(httpConfiguration.Routes["DefaultApi"],
                new HttpRouteValueDictionary { { "controller", "category" } });

            var ctl = new Moq.Mock<AppsController>();
            ctl.SetupAllProperties();
            //AppsController ctl = new AppsController();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/Apps");
            request.SetConfiguration(new HttpConfiguration());
            request.Content = new StringContent("id=7e86876ed1485f37f29020aa73dd0ddfbfe4c8a12e197f70bc2a6aa98a50bfe8be892751d4d7fba2370b1e9e92d8917e43358ee7b00648bfb21ab993f8d47776b725a9c159351d566c43579b0f7ccf1476a3c456969e6ea567dd435b0d114ce6",
                Encoding.UTF8, "application/json");

            // Changing the folder path
            string path = AppDomain.CurrentDomain.BaseDirectory;
            ctl.Setup(fake => fake.AppDomainAppVirtualPath).Returns(path);
            Assert.AreEqual(path, ctl.Object.AppDomainAppVirtualPath);

            // Send the POST
            ctl.Object.Post(request);

            // ASSERT
            // TODO
        }
    }
}


