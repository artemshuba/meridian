namespace Meridian.Interfaces
{
    /// <summary>
    /// Playlist
    /// </summary>
    public interface IPlaylist
    {
        string Id { get; set; }

        string Title { get; set; }

        int TracksCount { get; set; }

        string Artist { get; set; }

        string Description { get; set; }
    }
}
