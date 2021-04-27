using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LastFmLib.Core.Track;
using System.Linq;

namespace LastFmLib.Core.Tag
{
    public class LastFmTagRequest
    {
        private readonly LastFm _lastFm;

        public LastFmTagRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }


        public async Task<IEnumerable<LastFmTrack>> GetTopTracks(string tag, int limit = 20, int page = 0)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("tag", tag);

            if (limit > 0)
                parameters.Add("limit", limit.ToString());

            if (page > 0)
                parameters.Add("page", page.ToString());

            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await new CoreRequest(new Uri(LastFmConst.MethodBase + "tag.getTopTracks"), parameters).Execute();

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("tracks.track") != null)
            {
                return from t in response.SelectToken("tracks.track") select LastFmTrack.FromJson(t);
            }

            return null;
        }
    }
}
