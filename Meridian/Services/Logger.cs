using Jupiter.Utils.Helpers;
//using MetroLog;
//using MetroLog.Targets;
//using Microsoft.HockeyApp;
using System;
using System.Diagnostics;
//using Windows.UI.Xaml;
//using Yandex.Metrica;

namespace Meridian.Services
{
    public static class Logger
    {
        //private static ILogger _logger;

        static Logger()
        {
            //LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());
            //_logger = LogManagerFactory.DefaultLogManager.GetLogger<Application>();
            
            //HockeyClient.Current.Configure("cab3ee8577ca40159b70ff9bec71dc42");
        }

        public static void AppStart()
        {
            //YandexMetrica.Activate("567e94f7-257a-4c7a-b0aa-d1c8dc73d8c9");

            //_logger.Info("App started. v" + AppInfoHelper.GetAppVersionString());
        }

        public static void Info(string message)
        {
            Debug.WriteLine(message);

            //_logger.Info(message);
        }

        public static void Error(Exception ex, string message = null)
        {
            if (ex == null)
                ex = new Exception();

            Debug.WriteLine(message + "\r\n" + ex);

            //YandexMetrica.ReportError(message, ex);

            //HockeyClient.Current.TrackException(ex);

            //_logger.Error(message, ex);
        }

        public static void Fatal(Exception ex, string message)
        {
            Debug.WriteLine("Fatal error: " + message + "\r\n" + ex);

            //YandexMetrica.ReportUnhandledException(ex);

            //HockeyClient.Current.TrackException(ex);

            //_logger.Fatal(message, ex);
        }
    }
}