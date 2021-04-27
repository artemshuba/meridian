using System;

namespace Meridian.Model.Discovery
{
    public class DiscoveryAlbum
    {
        public string Id { get; set; }

        public DiscoveryArtist Artist { get; set; }

        public string Title { get; set; }

        public Uri Cover { get; set; }

        public Uri CoverLarge { get; set; }

        public int TracksCount { get; set; }

        public DateTime? ReleaseDate { get; set; }
    }
}