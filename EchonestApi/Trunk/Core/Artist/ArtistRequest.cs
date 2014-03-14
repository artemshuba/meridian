using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchonestApi.Core.Artist
{
    public class ArtistRequest
    {
        private readonly Echonest _echonest;

        internal ArtistRequest(Echonest echonest)
        {
            _echonest = echonest;
        }

        public async Task<List<EchoArtist>> Search(string name)
        {
            var parameters = new Dictionary<string, object>();

            parameters.Add("name", name);

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "artist/search"), parameters).Execute();

            var token = response.SelectToken("response.artists");
            if (token != null && token.HasValues)
            {
                return new List<EchoArtist>(token.Select(EchoArtist.FromJson).ToList());
            }

            return null;
        }
    }
}
