using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View.Flyouts;
using Neptune.Messages;
using VkLib.Core.Auth;
using VkLib.Error;

namespace Meridian.ViewModel.Main
{
    public class LoginViewModel : ViewModelBase
    {
        private string _login;
        private string _password;
        private bool _canLogin;
        private bool _isLoginFormVisible;

        private string _captchaImg;
        private string _captchaSid;
        private string _captchaKey;
        private bool _showCaptcha;
        private string _loginError;

        #region Commands

        /// <summary>
        /// Команда авторизации
        /// </summary>
        public RelayCommand LoginCommand { get; private set; }

        /// <summary>
        /// Команда перехода к регистрации ВКонтакте
        /// </summary>
        public RelayCommand SignUpVkCommand { get; private set; }

        /// <summary>
        /// Команда перехода к форме авторизации
        /// </summary>
        public RelayCommand LoginVkCommand { get; private set; }

        /// <summary>
        /// Команда отмены авторизации
        /// </summary>
        public RelayCommand CancelLoginVkCommand { get; private set; }

        #endregion

        /// <summary>
        /// Логин
        /// </summary>
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

        /// <summary>
        /// Пароль
        /// </summary>
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

        /// <summary>
        /// Доступна ли кнопка авторизации
        /// </summary>
        public bool CanLogin
        {
            get
            {
                return _canLogin;
            }
            set
            {
                if (_canLogin == value)
                    return;

                _canLogin = value;

                RaisePropertyChanged("CanLogin");
            }
        }

        /// <summary>
        /// Отображается ли форма авторизации
        /// </summary>
        public bool IsLoginFormVisible
        {
            get { return _isLoginFormVisible; }
            set
            {
                if (_isLoginFormVisible == value)
                    return;

                _isLoginFormVisible = value;
                RaisePropertyChanged("IsLoginFormVisible");
            }
        }

        /// <summary>
        /// Ссылка на капчу
        /// </summary>
        public string CaptchaImg
        {
            get { return _captchaImg; }
            set
            {
                Set(ref _captchaImg, value);
            }
        }

        /// <summary>
        /// Ключ капчи
        /// </summary>
        public string CaptchaKey
        {
            get { return _captchaKey; }
            set
            {
                Set(ref _captchaKey, value);
            }
        }

        /// <summary>
        /// Отображается ли капча
        /// </summary>
        public bool ShowCaptcha
        {
            get { return _showCaptcha; }
            set
            {
                Set(ref _showCaptcha, value);
            }
        }

        public string LoginError
        {
            get { return _loginError; }
            set { Set(ref _loginError, value); }
        }

        public LoginViewModel()
        {
            InitiailizeCommands();
        }

        private void InitiailizeCommands()
        {
            LoginCommand = new RelayCommand(DoLogin);
            SignUpVkCommand = new RelayCommand(DoSignUp);

            LoginVkCommand = new RelayCommand(() => IsLoginFormVisible = true);
            CancelLoginVkCommand = new RelayCommand(() =>
            {
                IsLoginFormVisible = false;
                LoginError = null;
            });
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
            LoginError = null;

            try
            {
                await AccountManager.LoginVk(Login, Password, _captchaSid, _captchaKey);

                MessengerInstance.Send(new NavigateToPageMessage() {Page = "/Main.MusicView"});
            }
            catch (VkCaptchaNeededException ex)
            {
                CaptchaImg = ex.CaptchaImg;
                _captchaSid = ex.CaptchaSid;

                ShowCaptcha = true;

                IsWorking = false;
                IsLoginFormVisible = true;
            }
            catch (VkInvalidClientException ex)
            {
                LoginError = ErrorResources.LoginErrorInvalidClient;
                IsWorking = false;

                LoggingService.Log(ex.ToString());
            }
            catch (VkNeedValidationException ex)
            {
                LoginError = ErrorResources.LoginErrorNeedValidation;
                IsWorking = false;

                ValidateUser(ex.RedirectUri);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex.ToString());

                //var message = ErrorResources.LoginCommonError;
                //MessageBoxHelper.Show(message);

                IsWorking = false;
            }
        }

        private void DoSignUp()
        {
            var uri = new Uri("http://vk.com");

            Process.Start(uri.OriginalString);
        }

        private async void ValidateUser(Uri redirectUri)
        {
            var flyout = new FlyoutControl();
            flyout.FlyoutContent = new WebValidationView(redirectUri);
            var token = await flyout.ShowAsync() as AccessToken;
            if (token != null)
            {
                AccountManager.SetLoginVk(token);

                MessengerInstance.Send(new NavigateToPageMessage() { Page = "/Main.MusicView" });
            }
        }
    }
}
