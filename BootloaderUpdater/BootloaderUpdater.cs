using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BootloaderUpdater
{
    public static class BootloaderUpdater
    {
        /// <summary>
        /// Checks if an update for the bootloader is available.
        /// Runs the update if available. Returns FALSE if
        /// operation failed because it needs admin rights. In this
        /// case, you need to restart your app with admin rights.
        /// </summary>
        /// <returns></returns>
        public static bool UpdateBootloader()
        {
            if (!IsBootloaderUpdateAvailable()) return true;
            Logger log = new Logger();
            log.LogLine("New Bootloader available.");

            // Run the update
            try
            {
                File.Copy("Bootloader_update.exe", "Bootloader.exe", true);
                log.LogLine("Bootloader.exe updated.");
            }
            catch { return false; }

            // Check the update succeeded
            if (IsBootloaderUpdateAvailable()) return false;

            // Delete the update file
            try
            {
                File.Delete("Bootloader_update.exe");
                log.LogLine("Bootloader_update.exe deleted.");
            }
            catch { return false; }

            return true;
        }

        private static bool IsBootloaderUpdateAvailable()
        {
            if (!File.Exists("Bootloader_update.exe")) return false;

            if (File.Exists("Bootloader.exe"))
            {
                Version CurVersion = AssemblyName.GetAssemblyName("Bootloader.exe").Version;
                Version NewVersion = AssemblyName.GetAssemblyName("Bootloader_update.exe").Version;

                if (CurVersion.CompareTo(NewVersion) < 0)
                    return true;
                return false;
            }

            return true;
        }
    }
}
