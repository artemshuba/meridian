using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Meridian.ViewModel;
using Meridian.ViewModel.People;
using VkLib.Core.Users;

namespace Meridian.View.People
{
    /// <summary>
    /// Interaction logic for SocietiesView.xaml
    /// </summary>
    public partial class SocietiesView : Page
    {
        private SocietiesViewModel _viewModel;

        public SocietiesView()
        {
            InitializeComponent();

            _viewModel = new SocietiesViewModel();
            this.DataContext = _viewModel;
        }

        private void SocietiesView_OnLoaded(object sender, RoutedEventArgs e)
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
            return profile.Name.ToLower().Contains(query);
        }
    }
}
