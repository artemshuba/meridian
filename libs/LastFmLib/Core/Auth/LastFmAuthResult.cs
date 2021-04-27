using System;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Auth
{
    public class LastFmAuthResult
    {
        public string Username { get; set; }

        public string Key { get; set; }

        public static LastFmAuthResult FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new LastFmAuthResult();

            result.Username = json["name"].Value<string>();
            result.Key = json["key"].Value<string>();

            return result;
        }
    }
}
