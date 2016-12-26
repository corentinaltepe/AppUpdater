﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUpdaterClient
{
    public class HikFileStream : MemoryStream
    {
        public HikFileStream() : base()
        { }

        public long CurrentSize { get; private set; }
        
        public event EventHandler Progress;

        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            CurrentSize += count;
            var h = Progress;
            if (h != null)
                h(this, EventArgs.Empty);//WARN: THIS SHOULD RETURNS ASAP!
        }

    }
}
