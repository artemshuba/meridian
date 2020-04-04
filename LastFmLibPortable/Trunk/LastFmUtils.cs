using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
            using(var md5 = MD5.Create())
            {
                return GetMd5Hash(md5, input);
            }
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
