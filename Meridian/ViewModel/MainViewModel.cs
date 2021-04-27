using GalaSoft.MvvmLight.Messaging;
using Jupiter.Mvvm;
using Meridian.Controls;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services;
using Meridian.Services.VK;
using Meridian.Utils.Messaging;
using Meridian.View;
using Meridian.View.VK;
using Meridian.View.Settings;
using System;
using System.Collections.Generic;
using VkLib.Core.Users;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Linq;
using Microsoft.UI.Xaml;

namespace Meridian.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private VkTracksService _tracksService;
        private VkProfile _currentUser;

        #region Commands

        /// <summary>
        /// Go to settings
        /// </summary>
        public DelegateCommand GoToSettingsCommand { get; private set; }

        /// <summary>
        /// Play single track
        /// </summary>
        public DelegateCommand<IAudio> PlaySingleTrackCommand { get; private set; }

        /// <summary>
        /// Add track to now playing
        /// </summary>
        public DelegateCommand<IAudio> AddTrackToNowPlayingCommand { get; private set; }

        /// <summary>
        /// Remove track from now playing
        /// </summary>
        public DelegateCommand<IAudio> RemoveTrackFromNowPlayingCommand { get; private set; }

        /// <summary>
        /// Play next
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackNextCommand { get; private set; }

        /// <summary>
        /// Add track to my music
        /// </summary>
        public DelegateCommand<AudioVk> AddTrackToMyMusicCommand { get; private set; }

        /// <summary>
        /// Edit track
        /// </summary>
        public DelegateCommand<AudioVk> EditTrackCommand { get; private set; }

        /// <summary>
        /// Remove track from my music
        /// </summary>
        public DelegateCommand<AudioVk> RemoveTrackFromMyMusicCommand { get; private set; }

        /// <summary>
        /// Show lyrics for track
        /// </summary>
        public DelegateCommand<AudioVk> ShowTrackLyricsCommand { get; private set; }

        /// <summary>
        /// Find more music for specified track
        /// </summary>
        public DelegateCommand<IAudio> FindMoreForTrackCommand { get; private set; }

        /// <summary>
        /// Copy track title to clipboard
        /// </summary>
        public DelegateCommand<IAudio> CopyTrackTitleCommand { get; private set; }

        /// <summary>
        /// Search command
        /// </summary>
        public DelegateCommand<string> SearchCommand { get; private set; }


        /// <summary>
        /// Common command to go to user profile (opens web page)
        /// </summary>
        public DelegateCommand<AudioPost> GoToPostAuthorCommand { get; private set; }

        /// <summary>
        /// Common command to go to post (opens web page)
        /// </summary>
        public DelegateCommand<AudioPost> GoToPostCommand { get; private set; }

        /// <summary>
        /// Play playlist command
        /// </summary>
        public DelegateCommand<PlaylistVk> PlayPlaylistCommand { get; private set; }

        /// <summary>
        /// Edit playlist command
        /// </summary>
        public DelegateCommand<PlaylistVk> EditPlaylistCommand { get; private set; }

        /// <summary>
        /// Delete playlist command
        /// </summary>
        public DelegateCommand<PlaylistVk> DeletePlaylistCommand { get; private set; }

        /// <summary>
        /// Save playlist command
        /// </summary>
        public DelegateCommand<PlaylistVk> SavePlaylistCommand { get; private set; }


        /// <summary>
        /// Add track to playlist command
        /// </summary>
        public DelegateCommand<AudioVk> AddTrackToPlaylistCommand { get; private set; }

        #endregion

        /// <summary>
        /// Current user
        /// </summary>
        public VkProfile CurrentUser
        {
            get { return _currentUser; }
            set { Set(ref _currentUser, value); }
        }

        public MainViewModel()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();

            Messenger.Default.Register<MessageUserAuthChanged>(this, OnMessageUserAuthChanged);
        }

        protected override void InitializeCommands()
        {
            GoToSettingsCommand = new DelegateCommand(() =>
            {
               (Application.Current as App).NavigationService.Navigate(typeof(SettingsView));
            });

            PlaySingleTrackCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.PlayAudio(track, new List<IAudio> { track });
            });

            AddTrackToNowPlayingCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.CurrentPlaylist.Add(track);
            });

            RemoveTrackFromNowPlayingCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.CurrentPlaylist.Remove(track);
            });

            PlayTrackNextCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.CurrentPlaylist.AddAfterCurrent(track);
            });

            AddTrackToMyMusicCommand = new DelegateCommand<AudioVk>(AddTrackToMyMusic);

            EditTrackCommand = new DelegateCommand<AudioVk>(async track =>
            {
                await PopupControl.Show<EditTrackView>(new Dictionary<string, object>
                {
                    ["track"] = track
                });
            });

            RemoveTrackFromMyMusicCommand = new DelegateCommand<AudioVk>(RemoveTrackFromMyMusic);

            ShowTrackLyricsCommand = new DelegateCommand<AudioVk>(async track =>
            {
                await PopupControl.Show<TrackLyricsView>(new Dictionary<string, object>
                {
                    ["track"] = track
                });
            });

            FindMoreForTrackCommand = new DelegateCommand<IAudio>(track =>
            {
                (Application.Current as App).NavigationService.Navigate(typeof(SearchView), new Dictionary<string, object>
                {
                    ["query"] = track.Artist
                });
            });

            CopyTrackTitleCommand = new DelegateCommand<IAudio>(track =>
            {
                var data = new DataPackage();
                data.SetText($"{track.Artist} - {track.Title}");
                Clipboard.SetContent(data);
            });

            SearchCommand = new DelegateCommand<string>(query =>
            {
                (Application.Current as App).NavigationService.Navigate(typeof(SearchView), new Dictionary<string, object>
                {
                    ["query"] = query
                });
            });

            GoToPostAuthorCommand = new DelegateCommand<AudioPost>(async post =>
            {
                await Launcher.LaunchUriAsync(post.AuthorUri);
            });

            GoToPostCommand = new DelegateCommand<AudioPost>(async post =>
            {
                await Launcher.LaunchUriAsync(post.PostUri);
            });

            PlayPlaylistCommand = new DelegateCommand<PlaylistVk>(async playlist =>
            {
                try
                {
                    var tracks = await _tracksService.GetTracks(long.Parse(playlist.OwnerId), long.Parse(playlist.Id), accessKey: playlist.AccessKey);
                    if (tracks.Count > 0)
                        AudioService.Instance.PlayAudio(tracks.First(), tracks);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to play playlist");
                }
            });

            EditPlaylistCommand = new DelegateCommand<PlaylistVk>(async playlist =>
            {
                await PopupControl.Show<EditPlaylistView>(new Dictionary<string, object>
                {
                    ["playlist"] = playlist
                });
            });

            DeletePlaylistCommand = new DelegateCommand<PlaylistVk>(async playlist =>
            {
                //TODO unfollow
                if (await _tracksService.DeletePlaylist(playlist))
                    Messenger.Default.Send(new MessagePlaylistRemoved() { Playlist = playlist });
            });

            AddTrackToPlaylistCommand = new DelegateCommand<AudioVk>(track =>
            {
                AddTracksToPlaylist(new List<AudioVk> { track });
            });

            SavePlaylistCommand = new DelegateCommand<PlaylistVk>(async playlist =>
            {
                try
                {
                    await _tracksService.FollowPlaylist(playlist);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to follow playlist");
                }
            });
        }

        private async void LoadCurrentUser()
        {
            try
            {
                var service = Ioc.Resolve<VkUserService>();
                CurrentUser = await service.GetUser();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load current user");
            }
        }

        private async void AddTrackToMyMusic(AudioVk track)
        {
            if (track.IsAddedByCurrentUser)
                return;

            try
            {
                //TODO captcha

                if (track.Id == null || track.OwnerId == null)
                {
                    //TODO resolve
                    throw new NotImplementedException();
                    //var resolvedTrack = await ServiceLocator.MusicResolverService.ResolveTrack(audio.Title, audio.Artist, audio.Duration);
                    //audio.Id = resolvedTrack.Id;
                    //audio.OwnerId = resolvedTrack.OwnerId;
                }

                var result = await _tracksService.AddTrack(track); //ServiceLocator.VkDataService.AddAudio(audio);
                if (result)
                {
                    Messenger.Default.Send(new MessageMyMusicChanged() { AddedTracks = new List<AudioVk> { track } });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to add track");
            }
        }

        private async void RemoveTrackFromMyMusic(AudioVk track)
        {
            if (!track.IsAddedByCurrentUser)
                return;

            try
            {
                //TODO captcha

                var result = await _tracksService.RemoveTrack(track);
                if (result)
                {
                    Messenger.Default.Send(new MessageMyMusicChanged() { RemovedTracks = new List<AudioVk> { track } });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to remove track");
            }
        }

        private async void AddTracksToPlaylist(List<AudioVk> tracks)
        {
            var playlist = (PlaylistVk)await PopupControl.Show<SelectPlaylistView>();
            if (playlist == null)
                return;

            try
            {
                await _tracksService.AddTracksToPlaylist(tracks, playlist.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to add tracks to playlist");
            }
        }

        #region Messaging

        private void OnMessageUserAuthChanged(MessageUserAuthChanged message)
        {
            if (message.IsLoggedIn)
                LoadCurrentUser();
            else
            {
                if (AppState.VkToken == null) //if we already logged out, do nothing
                    return;

                CurrentUser = null;
                AppState.Reset();

                (Application.Current as App).NavigationService.Navigate(typeof(LoginView), clearHistory: true);
            }
        }

        #endregion
    }
}
