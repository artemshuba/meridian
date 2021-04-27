using System;
using Meridian.Interfaces;

namespace Meridian.Model.Discovery
{
    public class DiscoveryTrack : IAudio
    {
        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string InternalId { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public long PlaylistId { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri Source { get; set; }

        public Uri AlbumCover { get; set; }
    }
}