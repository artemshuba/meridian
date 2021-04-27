using Meridian.Controls;
using Meridian.Interfaces;
using Meridian.Utils.Helpers;
using Meridian.ViewModel.VK;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Meridian.ViewModel.Discovery
{
    public class TracklistViewModel : TracksViewModelBase
    {
        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            var tracks = parameters["tracks"] as IList;
            if (tracks != null)
                Tracks = new ObservableCollection<IAudio>(tracks?.OfType<IAudio>().ToList());

            base.OnNavigatedTo(parameters, mode);
        }

        protected override void InitializeToolbar()
        {
            var shuffleItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_ShuffleAll"),
                Icon = new SymbolIcon(Symbol.Shuffle),
                Command = ShuffleAllCommand,
            };

            var selectionModeItem = new ToolbarButton()
            {
                Title = Resources.GetStringByKey("Toolbar_Select"),
                Command = SwitchSelectionModeCommand,
                Icon = new SymbolIcon(Symbol.Bullets)
            };

            ToolbarItems = new ObservableCollection<ToolbarItem>(new[] { shuffleItem, selectionModeItem });
        }
    }
}