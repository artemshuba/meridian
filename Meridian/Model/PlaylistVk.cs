using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using Meridian.Interfaces;
using System.Linq;
using VkLib.Core.Audio;

namespace Meridian.Model
{
    public class PlaylistVk : BindableBase, IPlaylist
    {
        private string _title;

        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
            }
        }

        public string Artist { get; set; }

        public string Description { get; set; }

        public int TracksCount { get; set; }

        public bool IsAddedByCurrentUser { get; set; }

        public string Image { get; set; }

        public bool IsEditable { get; set; }

        public string AccessKey { get; set; }

        public PlaylistVk()
        {

        }

        public PlaylistVk(VkAudioAlbum album)
        {
            Id = album.Id.ToString();
            OwnerId = album.OwnerId.ToString();
            Title = album.Title;
        }

        public PlaylistVk(VkPlaylist playlist)
        {
            Id = playlist.Id.ToString();
            OwnerId = playlist.OwnerId.ToString();
            Title = playlist.Title;
            TracksCount = playlist.Count;
            IsEditable = playlist.Original == null && IsAddedByCurrentUser;
            AccessKey = playlist.AccessKey;
            Description = playlist.Description;

            if (playlist.Photo != null)
                Image = playlist.Photo?.Photo135;
            else if (playlist.Thumbs?.Count > 0)
            {
                var thumb = playlist.Thumbs.FirstOrDefault();
                Image = thumb.Photo135;
            }

            if (!playlist.Artists.IsNullOrEmpty())
                Artist = playlist.Artists.First().Name;
        }
    }
}