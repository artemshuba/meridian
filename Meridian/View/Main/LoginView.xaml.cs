using System.Windows.Input;
using Meridian.Controls;
using Meridian.ViewModel;
using Meridian.ViewModel.Main;

namespace Meridian.View.Main
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : PageBase
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
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

        public override void OnNavigatedTo()
        {
            ViewModelLocator.Main.ShowSidebar = false;
        }
    }
}
