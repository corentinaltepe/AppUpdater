using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppUpdaterClient.Test
{
    [TestClass]
    public class AppUpdaterTest
    {
        private static string SERVER = "http://localhost:49473/";
        [TestMethod]
        public void CheckNewerVersionAvailableTest()
        {
            AppUpdater updater = new AppUpdater();
            updater.ServerAddress = SERVER;

            updater.CheckNewerVersionAvailable();
        }
    }
}
