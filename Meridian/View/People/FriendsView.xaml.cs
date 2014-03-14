using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.ViewModel;
using Meridian.ViewModel.People;
using VkLib.Core.Users;

namespace Meridian.View.People
{
    /// <summary>
    /// Interaction logic for FriendsView.xaml
    /// </summary>
    public partial class FriendsView : Page
    {
        private FriendsViewModel _viewModel;

        public FriendsView()
        {
            InitializeComponent();

            _viewModel = new FriendsViewModel();
            this.DataContext = _viewModel;
        }

        private void FriendsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Activate();

            LocalSearchBox.Filter = Filter;
        }

        private void LocalSearchItem_OnClick(object sender, RoutedEventArgs e)
        {
            HeaderMenuPopup.SetCurrentValue(Popup.IsOpenProperty, false);
            LocalSearchBox.IsActive = true;
        }

        private bool Filter(object o)
        {
            var profile = (VkProfileBase)o;
            var query = LocalSearchBox.Query.ToLower();
            return profile.Name.ToLower().StartsWith(query);
        }
    }
}
