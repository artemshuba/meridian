using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jupiter.Utils.Extensions
{
    public static class StringExtensions
    {
        public static string[] Split(this string input, string separator)
        {
            return input.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static Dictionary<string, string> ParseQueryString(this string input)
        {
            var dict = new Dictionary<string, string>();

            // remove anything other than query string from url
            if (input.Contains("?"))
            {
                input = input.Substring(input.IndexOf('?') + 1);
            }
            else if (input.Contains("#"))
            {
                input = input.Substring(input.IndexOf('#') + 1);
            }

            foreach (string vp in Regex.Split(input, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    dict.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    dict.Add(singlePair[0], string.Empty);
                }
            }

            return dict;
        }
    }
}
