using System.Windows;
using System.Windows.Controls.Primitives;
using Meridian.Controls;
using Meridian.Model;
using Meridian.ViewModel;
using Meridian.ViewModel.People;
using VkLib.Core.Users;

namespace Meridian.View.People
{
    /// <summary>
    /// Interaction logic for FriendsAudio.xaml
    /// </summary>
    public partial class FriendAudioView : PageBase
    {
        private FriendAudioViewModel _viewModel;

        public FriendAudioView()
        {
            InitializeComponent();

            _viewModel = new FriendAudioViewModel();
            this.DataContext = _viewModel;
        }

        public override void OnNavigatedTo()
        {
            var friend = (VkProfile)NavigationContext.Parameters["friend"];
            _viewModel.SelectedFriend = friend;

            _viewModel.Activate();

            LocalSearchBox.Filter = Filter;
        }

        private void FriendAudioView_OnUnloaded(object sender, RoutedEventArgs e)
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
