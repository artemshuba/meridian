using LastFmLib;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Utils.Helpers;
using System;
using System.Threading.Tasks;
using VkLib;

namespace Meridian.Services
{
    public class ScrobblingService
    {
        private LastFm _lastFm;
        private Vk _vk;

        public ScrobblingService(Vk vk, LastFm lastFm)
        {
            _lastFm = lastFm;
            _vk = vk;
        }

        public async Task SetMusicStatus(AudioVk audio)
        {
            long aid = 0;
            long oid = 0;
            if (audio != null)
            {
                aid = long.Parse(audio.Id);
                oid = long.Parse(audio.OwnerId);
            }

            var result = await _vk.Audio.SetBroadcast(aid, oid);
        }

        public async Task<bool> Scrobble(IAudio audio)
        {
            var time = (int)DateTimeHelper.ToUnixTime(DateTime.Now);

            await _lastFm.Track.Scrobble(audio.Artist, audio.Title, time.ToString(), null, (int)audio.Duration.TotalSeconds);

            return true;
        }

        public async Task<bool> UpdateNowPlaying(IAudio audio)
        {
            await _lastFm.Track.UpdateNowPlaying(audio.Artist, audio.Title, null, (int)audio.Duration.TotalSeconds);

            return true;
        }
    }
}
