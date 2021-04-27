using System.Collections.Generic;
using Meridian.Model;
using Meridian.ViewModel.Common;
using Microsoft.UI.Xaml.Navigation;
using Jupiter.Mvvm;
using System;
using Meridian.Services.VK;
using Meridian.Services;

namespace Meridian.ViewModel.VK
{
    public class EditTrackViewModel : PopupViewModelBase
    {
        private readonly VkTracksService _tracksService;

        private AudioVk _track;

        private string _title;
        private string _artist;

        private string _lyrics;

        private bool _lyricsChanged;

        #region Commands

        public DelegateCommand SaveCommand { get; private set; }

        #endregion

        public AudioVk Track
        {
            get { return _track; }
            private set
            {
                Set(ref _track, value);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
            }
        }

        public string Artist
        {
            get { return _artist; }
            set
            {
                Set(ref _artist, value);
            }
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

        public EditTrackViewModel()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Track = (AudioVk)parameters["track"];

            Title = Track.Title;
            Artist = Track.Artist;

            if (Track.LyricsId != 0)
                LoadLyrics();

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            SaveCommand = new DelegateCommand(Save);
        }

        private async void LoadLyrics()
        {
            try
            {
                Lyrics = await _tracksService.GetTrackLyrics(Track.LyricsId);

                _lyricsChanged = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load lyrics");
            }
        }

        private async void Save()
        {
            //TODO errors
            try
            {
                var lyrics = _lyricsChanged ? Lyrics.Replace("\r", Environment.NewLine) : null;
                var lyricsId = await _tracksService.EditTrack(_track, Title, Artist, lyrics);

                _track.LyricsId = lyricsId;

                _track.Title = Title;
                _track.Artist = Artist;

                Close(_track);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to edit track");
            }
        }
    }
}