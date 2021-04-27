using System;

namespace Meridian.Interfaces
{
    /// <summary>
    /// Audio
    /// </summary>
    public interface IAudio
    {
        /// <summary>
        /// Id
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Owner id
        /// </summary>
        string OwnerId { get; set; }

        /// <summary>
        /// Internal id to differentiate audios with same id
        /// </summary>
        string InternalId { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Artist
        /// </summary>
        string Artist { get; set; }

        /// <summary>
        /// Playlist id
        /// </summary>
        long PlaylistId { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        TimeSpan Duration { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        Uri Source { get; set; }

        /// <summary>
        /// Album cover
        /// </summary>
        Uri AlbumCover { get; set; }
    }
}
