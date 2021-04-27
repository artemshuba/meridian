using DeezerLib.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkLib.Core;

namespace DeezerLib
{
    public class Deezer
    {
        private const string ApiBaseUrl = "http://api.deezer.com";

        internal string AppId { get; set; }

        internal string SecretKey { get; set; }

        public Deezer(string appId, string secretKey)
        {
            AppId = AppId;
            SecretKey = SecretKey;
        }

        public async Task<List<DeezerArtist>> SearchArtists(string query)
        {
            var p = new Dictionary<string, string>
            {
                ["q"] = query
            };

            var response = await CoreRequest.GetAsync($"{ApiBaseUrl}/search/artist", p);

            if (response["data"] != null)
            {
                return JsonConvert.DeserializeObject<List<DeezerArtist>>(response["data"].ToString());
            }

            return null;
        }

        public async Task<List<DeezerAlbum>> SearchAlbums(string query)
        {
            var p = new Dictionary<string, string>
            {
                ["q"] = query
            };

            var response = await CoreRequest.GetAsync($"{ApiBaseUrl}/search/album", p);

            if (response["data"] != null)
            {
                return JsonConvert.DeserializeObject<List<DeezerAlbum>>(response["data"].ToString());
            }

            return null;
        }

        public async Task<List<DeezerTrack>> GetAlbumTracks(string id)
        {
            var p = new Dictionary<string, string>();

            var response = await CoreRequest.GetAsync($"{ApiBaseUrl}/album/{id}/tracks", p);

            if (response["data"] != null)
            {
                return JsonConvert.DeserializeObject<List<DeezerTrack>>(response["data"].ToString());
            }

            return null;
        }

        public async Task<List<DeezerTrack>> GetArtistTopTracks(string id, int limit = 0, int index = 0)
        {
            var p = new Dictionary<string, string>();

            if (limit > 0)
                p["limit"] = limit.ToString();

            if (index > 0)
                p["index"] = index.ToString();

            var response = await CoreRequest.GetAsync($"{ApiBaseUrl}/artist/{id}/top", p);

            if (response["data"] != null)
            {
                return JsonConvert.DeserializeObject<List<DeezerTrack>>(response["data"].ToString());
            }

            return null;
        }

        public async Task<List<DeezerArtist>> GetArtistRelated(string id)
        {
            var p = new Dictionary<string, string>();

            var response = await CoreRequest.GetAsync($"{ApiBaseUrl}/artist/{id}/related", p);

            if (response["data"] != null)
            {
                return JsonConvert.DeserializeObject<List<DeezerArtist>>(response["data"].ToString());
            }

            return null;
        }

        public async Task<List<DeezerAlbum>> GetArtistAlbums(string id)
        {
            var p = new Dictionary<string, string>();

            var response = await CoreRequest.GetAsync($"{ApiBaseUrl}/artist/{id}/albums", p);

            if (response["data"] != null)
            {
                return JsonConvert.DeserializeObject<List<DeezerAlbum>>(response["data"].ToString());
            }

            return null;
        }
    }
}