using GalaSoft.MvvmLight.Ioc;
using LastFmLib;
using Meridian.Services.VK;
using System.IO;
using VkLib;
using Windows.Storage;
using DeezerLib;
using Meridian.Services;
using Meridian.Services.Discovery;

namespace Meridian
{
    public class Ioc
    {
        private static readonly string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Meridian.db");

        public static void Setup()
        {
            //using official VK app for Windows appId and secret
            SimpleIoc.Default.Register(() => new Vk(appId: "2274003", clientSecret: "hHbZxrka2uZ6jB1inYsH", apiVersion: "5.116", userAgent: "VKAndroidApp/5.52-4543 (Android 5.1.1; SDK 22; x86_64; unknown Android SDK built for x86_64; en; 320x240)"));
            SimpleIoc.Default.Register(() => new LastFm(apiKey: "a012acc1e5f8a61bc7e58238ce3021d8", apiSecret: "86776d4f43a72633fb37fb28713a7798"));
            SimpleIoc.Default.Register(() => new Deezer(appId: "229622", secretKey: "da90d8606bf99c8e1b403a80e03aefa3"));

            SimpleIoc.Default.Register<VkTracksService>();
            SimpleIoc.Default.Register<VkUserService>();
            SimpleIoc.Default.Register<CacheService>();
            SimpleIoc.Default.Register<ImageService>();
            SimpleIoc.Default.Register<DiscoveryService>();
            SimpleIoc.Default.Register<MusicResolveService>();
            SimpleIoc.Default.Register<ScrobblingService>();
        }

        public static T Resolve<T>()
        {
            return SimpleIoc.Default.GetInstance<T>();
        }
    }
}