using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VkLib.Core.Auth
{
    /// <summary>
    /// Результат OAuth авторизации
    /// </summary>
    public class OAuthResult
    {
        /// <summary>
        /// Токен
        /// </summary>
        public AccessToken AccessToken { get; set; }

        /// <summary>
        /// Код для авторизации через сторонние сайты
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Текст ошибки
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Подробное описание ошибки
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Получить токен из адреса
        /// </summary>
        /// <param name="uri">Адрес, полученный после OAuth авторизации</param>
        /// <returns></returns>
        public static OAuthResult Parse(Uri uri)
        {
            var p = ParseInternal(uri);
            if (p == null)
                return null;

            var result = new OAuthResult();
            result.AccessToken = new AccessToken();

            if (p.ContainsKey("error"))
                result.Error = p["error"];

            if (p.ContainsKey("error_description"))
                result.ErrorDescription = p["error_description"];

            if (p.ContainsKey("code"))
                result.Code = p["code"];

            if (p.ContainsKey("token"))
                result.Code = p["token"];

            if (p.ContainsKey("access_token"))
                result.AccessToken.Token = p["access_token"];

            if (p.ContainsKey("expires_in"))
                result.AccessToken.ExpiresIn = DateTime.Now.Add(TimeSpan.FromSeconds(double.Parse(p["expires_in"])));

            if (p.ContainsKey("user_id"))
                result.AccessToken.UserId = long.Parse(p["user_id"]);

            return result;
        }


        private static Dictionary<string, string> ParseInternal(Uri uri)
        {
            try
            {
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var fragment = uri.Query.Substring(1); //get fragment without '?'
                    Dictionary<string, string> parameters = ParseFragment(fragment);
                    return parameters;
                }
                else if (!string.IsNullOrEmpty(uri.Fragment))
                {
                    var fragment = uri.Fragment.Substring(1); //get fragment without '#'
                    Dictionary<string, string> parameters = ParseFragment(fragment);
                    return parameters;
                }
                else if (uri.OriginalString.Contains("error"))
                {
                    var str = uri.OriginalString.Substring(uri.OriginalString.IndexOf('?') + 1);
                    Dictionary<string, string> parameters = ParseFragment(str);
                    return parameters;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to parse url. " + ex);
            }

            return null;
        }

        /// <summary>
        /// Parse a fragment to key-value pairs
        /// </summary>
        /// <param name="fragment">Fragment to parse</param>
        /// <returns>Returns a dictionary with keys and values.</returns>
        private static Dictionary<string, string> ParseFragment(string fragment)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(fragment))
                return result;

            string[] temp = fragment.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries); //get set of strings like "name=value"
            if (temp.Length == 0)
                return result;
            foreach (string t in temp)
            {
                if (!t.Contains("=")) //if string doesn't contain "=" then skip it
                    continue;
                int equalsPos = t.IndexOf('='); //the position of "=" char
                var key = t.Substring(0, equalsPos);
                var value = t.Substring(equalsPos + 1, t.Length - equalsPos - 1);
                result.Add(key, value);
            }

            return result;
        }
    }
}
