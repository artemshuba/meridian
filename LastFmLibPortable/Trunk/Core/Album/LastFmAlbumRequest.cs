using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Album
{
    public class LastFmAlbumRequest
    {
        private readonly LastFm _lastFm;

        public LastFmAlbumRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }

        public async Task<List<LastFmAlbum>> Search(string album)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("album", album);
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "album.search"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("results.albummatches.album") != null)
            {
                var albumJson = response.SelectToken("results.albummatches.album");
                if (albumJson is JArray)
                    return
                        (from a in response.SelectToken("results.albummatches.album") select LastFmAlbum.FromJson(a)).ToList();
                else
                    return new List<LastFmAlbum>() { LastFmAlbum.FromJson(albumJson) };
            }

            return null;
        }

        public async Task<LastFmAlbum> GetInfo(string mbid, string album, string artist, bool autoCorrect = true)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mbid))
                parameters.Add("mbid", mbid);
            else
            {
                parameters.Add("album", album);
                parameters.Add("artist", artist);
            }

            if (autoCorrect)
                parameters.Add("autocorrect", "1");

            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "album.getInfo"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response["album"] != null)
            {
                return LastFmAlbum.FromJson(response["album"]);
            }

            return null;
        }
    }
}
