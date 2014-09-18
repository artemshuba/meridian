using System;
using LastFmLib.Core.Track;
using Meridian.Model;
using Meridian.ViewModel;

namespace Meridian.Extensions
{
    public static class AudioExtensions
    {
        public static VkAudio ToAudio(this VkLib.Core.Audio.VkAudio audio)
        {
            var result = new VkAudio();
            result.Id = audio.Id.ToString();
            result.Title = audio.Title;
            result.Artist = audio.Artist;
            result.AlbumId = audio.AlbumId;
            result.Duration = audio.Duration;
            result.LyricsId = audio.LyricsId;
            result.OwnerId = audio.OwnerId;
            result.Source = audio.Url;
            result.GenreId = audio.GenreId;
            result.IsAddedByCurrentUser = audio.OwnerId == ViewModelLocator.Vkontakte.AccessToken.UserId;

            return result;
        }

        public static VkAudio ToAudio(this LastFmTrack audio)
        {
            var result = new VkAudio();
            result.Title = audio.Title;
            result.Artist = audio.Artist;
            result.Duration = TimeSpan.FromSeconds(audio.Duration);

            return result;
        }
    }
}
