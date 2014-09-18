using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Meridian.ViewModel;
using Neptune.UI.Extensions;
using VkLib.Core.Audio;
using VkAudio = Meridian.Model.VkAudio;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for AddToAlbumView.xaml
    /// </summary>
    public partial class AddToAlbumView : UserControl, INotifyPropertyChanged
    {
        private List<VkAudioAlbum> _albums;
        private VkAudio _track;

        public List<VkAudioAlbum> Albums
        {
            get { return _albums; }
            set
            {
                if (_albums == value)
                    return;

                _albums = value;
                OnPropertyChanged("Albums");
            }
        }

        public AddToAlbumView(VkAudio track)
        {
            _track = track;

            InitializeComponent();
        }


        private void Close(bool result = false)
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close(result);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void AddToAlbumView_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var albums = await DataService.GetUserAlbums();
                if (albums != null && albums.Items != null)
                {
                    Albums = albums.Items;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void AddButtonClick(object sender, RoutedEventArgs e)
        {
            var album = AlbumsComboBox.SelectedValue as VkAudioAlbum;
            if (album == null && !string.IsNullOrEmpty(AlbumsComboBox.Text))
            {
                album = await AddNewAlbum(AlbumsComboBox.Text);
            }

            if (album == null)
                return;

            if (!_track.IsAddedByCurrentUser)
            {
                var result = await DataService.AddAudio(_track);
                if (!result)
                {
                    LoggingService.Log("Unable to add audio " + _track.Id + " to current user.");
                    return;
                }
            }

            try
            {
                var result = await ViewModelLocator.Vkontakte.Audio.MoveToAlbum(album.Id,
                    new List<long>() { long.Parse(_track.Id) });
                if (!result)
                {
                    LoggingService.Log("Unable to move audio " + _track.Id + " to album " + album.Id + ".");
                    return;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            Close();
        }

        private async Task<VkAudioAlbum> AddNewAlbum(string title)
        {
            try
            {
                var newAlbumId = await ViewModelLocator.Vkontakte.Audio.AddAlbum(title);
                if (newAlbumId != 0)
                {
                    var album = new VkAudioAlbum();
                    album.Id = newAlbumId;
                    album.OwnerId = ViewModelLocator.Vkontakte.AccessToken.UserId;
                    album.Title = title;
                    return album;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }
    }
}
