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

        private bool CallbackExecuted = false;
        private bool NewAppFound = false;

        private void CheckNewerVersionAvailableCallback(bool res)
        {
            CallbackExecuted = true;
            NewAppFound = res;
        }


        [TestMethod, Timeout(2000)]
        public void CheckNewerVersionAvailableTest()
        {
            AppUpdater updater = new AppUpdater(SERVER);

            // Check for update and run the callback
            updater.CheckNewerVersionAvailableAsync(res => CheckNewerVersionAvailableCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // No newer app should have been found, but the callback should have fired
            Assert.IsFalse(NewAppFound);
        }
        
        [TestMethod, Timeout(100000)]
        public void DownloadNewerApplicationTest()
        {
            // Start identical to test above
            AppUpdater updater = new AppUpdater(SERVER, "App1.xml");

            // Check for update and run the callback
            updater.CheckNewerVersionAvailableAsync(res => CheckNewerVersionAvailableCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // Newer app should have been found
            Assert.IsTrue(NewAppFound);

            /*
            updater.CheckNewerVersionAvailable();
            while (receivedEvents.Count == 0) Thread.Sleep(20);
            Assert.AreEqual("NewerApp", receivedEvents[0]);

            // Now give the order to download and expect a notification
            updater.Download();
            while (receivedEvents.Count == 1) Thread.Sleep(20);
            Assert.AreEqual("IsUpdateDownloaded", receivedEvents[1]);*/

            updater.InstallUpdate();
        }

        [TestMethod]
        public void CheckNewerVersionAvailableNoAppTest()
        {
            List<string> receivedEvents = new List<string>();
            /*
            AppUpdater updater = new AppUpdater(SERVER);
            updater.PropertyChanged += (sender, e) => receivedEvents.Add(e.PropertyName);
            updater.CheckNewerVersionAvailable();
            Thread.Sleep(2000);
            
            Assert.AreEqual(0, receivedEvents.Count);
            Assert.IsNull(updater.NewerApp);*/
        }

        
    }
}
