using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using Meridian.Controls;
using Meridian.Enum;
using Meridian.Services;
using Meridian.Services.VK;
using Meridian.Utils.Helpers;
using Meridian.View.VK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VkLib.Core.Users;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace Meridian.ViewModel.VK
{
    public class FriendsViewModel : ViewModelBase
    {
        private readonly VkUserService _userService;

        private List<VkProfile> _friends;
        private CollectionViewSource _friendsCollection; //used for grouping

        protected ObservableCollection<ToolbarItem> _toolbarItems = new ObservableCollection<ToolbarItem>();

        protected PeopleSortType _selectedSortType;

        protected bool _isToolbarEnabled = true;

        #region Commands

        public DelegateCommand RefreshCommand { get; private set; }

        public DelegateCommand<VkProfile> GoToFriendCommand { get; private set; }

        #endregion

        /// <summary>
        /// Friends
        /// </summary>
        public List<VkProfile> Friends
        {
            get { return _friends; }
            private set
            {
                Set(ref _friends, value);
            }
        }

        /// <summary>
        /// Friends collection
        /// </summary>
        public CollectionViewSource FriendsCollection
        {
            get { return _friendsCollection; }
            private set
            {
                if (Set(ref _friendsCollection, value))
                    RaisePropertyChanged(nameof(Friends));
            }
        }


        /// <summary>
        /// Friends sort types
        /// </summary>
        public List<PeopleSortType> SortTypes { get; } = new List<PeopleSortType>
        {
            PeopleSortType.Rating,
            PeopleSortType.Name
        };

        /// <summary>
        /// Selected sort type
        /// </summary>
        public PeopleSortType SelectedSortType
        {
            get { return _selectedSortType; }
            set
            {
                if (Set(ref _selectedSortType, value))
                    ApplySort();
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

        /// <summary>
        /// True - toolbar buttons enabled
        /// </summary>
        public bool IsToolbarEnabled
        {
            get { return _isToolbarEnabled; }
            protected set { Set(ref _isToolbarEnabled, value); }
        }

        public FriendsViewModel()
        {
            _userService = Ioc.Resolve<VkUserService>();

            RegisterTasks("friends");

            InitializeToolbarItems();

            Load();
        }

        protected override void InitializeCommands()
        {
            GoToFriendCommand = new DelegateCommand<VkProfile>(friend =>
            {
                NavigationService.Navigate(typeof(FriendMusicView), new Dictionary<string, object>
                {
                    ["friend"] = friend
                });
            });
        }

        protected virtual void InitializeToolbarItems()
        {
            var refreshItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Refresh"),
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Resources/Images/Toolbar/refresh.png") },
                Command = RefreshCommand
            };

            var sortItem = new ToolbarPicker()
            {
                Title = Resources.GetStringByKey("Toolbar_Sort"),
                Items = {
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByRating") },
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_SortByName") }
                },

                OnSelectedItemChanged = index =>
                {
                    this.SelectedSortType = SortTypes[index];
                }
            };

            sortItem.SelectedItem = sortItem.Items.First();

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { (ToolbarItem)sortItem, refreshItem });
        }

        private async void Load()
        {
            var t = TaskStarted("friends");

            try
            {
                var response = await _userService.GetFriends();
                if (response.Friends != null)
                {
                    Friends = response.Friends;

                    ApplySort();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load friends");
            }
            finally
            {
                t.Finish();
            }
        }

        private void ApplySort()
        {
            if (_friends.IsNullOrEmpty())
                return;

            switch (_selectedSortType)
            {
                //by rating
                case PeopleSortType.Rating:
                    FriendsCollection = new CollectionViewSource() { Source = _friends };
                    break;

                //by name
                case PeopleSortType.Name:
                    FriendsCollection = new CollectionViewSource()
                    {
                        Source = _friends.ToAlphaGroups(t => t.Name),
                        ItemsPath = new PropertyPath("Value"),
                        IsSourceGrouped = true
                    };
                    break;
            }
        }
    }
}