using System.Windows.Controls;
using System.Windows.Input;
using Meridian.ViewModel;
using Meridian.ViewModel.Flyouts;

namespace Meridian.View.Flyouts
{
    /// <summary>
    /// Interaction logic for LoginLastFmView.xaml
    /// </summary>
    public partial class LoginLastFmView : UserControl
    {
        private LoginLastFmViewModel _viewModel;

        public LoginLastFmView()
        {
            InitializeComponent();

            _viewModel = new LoginLastFmViewModel();
            this.DataContext = _viewModel;
        }

        private void LoginTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!string.IsNullOrEmpty(LoginTextBox.Text))
            {
                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    _viewModel.LoginCommand.Execute(null);
                }
                else
                {
                    PasswordBox.Child.Focus();
                }
            }
        }

        private void PasswordBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                if (!string.IsNullOrEmpty(LoginTextBox.Text))
                {
                    _viewModel.LoginCommand.Execute(null);
                }
                else
                {
                    LoginTextBox.Focus();
                }
            }
        }
    }
}
