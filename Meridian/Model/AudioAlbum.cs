using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Meridian.Helpers;
using SQLite;

namespace Meridian.Model
{
    public class AudioAlbum : INotifyPropertyChanged
    {
        private ImageSource _cover;
        private bool _coverRequested;

        [PrimaryKey]
        [Unique]
        [NotNull]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string ArtistId { get; set; }

        public string CoverPath { get; set; }

        public int Year { get; set; }

        [Ignore]
        public List<Audio> Tracks { get; set; }

        [Ignore]
        public ImageSource Cover
        {
            get
            {
                GetCover();
                return _cover;
            }
            set
            {
                if (_cover == value)
                    return;

                _cover = value;
                OnPropertyChanged();
            }
        }

        private void GetCover()
        {
            if (_coverRequested || string.IsNullOrEmpty(CoverPath) || !File.Exists(CoverPath))
                return;
            _coverRequested = true;

            AlbumCoversHelper.RequestCover(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
