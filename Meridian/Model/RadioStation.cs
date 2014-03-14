using System.Collections.Generic;
using System.ComponentModel;
using EchonestApi.Core.Artist;
using EchonestApi.Core.Playlist;
using Newtonsoft.Json;

namespace Meridian.Model
{
    public class RadioStation : INotifyPropertyChanged
    {
        private List<EchoArtist> _artists;
        private List<EchoSong> _songs; 
        private string _imageUrl;
        private string _title;
        private bool _isPlaying;

        public List<EchoArtist> Artists
        {
            get { return _artists; }
            set
            {
                if (_artists == value)
                    return;

                _artists = value;
                OnPropertyChanged("Artists");
            }
        }

        public List<EchoSong> Songs
        {
            get { return _songs; }
            set
            {
                if (_songs == value)
                    return;

                _songs = value;
                OnPropertyChanged("Songs");
            }
        }

        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                if (_imageUrl == value)
                    return;

                _imageUrl = value;
                OnPropertyChanged("ImageUrl");
            }
        }

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

        [JsonIgnore]
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
