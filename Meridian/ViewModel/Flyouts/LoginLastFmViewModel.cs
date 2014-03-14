using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LastFmLib.Error;
using Meridian.Controls;
using Meridian.Resources.Localization;
using Meridian.Services;
using Neptune.UI.Extensions;

namespace Meridian.ViewModel.Flyouts
{
    public class LoginLastFmViewModel : ViewModelBase
    {
        private string _login;
        private string _password;
        private bool _canLogin;
        private string _loginError;

        #region Commands

        public RelayCommand CancelCommand { get; private set; }

        public RelayCommand LoginCommand { get; private set; }

        #endregion

        public string Login
        {
            get { return _login; }
            set
            {
                if (_login == value)
                    return;

                _login = value;
                UpdateCanLogin();
                RaisePropertyChanged("Login");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value)
                    return;

                _password = value;
                UpdateCanLogin();
                RaisePropertyChanged("Password");
            }
        }

        public bool CanLogin
        {
            get
            {
                return _canLogin;
            }
            set { Set(ref _canLogin, value); }
        }

        public string LoginError
        {
            get { return _loginError; }
            set { Set(ref _loginError, value); }
        }

        public LoginLastFmViewModel()
        {
            InitalizeCommands();
        }

        private void InitalizeCommands()
        {
            CancelCommand = new RelayCommand(() =>
            {
                var flyout = Application.Current.MainWindow.GetVisualDescendents().FirstOrDefault(c => c is FlyoutControl) as FlyoutControl;
                if (flyout != null)
                    flyout.Close();
            });

            LoginCommand = new RelayCommand(DoLogin);
        }

        private void UpdateCanLogin()
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
                CanLogin = false;
            else
                CanLogin = true;
        }

        private async void DoLogin()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
                return;

            IsWorking = true;
            CanLogin = false;

            try
            {
                await AccountManager.LoginLastFm(Login, Password);

                CancelCommand.Execute(null);
            }
            catch (LastFmLoginException ex)
            {
                LoggingService.Log(ex.ToString());

                LoginError = ErrorResources.LoginErrorInvalidClient;
                CanLogin = true;
                IsWorking = false;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex.ToString());

                CanLogin = true;
                IsWorking = false;
            }
        }
    }
}
