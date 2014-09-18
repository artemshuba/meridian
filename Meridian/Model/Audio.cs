using System;
using System.ComponentModel;
using Newtonsoft.Json;
using SQLite;

namespace Meridian.Model
{
    /// <summary>
    /// Base audio class
    /// </summary>
    public class Audio : INotifyPropertyChanged
    {
        private string _title;
        private string _artist;
        private TimeSpan _duration;

        private bool _isPlaying;
        private string _lyrics;
        private int _order;
        private bool _isAddedByCurrentUser;
        private string _artistId;

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// File path or url
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;

                _title = value;
                OnPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Artist
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set
            {
                if (_artist == value)
                    return;

                _artist = value;
                OnPropertyChanged("Artist");
            }
        }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (_duration == value)
                    return;

                _duration = value;
                OnPropertyChanged("Duration");
            }
        }

        /// <summary>
        /// Is audio playing
        /// </summary>
        [JsonIgnore]
        [Ignore]
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying == value)
                    return;

                _isPlaying = value;
                OnPropertyChanged("IsPlaying");
            }
        }

        /// <summary>
        /// Audio order in list (used for numeric lists)
        /// </summary>
        [JsonIgnore]
        [Ignore]
        public int Order
        {
            get { return _order; }
            set
            {
                if (_order == value)
                    return;

                _order = value;
                OnPropertyChanged("Order");
            }
        }

        /// <summary>
        /// Returns a copy of current audio object
        /// </summary>
        /// <returns></returns>
        public virtual Audio Clone()
        {
            var audio = new Audio();
            audio.Id = this.Id;
            audio.Title = this.Title;
            audio.Artist = this.Artist;
            audio.Duration = this.Duration;

            return audio;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Vk audio
    /// </summary>
    public class VkAudio : Audio
    {
        private string _lyrics;
        private bool _isAddedByCurrentUser;

        /// <summary>
        /// Album id
        /// </summary>
        public long AlbumId { get; set; }

        /// <summary>
        /// Owner id
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// Lyrics
        /// </summary>
        public string Lyrics
        {
            get { return _lyrics; }
            set
            {
                if (_lyrics == value)
                    return;

                _lyrics = value;
                OnPropertyChanged("Lyrics");
            }
        }

        /// <summary>
        /// Lyrics id
        /// </summary>
        public long LyricsId { get; set; }

        /// <summary>
        /// Genre id
        /// </summary>
        public long GenreId { get; set; }

        /// <summary>
        /// Has lyrics (used for binding)
        /// </summary>
        public bool HasLyrics
        {
            get { return LyricsId != 0; }
        }

        /// <summary>
        /// Is audio added by current user
        /// </summary>
        public bool IsAddedByCurrentUser
        {
            get { return _isAddedByCurrentUser; }
            set
            {
                if (_isAddedByCurrentUser == value)
                    return;

                _isAddedByCurrentUser = value;
                OnPropertyChanged("IsAddedByCurrentUser");
            }
        }

        public override Audio Clone()
        {
            var audio = new VkAudio();
            audio.Id = this.Id;
            audio.OwnerId = this.OwnerId;
            audio.AlbumId = this.AlbumId;
            audio.Title = this.Title;
            audio.Artist = this.Artist;
            audio.Duration = this.Duration;
            audio.GenreId = this.GenreId;
            audio.Lyrics = this.Lyrics;
            audio.LyricsId = this.LyricsId;
            audio.IsAddedByCurrentUser = this.IsAddedByCurrentUser;

            return audio;
        }
    }

    public class LocalAudio : Audio
    {

    }
}
