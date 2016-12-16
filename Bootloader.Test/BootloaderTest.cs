using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bootloader.ViewModel;

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
            Assert.AreEqual("Updating Application1", vm.Title);

            Assert.IsFalse(vm.IsProcessIdGiven);
            vm.CallingProcessId = 9999;
            Assert.IsTrue(vm.IsProcessIdGiven);

            vm.Filename = filename;
            Assert.IsNotNull(vm.NewerApp);
            
        }
    }
}
