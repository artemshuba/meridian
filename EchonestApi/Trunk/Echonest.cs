using EchonestApi.Core.Artist;
using EchonestApi.Core.Playlist;
using EchonestApi.Core.Song;

namespace EchonestApi
{
    /// <summary>
    /// Core object for data access
    /// </summary>
    public class Echonest
    {
        public string ApiKey { get; set; }

        public PlaylistRequest Playlist
        {
            get
            {
                return new PlaylistRequest(this);
            }
        }

        public ArtistRequest Artist
        {
            get
            {
                return new ArtistRequest(this);
            }
        }

        public SongRequest Song
        {
            get
            {
                return new SongRequest(this);
            }
        }

        public Echonest(string apiKey)
        {
            ApiKey = apiKey;
        }
    }
}
