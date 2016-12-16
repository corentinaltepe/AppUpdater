using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bootloader.ViewModel;
using System.Diagnostics;
using System.Threading;

namespace Bootloader.Test
{
    [TestClass]
    public class BootloaderTest
    {
        [TestMethod]
        public void BootloaderTest_update_0dd6865c()
        {
            string filename = @"C:\Users\Corentin\AppData\Local\Temp\update_0dd6865c-e9f4-4d47-aa05-c6143691d279.zip";

            // Create the VM & assign filename
            MainViewModel vm = new MainViewModel();
            Assert.IsNotNull(vm.CurrentApp);
            Assert.AreEqual("Updating Japanese Dive Planner", vm.Title);

            Assert.IsFalse(vm.IsProcessGiven);

            // Get the calling process, if still active
            Process caller = null;
            try {
                //caller = Process.GetProcessById(8548);
            }
            catch { }
            vm.CallingProcess = caller;

            Assert.IsTrue(vm.IsProcessGiven);

            vm.Filename = filename;
            Assert.IsNotNull(vm.NewerApp);

            vm.UpdateApplication();

            Thread.Sleep(100000);
        }
    }
}
