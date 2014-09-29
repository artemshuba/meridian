using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Meridian.Helpers;
using SQLite;

namespace Meridian.Model
{
    public class AudioArtist : INotifyPropertyChanged
    {
        private ImageSource _image;
        private bool _imageRequested;

        [PrimaryKey]
        [Unique]
        [NotNull]
        public string Id { get; set; }

        public string Title { get; set; }

        [Ignore]
        public List<Audio> Tracks { get; set; }

        [Ignore]
        public List<AudioAlbum> Albums { get; set; }

        [Ignore]
        public ImageSource Image
        {
            get
            {
                GetImage();
                return _image;
            }
            set
            {
                if (_image == value)
                    return;

                _image = value;
                OnPropertyChanged();
            }
        }

        public AudioArtist()
        {
            Tracks = new List<Audio>();
            Albums = new List<AudioAlbum>();
        }


        private void GetImage()
        {
            if (_imageRequested)
                return;
            _imageRequested = true;

            ArtistImageHelper.RequestCover(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
