using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LastFmLib.Extensions
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Constructs a QueryString (string).
        /// Consider this method to be the opposite of "System.Web.HttpUtility.ParseQueryString"
        /// </summary>
        /// <param name="parameters">NameValueCollection</param>
        /// <returns>string</returns>
        public static string ConstructQueryString(this Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(pair => pair.Key).Distinct().Select(name => string.Concat(name, "=", WebUtility.HtmlEncode(parameters[name]))).ToArray());
        }
    }
}
