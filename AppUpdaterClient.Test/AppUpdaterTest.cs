using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using AppLib;

namespace AppUpdaterClient.Test
{
    [TestClass]
    public class AppUpdaterTest
    {
        private static string SERVER = "http://localhost:8112/api";

        private bool CallbackExecuted = false;
        private bool NewAppFound = false;
        private bool AppDownloaded = false;

        /// <summary>
        /// Callback once the server has replied or failed to reply to the 
        /// request for app information for update.
        /// res: True if newer app is available. False otherwise 
        /// (error or no update available).
        /// </summary>
        /// <param name="res"></param>
        private void CheckNewerVersionAvailableCallback(bool res)
        {
            CallbackExecuted = true;
            NewAppFound = res;
        }

        /// <summary>
        /// Callback once the application has been downloaded (or failed to).
        /// Res: true if the application was properly downloaded. False otherwise
        /// (no file available, sha256 wrong, decryption failed, etc).
        /// </summary>
        /// <param name="res"></param>
        private void DownloadNewAppCallback(bool res)
        {
            CallbackExecuted = true;
            AppDownloaded = res;
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
        
        [TestMethod, Timeout(4000)]
        public void DownloadNewerApplicationTest1()
        {
            // Start identical to test above
            AppUpdater updater = new AppUpdater(SERVER, "App1.xml");

            // Check for update and run the callback
            updater.CheckNewerVersionAvailableAsync(res => CheckNewerVersionAvailableCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // Newer app should have been found
            Assert.IsTrue(NewAppFound);

            // Now request to download app (.zip)
            CallbackExecuted = false;
            updater.DownloadAsync(res => DownloadNewAppCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // The download should have failed since no .zip file is available
            Assert.IsFalse(AppDownloaded);
        }

        [TestMethod, Timeout(4000)]
        public void DownloadNewerApplicationTest2()
        {
            // Current App's filename is given but not the filesize. Expected error message from the server.
            AppManifest currentApp = AppUpdater.ReadAppXML();
            currentApp.Id = "ujrWZlyKQ4FLAS4b";
            currentApp.Filename = "SampleApp3.zip";
            AppUpdater updater = new AppUpdater(SERVER, currentApp);

            // Check for update and run the callback
            updater.CheckNewerVersionAvailableAsync(res => CheckNewerVersionAvailableCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // Newer app should have been found
            Assert.IsTrue(NewAppFound);

            // Now request to download app (.zip)
            CallbackExecuted = false;
            updater.DownloadAsync(res => DownloadNewAppCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // The download should have failed since no .zip file is available
            Assert.IsFalse(AppDownloaded);
        }

        [TestMethod, Timeout(4000)]
        public void DownloadNewerApplicationTest3()
        {
            // Current App's filename and filesize is given but not the sha256. 
            // Expected error message from the server.
            AppManifest currentApp = AppUpdater.ReadAppXML();
            currentApp.Id = "ujrWZlyKQ4FLAS4b";
            currentApp.Filename = "SampleApp3.zip";
            currentApp.Filesize = 185;
            AppUpdater updater = new AppUpdater(SERVER, currentApp);

            // Check for update and run the callback
            updater.CheckNewerVersionAvailableAsync(res => CheckNewerVersionAvailableCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // Newer app should have been found
            Assert.IsTrue(NewAppFound);

            // Now request to download app (.zip)
            CallbackExecuted = false;
            updater.DownloadAsync(res => DownloadNewAppCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // The download should have failed since no .zip file is available
            Assert.IsFalse(AppDownloaded);
        }

        [TestMethod, Timeout(1000000)]
        public void DownloadNewerApplicationTest4()
        {
            // Current App's filename, filesize and sha256 given. 
            // Expected application to be downloaded to tmp folder.
            AppManifest currentApp = AppUpdater.ReadAppXML();
            currentApp.Id = "ujrWZlyKQ4FLAS4c";
            currentApp.Key = "tSzmfr1C35YAYI6r";
            AppUpdater updater = new AppUpdater(SERVER, currentApp);

            // Check for update and run the callback
            updater.CheckNewerVersionAvailableAsync(res => CheckNewerVersionAvailableCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // Newer app should have been found
            Assert.IsTrue(NewAppFound);

            // Now request to download app (.zip)
            CallbackExecuted = false;
            Assert.AreEqual(0, updater.Progress);
            updater.DownloadAsync(res => DownloadNewAppCallback(res));
            while (!CallbackExecuted) Thread.Sleep(20);

            // The download should have failed since no .zip file is available
            Assert.IsTrue(AppDownloaded);
            Assert.IsTrue(99.0 < updater.Progress);

            // Remove file from TMP folder
            Assert.IsTrue(updater.RemoveAppFile());
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
