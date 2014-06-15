using System.Collections.Generic;

namespace Meridian.Model
{
    public class AudioArtist
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public List<Audio> Tracks { get; set; }

        public AudioArtist()
        {
            Tracks = new List<Audio>();
        }
    }
}
