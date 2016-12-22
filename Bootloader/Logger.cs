using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootloader
{
    public class Logger
    {
        public string Filename { get; set; }

        public Logger(string filename = "bootloader.log")
        {
            this.Filename = filename;
        }

        public void LogLine(string text)
        {
            using (StreamWriter sw = File.AppendText(Filename))
            {
                DateTime now = DateTime.Now;
                sw.Write(now.ToShortDateString() + " " + now.ToShortTimeString() + ": ");
                sw.WriteLine(text);
            }
        }
    }
}
