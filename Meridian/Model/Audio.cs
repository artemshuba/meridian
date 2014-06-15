using System.ComponentModel;
using VkLib.Core.Audio;

namespace Meridian.Model
{
    public class Audio : VkAudio, INotifyPropertyChanged
    {
        private bool _isPlaying;
        private string _lyrics;
        private int _order;
        private bool _isAddedByCurrentUser;
        private string _artistId;

        public new string Title
        {
            get { return base.Title; }
            set
            {
                if (base.Title == value)
                    return;

                base.Title = value;
                OnPropertyChanged("Title");
            }
        }

        public new string Artist
        {
            get { return base.Artist; }
            set
            {
                if (base.Artist == value)
                    return;

                base.Artist = value;
                OnPropertyChanged("Artist");
            }
        }

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

        public bool HasLyrics
        {
            get { return LyricsId != 0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
