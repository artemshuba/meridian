using System.Collections.Generic;
using SQLite;

namespace Meridian.Model
{
    public class AudioArtist
    {
        [PrimaryKey]
        [Unique]
        [NotNull]
        public string Id { get; set; }

        public string Title { get; set; }

        [Ignore]
        public List<Audio> Tracks { get; set; }

        public AudioArtist()
        {
            Tracks = new List<Audio>();
        }
    }
}
