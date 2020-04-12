using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop.Utilities;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.RemotePlay;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View;
using Meridian.View.Compact;
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
            new MainMenuItem() {Group = MainResources.MainMenuLocalTitle, GroupIcon = Application.Current.Resources["DeviceIcon"], Page = "/Local.LocalCollectionView", Title =  MainResources.MainMenuCollection},
            new MainMenuItem() {Group = MainResources.MainMenuLocalTitle, GroupIcon = Application.Current.Resources["DeviceIcon"], Page = "/Main.NowPlayingView", Title = MainResources.MainMenuNowPlaying, Icon = Application.Current.Resources["NowPlayingIcon"]},

            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/Main.MusicView", Title = MainResources.MainMenuMyMusic},
            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/Main.FeedView", Title = MainResources.MainMenuFeed},
            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/Main.PopularAudioView", Title = MainResources.MainMenuPopular},
            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/Main.RecommendationsView", Title = MainResources.MainMenuRecommendations},

            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/People.FriendsView", Title = MainResources.MainMenuFriends},
            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/People.SocietiesView", Title = MainResources.MainMenuSocieties},
            new MainMenuItem() {Group = MainResources.MainMenuVkTitle, GroupIcon = Application.Current.Resources["VkIcon"], Page = "/People.SubscriptionsView", Title = MainResources.MainMenuSubscriptions},
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
        private CancellationTokenSource _coverCancellationToken = new CancellationTokenSource();

        #region Commands

        public RelayCommand CloseWindowCommand { get; private set; }

        public RelayCommand MinimizeWindowCommand { get; private set; }

        public RelayCommand MaximizeWindowCommand { get; private set; }

        public RelayCommand<string> GoToPageCommand { get; private set; }

        public RelayCommand GoBackCommand { get; private set; }

        public RelayCommand GoToSettingsCommand { get; private set; }

        public RelayCommand<string> SearchCommand { get; private set; }

        public RelayCommand<KeyEventArgs> SearchKeyUpCommand { get; private set; }

        public RelayCommand NextAudioCommand { get; private set; }

        public RelayCommand PrevAudioCommand { get; private set; }

        public RelayCommand PlayPauseCommand { get; private set; }

        public RelayCommand RestartCommand { get; private set; }

        public RelayCommand SwitchUIModeCommand { get; private set; }

        public RelayCommand<string> SwitchToUIModeCommand { get; private set; }

        public RelayCommand<VkAudio> AddRemoveAudioCommand { get; private set; }

        public RelayCommand<VkAudio> EditAudioCommand { get; private set; }

        public RelayCommand<VkAudio> ShareAudioCommand { get; private set; }

        public RelayCommand<VkAudio> ShowLyricsCommand { get; private set; }

        public RelayCommand<Audio> CopyInfoCommand { get; private set; }

        public RelayCommand<Audio> PlayAudioNextCommand { get; private set; }

        public RelayCommand<Audio> AddToNowPlayingCommand { get; private set; }

        public RelayCommand<Audio> RemoveFromNowPlayingCommand { get; private set; }

        public RelayCommand<string> ShowArtistInfoCommand { get; private set; }

        public RelayCommand<Audio> StartTrackRadioCommand { get; private set; }

        public RelayCommand<VkAudio> AddToAlbumCommand { get; private set; }

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

        public bool ShowBackgroundArtCompactMode
        {
            get { return Settings.Instance.ShowBackgroundArtCompactMode; }
            set
            {
                if (Settings.Instance.ShowBackgroundArtCompactMode == value)
                    return;

                Settings.Instance.ShowBackgroundArtCompactMode = value;
                RaisePropertyChanged("ShowBackgroundArtCompactMode");
            }
        }

        public bool CanBroadcast
        {
            get { return CurrentAudio is VkAudio; }
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

            if (Settings.Instance.EnableRemotePlay)
                RemotePlayService.Instance.Start();
        }

        public async void LoadUserInfo()
        {
            try
            {
                User = await DataService.GetUserInfo();

                //Track user for stats
                await ViewModelLocator.Vkontakte.Stats.TrackVisitor();
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
                OnNavigateToPage(new NavigateToPageMessage() { Page = "/Settings.SettingsView" });
                ShowSidebar = false;
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

            SearchKeyUpCommand = new RelayCommand<KeyEventArgs>(args =>
            {
                if (args.Key == Key.Enter)
                {
                    var textBox = args.Source as TextBox;
                    if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
                        SearchCommand.Execute(textBox.Text);
                }
            });

            RestartCommand = new RelayCommand(() =>
            {
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            });

            AddRemoveAudioCommand = new RelayCommand<VkAudio>(audio =>
            {
                audio.IsAddedByCurrentUser = !audio.IsAddedByCurrentUser;
                LikeDislikeAudio(audio);
            });

            EditAudioCommand = new RelayCommand<VkAudio>(audio =>
            {
                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new EditAudioView(audio);
                flyout.Show();
            });

            ShowLyricsCommand = new RelayCommand<VkAudio>(audio =>
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
                NotificationService.Notify(MainResources.NotificationAddedToNowPlaying);
                AudioService.Playlist.Add(track);
            });

            RemoveFromNowPlayingCommand = new RelayCommand<Audio>(track =>
            {
                AudioService.Playlist.Remove(track);
            });

            ShareAudioCommand = new RelayCommand<VkAudio>(audio =>
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
                    SwitchUIMode(Settings.Instance.LastCompactMode == UIMode.CompactLandscape ? UIMode.CompactLandscape : UIMode.Compact);
                else
                    SwitchUIMode(UIMode.Normal);
            });

            SwitchToUIModeCommand = new RelayCommand<string>(s =>
            {
                UIMode mode;
                if (Enum.TryParse(s, true, out mode))
                    SwitchUIMode(mode);
            });

            ShowArtistInfoCommand = new RelayCommand<string>(async artist =>
            {
                NotificationService.Notify(MainResources.NotificationLookingArtist);

                try
                {
                    var response = await DataService.GetArtistInfo(null, artist);
                    if (response != null)
                    {
                        MessengerInstance.Send(new NavigateToPageMessage()
                        {
                            Page = "/Search.ArtistView",
                            Parameters = new Dictionary<string, object>()
                            {
                                {"artist", response}
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

            AddToAlbumCommand = new RelayCommand<VkAudio>(track =>
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
                YandexMetrica.ReportEvent("page" + message.Page);

            if (typeof(Layout.PageBase).IsAssignableFrom(type))
            {
                var page = (Layout.PageBase)Activator.CreateInstance(type);
                page.NavigationContext.Parameters = message.Parameters;
                frame.Navigate(page);
            }
            else if (typeof(PageBase).IsAssignableFrom(type))
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
            RaisePropertyChanged("IsPlaying");
            RaisePropertyChanged("CurrentPlaylist");
            RaisePropertyChanged("WindowTitle");
            RaisePropertyChanged("CanBroadcast");

            _artRequested = false;
            _statusUpdated = false;
            _nowPlayingUpdated = false;
            _scrobbled = false;

            CancelCover();
            GetTrackImage(_coverCancellationToken.Token);

            if (Settings.Instance.ShowTrackNotifications && message.OldAudio != null)
                //disable show on first start by checking for null
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ShowTrackNotification(message.NewAudio);
                }));
            }
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
                            await DataService.SetMusicStatus(CurrentAudio as VkAudio);
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

        private async void GetTrackImage(CancellationToken token)
        {
            if (CurrentAudio == null)
                return;

            if (CurrentAudio is LocalAudio)
            {
                try
                {
                    using (var audioFile = TagLib.File.Create(CurrentAudio.Source))
                    {
                        var image = audioFile.Tag.Pictures.FirstOrDefault();
                        if (image != null)
                        {
                            var ms = new MemoryStream();
                            await ms.WriteAsync(image.Data.Data, 0, image.Data.Data.Length, token);
                            if (token.IsCancellationRequested)
                                return;

                            ms.Seek(0, SeekOrigin.Begin);

                            BitmapImage bi = null;
                            bi = new BitmapImage();
                            bi.BeginInit();
                            bi.StreamSource = ms;
                            bi.EndInit();
                            TrackImage = bi;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }

            if (Settings.Instance.DownloadAlbumArt)
            {
                try
                {
                    var artist = CurrentAudio.Artist;
                    var title = CurrentAudio.Title;

                    Uri imageUri = null;

                    if (CurrentAudio.AlbumCover != null)
                        imageUri = CurrentAudio.AlbumCover;
                    else
                    {
                        imageUri = await DataService.GetTrackImage(artist, title);
                        if (token.IsCancellationRequested)
                            return;

                        if (imageUri == null)
                        {
                            if (Settings.Instance.DownloadArtistArt)
                            {
                                imageUri = await DataService.GetArtistImage(artist, Settings.Instance.ShowBackgroundArt);

                                if (token.IsCancellationRequested)
                                    return;
                            }
                        }
                    }

                    if (imageUri == null)
                    {
                        TrackImage = null;
                        return;
                    }

                    //http://stackoverflow.com/questions/11326528/error-hresult-0x88982f72-when-trying-streaming-image-file
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bi.UriSource = imageUri;
                    bi.EndInit();


                    TrackImage = bi;
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

        private async Task LikeDislikeAudio(VkAudio audio, bool captchaNeeded = false, string captchaSid = null, string captchaImg = null)
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

            Window w;

            if (CurrentUIMode == UIMode.Normal)
                w = Application.Current.MainWindow;
            else
            {
                var t = CurrentUIMode == UIMode.CompactLandscape ? typeof(CompactLandscapeView) : typeof(CompactView);
                w = CurrentUIMode == UIMode.CompactLandscape
                    ? (Window)Application.Current.Windows.OfType<CompactLandscapeView>().FirstOrDefault()
                    : (Window)Application.Current.Windows.OfType<CompactView>().FirstOrDefault();
            }

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
            switch (mode)
            {
                case UIMode.Compact:
                    foreach (var window in Application.Current.Windows)
                    {
                        if (window is CompactLandscapeView)
                        {
                            ((Window)window).Close();
                        }
                    }

                    Application.Current.MainWindow.Hide();

                    var compactView = new CompactView();
                    compactView.Show();

                    Settings.Instance.LastCompactMode = UIMode.Compact;
                    Settings.Instance.Save();
                    break;

                case UIMode.CompactLandscape:
                    foreach (var window in Application.Current.Windows)
                    {
                        if (window is CompactView)
                        {
                            ((Window)window).Close();
                        }
                    }

                    Application.Current.MainWindow.Hide();

                    var compactLandscapeView = new CompactLandscapeView();
                    compactLandscapeView.Show();
                    Settings.Instance.LastCompactMode = UIMode.CompactLandscape;
                    Settings.Instance.Save();
                    break;

                default:
                    foreach (var window in Application.Current.Windows)
                    {
                        if (window is CompactLandscapeView || window is CompactView)
                        {
                            ((Window)window).Close();
                        }
                    }

                    Application.Current.MainWindow.Show();
                    break;
            }

            CurrentUIMode = mode;
        }

        private void CancelArt()
        {
            _artCancellationToken.Cancel();
            _artCancellationToken = new CancellationTokenSource();
        }

        private void CancelCover()
        {
            _coverCancellationToken.Cancel();
            _coverCancellationToken = new CancellationTokenSource();
        }
    }
}
