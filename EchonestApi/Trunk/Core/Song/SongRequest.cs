using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EchonestApi.Core.Playlist;

namespace EchonestApi.Core.Song
{
    public class SongRequest
    {
        private readonly Echonest _echonest;

        internal SongRequest(Echonest echonest)
        {
            _echonest = echonest;
        }

        public async Task<List<EchoSong>> Search(string title, string artist)
        {
            var parameters = new Dictionary<string, object>();

            parameters.Add("title", title);
            parameters.Add("artist", artist);

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "song/search"), parameters).Execute();

            var token = response.SelectToken("response.songs");
            if (token != null && token.HasValues)
            {
                return new List<EchoSong>(token.Select(EchoSong.FromJson).ToList());
            }

            return null;
        }
    }
}
