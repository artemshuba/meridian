using System.Collections.Generic;
using System.Threading.Tasks;
using Meridian.Model;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System;
using Meridian.Services;
using Meridian.Interfaces;
using Jupiter.Mvvm;

namespace Meridian.ViewModel.VK
{
    public class PlaylistViewModel : TracksViewModelBase
    {
        private PlaylistVk _playlist;

        #region Commands

        public DelegateCommand FollowCommand { get; private set; }

        #endregion

        public PlaylistVk Playlist
        {
            get { return _playlist; }
            set
            {
                Set(ref _playlist, value);
            }
        }

        public bool IsAddedToMyMusic
        {
            get { return Playlist.IsAddedByCurrentUser; }
        }

        public PlaylistViewModel()
        {
            RegisterTasks("follow");
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Playlist = (PlaylistVk)parameters["playlist"];

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            FollowCommand = new DelegateCommand(FollowPlaylist);
        }

        protected override async Task Load(bool force = false)
        {
            TaskStarted("tracks");

            try
            {
                var tracks = await _tracksService.GetTracks(userId: long.Parse(Playlist.OwnerId), albumId: long.Parse(Playlist.Id), accessKey: Playlist.AccessKey);
                if (tracks == null)
                    return;

                Tracks = new ObservableCollection<IAudio>(tracks);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load tracks from playlist");
            }
            finally
            {
                TaskFinished("tracks");
            }
        }

        private async void FollowPlaylist()
        {
            TaskStarted("follow");

            try
            {
                var result = await _tracksService.FollowPlaylist(Playlist);
                if (result)
                {
                    RaisePropertyChanged(nameof(IsAddedToMyMusic));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to follow playlist");
            }
            finally
            {
                TaskFinished("follow");
            }
        }
    }
}