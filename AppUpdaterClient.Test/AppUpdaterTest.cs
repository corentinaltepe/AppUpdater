using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;

namespace AppUpdaterClient.Test
{
    [TestClass]
    public class AppUpdaterTest
    {
        private static string SERVER = "http://localhost:8112/api";


        [TestMethod]
        public void CheckNewerVersionAvailableTest()
        {
            List<string> receivedEvents = new List<string>();
            AppUpdater updater = new AppUpdater(SERVER);

            // Await
            updater.PropertyChanged += (sender, e) => receivedEvents.Add(e.PropertyName);

            updater.CheckNewerVersionAvailable();
            while (receivedEvents.Count == 0) Thread.Sleep(20);
            Assert.AreEqual("NewerApp", receivedEvents[0]);
        }

        [TestMethod, Timeout(100000)]
        public void DownloadNewerApplicationTest()
        {
            // Start identical to test above
            List<string> receivedEvents = new List<string>();
            AppUpdater updater = new AppUpdater(SERVER);

            // Await
            updater.PropertyChanged += (sender, e) => receivedEvents.Add(e.PropertyName);

            updater.CheckNewerVersionAvailable();
            while (receivedEvents.Count == 0) Thread.Sleep(20);
            Assert.AreEqual("NewerApp", receivedEvents[0]);

            // Now give the order to download and expect a notification
            updater.Download();
            while (receivedEvents.Count == 1) Thread.Sleep(20);
            Assert.AreEqual("IsUpdateDownloaded", receivedEvents[1]);

            updater.InstallUpdate();
        }

        [TestMethod]
        public void CheckNewerVersionAvailableNoAppTest()
        {
            List<string> receivedEvents = new List<string>();

            AppUpdater updater = new AppUpdater(SERVER);
            updater.PropertyChanged += (sender, e) => receivedEvents.Add(e.PropertyName);
            updater.CheckNewerVersionAvailable();
            Thread.Sleep(2000);
            
            Assert.AreEqual(0, receivedEvents.Count);
            Assert.IsNull(updater.NewerApp);
        }

        
    }
}
