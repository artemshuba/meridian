using System;
using System.Collections.Generic;
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
        private double _progress;

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

        public double Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }

        public LocalMusicViewModel()
        {
            InitializeCommands();

            RegisterTask("tracks");

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
            //await ServiceLocator.LocalMusicService.ScanMusic(new Progress<double>(UpdateProgress));
            Tracks = await ServiceLocator.LocalMusicService.GetLocalTracks();

            if (Tracks == null || Tracks.Count == 0)
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new MusicScanView();
                flyout.Show();

                Tracks = await ServiceLocator.LocalMusicService.GetLocalTracks();
            }
        }

        private void UpdateProgress(double progress)
        {
            Progress = progress;
        }
    }
}
