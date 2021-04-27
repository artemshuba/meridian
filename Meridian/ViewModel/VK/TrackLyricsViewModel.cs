using Jupiter.Mvvm;
using Meridian.Model;
using Meridian.Services;
using Meridian.Services.VK;
using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.Common
{
    public class TrackLyricsViewModel : PopupViewModelBase
    {
        private readonly VkTracksService _tracksService;

        private string _lyrics;
        private AudioVk _track;

        public AudioVk Track
        {
            get { return _track; }
            private set
            {
                Set(ref _track, value);
            }
        }

        public string Lyrics
        {
            get { return _lyrics; }
            set { Set(ref _lyrics, value); }
        }

        public TrackLyricsViewModel()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();

            RegisterTasks("lyrics");
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Track = (AudioVk)parameters["track"];

            Load();
        }

        protected override void InitializeCommands()
        {
            CloseCommand = new DelegateCommand<object>(result =>
            {
                Close(Lyrics);
            });
        }

        private async void Load()
        {
            var t = TaskStarted("lyrics");

            try
            {
                Lyrics = await _tracksService.GetTrackLyrics(_track.LyricsId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load track lyrics");
            }
            finally
            {
                t.Finish();
            }
        }
    }
}