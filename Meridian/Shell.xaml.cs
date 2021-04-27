using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Jupiter.Application;
using Meridian.View;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using Meridian.View.VK;
using Meridian.View.Common;
using Meridian.Controls;
using Meridian.View.Settings;
using Meridian.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Meridian
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        private class MenuItem
        {
            public string Title { get; set; }

            public Type Page { get; set; }

            public object Icon { get; set; }
        }

        private class MenuItemsGroup
        {
            public string Title { get; set; }

            public List<MenuItem> Items { get; set; } = new List<MenuItem>();
        }

        public double DefaultMenuWidth { get; } = 210;

        public bool EnableBackgroundArtBlur
        {
            get { return BackgroundArtBlur.Visibility != Visibility.Collapsed; }
            set
            {
                BackgroundArtBlur.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private List<MenuItemsGroup> MenuItems { get; } = new List<MenuItemsGroup>()
        {
            new MenuItemsGroup()
            {
                Items = new List<MenuItem>()
                {
                    new MenuItem() { Page = typeof(ExploreView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemExplore") },
                    new MenuItem() { Page = typeof(NowPlayingView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemNowPlaying") }
                }
            },

            new MenuItemsGroup()
            {
                Title = "-",
                Items = new List<MenuItem>()
                {
                    new MenuItem() { Page = typeof(MyMusicView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemMyMusic") },
                    //new MenuItem() { Page = typeof(FeedView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemFeed") },
                    new MenuItem() { Page = typeof(PopularMusicView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemPopular") },
                    //new MenuItem() { Page = typeof(RecommendationsView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemRecommendations") },
                    new MenuItem() { Page = typeof(FriendsView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemFriends") },
                    new MenuItem() { Page = typeof(SocietiesView), Title = Utils.Helpers.Resources.GetStringByKey("Main_MenuItemSocieties") }
                }
            }
        };

        private readonly List<Type> _menuDisablePages = new List<Type>()
        {
            typeof(LoginView)
        };

        private readonly List<Type> _miniPlayerDisablePages = new List<Type>()
        {
            typeof(LoginView), typeof(NowPlayingView), typeof(SettingsView)
        };

        public static Shell Current { get; private set; }

        public SplitViewDisplayMode DisplayMode => SplitView.DisplayMode;

        #region Events

        public event EventHandler<SplitViewDisplayMode> DisplayModeChanged;

        #endregion

        public Shell()
        {
            this.InitializeComponent();

            Current = this;

            SplitView.OpenPaneLength = DefaultMenuWidth;

            EnableBackgroundArtBlur = AppState.EnableBackgroundBlur;
        }

        public void AddPopup(PopupControl popup)
        {
            PopupHost.Child = popup;
        }

        public void RemovePopup()
        {
            PopupHost.Child = null;
        }

        private void ContentFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (_menuDisablePages.Any(p => p == e.SourcePageType))
            {
                SplitView.OpenPaneLength = 0;
                HostBackDrop.Visibility = Visibility.Collapsed;
                MenuButton.Opacity = 0;
                BackgroundArtGrid.Opacity = 0;
            }
            else
            {
                SplitView.OpenPaneLength = DefaultMenuWidth;
                MenuButton.Opacity = 1;
                BackgroundArtGrid.Opacity = 1;
            }

            //if (_miniPlayerDisablePages.Any(p => p == e.SourcePageType))
            //{
            //    MiniPlayerHost.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    MiniPlayerHost.Visibility = Visibility.Visible;
            //}

            var allMenuItems = MenuItems.SelectMany(m => m.Items).ToList();
            if (allMenuItems.Any(i => i.Page == e.SourcePageType) && _menuListView.Items?.Count > 0)
                _menuListView.SelectedIndex = allMenuItems.IndexOf(allMenuItems.FirstOrDefault(i => i.Page == e.SourcePageType));
            else
                _menuListView.SelectedIndex = -1;
        }

        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (DisplayMode != SplitViewDisplayMode.Inline)
                SplitView.IsPaneOpen = false;

            if (_miniPlayerDisablePages.Any(p => p == e.SourcePageType))
            {
                MiniPlayerHost.Visibility = Visibility.Collapsed;
            }
            else
            {
                MiniPlayerHost.Visibility = Visibility.Visible;
            }

            Analytics.TrackPageView(e.SourcePageType);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void MenuListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (MenuItem)e.ClickedItem;
            if (ContentFrame.SourcePageType == item.Page) //if we are already on required page do nothing
                return;

            (Application.Current as App).NavigationService.Navigate(item.Page);

            if (SplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                SplitView.IsPaneOpen = false;
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            //Window.Current.SetTitleBar(TitleBar);

            SplitView.RegisterPropertyChangedCallback(Microsoft.UI.Xaml.Controls.SplitView.DisplayModeProperty, SplitViewDisplayModeChanged);

            if (ContentFrame.SourcePageType != null)
                UpdateSelectedIndex(ContentFrame.SourcePageType);
        }

        private void SplitViewDisplayModeChanged(DependencyObject e, DependencyProperty p)
        {
            DisplayModeChanged?.Invoke(this, SplitView.DisplayMode);
        }

        private void UpdateSelectedIndex(Type page)
        {
            var allMenuItems = MenuItems.SelectMany(m => m.Items).ToList();
            if (allMenuItems.Any(i => i.Page == page) && _menuListView.Items?.Count > 0)
                _menuListView.SelectedIndex = allMenuItems.IndexOf(allMenuItems.FirstOrDefault(i => i.Page == page));
            else
                _menuListView.SelectedIndex = -1;
        }
    }
}