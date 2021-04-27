using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using Yandex.Metrica;

namespace Meridian.Services
{

    public enum AnalyticsEvent
    {
        //Settings
        SettingsChangeTheme,
        SettingsChangeStartPage,
        SettingsChangeAccent,
        SettingsChangeLanguage,
        SignOutVk,

        //views
        WallTab,
        NewsTab,

        ExploreNewAlbum,
        ExplorePlaylist,
        ExploreFriend,
        ExploreCommunity,

        ExplorePlaySpecial,
        ExplorePlayNew,
        ExplorePlayRecent,
        ExplorePlaySimilar,
        ExplorePlayPopular,

        //search
        SearchGoToAlbum,
        SearchGoToArtist,
        SearchGoToRelatedArtist,
        SearchGoToArtistAlbum
    }

    public static class Analytics
    {
        public static void TrackPageView(Type pageType)
        {
            if (pageType == null)
                return;

            TrackEvent("View/" + pageType.Name);
        }

        public static void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            string jsonData = parameters != null ? JsonConvert.SerializeObject(parameters) : null;
            //YandexMetrica.ReportEvent(eventName, jsonData);

            Logger.Info("Analytics: " + eventName + " data: " + jsonData);
        }

        public static void TrackEvent(AnalyticsEvent e, Dictionary<string, object> parameters = null)
        {
            TrackEvent(e.ToString(), parameters);
        }
    }
}