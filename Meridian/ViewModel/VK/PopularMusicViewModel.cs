using System.Threading.Tasks;
using System;
using Meridian.Services;
using System.Collections.ObjectModel;
using Meridian.Interfaces;
using System.Collections.Generic;
using Meridian.Utils.Helpers;
using VkLib.Core.Audio;
using System.Linq;
using Meridian.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Meridian.ViewModel.VK
{
    public class PopularMusicViewModel : TracksViewModelBase
    {
        private int _selectedFilterTypeIndex;
        private List<VkGenre> _genres;
        private VkGenre _selectedGenre;

        /// <summary>
        /// Filter types
        /// </summary>
        public List<string> FilterTypes { get; } = new List<string>()
        {
            Resources.GetStringByKey("Toolbar_PopularFilterAll"),
            Resources.GetStringByKey("Toolbar_PopularFilterForeign")
        };

        /// <summary>
        /// Selected filter type index
        /// </summary>
        public int SelectedFilterTypeIndex
        {
            get { return _selectedFilterTypeIndex; }
            set
            {
                if (Set(ref _selectedFilterTypeIndex, value))
                {
                    _ = Load();
                }
            }
        }

        /// <summary>
        /// Genres
        /// </summary>
        public List<VkGenre> Genres
        {
            get { return _genres; }
            set { Set(ref _genres, value); }
        }

        /// <summary>
        /// Selected genre
        /// </summary>
        public VkGenre SelectedGenre
        {
            get { return _selectedGenre; }
            set
            {
                if (Set(ref _selectedGenre, value))
                {
                    _ = Load();
                }
            }
        }

        public PopularMusicViewModel()
        {
            _genres = _tracksService.GetGenres();
            _genres.Insert(0, new VkGenre() { Title = Resources.GetStringByKey("Genres_All") });

            _selectedGenre = _genres.First();
        }

        protected override async Task Load(bool force = false)
        {
            IsToolbarEnabled = false;

            TaskStarted("tracks");

            try
            {
                var tracks = await _tracksService.GetPopularTracks(foreignOnly: _selectedFilterTypeIndex == 1, genreId: _selectedGenre.Id);

                Tracks = new ObservableCollection<IAudio>(tracks);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load popular tracks");
            }

            TaskFinished("tracks");

            IsToolbarEnabled = true;
        }

        protected override void InitializeToolbar()
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
                    this.SelectedSortType = SelectedSortType = SortTypes[index];
                }
            };

            var filterItem = new ToolbarPicker()
            {
                Title = Resources.GetStringByKey("Toolbar_PopularFilter"),
                Items = {
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_PopularFilterAll") },
                    new ToolbarButton() { Title = Resources.GetStringByKey("Toolbar_PopularFilterForeign") },
                },

                OnSelectedItemChanged = index =>
                {
                    this.SelectedFilterTypeIndex = index;
                }
            };

            var selectionModeItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Select"),
                Command = SwitchSelectionModeCommand,
                Icon = new SymbolIcon(Symbol.Bullets)
            };

            sortItem.SelectedItem = sortItem.Items.First();
            filterItem.SelectedItem = filterItem.Items.First();

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { shuffleItem, (ToolbarItem)sortItem, (ToolbarItem)filterItem, selectionModeItem, refreshItem});
        }
    }
}
