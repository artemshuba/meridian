using Jupiter.Mvvm;
using LastFmLib;
using Meridian.Services;
using Meridian.Utils.Helpers;
using System;

namespace Meridian.ViewModel.Common
{
    public class LastFmLoginViewModel : PopupViewModelBase
    {
        private string _login;

        private string _password;

        private bool _canLogin;

        #region Commands

        public DelegateCommand LoginCommand { get; private set; }

        #endregion

        public string Login
        {
            get { return _login; }
            set
            {
                if (Set(ref _login, value))
                    UpdateCanLogin();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (Set(ref _password, value))
                    UpdateCanLogin();
            }
        }

        public bool CanLogin
        {
            get { return _canLogin; }
            set
            {
                Set(ref _canLogin, value);
            }
        }

        public LastFmLoginViewModel()
        {
            RegisterTasks("login");
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            LoginCommand = new DelegateCommand(DoLogin);
        }

        private void UpdateCanLogin()
        {
            CanLogin = !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password) && !Operations["login"].IsWorking;
        }

        private async void DoLogin()
        {
            var t = TaskStarted("login");

            UpdateCanLogin();

            try
            {
                var lastFm = Ioc.Resolve<LastFm>();
                var result = await lastFm.Auth.GetMobileSession(Login, Password);
                if (result != null)
                {
                    AppState.LastFmSession = result;
                    lastFm.SessionKey = result.Key;

                    Close(null);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to login to last.fm");

                t.Error = Resources.GetStringByKey("LastFmLogin_Error");
            }
            finally
            {
                t.Finish();
                UpdateCanLogin();
            }
        }
    }
}