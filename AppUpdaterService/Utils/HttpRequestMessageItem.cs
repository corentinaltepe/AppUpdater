using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppUpdaterService.Utils
{
    public class HttpRequestMessageItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public HttpRequestMessageItem(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}