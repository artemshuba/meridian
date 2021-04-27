using System.Collections.Generic;
using Jupiter.Mvvm;
using Meridian.Model.Discovery;
using Microsoft.UI.Xaml.Navigation;
using System;
using Meridian.Services;
using Meridian.Services.Discovery;
using Meridian.Interfaces;
using System.Linq;

namespace Meridian.ViewModel.Discovery
{
    public class AlbumViewModel : ViewModelBase
    {
        private readonly DiscoveryService _discoveryService;

        private DiscoveryAlbum _album;
        private List<DiscoveryTrack> _tracks;

        #region Commands

        /// <summary>
        /// Play track command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Album
        /// </summary>
        public DiscoveryAlbum Album
        {
            get { return _album; }
            set
            {
                if (Set(ref _album, value))
                    Load();
            }
        }

        /// <summary>
        /// Tracks
        /// </summary>
        public List<DiscoveryTrack> Tracks
        {
            get { return _tracks; }
            set
            {
                Set(ref _tracks, value);
            }
        }

        public AlbumViewModel()
        {
            _discoveryService = Ioc.Resolve<DiscoveryService>();

            RegisterTasks("tracks");
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Album = (DiscoveryAlbum)parameters["album"];

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            PlayTrackCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.PlayAudio(track, Tracks.OfType<IAudio>().ToList());
            });
        }

        private async void Load()
        {
            var t = TaskStarted("tracks");

            try
            {
                Tracks = await _discoveryService.GetAlbumTracks(Album.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load album tracks");
            }
            finally
            {
                t.Finish();
            }
        }
    }
}