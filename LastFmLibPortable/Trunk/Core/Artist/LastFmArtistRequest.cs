using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LastFmLib.Core.Album;
using LastFmLib.Core.Image;
using LastFmLib.Core.Track;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Artist
{
    public class LastFmArtistRequest
    {
        private readonly LastFm _lastFm;

        public LastFmArtistRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }

        [Obsolete("Last.FM no longer provides method artist.getImages.", true)]
        public async Task<List<LastFmImage>> GetImages(string artist, string mbid, int limit, bool autoCorrect = true, int minWidth = 500, int minHeight = 200)
        {
            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(mbid))
                parameters.Add("mbid", mbid);
            else
                parameters.Add("artist", artist);

            if (autoCorrect)
                parameters.Add("autocorrect", "1");

            if (limit > 0)
                parameters.Add("limit", limit.ToString());

            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "artist.getImages"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);

            if (response["images"] != null && response["images"]["image"] != null)
            {
                var ie = new List<LastFmImage>();
                foreach (var image in response["images"]["image"])
                {
                    ie.Add(LastFmImage.FromJson(image));
                }
                return ie;
            }

            return null;
        }


        public async Task<List<LastFmArtist>> Search(string artist)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("artist", artist);
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "artist.search"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("results.artistmatches.artist") != null)
            {

                var artistJson = response.SelectToken("results.artistmatches.artist");
                if (artistJson is JArray)
                    return (from a in response.SelectToken("results.artistmatches.artist")
                            select LastFmArtist.FromJson(a)).ToList();
                else
                    return new List<LastFmArtist>() { LastFmArtist.FromJson(artistJson) };
            }

            return null;
        }

        public async Task<List<LastFmArtist>> GetSimilar(string artist, int count = 0)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("artist", artist);
            if (count > 0)
                parameters.Add("limit", count.ToString());
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "artist.getSimilar"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("similarartists.artist") != null)
            {
                return (from a in response.SelectToken("similarartists.artist") select LastFmArtist.FromJson(a)).ToList();
            }

            return null;
        }

        public async Task<List<LastFmAlbum>> GetTopAlbums(string mbid, string artist, int count = 0)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mbid))
                parameters.Add("mbid", mbid);
            else
                parameters.Add("artist", artist);
            if (count > 0)
                parameters.Add("limit", count.ToString());
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "artist.getTopAlbums"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("topalbums.album") != null)
            {
                var albumToken = response.SelectToken("topalbums.album");
                if (albumToken.GetType() == typeof(JArray))
                    return (from a in albumToken select LastFmAlbum.FromJson(a)).ToList();
                else if (albumToken.GetType() == typeof(JObject))
                {
                    var result = new List<LastFmAlbum>();
                    result.Add(LastFmAlbum.FromJson(albumToken));
                    return result;
                }
            }

            return null;
        }

        public async Task<List<LastFmTrack>> GetTopTracks(string mbid, string artist, int count = 0)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mbid))
                parameters.Add("mbid", mbid);
            else
                parameters.Add("artist", artist);
            if (count > 0)
                parameters.Add("limit", count.ToString());
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "artist.getTopTracks"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("toptracks.track") != null)
            {
                var tracksJson = response.SelectToken("toptracks.track");
                if (tracksJson is JArray)
                    return (from a in tracksJson select LastFmTrack.FromJson(a)).ToList();
                else
                    return new List<LastFmTrack>() { LastFmTrack.FromJson(tracksJson) };
            }

            return null;
        }

        public async Task<LastFmArtist> GetInfo(string mbid, string artist)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mbid))
                parameters.Add("mbid", mbid);
            else
                parameters.Add("artist", artist);
            if (!string.IsNullOrEmpty(_lastFm.Lang))
                parameters.Add("lang", _lastFm.Lang);
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "artist.getInfo"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);

            if (response["artist"] != null)
            {
                return LastFmArtist.FromJson(response["artist"]);
            }

            return null;
        }
    }
}
