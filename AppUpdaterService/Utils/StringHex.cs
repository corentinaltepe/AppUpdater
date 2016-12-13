using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppUpdaterService.Utils
{
    public static class StringHex
    {
        public static string ToHexStr(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "").ToLower();
        }

        public static byte[] ToByte(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}