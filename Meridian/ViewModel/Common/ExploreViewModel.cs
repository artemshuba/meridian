using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services;
using Meridian.Services.VK;
using Meridian.View.Common;
using Meridian.View.Compact.Vk;
using Meridian.View.Discovery;
using Meridian.View.VK;
using System;
using System.Collections.Generic;
using System.Linq;
using VkLib.Core.Groups;
using VkLib.Core.Users;

namespace Meridian.ViewModel.Common
{
    public class ExploreViewModel : ViewModelBase
    {
        private VkTracksService _tracksService;
        private List<CatalogBlock> _blocks;

        #region Commands

        /// <summary>
        /// Play special block tracks
        /// </summary>
        public DelegateCommand<CatalogBlock> PlaySpecialBlockCommand { get; protected set; }

        /// <summary>
        /// Play track from recent command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackFromRecentBlockCommand { get; protected set; }

        /// <summary>
        /// Play track from new tracks command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackFromNewBlockCommand { get; protected set; }

        /// <summary>
        /// Play track from similar tracks command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackFromSimilarToBlockCommand { get; protected set; }

        /// <summary>
        /// Play track from popular tracks command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackFromPopularBlockCommand { get; protected set; }

        /// <summary>
        /// Go to profile command
        /// </summary>
        public DelegateCommand<VkProfileBase> GoToProfileCommand { get; private set; }

        /// <summary>
        /// Show more tracks command
        /// </summary>
        public DelegateCommand<List<IAudio>> ShowMoreTracksCommand { get; private set; }

        /// <summary>
        /// Show more playlists command
        /// </summary>
        public DelegateCommand<List<IPlaylist>> ShowMorePlaylistsCommand { get; private set; }

        /// <summary>
        /// Show more friends command
        /// </summary>
        public DelegateCommand<List<VkProfileBase>> ShowMorePeopleCommand { get; private set; }

        /// <summary>
        /// Go to playlist command
        /// </summary>
        public DelegateCommand<IPlaylist> GoToPlaylistCommand { get; private set; }

        #endregion

        /// <summary>
        /// Blocks
        /// </summary>
        public List<CatalogBlock> Blocks
        {
            get { return _blocks; }
            set
            {
                Set(ref _blocks, value);
            }
        }

        public ExploreViewModel()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();

            RegisterTasks("blocks");

            Load();
        }

        protected override void InitializeCommands()
        {
            PlaySpecialBlockCommand = new DelegateCommand<CatalogBlock>(block =>
            {
                if (block.Tracks.IsNullOrEmpty())
                    return;

                AudioService.Instance.PlayAudio(block.Tracks.First(), block.Tracks);

                Analytics.TrackEvent(AnalyticsEvent.ExplorePlaySpecial);
            });

            PlayTrackFromRecentBlockCommand = new DelegateCommand<IAudio>(track =>
            {
                var block = Blocks.FirstOrDefault(b => b.Source == "recoms_recent_audios");
                AudioService.Instance.PlayAudio(track, block.Tracks);

                Analytics.TrackEvent(AnalyticsEvent.ExplorePlayRecent);
            });

            PlayTrackFromNewBlockCommand = new DelegateCommand<IAudio>(track =>
            {
                var block = Blocks.FirstOrDefault(b => b.Source == "recoms_new_audios");
                AudioService.Instance.PlayAudio(track, block.Tracks);

                Analytics.TrackEvent(AnalyticsEvent.ExplorePlayNew);
            });

            PlayTrackFromSimilarToBlockCommand = new DelegateCommand<IAudio>(track =>
            {
                var block = Blocks.FirstOrDefault(b => b.Source == "recoms_recent_recommendation");
                AudioService.Instance.PlayAudio(track, block.Tracks);

                Analytics.TrackEvent(AnalyticsEvent.ExplorePlaySimilar);
            });

            PlayTrackFromPopularBlockCommand = new DelegateCommand<IAudio>(track =>
            {
                var block = Blocks.FirstOrDefault(b => b.Source == "recoms_top_audios_global");
                AudioService.Instance.PlayAudio(track, block.Tracks);

                Analytics.TrackEvent(AnalyticsEvent.ExplorePlayPopular);
            });

            GoToProfileCommand = new DelegateCommand<VkProfileBase>(profile =>
            {
                if (profile is VkProfile)
                {
                    NavigationService.Navigate(typeof(FriendMusicView), new Dictionary<string, object>
                    {
                        ["friend"] = profile
                    });

                    Analytics.TrackEvent(AnalyticsEvent.ExploreCommunity);
                }
                else
                {
                    NavigationService.Navigate(typeof(SocietyMusicView), new Dictionary<string, object>
                    {
                        ["society"] = profile
                    });

                    Analytics.TrackEvent(AnalyticsEvent.ExploreCommunity, new Dictionary<string, object>
                    {
                        ["communityId"] = profile.Id,
                        ["communityName"] = profile.Name
                    });
                }
            });

            ShowMoreTracksCommand = new DelegateCommand<List<IAudio>>(tracks =>
            {
                NavigationService.Navigate(typeof(TracklistView), new Dictionary<string, object>
                {
                    ["tracks"] = tracks
                });
            });

            ShowMorePlaylistsCommand = new DelegateCommand<List<IPlaylist>>(playlists =>
            {
                NavigationService.Navigate(typeof(PlaylistsListView), new Dictionary<string, object>
                {
                    ["playlists"] = playlists
                });
            });

            ShowMorePeopleCommand = new DelegateCommand<List<VkProfileBase>>(people =>
            {
                if (people.FirstOrDefault() is VkGroup)
                    NavigationService.Navigate(typeof(SocietiesView));
                else
                    NavigationService.Navigate(typeof(FriendsView));
            });

            GoToPlaylistCommand = new DelegateCommand<IPlaylist>(playlist =>
            {
                NavigationService.Navigate(typeof(PlaylistView), new Dictionary<string, object>
                {
                    ["playlist"] = playlist
                });

                AnalyticsTrackPlaylistOpen(playlist);
            });
        }

        private async void Load()
        {
            var t = TaskStarted("blocks");

            try
            {
                Blocks = await _tracksService.GetPersonalRecommendations();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load personal recommendations");
            }
            finally
            {
                t.Finish();
            }
        }

        private void AnalyticsTrackPlaylistOpen(IPlaylist playlist)
        {
            //looking for block with specified playlist
            var block = _blocks.FirstOrDefault(b => b.Playlists != null && b.Playlists.Any(p => p.Id == playlist.Id));

            if (block == null)
                return;

            AnalyticsEvent analyticsEvent;

            switch (block.Source)
            {
                case "recoms_playlists":
                    analyticsEvent = AnalyticsEvent.ExplorePlaylist;
                    break;

                case "recoms_new_albums":
                    analyticsEvent = AnalyticsEvent.ExploreNewAlbum;
                    break;

                default:
                    return;
            }

            Analytics.TrackEvent(analyticsEvent, new Dictionary<string, object>
            {
                ["playlistId"] = playlist.Id,
                ["playlistTitle"] = playlist.Title
            });
        }
    }
}
