using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LastFmLib.Core.Track
{
    public class LastFmTrackRequest
    {
        private readonly LastFm _lastFm;

        public LastFmTrackRequest(LastFm lastFm)
        {
            _lastFm = lastFm;
        }

        public async Task<LastFmTrack> GetInfo(string title, string artist, bool autoCorrect = true, string mbid = null)
        {
            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(mbid))
                parameters.Add("mbid", mbid);
            else
            {
                parameters.Add("artist", artist);
                parameters.Add("track", title);
            }

            if (autoCorrect)
                parameters.Add("autocorrect", "1");

            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await new CoreRequest(new Uri(LastFmConst.MethodBase + "track.getInfo"), parameters).Execute();

            LastFmErrorProcessor.ProcessError(response);


            if (response["track"] != null)
            {
                return LastFmTrack.FromJson(response["track"]);
            }

            return null;
        }

        public async Task UpdateNowPlaying(string artist, string track, string mbid = null, int duration = 0,
                                           string album = null, int trackNumber = -1, string albumArtist = null)
        {

            const string method = "track.updateNowPlaying";

            var parameters = new Dictionary<string, string>();

            parameters.Add("artist", artist);
            parameters.Add("track", track);

            if (album != null)
                parameters.Add("album", album);

            if (trackNumber > -1)
                parameters.Add("trackNumber", trackNumber.ToString());

            if (mbid != null)
                parameters.Add("mbid", mbid);

            if (duration > 0)
                parameters.Add("duration", duration.ToString());

            if (albumArtist != null)
                parameters.Add("albumArtist", albumArtist);

            parameters.Add("api_key", _lastFm.ApiKey);
            parameters.Add("sk", _lastFm.SessionKey);
            parameters.Add("api_sig", LastFmUtils.BuildSig(_lastFm.ApiSecret, method, parameters));

            parameters["track"] = Uri.EscapeDataString(track); //fix ampersand scrobbling
            parameters["artist"] = Uri.EscapeDataString(artist); //fix ampersand scrobbling


            var response = await new CoreRequest(new Uri(LastFmConst.UrlBaseSecure), null, "POST", parameters).Execute();

            LastFmErrorProcessor.ProcessError(response);
        }

        public async Task Scrobble(string artist, string track, string timeStamp, string mbid = null, int duration = 0,
                                   string album = null, int trackNumber = -1, string albumArtist = null)
        {

            const string method = "track.scrobble";

            var parameters = new Dictionary<string, string>();

            parameters.Add("artist", artist);
            parameters.Add("track", track);
            parameters.Add("timestamp", timeStamp);

            if (album != null)
                parameters.Add("album", album);

            if (trackNumber > -1)
                parameters.Add("trackNumber", trackNumber.ToString());

            if (mbid != null)
                parameters.Add("mbid", mbid);

            if (albumArtist != null)
                parameters.Add("albumArtist", albumArtist);

            if (duration > 0)
                parameters.Add("duration", duration.ToString());

            parameters.Add("api_key", _lastFm.ApiKey);
            parameters.Add("sk", _lastFm.SessionKey);
            parameters.Add("api_sig", LastFmUtils.BuildSig(_lastFm.ApiSecret, method, parameters));


            parameters["track"] = Uri.EscapeDataString(track); //fix ampersand scrobbling
            parameters["artist"] = Uri.EscapeDataString(artist); //fix ampersand scrobbling

            var response = await new CoreRequest(new Uri(LastFmConst.UrlBaseSecure), null, "POST", parameters).Execute();

            LastFmErrorProcessor.ProcessError(response);
        }

        public async Task<List<LastFmTrack>> Search(string track, string artist)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("track", track);
            if (!string.IsNullOrEmpty(artist))
                parameters.Add("artist", artist);

            parameters.Add("api_key", _lastFm.ApiKey);

            var response = await new CoreRequest(new Uri(LastFmConst.MethodBase + "track.search"), parameters).Execute();

            LastFmErrorProcessor.ProcessError(response);

            if (response.SelectToken("results.trackmatches.track") != null)
            {
                var trackJson = response.SelectToken("results.trackmatches.track");
                if (trackJson is JArray)
                    return (from t in response.SelectToken("results.trackmatches.track")
                           select LastFmTrack.FromJson(t)).ToList();
                else
                    return new List<LastFmTrack>() { LastFmTrack.FromJson(trackJson) };
            }

            return null;
        }
    }
}
