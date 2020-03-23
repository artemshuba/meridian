using System;
using System.Collections.Generic;
using System.Linq;

namespace VkLib.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToUrlParams(this Dictionary<string, string> input)
        {
            var paramStr = string.Join("&", input.Select(kp => string.Format("{0}={1}", Uri.EscapeDataString(kp.Key), Uri.EscapeDataString(kp.Value))));
            return paramStr;
        }
    }
}
