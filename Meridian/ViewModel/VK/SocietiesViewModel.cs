using Jupiter.Mvvm;
using Meridian.Controls;
using Meridian.Enum;
using Meridian.Services.VK;
using Meridian.View.VK;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VkLib.Core.Groups;
using Microsoft.UI.Xaml.Controls;
using Meridian.Utils.Helpers;
using System;
using Meridian.Services;
using Microsoft.UI.Xaml.Data;
using Jupiter.Utils.Extensions;
using Microsoft.UI.Xaml;

namespace Meridian.ViewModel.VK
{
    public class SocietiesViewModel : ViewModelBase
    {
        private readonly VkUserService _userService;

        private List<VkGroup> _societies;

        private CollectionViewSource _societiesCollection; //used for grouping

        protected ObservableCollection<ToolbarItem> _toolbarItems = new ObservableCollection<ToolbarItem>();

        protected PeopleSortType _selectedSortType;

        protected bool _isToolbarEnabled = true;

        #region Commands

        public DelegateCommand RefreshCommand { get; private set; }

        public DelegateCommand<VkGroup> GoToSocietyCommand { get; private set; }

        #endregion

        /// <summary>
        /// Societies
        /// </summary>
        public List<VkGroup> Societies
        {
            get { return _societies; }
            private set
            {
                Set(ref _societies, value);
            }
        }

        /// <summary>
        /// Societies collection
        /// </summary>
        public CollectionViewSource SocietiesCollection
        {
            get { return _societiesCollection; }
            private set
            {
                if (Set(ref _societiesCollection, value))
                    RaisePropertyChanged(nameof(Societies));
            }
        }

        /// <summary>
        /// Societies sort types
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

        public SocietiesViewModel()
        {
            _userService = Ioc.Resolve<VkUserService>();

            RegisterTasks("societies");

            InitializeToolbarItems();

            Load();
        }

        protected override void InitializeCommands()
        {
            GoToSocietyCommand = new DelegateCommand<VkGroup>(society =>
            {
                NavigationService.Navigate(typeof(SocietyMusicView), new Dictionary<string, object>
                {
                    ["society"] = society
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
            var t = TaskStarted("societies");

            try
            {
                var response = await _userService.GetSocieties();
                if (response.Societies != null)
                {
                    Societies = response.Societies;

                    ApplySort();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load societies");
            }
            finally
            {
                t.Finish();
            }
        }

        private void ApplySort()
        {
            if (_societies.IsNullOrEmpty())
                return;

            switch (_selectedSortType)
            {
                //by rating
                case PeopleSortType.Rating:
                    SocietiesCollection = new CollectionViewSource() { Source = _societies };
                    break;

                //by name
                case PeopleSortType.Name:
                    SocietiesCollection = new CollectionViewSource()
                    {
                        Source = _societies.ToAlphaGroups(t => t.Name),
                        ItemsPath = new PropertyPath("Value"),
                        IsSourceGrouped = true
                    };
                    break;
            }
        }
    }
}