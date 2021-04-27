using Jupiter.Mvvm;
using Meridian.Controls;
using Meridian.Interfaces;
using Meridian.Services;
using Meridian.Utils.Helpers;
using Meridian.ViewModel.VK;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Meridian.ViewModel.Common
{
    public class NowPlayingViewModel : TracksViewModelBase
    {
        #region Commands

        public DelegateCommand ClearAllCommand { get; private set; }

        #endregion

        public NowPlayingViewModel()
        {
            Tracks = AudioService.Instance.CurrentPlaylist.Items as ObservableCollection<IAudio>;
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            ClearAllCommand = new DelegateCommand(() =>
            {
                AudioService.Instance.CurrentPlaylist.ClearAllExceptCurrent();
            });
        }

        protected override void InitializeToolbar()
        {
            var shuffleItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_ShuffleAll"),
                Icon = new SymbolIcon(Symbol.Shuffle),
                Command = ShuffleAllCommand,
            };

            var clearItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_ClearAll"),
                Icon = new SymbolIcon(Symbol.Cancel),
                Command = ClearAllCommand,
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

            var selectionModeItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Select"),
                Command = SwitchSelectionModeCommand,
                Icon = new SymbolIcon(Symbol.Bullets)
            };

            sortItem.SelectedItem = sortItem.Items.First();

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { shuffleItem, clearItem, (ToolbarItem)sortItem, selectionModeItem });
        }
    }
}