using System;
using Newtonsoft.Json.Linq;

namespace EchonestApi.Core.Artist
{
    public class EchoArtist
    {
        public string Name { get; set; }

        public string Id { get; set; }

        internal static EchoArtist FromJson(JToken json)
        {
            if (json == null)
                throw new ArgumentException("Json can not be null.");

            var result = new EchoArtist();
            result.Id = json["id"].Value<string>();
            result.Name = json["name"].Value<string>();

            return result;
        }
    }
}
