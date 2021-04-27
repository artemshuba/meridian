using System;
using Meridian.Interfaces;
using VkLib.Core.Audio;
using Jupiter.Mvvm;

namespace Meridian.Model
{
    /// <summary>
    /// Vk audio
    /// </summary>
    public class AudioVk : BindableBase, IAudio
    {
        private bool _isAddedByCurrentUser;
        private string _title;
        private string _artist;

        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string InternalId { get; set; }

        public string Artist
        {
            get
            {
                return _artist;
            }
            set
            {
                Set(ref _artist, value);
            }
        }

        public TimeSpan Duration { get; set; }

        public Uri Source { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
            }
        }

        public long PlaylistId { get; set; }

        public long LyricsId { get; set; }

        public bool IsAddedByCurrentUser
        {
            get { return _isAddedByCurrentUser; }
            set
            {
                Set(ref _isAddedByCurrentUser, value);
            }
        }

        public bool HasLyrics
        {
            get { return LyricsId != 0; }
        }

        public Uri AlbumCover { get; set; }

        public AudioVk()
        {

        }

        public AudioVk(VkAudio audio)
        {
            Id = audio.Id.ToString();
            OwnerId = audio.OwnerId.ToString();
            Title = audio.Title;
            Artist = audio.Artist;
            Duration = audio.Duration;
            PlaylistId = audio.AlbumId;
            LyricsId = audio.LyricsId;
            if (!string.IsNullOrEmpty(audio.Url))
                Source = new Uri(audio.Url);
            if (audio.Album != null && audio.Album.Thumb != null && !string.IsNullOrWhiteSpace(audio.Album.Thumb.Photo300))
                AlbumCover = new Uri(audio.Album.Thumb.Photo300);
        }
    }
}
