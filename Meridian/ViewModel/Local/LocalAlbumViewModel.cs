using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Meridian.Model;
using Meridian.Services;

namespace Meridian.ViewModel.Local
{
    public class LocalAlbumViewModel : ViewModelBase
    {
        private AudioAlbum _album;
        private List<LocalAudio> _tracks;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        #endregion

        public AudioAlbum Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public List<LocalAudio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public LocalAlbumViewModel()
        {
            InitializeCommands();

            RegisterTasks("tracks");
        }

        public override void Activate()
        {
            Load();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });
        }

        private async void Load()
        {
            OnTaskStarted("tracks");

            try
            {
                Tracks = await ServiceLocator.LocalMusicService.GetAlbumTracks(Album.Id);
            }
            catch (Exception ex)
            {
                OnTaskError("tracks", "~Unable to load tracks");

                LoggingService.Log(ex);
            }

            OnTaskFinished("tracks");
        }
    }
}
