using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using Meridian.Controls;
using Meridian.Enum;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services;
using Meridian.Services.VK;
using Meridian.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.VK
{
    public class TracksViewModelBase : ViewModelBase
    {
        protected VkTracksService _tracksService;

        protected ObservableCollection<IAudio> _tracks;

        protected TracksSortType _selectedSortType;

        protected CollectionViewSource _tracksCollection;

        protected bool _isToolbarEnabled = true;

        protected bool _isTracksSelectionEnabled;

        protected ObservableCollection<ToolbarItem> _toolbarItems = new ObservableCollection<ToolbarItem>();

        protected ObservableCollection<IAudio> _selectedTracks = new ObservableCollection<IAudio>();

        #region Commands

        /// <summary>
        /// Play track command
        /// </summary>
        public DelegateCommand<IAudio> PlayTrackCommand { get; protected set; }

        /// <summary>
        /// Play track from audio post command
        /// </summary>
        public DelegateCommand<AudioContainer> PlayPostTrackCommand { get; protected set; }

        /// <summary>
        /// Refresh command
        /// </summary>
        public DelegateCommand RefreshCommand { get; protected set; }

        /// <summary>
        /// Selection mode command
        /// </summary>
        public DelegateCommand SwitchSelectionModeCommand { get; protected set; }

        /// <summary>
        /// Play selected tracks command
        /// </summary>
        public DelegateCommand PlaySelectedTracksCommand { get; private set; }

        /// <summary>
        /// Shuffle tracks command
        /// </summary>
        public DelegateCommand ShuffleAllCommand { get; protected set; }

        /// <summary>
        /// Play all tracks (from posts) command
        /// </summary>
        public DelegateCommand PlayAllCommand { get; protected set; }

        #endregion

        /// <summary>
        /// Tracks sort types
        /// </summary>
        public List<TracksSortType> SortTypes { get; } = System.Enum.GetValues(typeof(TracksSortType)).Cast<TracksSortType>().ToList();

        /// <summary>
        /// Selected sort type
        /// </summary>
        public TracksSortType SelectedSortType
        {
            get { return _selectedSortType; }
            set
            {
                if (Set(ref _selectedSortType, value))
                    ApplySort();
            }
        }

        /// <summary>
        /// True - toolbar buttons enabled
        /// </summary>
        public bool IsToolbarEnabled
        {
            get { return _isToolbarEnabled; }
            protected set { Set(ref _isToolbarEnabled, value); }
        }

        /// <summary>
        /// Tracks
        /// </summary>
        public ObservableCollection<IAudio> Tracks
        {
            get { return _tracks; }
            protected set
            {
                if (Set(ref _tracks, value))
                    ApplySort();
            }
        }

        /// <summary>
        /// Grouped tracks collection
        /// </summary>
        public CollectionViewSource TracksCollection
        {
            get { return _tracksCollection; }
            protected set
            {
                Set(ref _tracksCollection, value);
            }
        }

        /// <summary>
        /// Is tracks selection enabled
        /// </summary>
        public bool IsTracksSelectionEnabled
        {
            get { return _isTracksSelectionEnabled; }
            set
            {
                if (Set(ref _isTracksSelectionEnabled, value))
                {
                    if (value)
                        InitializeSelectedToolbarItems();
                    else
                        InitializeToolbar();
                }
            }
        }

        /// <summary>
        /// Toolbar items
        /// </summary>
        public ObservableCollection<ToolbarItem> ToolbarItems
        {
            get { return _toolbarItems; }
            protected set { Set(ref _toolbarItems, value); }
        }

        public TracksViewModelBase()
        {
            _tracksService = Ioc.Resolve<VkTracksService>();

            RegisterTasks("tracks");

            SelectedSortType = SortTypes.First();

            InitializeToolbar();
        }

        public override async void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            await Load();
        }

        public void TracksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;

            foreach (var addedItem in e.AddedItems.OfType<IAudio>())
            {
                _selectedTracks.Add(addedItem);
            }

            foreach (var removedItem in e.RemovedItems.OfType<IAudio>())
            {
                _selectedTracks.Remove(removedItem);
            }
        }


        protected override void InitializeCommands()
        {
            PlayTrackCommand = new DelegateCommand<IAudio>(track =>
            {
                AudioService.Instance.PlayAudio(track, Tracks);
            });

            PlayPostTrackCommand = new DelegateCommand<AudioContainer>(container =>
            {
                AudioService.Instance.PlayAudio(container.Track, container.Tracklist);
            });

            RefreshCommand = new DelegateCommand(async () =>
            {
                await Load(force: true);
            });

            SwitchSelectionModeCommand = new DelegateCommand(() =>
            {
                IsTracksSelectionEnabled = !IsTracksSelectionEnabled;
            });

            PlaySelectedTracksCommand = new DelegateCommand(() =>
            {
                var tracklist = _selectedTracks.ToList();

                IsTracksSelectionEnabled = false;

                if (tracklist.Count == 0)
                    return;

                AudioService.Instance.PlayAudio(tracklist.First(), tracklist);
            });

            ShuffleAllCommand = new DelegateCommand(() =>
            {
                var playlist = Tracks.ToList();
                playlist.Shuffle();
                AudioService.Instance.PlayAudio(playlist.First(), playlist);
            });
        }

        protected virtual void ApplySort()
        {
            switch (_selectedSortType)
            {
                case TracksSortType.DateAdded:
                    TracksCollection = new CollectionViewSource() { Source = _tracks, IsSourceGrouped = false };
                    break;

                case TracksSortType.Title:
                    TracksCollection = new CollectionViewSource()
                    {
                        Source = _tracks.ToAlphaGroups(t => t.Title).Select(g => new AudioGroup(g.Key, g.Value)).ToList(),
                        ItemsPath = new PropertyPath("Items"),
                        IsSourceGrouped = true
                    };
                    break;

                case TracksSortType.Artist:
                    TracksCollection = new CollectionViewSource()
                    {
                        Source = _tracks.GroupBy(t => t.Artist.ToUpper()).OrderBy(g => g.Key).Select(g => new AudioGroup(g.Key, g.ToList())).ToList(),
                        ItemsPath = new PropertyPath("Items"),
                        IsSourceGrouped = true
                    };
                    break;
            }
        }

        protected virtual void InitializeToolbar()
        {
            InitializeTracksToolbarItems();
        }

        protected virtual void InitializeTracksToolbarItems()
        {
            var refreshItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Refresh"),
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Resources/Images/Toolbar/refresh.png") },
                Command = RefreshCommand
            };

            var shuffleItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_ShuffleAll"),
                Icon = new SymbolIcon(Symbol.Shuffle),
                Command = ShuffleAllCommand,
            };

            var sortItem = new ToolbarPicker()
            {
                Title = Resources.GetStringByKey("Toolbar_Sort"),
                Items = {
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByDateAdded") },
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByTitle") },
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByArtist") }
                },

                OnSelectedItemChanged = index =>
                {
                    this.SelectedSortType = SortTypes[index];
                }
            };

            var selectionModeItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Select"),
                Command = SwitchSelectionModeCommand,
                Icon = new SymbolIcon(Symbol.Bullets)
            };

            sortItem.SelectedItem = sortItem.Items.First();

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { shuffleItem, (ToolbarItem)sortItem, selectionModeItem, refreshItem });
        }

        protected virtual void InitializePostsToolbarItems()
        {
            var refreshItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Refresh"),
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Resources/Images/Toolbar/refresh.png") },
                Command = RefreshCommand
            };

            var shuffleItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_ShuffleAll"),
                Icon = new SymbolIcon(Symbol.Shuffle),
                Command = ShuffleAllCommand,
            };

            var playItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_PlayAll"),
                Icon = new SymbolIcon(Symbol.Shuffle),
                Command = PlayAllCommand,
            };

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { playItem, shuffleItem, refreshItem });
        }

        protected virtual void InitializeSelectedToolbarItems()
        {
            var playItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_PlaySelected"),
                Icon = new SymbolIcon(Symbol.Play),
                Command = PlaySelectedTracksCommand
            };

            var cancelItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Cancel"),
                Icon = new SymbolIcon(Symbol.Cancel),
                Command = SwitchSelectionModeCommand
            };

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { playItem, cancelItem });
        }

        protected virtual async Task Load(bool force = false)
        {
            await Task.CompletedTask;
        }

        protected virtual void ShuffleAll()
        {
            var playlist = Tracks.ToList();
            playlist.Shuffle();
            AudioService.Instance.PlayAudio(playlist.First(), playlist);
        }
    }
}
