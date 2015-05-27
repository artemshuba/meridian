using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Neptune.UI.Extensions;

namespace Meridian.ViewModel.Flyouts
{
    public class EditAudioViewModel : ViewModelBase
    {
        private VkAudio _track;
        private string _title;
        private string _artist;
        private string _lyrics;
        private bool _lyricsChanged;

        #region Commands

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand CloseCommand { get; private set; }

        #endregion

        public VkAudio Track
        {
            get { return _track; }
            set
            {
                if (Set(ref _track, value))
                    Load();
            }
        }

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        public string Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public string Lyrics
        {
            get { return _lyrics; }
            set
            {
                if (Set(ref _lyrics, value))
                    _lyricsChanged = true;
            }
        }

        public EditAudioViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            CloseCommand = new RelayCommand(Close);

            SaveCommand = new RelayCommand(Save);
        }

        private async void Load()
        {
            Title = _track.Title;
            Artist = _track.Artist;

            if (_track.HasLyrics && string.IsNullOrEmpty(_track.Lyrics))
                await LoadLyrics();
            else
            {
                _lyrics = _track.Lyrics;
                RaisePropertyChanged("Lyrics");
            }
        }

        private async Task LoadLyrics()
        {
            IsWorking = true;

            try
            {
                _lyrics = await DataService.GetLyrics(_track.LyricsId.ToString());
                RaisePropertyChanged("Lyrics");
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async void Save()
        {
            IsWorking = true;

            try
            {
                var lyricsId = await DataService.EditAudio(_track.Id, _track.OwnerId.ToString(), Title, Artist, Lyrics);
                if (lyricsId != null)
                {
                    _track.Title = Title;
                    _track.Artist = Artist;
                    if (lyricsId != "0")
                        _track.LyricsId = long.Parse(lyricsId);
                    else
                        _track.LyricsId = 0;
                    Close();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private void Close()
        {
            var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
            if (flyout != null)
                flyout.Close();
        }
    }
}
