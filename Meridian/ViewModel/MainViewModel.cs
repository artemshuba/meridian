using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop.Utilities;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View;
using Meridian.View.Flyouts;
using Meridian.ViewModel.Flyouts;
using Meridian.ViewModel.Messages;
using Neptune.Messages;
using Neptune.UI.Extensions;
using VkLib.Core.Users;
using VkLib.Error;
using Yandex.Metrica;

namespace Meridian.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly MenuItemsCollection _mainMenuItems = new MenuItemsCollection()
        {
            new MainMenuItem() {Group = "", Page = "/Main.MusicView", Title = MainResources.MainMenuMyMusic},
            new MainMenuItem() {Group = "", Page = "/Main.PopularAudioView", Title = MainResources.MainMenuPopular},
            new MainMenuItem() {Group = "", Page = "/Main.RecommendationsView", Title = MainResources.MainMenuRecommendations},
            new MainMenuItem() {Group = "", Page = "/Main.RadioView", Title = MainResources.MainMenuRadio},

            new MainMenuItem() {Group = MainResources.MainMenuPeopleTitle, Page = "/People.FriendsView", Title = MainResources.MainMenuFriends},
            new MainMenuItem() {Group = MainResources.MainMenuPeopleTitle, Page = "/People.SocietiesView", Title = MainResources.MainMenuSocieties},
            new MainMenuItem() {Group = MainResources.MainMenuPeopleTitle, Page = "/People.SubscriptionsView", Title = MainResources.MainMenuSubscriptions},

            new MainMenuItem() {Page = "/Main.NowPlayingView", Title = MainResources.MainMenuNowPlaying, Icon = Application.Current.Resources["NowPlayingIcon"]},
        };

        private bool _showSidebar;
        private bool _showShareBar;
        private bool _showWindowButtons = true;
        private MainMenuItem _selectedMainMenuItem;
        private int _selectedMainMenuItemIndex;
        private WindowState _windowState;
        private ImageSource _artistImage;
        private ImageSource _trackImage;
        private bool _artRequested;
        private VkProfile _user;
        private bool _canNavigate = true;
        private HotKeyManager _hotKeyManager;
        private bool _statusUpdated;
        private bool _nowPlayingUpdated;
        private bool _scrobbled;
        private UIMode _currentUIMode;
        private string _lastArtist;
        private CancellationTokenSource _artCancellationToken = new CancellationTokenSource();

        #region Commands

        public RelayCommand CloseWindowCommand { get; private set; }

        public RelayCommand MinimizeWindowCommand { get; private set; }

        public RelayCommand MaximizeWindowCommand { get; private set; }

        public RelayCommand<string> GoToPageCommand { get; private set; }

        public RelayCommand GoBackCommand { get; private set; }

        public RelayCommand GoToSettingsCommand { get; private set; }

        public RelayCommand<string> SearchCommand { get; private set; }

        public RelayCommand NextAudioCommand { get; private set; }

        public RelayCommand PrevAudioCommand { get; private set; }

        public RelayCommand PlayPauseCommand { get; private set; }

        public RelayCommand RestartCommand { get; private set; }

        public RelayCommand SwitchUIModeCommand { get; private set; }


        public RelayCommand<Audio> AddRemoveAudioCommand { get; private set; }

        public RelayCommand<Audio> EditAudioCommand { get; private set; }

        public RelayCommand<Audio> ShareAudioCommand { get; private set; }

        public RelayCommand<Audio> ShowLyricsCommand { get; private set; }

        public RelayCommand<Audio> CopyInfoCommand { get; private set; }

        public RelayCommand<Audio> PlayAudioNextCommand { get; private set; }

        public RelayCommand<Audio> AddToNowPlayingCommand { get; private set; }

        public RelayCommand<Audio> RemoveFromNowPlayingCommand { get; private set; }

        public RelayCommand<string> ShowArtistInfoCommand { get; private set; }

        public RelayCommand<Audio> StartTrackRadioCommand { get; private set; }

        public RelayCommand<Audio> AddToAlbumCommand { get; private set; }

        public RelayCommand ShowLocalSearchCommand { get; private set; }

        #endregion

        #region Window

        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                if (_windowState == value)
                    return;

                _windowState = value;
                if (value == WindowState.Maximized)
                    Settings.Instance.IsWindowMaximized = true;
                else
                    Settings.Instance.IsWindowMaximized = false;
                RaisePropertyChanged("WindowState");
                RaisePropertyChanged("IsWindowMaximized");
            }
        }

        public bool IsWindowMaximized
        {
            get { return WindowState == WindowState.Maximized; }
        }

        public double WindowLeft
        {
            get
            {
                return Settings.Instance.Left;
            }
            set
            {
                Settings.Instance.Left = value;
            }
        }

        public double WindowTop
        {
            get { return Settings.Instance.Top; }
            set { Settings.Instance.Top = value; }
        }

        public double WindowWidth
        {
            get { return Settings.Instance.Width; }
            set { Settings.Instance.Width = value; }
        }

        public double WindowHeight
        {
            get { return Settings.Instance.Height; }
            set
            {
                Settings.Instance.Height = value;
            }
        }

        public string WindowTitle
        {
            get
            {
                if (CurrentAudio != null)
                    return CurrentAudio.Title + " (" + CurrentAudio.Artist + ")";
                else
                    return "Meridian";
            }
        }

        #endregion

        public bool CanGoBack
        {
            get
            {
                var frame = Application.Current.MainWindow.GetVisualDescendents().OfType<Frame>().FirstOrDefault();
                if (frame == null)
                    return false;
                return frame.CanGoBack;
            }
        }

        public MenuItemsCollection MainMenuItems
        {
            get { return _mainMenuItems; }
        }

        public bool ShowSidebar
        {
            get
            {
                if (IsInDesignMode)
                    return true;
                return _showSidebar;
            }
            set
            {
                Set(ref _showSidebar, value);
            }
        }

        public bool ShowShareBar
        {
            get
            {
                return _showShareBar;
            }
            set
            {
                Set(ref _showShareBar, value);
            }
        }


        public bool ShowWindowButtons
        {
            get
            {
                return _showWindowButtons;
            }
            set
            {
                Set(ref _showWindowButtons, value);
            }
        }

        public VkProfile User
        {
            get { return _user; }
            set
            {
                if (_user == value)
                    return;

                _user = value;
                RaisePropertyChanged("User");
            }
        }

        public string LastFmUser
        {
            get { return Settings.Instance.LastFmUsername; }
            set
            {
                if (Settings.Instance.LastFmUsername == value)
                    return;

                Settings.Instance.LastFmUsername = value;
                RaisePropertyChanged("LastFmUser");
            }
        }

        public MainMenuItem SelectedMainMenuItem
        {
            get { return _selectedMainMenuItem; }
            set
            {
                if (Set(ref _selectedMainMenuItem, value) && value != null && _canNavigate)
                    OnNavigateToPage(new NavigateToPageMessage() { Page = value.Page });
            }
        }

        public int SelectedMainMenuItemIndex
        {
            get
            {
                return _selectedMainMenuItemIndex;
            }
            set
            {
                Set(ref _selectedMainMenuItemIndex, value);
            }
        }

        public Audio CurrentAudio
        {
            get
            {
                if (!IsInDesignMode)
                    return AudioService.CurrentAudio;
                else
                {
                    var a = new Audio() { Artist = "Artist", Title = "Title" };
                    return a;
                }
            }
        }

        public RadioStation CurrentRadio
        {
            get { return RadioService.CurrentRadio; }
        }

        public TimeSpan CurrentAudioPosition
        {
            get
            {
                if (IsInDesignMode)
                    return TimeSpan.Zero;
                return AudioService.CurrentAudioPosition;
            }
        }

        public double CurrentAudioPositionSeconds
        {
            get
            {
                if (IsInDesignMode)
                    return 0;
                return AudioService.CurrentAudioPosition.TotalSeconds;
            }
            set
            {
                AudioService.CurrentAudioPosition = TimeSpan.FromSeconds(value);
            }
        }

        public TimeSpan CurrentAudioDuration
        {
            get
            {
                if (AudioService.CurrentAudioDuration == TimeSpan.Zero)
                    return TimeSpan.FromMilliseconds(1000);
                return AudioService.CurrentAudioDuration;
            }
        }

        public bool IsPlaying
        {
            get { return AudioService.IsPlaying; }
            set
            {
                //leave empty, used to avoid binding errors
            }
        }

        public bool Shuffle
        {
            get { return AudioService.Shuffle; }
            set
            {
                if (AudioService.Shuffle == value)
                    return;

                AudioService.Shuffle = value;
                RaisePropertyChanged("Shuffle");
            }
        }

        public bool Repeat
        {
            get { return AudioService.Repeat; }
            set
            {
                if (AudioService.Repeat == value)
                    return;

                AudioService.Repeat = value;
                RaisePropertyChanged("Repeat");
            }
        }

        public float Volume
        {
            get { return (float)Math.Round(AudioService.Volume * 100.0f); }
            set
            {
                if (AudioService.Volume * 100 == value)
                    return;

                AudioService.Volume = value / 100;
                RaisePropertyChanged("Volume");
            }
        }

        public IList<Audio> CurrentPlaylist
        {
            get { return AudioService.Playlist; }
        }

        public ImageSource ArtistImage
        {
            get { return _artistImage; }
            set { Set(ref _artistImage, value); }
        }

        public ImageSource TrackImage
        {
            get { return _trackImage; }
            set { Set(ref _trackImage, value); }
        }

        public HotKeyManager HotKeyManager
        {
            get { return _hotKeyManager; }
        }

        public bool EnableStatusBroadcasting
        {
            get { return Settings.Instance.EnableStatusBroadcasting; }
            set
            {
                if (Settings.Instance.EnableStatusBroadcasting == value)
                    return;

                Settings.Instance.EnableStatusBroadcasting = value;
                if (!value)
                {
                    DataService.SetMusicStatus(null);
                    _statusUpdated = false;
                }
                RaisePropertyChanged("EnableStatusBroadcasting");
            }
        }

        public bool EnableScrobbling
        {
            get { return Settings.Instance.EnableScrobbling; }
            set
            {
                if (Settings.Instance.EnableScrobbling == value)
                    return;

                if (value && !AccountManager.IsLoggedInLastFm())
                {
                    var flyout = new FlyoutControl();
                    flyout.FlyoutContent = new LoginLastFmMessageView();
                    flyout.Show();
                }
                else
                {
                    Settings.Instance.EnableScrobbling = value;

                    RaisePropertyChanged("EnableScrobbling");
                }
            }
        }

        public UIMode CurrentUIMode
        {
            get { return _currentUIMode; }
            set { Set(ref _currentUIMode, value); }
        }

        public bool ShowTrayIcon
        {
            get { return Settings.Instance.EnableTrayIcon; }
        }

        public bool ShowBackgroundArt
        {
            get { return Settings.Instance.ShowBackgroundArt; }
            set
            {
                if (Settings.Instance.ShowBackgroundArt == value)
                    return;

                Settings.Instance.ShowBackgroundArt = value;
                RaisePropertyChanged("ShowBackgroundArt");
            }
        }

        public MainViewModel()
        {
            InitializeCommands();
            InitializeMessageInterception();
        }

        public void Initialize()
        {
            WindowState = Settings.Instance.IsWindowMaximized
                              ? WindowState.Maximized
                              : WindowState.Normal;

            _hotKeyManager = new HotKeyManager(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            _hotKeyManager.InitializeHotkeys();
            ViewModelLocator.LastFm.SessionKey = Settings.Instance.LastFmSession;
        }

        public async void LoadUserInfo()
        {
            try
            {
                User = await DataService.GetUserInfo();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private void InitializeCommands()
        {
            CloseWindowCommand = new RelayCommand(() => Application.Current.MainWindow.Close());

            MinimizeWindowCommand = new RelayCommand(() => WindowState = WindowState.Minimized);

            MaximizeWindowCommand = new RelayCommand(() => WindowState = IsWindowMaximized ? WindowState.Normal : WindowState.Maximized);

            GoToPageCommand = new RelayCommand<string>(page => OnNavigateToPage(new NavigateToPageMessage() { Page = page }));

            GoToSettingsCommand = new RelayCommand(() =>
            {
                ShowSidebar = false;
                OnNavigateToPage(new NavigateToPageMessage() { Page = "/Settings.SettingsView" });
            });

            PrevAudioCommand = new RelayCommand(AudioService.Prev);

            NextAudioCommand = new RelayCommand(AudioService.SkipNext);

            PlayPauseCommand = new RelayCommand(() =>
            {
                if (IsPlaying)
                    AudioService.Pause();
                else
                    AudioService.Play();
            });

            GoBackCommand = new RelayCommand(() =>
            {
                var frame = Application.Current.MainWindow.GetVisualDescendents().OfType<Frame>().FirstOrDefault(f => f.Name == "RootFrame");
                if (frame == null)
                    return;

                if (frame.CanGoBack)
                    frame.GoBack();

                UpdateCanGoBack();
            });

            SearchCommand = new RelayCommand<string>(query =>
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    MessengerInstance.Send(new NavigateToPageMessage()
                    {
                        Page = "/Search.SearchResultsView",
                        Parameters = new Dictionary<string, object>()
                                {
                                    {"query", query}
                                }
                    });
                }
            });

            RestartCommand = new RelayCommand(() =>
            {
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            });

            AddRemoveAudioCommand = new RelayCommand<Audio>(audio =>
            {
                audio.IsAddedByCurrentUser = !audio.IsAddedByCurrentUser;
                LikeDislikeAudio(audio);
            });

            EditAudioCommand = new RelayCommand<Audio>(audio =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new EditAudioView(audio);
                flyout.Show();
            });

            ShowLyricsCommand = new RelayCommand<Audio>(audio =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new LyricsView(audio);
                flyout.Show();
            });

            CopyInfoCommand = new RelayCommand<Audio>(audio =>
            {
                if (audio == null)
                    return;

                try
                {
                    Clipboard.SetData(DataFormats.UnicodeText, audio.Artist + " - " + audio.Title);
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            });

            PlayAudioNextCommand = new RelayCommand<Audio>(track =>
            {
                AudioService.PlayNext(track);
            });

            AddToNowPlayingCommand = new RelayCommand<Audio>(track =>
            {
                AudioService.Playlist.Add(track);
            });

            RemoveFromNowPlayingCommand = new RelayCommand<Audio>(track =>
            {
                AudioService.Playlist.Remove(track);
            });

            ShareAudioCommand = new RelayCommand<Audio>(audio =>
            {
                ShowShareBar = true;

                //костыль #2
                var shareControl = Application.Current.MainWindow.GetVisualDescendent<ShareBarControl>();
                if (shareControl == null)
                    return;

                var shareViewModel = ((ShareViewModel)((FrameworkElement)shareControl.Content).DataContext);
                shareViewModel.Tracks.Add(audio);
            });

            SwitchUIModeCommand = new RelayCommand(() =>
            {
                if (CurrentUIMode == UIMode.Normal)
                    SwitchUIMode(UIMode.Compact);
                else
                    SwitchUIMode(UIMode.Normal);
            });

            StartTrackRadioCommand = new RelayCommand<Audio>(track =>
            {
                RadioService.StartRadioFromSong(track.Title, track.Artist);
                MessengerInstance.Send(new NavigateToPageMessage() { Page = "/Main.NowPlayingView" });
            });

            ShowArtistInfoCommand = new RelayCommand<string>(async artist =>
            {
                NotificationService.Notify(MainResources.NotificationLookingArtist);

                try
                {
                    var artists = await DataService.SearchArtists(artist);
                    if (artists != null && artists.Count > 0)
                    {
                        var a = artists.FirstOrDefault(x => x.Name.ToLower() == artist.ToLower());
                        if (a == null)
                            a = artists.First();

                        MessengerInstance.Send(new NavigateToPageMessage()
                        {
                            Page = "/Search.ArtistView",
                            Parameters = new Dictionary<string, object>()
                            {
                                {"artist", a}
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                    NotificationService.Notify(MainResources.NotificationArtistNotFound);
                }
            });

            ShowLocalSearchCommand = new RelayCommand(() =>
            {
                var frame = Application.Current.MainWindow.GetVisualDescendents().OfType<Frame>().FirstOrDefault();
                if (frame == null)
                    return;

                var page = (Page)frame.Content;
                if (page != null)
                {
                    var localSearchBox = page.GetVisualDescendents().OfType<LocalSearchControl>().FirstOrDefault();
                    if (localSearchBox != null)
                        localSearchBox.IsActive = true;
                }
            });

            AddToAlbumCommand = new RelayCommand<Audio>(track =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new AddToAlbumView(track);
                flyout.Show();
            });
        }

        private void InitializeMessageInterception()
        {
            MessengerInstance.Register<NavigateToPageMessage>(this, OnNavigateToPage);
            MessengerInstance.Register<LoginMessage>(this, OnLoginMessage);
            MessengerInstance.Register<CurrentAudioChangedMessage>(this, OnCurrentAudioChanged);
            MessengerInstance.Register<PlayerPositionChangedMessage>(this, OnPlayerPositionChanged);
            MessengerInstance.Register<PlayStateChangedMessage>(this, OnPlayStateChanged);
        }

        private void OnNavigateToPage(NavigateToPageMessage message)
        {
            Type type = Type.GetType("Meridian.View." + message.Page.Substring(1), false);
            if (type == null)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                return;
            }

            var frame = Application.Current.MainWindow.GetVisualDescendents().OfType<Frame>().FirstOrDefault();
            if (frame == null)
                return;

            if (Settings.Instance.SendStats)
                Counter.ReportEvent("page" + message.Page);

            if (typeof(PageBase).IsAssignableFrom(type))
            {
                var page = (PageBase)Activator.CreateInstance(type);
                page.NavigationContext.Parameters = message.Parameters;
                frame.Navigate(page);
            }
            else if (typeof(Page).IsAssignableFrom(type))
            {
                frame.Navigate(Activator.CreateInstance(type));
            }

            UpdateCanGoBack();
        }

        private void OnLoginMessage(LoginMessage message)
        {
            if (message.Type == LoginType.LogIn)
            {

                if (message.Service == "vk")
                {
                    ShowSidebar = true;
                    LoadUserInfo();
                }

                if (message.Service == "lastfm")
                    RaisePropertyChanged("LastFmUser");
            }
            else if (message.Type == LoginType.LogOut)
            {
                if (message.Service == "lastfm")
                    RaisePropertyChanged("LastFmUser");
            }
        }

        private void OnCurrentAudioChanged(CurrentAudioChangedMessage message)
        {
            RaisePropertyChanged("CurrentAudio");
            RaisePropertyChanged("CurrentRadio");
            RaisePropertyChanged("IsPlaying");
            RaisePropertyChanged("CurrentPlaylist");
            RaisePropertyChanged("WindowTitle");


            _artRequested = false;
            _statusUpdated = false;
            _nowPlayingUpdated = false;
            _scrobbled = false;

            GetTrackImage();

            if (Settings.Instance.ShowTrackNotifications && message.OldAudio != null) //disable show on first start by checking for null
                ShowTrackNotification(message.NewAudio);
        }

        private async void OnPlayerPositionChanged(PlayerPositionChangedMessage message)
        {
            RaisePropertyChanged("CurrentAudioPosition");
            RaisePropertyChanged("CurrentAudioPositionSeconds");
            RaisePropertyChanged("CurrentAudioDuration");


            if (message.NewPosition.TotalSeconds >= 3)
            {
                if (!_statusUpdated)
                {
                    if (Settings.Instance.EnableStatusBroadcasting)
                    {
                        _statusUpdated = true;

                        try
                        {
                            await DataService.SetMusicStatus(CurrentAudio);
                        }
                        catch (VkAccessDeniedException ex)
                        {
                            //ViewModelLocator.AuthService.LogOutVk();
                            LoggingService.Log(ex);
                        }
                        catch (VkStatusBroadcastDisabledException ex)
                        {
                            LoggingService.Log(ex);
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Log(ex);
                        }
                    }
                }

                if (!_nowPlayingUpdated && EnableScrobbling)
                {
                    _nowPlayingUpdated = true;

                    try
                    {
                        await DataService.UpdateNowPlaying(CurrentAudio);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                if (!_artRequested)
                {
                    CancelArt();
                    _artRequested = true;
                    GetArtistImage(_artCancellationToken.Token);
                }
            }

            if (CurrentAudio != null && message.NewPosition.TotalSeconds >= CurrentAudio.Duration.TotalSeconds / 3)
            {
                if (!_scrobbled && EnableScrobbling)
                {
                    _scrobbled = true;
                    try
                    {
                        await DataService.Scrobble(CurrentAudio);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        private void OnPlayStateChanged(PlayStateChangedMessage message)
        {
            RaisePropertyChanged("IsPlaying");
        }

        public void UpdateCanGoBack()
        {
            RaisePropertyChanged("CanGoBack");


            var frame = Application.Current.MainWindow.GetVisualDescendents().OfType<Frame>().FirstOrDefault(f => f.Name == "RootFrame");
            if (frame != null && frame.Content != null)
            {
                var source = frame.Content.GetType().Name;
                _canNavigate = false;
                SelectedMainMenuItemIndex = _mainMenuItems.IndexOf(_mainMenuItems.FirstOrDefault(t => t.Page.EndsWith(source)));
                _canNavigate = true;
            }
        }

        private async void GetArtistImage(CancellationToken token)
        {
            if (CurrentAudio == null)
                return;

            if (CurrentAudio.Artist == _lastArtist)
                return;

            _lastArtist = CurrentAudio.Artist;
            string imageType = Settings.Instance.ShowBackgroundArt ? "big" : "medium";

            try
            {
                var cachedImage = await CacheService.GetCachedImage("artists/" + CacheService.GetSafeFileName(CurrentAudio.Artist + "_" + imageType + ".jpg"));
                if (cachedImage != null)
                {
                    ArtistImage = cachedImage;
                    return;
                }

                if (Settings.Instance.DownloadArtistArt)
                {
                    var imageUri =
                        await DataService.GetArtistImage(CurrentAudio.Artist, Settings.Instance.ShowBackgroundArt);
                    if (imageUri != null)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        cachedImage = await CacheService.CacheImage(imageUri.OriginalString, "artists/" + CacheService.GetSafeFileName(CurrentAudio.Artist + "_" + imageType + ".jpg"));

                        if (token.IsCancellationRequested)
                            return;

                        ArtistImage = cachedImage;
                        return;
                    }
                }

                ArtistImage = null;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        private async void GetTrackImage()
        {
            if (CurrentAudio == null)
                return;

            if (Settings.Instance.DownloadAlbumArt)
            {
                try
                {
                    var imageUri = await DataService.GetTrackImage(CurrentAudio.Artist, CurrentAudio.Title);
                    if (imageUri == null)
                    {
                        TrackImage = null;
                        return;
                    }

                    TrackImage = new BitmapImage(imageUri);
                }
                catch (Exception ex)
                {

                    LoggingService.Log(ex);
                }
            }
            else
            {
                TrackImage = null;
            }
        }

        private async Task LikeDislikeAudio(Audio audio, bool captchaNeeded = false, string captchaSid = null, string captchaImg = null)
        {
            if (audio == null)
                return;

            IsWorking = true;

            string captchaKey = null;
            if (captchaNeeded)
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new CaptchaRequestView(captchaSid, captchaImg);
                var result = await flyout.ShowAsync();
                if (string.IsNullOrEmpty((string)result))
                {
                    IsWorking = false;
                    return;
                }
                else
                {
                    captchaKey = (string)result;
                }
            }

            try
            {
                bool result;

                if (!audio.IsAddedByCurrentUser)
                {
                    result = await DataService.RemoveAudio(audio);
                }
                else
                    result = await DataService.AddAudio(audio, captchaSid, captchaKey);

                if (result)
                {
                    //нужно, чтобы обновить список треков пользователя, если он открыт в данный момент
                    MessengerInstance.Send(new UserTracksChangedMessage());
                }
                else
                {
                    audio.IsAddedByCurrentUser = !audio.IsAddedByCurrentUser;
                    LoggingService.Log("Can't add/remove audio " + audio.Id + " owner: " + audio.OwnerId + ".");
                }
            }
            catch (VkCaptchaNeededException ex)
            {
                LikeDislikeAudio(audio, true, ex.CaptchaSid, ex.CaptchaImg);
            }
            catch (Exception ex)
            {
                audio.IsAddedByCurrentUser = !audio.IsAddedByCurrentUser;
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private void ShowTrackNotification(Audio track)
        {
            if (track == null)
                return;

            var w = CurrentUIMode == UIMode.Normal
                ? Application.Current.MainWindow
                : Application.Current.Windows.OfType<CompactView>().FirstOrDefault();

            if (w == null)
                return;

            if (w.IsActive &&
                w.WindowState != WindowState.Minimized)
                return;

            var notificationView = Application.Current.Windows.OfType<TrackNotifcationView>().FirstOrDefault();
            if (notificationView == null)
            {
                notificationView = new TrackNotifcationView(track);
                notificationView.Show();
            }
            else
            {
                notificationView.Track = track;
            }
        }

        private void SwitchUIMode(UIMode mode)
        {
            if (mode == UIMode.Compact)
            {
                Application.Current.MainWindow.Hide();

                var compactView = new CompactView();
                compactView.Show();
            }
            else
            {
                foreach (var window in Application.Current.Windows)
                {
                    if (window is CompactView)
                    {
                        ((Window)window).Close();
                    }
                }

                Application.Current.MainWindow.Show();
            }

            CurrentUIMode = mode;
        }

        private void CancelArt()
        {
            _artCancellationToken.Cancel();
            _artCancellationToken = new CancellationTokenSource();
        }
    }
}
