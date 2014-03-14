using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LastFmLib.Core.Artist;

namespace LastFmLib.Core.User
{
    public class LastFmUserRequest
    {
        private readonly LastFm _lastFm;

        public LastFmUserRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }

        public async Task<IEnumerable<LastFmArtist>> GetRecommendedArtists(int count = 0)
        {
            var parameters = new Dictionary<string, string>();

            if (count > 0)
                parameters.Add("limit", count.ToString());

            parameters.Add("api_key", _lastFm.ApiKey);
            parameters.Add("sk", _lastFm.SessionKey);
            parameters.Add("api_sig", LastFmUtils.BuildSig(_lastFm.ApiSecret, "user.getRecommendedArtists", parameters));

            var response = await new CoreRequest(new Uri(LastFmConst.UrlBase), parameters, "POST").Execute();

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("recommendations.artist") != null)
            {
                return from a in response.SelectToken("recommendations.artist") select LastFmArtist.FromJson(a);
            }

            return null;
        }
    }
}
