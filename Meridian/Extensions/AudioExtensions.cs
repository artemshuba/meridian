using System;
using LastFmLib.Core.Track;
using Meridian.Model;
using Meridian.ViewModel;
using VkLib.Core.Audio;

namespace Meridian.Extensions
{
    public static class AudioExtensions
    {
        public static Audio ToAudio(this VkAudio audio)
        {
            var result = new Audio();
            result.Id = audio.Id.ToString();
            result.Title = audio.Title;
            result.Artist = audio.Artist;
            result.AlbumId = audio.AlbumId;
            result.Duration = audio.Duration;
            result.LyricsId = audio.LyricsId;
            result.OwnerId = audio.OwnerId;
            result.Url = audio.Url;
            result.GenreId = audio.GenreId;
            result.IsAddedByCurrentUser = audio.OwnerId == ViewModelLocator.Vkontakte.AccessToken.UserId;

            return result;
        }

        public static Audio ToAudio(this LastFmTrack audio)
        {
            var result = new Audio();
            result.Title = audio.Title;
            result.Artist = audio.Artist;
            result.Duration = TimeSpan.FromSeconds(audio.Duration);

            return result;
        }
    }
}
