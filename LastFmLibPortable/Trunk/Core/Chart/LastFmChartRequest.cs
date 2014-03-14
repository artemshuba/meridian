using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LastFmLib.Core.Track;

namespace LastFmLib.Core.Chart
{
    public class LastFmChartRequest
    {
        private readonly LastFm _lastFm;

        public LastFmChartRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }


        public async Task<IEnumerable<LastFmTrack>> GetTopTracks(int count = 0)
        {
            var parameters = new Dictionary<string, string>();
            if (count > 0)
                parameters.Add("limit", count.ToString());
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "chart.getTopTracks"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("tracks.track") != null)
            {
                return from a in response.SelectToken("tracks.track") select LastFmTrack.FromJson(a);
            }

            return null;
        }

        public async Task<IEnumerable<LastFmTrack>> GetHypedTracks(int count = 0)
        {
            var parameters = new Dictionary<string, string>();
            if (count > 0)
                parameters.Add("limit", count.ToString());
            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await (new CoreRequest(new Uri(LastFmConst.MethodBase + "chart.getHypedTracks"), parameters).Execute());

            LastFmErrorProcessor.ProcessError(response);


            if (response.SelectToken("tracks.track") != null)
            {
                return from a in response.SelectToken("tracks.track") select LastFmTrack.FromJson(a);
            }

            return null;
        }
    }
}
