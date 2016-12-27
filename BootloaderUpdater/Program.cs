using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootloaderUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            // No arguments needed.
            // Minimal application, called with admin rights.
            Console.WriteLine("Start of bootloader update...");

            bool result = BootloaderUpdater.UpdateBootloader();

            if (result)
                Console.WriteLine("Bootloader successfully updated.");
            else
                Console.WriteLine("Bootloader update failed!");

#if DEBUG
            Console.WriteLine("Press a key to continue.");
            Console.ReadKey();
#endif
        }
    }
}
