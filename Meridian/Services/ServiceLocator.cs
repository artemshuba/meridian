using Meridian.Services.Music;

namespace Meridian.Services
{
    /// <summary>
    /// Contains instances of frequnetly used services
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly LocalMusicService _localMusicService = new LocalMusicService();
        private static readonly DataBaseService _dataBaseService = new DataBaseService();

        public static LocalMusicService LocalMusicService
        {
            get { return _localMusicService; }
        }

        public static DataBaseService DataBaseService
        {
            get { return _dataBaseService; }
        }
    }
}
