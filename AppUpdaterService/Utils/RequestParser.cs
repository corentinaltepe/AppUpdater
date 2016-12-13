using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppUpdaterService.Utils
{
    public class RequestParser
    {
        public List<HttpRequestMessageItem> Items;

        public RequestParser(string message)
        {
            this.Items = ParseMessage(message);
        }

        private List<HttpRequestMessageItem> ParseMessage(string message)
        {
            List<HttpRequestMessageItem> items = new List<HttpRequestMessageItem>();

            string[] strItems = message.Split('&');
            foreach (string strItem in strItems)
            {
                string[] vals = strItem.Split('=');
                HttpRequestMessageItem item = new HttpRequestMessageItem(vals[0], vals[1]);
                items.Add(item);
            }

            return items;
        }

        public string FindValue(string key)
        {
            foreach (HttpRequestMessageItem item in Items)
                if (item.Key.Equals(key)) return item.Value;

            return null;
        }
    }
}