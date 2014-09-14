using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Xbox.Music.Core
{
    /// <summary>
    /// An object that performs request to echonest server
    /// </summary>
    internal class CoreRequest
    {
        private readonly Uri _uri;
        private readonly string _method;
        private readonly Dictionary<string, string> _parameters;

        public Dictionary<string, string> Parameters
        {
            get { return _parameters; }
        }

        public CoreRequest(Uri uri)
        {
            _uri = uri;
            _method = "GET";
        }

        public CoreRequest(Uri uri, Dictionary<string, string> parameters, string method = "GET")
        {
            _uri = uri;
            _method = method;
            _parameters = parameters;
        }

        public async Task<JObject> Execute()
        {
            var uri = GetFullUri();
#if DEBUG
            Debug.WriteLine("Invoking " + uri);
#endif

            JObject response = null;

            var httpClient = new HttpClient();
            if (_method == "GET")
            {
                HttpResponseMessage responseMessage = await httpClient.GetAsync(uri);
                var content = await responseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                    response = JObject.Parse(content);
            }
            else
            {
                var postContent = new FormUrlEncodedContent(_parameters);
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                System.Net.Http.HttpResponseMessage responseMessage = await httpClient.PostAsync(uri, postContent);
                var content = await responseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                    response = JObject.Parse(content);
            }

            return response;
        }

        private Uri GetFullUri()
        {
            if (_method == "GET" && _parameters != null && _parameters.Count > 0)
            {
                var paramStr = string.Join("&",
                                           _parameters.Select(
                                               kp =>
                                               {
                                                   if (kp.Value is IEnumerable && !(kp.Value is string))
                                                   {
                                                       var l = new List<string>();
                                                       foreach (var v in (IEnumerable)kp.Value)
                                                       {
                                                           l.Add(string.Format("{0}={1}",
                                                               Uri.EscapeDataString(kp.Key),
                                                               Uri.EscapeDataString(v.ToString())));
                                                       }

                                                       return string.Join("&", l);
                                                   }
                                                   else return string.Format("{0}={1}", Uri.EscapeDataString(kp.Key),
                                                           Uri.EscapeDataString(kp.Value.ToString()));
                                               }));

                return new Uri(string.Concat(_uri, "?", paramStr));
            }

            return _uri;
        }
    }
}
