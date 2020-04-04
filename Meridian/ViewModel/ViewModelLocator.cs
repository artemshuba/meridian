using System;
using LastFmLib;
using Meridian.Services;
using Meridian.ViewModel.Main;
using VkLib;

namespace Meridian.ViewModel
{
    public class ViewModelLocator
    {
        private static Lazy<MainViewModel> _main = new Lazy<MainViewModel>(() => new MainViewModel());
        private static Lazy<NowPlayingViewModel> _nowPlaying = new Lazy<NowPlayingViewModel>(() => new NowPlayingViewModel());
        private static readonly Vk _vkontakte = new Vk(appId: "2274003", clientSecret: "hHbZxrka2uZ6jB1inYsH", apiVersion: "5.116", userAgent: "VKAndroidApp/5.52-4543 (Android 5.1.1; SDK 22; x86_64; unknown Android SDK built for x86_64; en; 320x240)");
        private static readonly LastFm _lastFm = new LastFm("a012acc1e5f8a61bc7e58238ce3021d8", "86776d4f43a72633fb37fb28713a7798");
        private static readonly UpdateService _updateService = new UpdateService();

        public static MainViewModel Main
        {
            get { return _main.Value; }
        }

        public static NowPlayingViewModel NowPlaying
        {
            get { return _nowPlaying.Value; }
        }

        public static Vk Vkontakte
        {
            get { return _vkontakte; }
        }

        public static LastFm LastFm
        {
            get { return _lastFm; }
        }

        public static UpdateService UpdateService
        {
            get { return _updateService; }
        }

        public ViewModelLocator()
        {

        }
    }
}
