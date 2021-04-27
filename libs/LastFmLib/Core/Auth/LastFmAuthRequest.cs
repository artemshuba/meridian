using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LastFmLib.Core.Auth
{
    public class LastFmAuthRequest
    {
        private readonly LastFm _lastFm;

        public LastFmAuthRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }


        public async Task<LastFmAuthResult> GetMobileSession(string username, string password)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("username", username);
            parameters.Add("password", password);

            parameters.Add("api_key", _lastFm.ApiKey);
            parameters.Add("api_sig", LastFmUtils.BuildSig(_lastFm.ApiSecret, "auth.getMobileSession", parameters));

            var response = await new CoreRequest(new Uri(LastFmConst.MethodBaseSecure), null, "POST", parameters).Execute();

            if (!LastFmErrorProcessor.ProcessError(response))
                return null;

            if (response["session"] != null)
            {
                return LastFmAuthResult.FromJson(response["session"]);
            }

            return null;
        }
    }
}
