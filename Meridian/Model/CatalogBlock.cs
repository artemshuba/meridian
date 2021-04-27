using Meridian.Interfaces;
using System.Collections.Generic;
using VkLib.Core.Audio;
using VkLib.Core.Users;

namespace Meridian.Model
{
    public class CatalogBlock
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Source { get; set; }

        public List<IAudio> Tracks { get; set; }

        public List<VkProfileBase> Owners { get; set; }

        public List<IPlaylist> Playlists { get; set; }

        public List<VkThumb> Thumbs { get; set; }

        public CatalogBlock()
        {

        }

        public CatalogBlock(VkCatalogBlock catalogBlock, List<IAudio> tracks = null, List<IPlaylist> playlists = null, List<IPlaylist> extendedPlaylists = null)
        {
            Id = catalogBlock.Id;
            Title = catalogBlock.Title;
            Subtitle = catalogBlock.Subtitle;
            Source = catalogBlock.Source;
            Owners = catalogBlock.Owners;
            Tracks = tracks;
            Playlists = playlists ?? extendedPlaylists;
            Thumbs = catalogBlock.Thumbs;
        }
    }
}
