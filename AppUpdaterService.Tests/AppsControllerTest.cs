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
using System.Web.Http.Results;

namespace AppUpdaterService.Tests
{
    [TestClass]
    public class AppsControllerTest
    {
        /// <summary>
        /// Testing an info request for an app with the correct encrypted ID.
        /// </summary>
        [TestMethod]
        public void PostTest1()
        {
            // Arrange
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            var httpRouteData = new HttpRouteData(httpConfiguration.Routes["DefaultApi"],
                new HttpRouteValueDictionary { { "controller", "category" } });

            var ctl = new AppsController();
            //ctl.SetupAllProperties();
            //AppsController ctl = new AppsController();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/Apps");
            request.SetConfiguration(new HttpConfiguration());
            request.Content = new StringContent("id=0cac65551f91a4068955bfbefef01b0f65e8ca73261b05ded3d5b7d62e6c3dddbf169a10e604639ae59bc09ba8b2d8751d0ba0e4910f44b5022d6e1a1da2f5794df9d5e25bc1d3f17dc38ac6d09405f369a57a14098b12b1ceeb5b3232684516",
                Encoding.UTF8, "application/json");

            // Changing the folder path in order to run the unit test
            ctl.AppDomainAppPath = AppDomain.CurrentDomain.BaseDirectory;

            // 
            IHttpActionResult actionResult = ctl.Post(request);
            var contentResult = actionResult as OkNegotiatedContentResult<App>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(typeof(App), contentResult.Content.GetType());

            App app = contentResult.Content;
            Assert.IsNull(app.Id);
            Assert.IsNull(app.Key);
            Assert.AreEqual("My Application Name", app.Name);
        }

        [TestMethod]
        public void PostTest2()
        {
            // Arrange
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            var httpRouteData = new HttpRouteData(httpConfiguration.Routes["DefaultApi"],
                new HttpRouteValueDictionary { { "controller", "category" } });

            var ctl = new AppsController();
            //ctl.SetupAllProperties();
            //AppsController ctl = new AppsController();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/Apps");
            request.SetConfiguration(new HttpConfiguration());
            request.Content = new StringContent("id=wrongIdfuzeheizufhizeufhiez",
                Encoding.UTF8, "application/json");

            // Changing the folder path in order to run the unit test
            ctl.AppDomainAppPath = AppDomain.CurrentDomain.BaseDirectory;

            // 
            IHttpActionResult actionResult = ctl.Post(request);
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(contentResult);
        }

        [TestMethod]
        public void PostTest3()
        {
            // Arrange
            var ctl = new AppsController();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/Apps");
            request.SetConfiguration(new HttpConfiguration());

            App app = new App() { Id = "ujrWZlyKQ4FLAS4a", Key = "tSzmfr1C35YAYI6r" };
            request.Content = new StringContent("id="+app.EncryptedId(),
                Encoding.UTF8, "application/json");

            // Changing the folder path in order to run the unit test
            ctl.AppDomainAppPath = AppDomain.CurrentDomain.BaseDirectory;
            
            IHttpActionResult actionResult = ctl.Post(request);
            var contentResult = actionResult as OkNegotiatedContentResult<App>;

            // Assert
            Assert.IsNotNull(contentResult);
        }

    }
}


