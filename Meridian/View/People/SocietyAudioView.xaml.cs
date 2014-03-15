using System.Windows;
using System.Windows.Controls.Primitives;
using Meridian.Controls;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.People;
using VkLib.Core.Groups;

namespace Meridian.View.People
{
    /// <summary>
    /// Interaction logic for SocietyAudioView.xaml
    /// </summary>
    public partial class SocietyAudioView : PageBase
    {
        private SocietyAudioViewModel _viewModel;

        public SocietyAudioView()
        {
            InitializeComponent();

            _viewModel = new SocietyAudioViewModel();
            this.DataContext = _viewModel;
        }


        public override void OnNavigatedTo()
        {
            var society = (VkGroup)NavigationContext.Parameters["society"];
            _viewModel.SelectedSociety = society;

            _viewModel.Activate();

            LocalSearchBox.Filter = Filter;
        }

        private void SocietyAudioView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Deactivate();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
        }

        private void LocalSearchItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
            LocalSearchBox.IsActive = true;
        }

        private bool Filter(object o)
        {
            var track = (Audio)o;
            var query = LocalSearchBox.Query.ToLower();
            return track.Title.ToLower().StartsWith(query) || track.Artist.ToLower().StartsWith(query);
        }
    }
}
