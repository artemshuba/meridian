using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Services;
using Meridian.View.Flyouts;
using Meridian.View.Flyouts.Local;

namespace Meridian.ViewModel.Local
{
    public class LocalMusicViewModel : ViewModelBase
    {
        private List<LocalAudio> _tracks;
        private List<AudioAlbum> _albums;
        private double _progress;
        private int _selectedTabIndex;

        #region Commands

        /// <summary>
        /// Play audio command
        /// </summary>
        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        #endregion

        public List<LocalAudio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }


        public List<AudioAlbum> Albums
        {
            get { return _albums; }
            set { Set(ref _albums, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set(ref _selectedTabIndex, value); }
        }

        public LocalMusicViewModel()
        {
            InitializeCommands();

            RegisterTasks("tracks","albums");

            LoadTracks();
            LoadAlbums();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });
        }

        private async void LoadTracks()
        {
            OnTaskStarted("tracks");

            try
            {
                Tracks = await ServiceLocator.LocalMusicService.GetTracks();

                if (Tracks == null || Tracks.Count == 0)
                {
                    var flyout = new FlyoutControl();
                    flyout.FlyoutContent = new MusicScanView();
                    await flyout.ShowAsync();

                    Tracks = await ServiceLocator.LocalMusicService.GetTracks();
                }
            }
            catch (Exception ex)
            {
                OnTaskError("tracks", "~Unable to load tracks");

                LoggingService.Log(ex);
            }

            OnTaskFinished("tracks");
        }

        private async void LoadAlbums()
        {
            OnTaskStarted("albums");

            try
            {
                Albums = await ServiceLocator.LocalMusicService.GetAlbums();
            }
            catch (Exception ex)
            {
                OnTaskError("albums", "~Unable to load albums");

                LoggingService.Log(ex);
            }

            OnTaskFinished("albums");
        }
    }
}
