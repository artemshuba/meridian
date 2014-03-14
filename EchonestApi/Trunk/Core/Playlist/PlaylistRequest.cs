using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EchonestApi.Core.Playlist
{
    public enum EchoPlaylistType
    {
        Artist,
        /// <summary>
        /// Plays songs for the given artists and similar artists.
        /// </summary>
        ArtistRadio,
        /// <summary>
        /// Plays songs from artists matching the given description, style or mood.
        /// </summary>
        ArtistDescription,
        /// <summary>
        /// Plays songs similar to the song specified.
        /// </summary>
        SongRadio,
        /// <summary>
        /// Plays songs from artists matching the given genre.
        /// </summary>
        GenreRadio
    }

    public class PlaylistRequest
    {
        private readonly Echonest _echonest;

        internal PlaylistRequest(Echonest echonest)
        {
            _echonest = echonest;
        }

        /// <summary>
        /// Returns a basic playlist. A basic playlist is generated once from an initial set of parameters, and returned as an ordered list of songs.
        /// <remarks>
        /// Songs are never repeated.
        /// Artists may be repeated.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public async Task<List<EchoSong>> Basic(EchoPlaylistType type = EchoPlaylistType.ArtistRadio, IEnumerable<string> artistIds = null, IEnumerable<string> artists = null, IEnumerable<string> songIds = null, IEnumerable<string> genres = null,
                                IEnumerable<string> trackIds = null, int count = 15)
        {
            var parameters = new Dictionary<string, object>();

            switch (type)
            {
                case EchoPlaylistType.Artist:
                case EchoPlaylistType.ArtistDescription:
                    throw new ArgumentOutOfRangeException("type", "Basic playlist doesn't support artist and artist-description type.");

                case EchoPlaylistType.ArtistRadio:
                    parameters.Add("type", "artist-radio");
                    break;

                case EchoPlaylistType.SongRadio:
                    parameters.Add("type", "song-radio");
                    break;

                case EchoPlaylistType.GenreRadio:
                    parameters.Add("type", "genre-radio");
                    break;
            }

            if (artistIds != null)
                parameters.Add("artist_id", artistIds);

            if (artists != null)
                parameters.Add("artist", artists);

            if (songIds != null)
                parameters.Add("song_id", songIds);

            if (genres != null)
                parameters.Add("genre", genres);

            if (trackIds != null)
                parameters.Add("track_id", trackIds);

            if (count > 0)
                parameters.Add("results", count.ToString(CultureInfo.InvariantCulture));

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "playlist/basic"), parameters).Execute();

            var token = response.SelectToken("response.songs");
            if (token != null && token.HasValues)
            {
                return new List<EchoSong>(token.Select(EchoSong.FromJson).ToList());
            }

            return null;
        }

        public async Task<List<EchoSong>> Static(EchoPlaylistType type = EchoPlaylistType.Artist,
            float variety = 0.5f, IEnumerable<string> artistIds = null, IEnumerable<string> artists = null,
            IEnumerable<string> songIds = null, IEnumerable<string> trackIds = null, string genres = null,
            int count = 15, IEnumerable<string> descriptions = null, IEnumerable<string> styles = null, IEnumerable<string> moods = null)
        {
            var parameters = new Dictionary<string, object>();

            switch (type)
            {
                case EchoPlaylistType.Artist:
                    parameters.Add("type", "artist");
                    break;

                case EchoPlaylistType.ArtistRadio:
                    parameters.Add("type", "artist-radio");
                    break;

                case EchoPlaylistType.ArtistDescription:
                    parameters.Add("type", "artist-description");
                    break;

                case EchoPlaylistType.SongRadio:
                    parameters.Add("type", "song-radio");
                    break;

                case EchoPlaylistType.GenreRadio:
                    parameters.Add("type", "genre-radio");
                    break;
            }

            if (artistIds != null)
                parameters.Add("artist_id", artistIds);

            if (artists != null)
                parameters.Add("artist", artists);

            if (songIds != null)
                parameters.Add("song_id", songIds);

            if (genres != null)
                parameters.Add("genre", genres);

            if (trackIds != null)
                parameters.Add("track_id", trackIds);

            if (count > 0)
                parameters.Add("results", count.ToString(CultureInfo.InvariantCulture));

            if (descriptions != null)
                parameters.Add("description", descriptions);

            if (styles != null)
                parameters.Add("style", styles);

            if (moods != null)
                parameters.Add("mood", moods);

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "playlist/static"), parameters).Execute();

            var token = response.SelectToken("response.songs");
            if (token != null && token.HasValues)
            {
                return new List<EchoSong>(token.Select(EchoSong.FromJson).ToList());
            }

            return null;
        }

        public async Task<Tuple<string, EchoSong>> DynamicCreate(EchoPlaylistType type = EchoPlaylistType.ArtistRadio, IEnumerable<string> artistIds = null, IEnumerable<string> artists = null, IEnumerable<string> songIds = null, IEnumerable<string> genres = null,
                                IEnumerable<string> trackIds = null, bool returnTrack = true)
        {
            var parameters = new Dictionary<string, object>();

            switch (type)
            {
                case EchoPlaylistType.Artist:
                case EchoPlaylistType.ArtistDescription:
                    throw new ArgumentOutOfRangeException("type", "Basic playlist doesn't support artist and artist-description type.");

                case EchoPlaylistType.ArtistRadio:
                    parameters.Add("type", "artist-radio");
                    break;

                case EchoPlaylistType.SongRadio:
                    parameters.Add("type", "song-radio");
                    break;

                case EchoPlaylistType.GenreRadio:
                    parameters.Add("type", "genre-radio");
                    break;
            }

            if (artistIds != null)
                parameters.Add("artist_id", artistIds);

            if (artists != null)
                parameters.Add("artist", artists);

            if (songIds != null)
                parameters.Add("song_id", songIds);

            if (genres != null)
                parameters.Add("genre", genres);

            if (trackIds != null)
                parameters.Add("track_id", trackIds);

            if (returnTrack)
                parameters.Add("results", "1");

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "playlist/dynamic/create"), parameters).Execute();

            string sessionId = null;
            EchoSong track = null;

            var token = response.SelectToken("response.songs");
            if (token != null && token.HasValues)
            {
                track = token.Select(EchoSong.FromJson).FirstOrDefault();
            }

            if (response["response"]["session_id"] != null)
                sessionId = response["response"]["session_id"].Value<string>();

            return new Tuple<string, EchoSong>(sessionId, track);
        }

        public async Task<List<EchoSong>> DynamicNext(string sessionId, int count = 1)
        {
            var parameters = new Dictionary<string, object>();

            parameters.Add("session_id", sessionId);
            parameters.Add("results", count.ToString());

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "playlist/dynamic/next"), parameters).Execute();

            var token = response.SelectToken("response.songs");
            if (token != null && token.HasValues)
            {
                return new List<EchoSong>(token.Select(EchoSong.FromJson).ToList());
            }

            return null;
        }

        public async Task DynamicFeedback(string sessionId, IEnumerable<string> banArtists = null, IEnumerable<string> favoriteArtists = null, IEnumerable<string> banSongs = null,
            IEnumerable<string> skipSongs = null, IEnumerable<string> favoriteSongs = null, IEnumerable<string> invalidateSong = null)
        {
            var parameters = new Dictionary<string, object>();

            parameters.Add("session_id", sessionId);

            if (banArtists != null)
                parameters.Add("ban_artist", banArtists);

            if (favoriteArtists != null)
                parameters.Add("favorite_artist", favoriteArtists);

            if (banSongs != null)
                parameters.Add("ban_song", banSongs);

            if (skipSongs != null)
                parameters.Add("skip_song", skipSongs);

            if (favoriteSongs != null)
                parameters.Add("favorite_song", favoriteSongs);

            if (invalidateSong != null)
                parameters.Add("invalidate_song", invalidateSong);

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "playlist/dynamic/feedback"), parameters).Execute();
        }

        public async Task DynamicDelete(string sessionId)
        {
            var parameters = new Dictionary<string, object>();

            parameters.Add("session_id", sessionId);

            parameters.Add("api_key", _echonest.ApiKey);

            var response = await new EchoRequest(new Uri(EchoConst.MethodBase + "playlist/dynamic/delete"), parameters).Execute();
        }
    }
}
