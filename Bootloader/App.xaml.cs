using Bootloader.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Bootloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var context = new MainViewModel();
            
            foreach(string arg in e.Args)
            {
                int processId;

                // Show UI
                if(arg.Equals("-show"))
                {
                    var app = new MainWindow();
                    app.DataContext = context;
                    app.Show();
                }

                // Path to the .zip
                else if(File.Exists(arg))
                {
                    FileInfo file = new FileInfo(arg);
                    if (file.Exists) //make sure it's actually a file
                    {
                        context.Filename = file.FullName;
                    }
                }

                // Process ID to be killed before update
                else if(int.TryParse(arg, out processId))
                {
                    // Get the calling process, if still active
                    Process caller = null;
                    try { caller = Process.GetProcessById(processId); }
                    catch { }

                    context.CallingProcess = caller;
                }
            }

            // Start the update
            context.UpdateApplication();
        }
    }
}
