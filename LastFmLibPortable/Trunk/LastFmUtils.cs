using System.Collections.Generic;
using System.Linq;
using System.Text;
using xBrainLab.Security.Cryptography;

namespace LastFmLib
{
    internal class LastFmUtils
    {
        public static string BuildSig(string secretKey, string method, IDictionary<string, string> parameters)
        {
            parameters.Add("method", method);
            var temp = parameters.OrderBy(x => x.Key);
            var s = new StringBuilder();
            foreach (var p in temp)
            {
                s.Append(p.Key);
                s.Append(p.Value);
            }

            s.Append(secretKey);
            return Md5(s.ToString());
        }

        public static string Md5(string input)
        {
            return MD5.GetHashString(input);
        }
    }
}
