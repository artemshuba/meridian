using LastFmLib.Core.Album;
using LastFmLib.Core.Artist;
using LastFmLib.Core.Auth;
using LastFmLib.Core.Chart;
using LastFmLib.Core.Tag;
using LastFmLib.Core.Track;
using LastFmLib.Core.User;

namespace LastFmLib
{
    public class LastFm
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;

        private LastFmArtistRequest _artist;
        private LastFmAlbumRequest _album;
        private LastFmTrackRequest _track;
        private LastFmChartRequest _chart;
        private LastFmAuthRequest _auth;
        private LastFmUserRequest _user;
        private LastFmTagRequest _tag;

        internal string ApiKey
        {
            get { return _apiKey; }
        }

        internal string ApiSecret
        {
            get { return _apiSecret; }
        }

        public LastFmArtistRequest Artist
        {
            get
            {
                if (_artist == null)
                    _artist = new LastFmArtistRequest(this);

                return _artist;
            }
        }

        public LastFmAlbumRequest Album
        {
            get
            {
                if (_album == null)
                    _album = new LastFmAlbumRequest(this);

                return _album;
            }
        }

        public LastFmTrackRequest Track
        {
            get
            {
                if (_track == null)
                    _track = new LastFmTrackRequest(this);

                return _track;
            }
        }


        public LastFmChartRequest Chart
        {
            get
            {
                if (_chart == null)
                    _chart = new LastFmChartRequest(this);

                return _chart;
            }
        }

        public LastFmUserRequest User
        {
            get
            {
                if (_user == null)
                    _user = new LastFmUserRequest(this);

                return _user;
            }
        }

        public LastFmTagRequest Tag
        {
            get
            {
                if (_tag == null)
                    _tag = new LastFmTagRequest(this);

                return _tag;
            }
        }

        public LastFmAuthRequest Auth
        {
            get
            {
                if (_auth == null)
                    _auth = new LastFmAuthRequest(this);
                return _auth;
            }
        }

        public string Lang { get; set; }

        public string SessionKey { get; set; }

        public LastFm(string apiKey, string apiSecret, string lang = null)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            Lang = lang;
        }
    }
}