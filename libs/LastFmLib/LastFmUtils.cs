using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace LastFmLib
{
    internal static class LastFmUtils
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
            var hash = new StringBuilder();
            var md5provider = new MD5CryptoServiceProvider();

            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
