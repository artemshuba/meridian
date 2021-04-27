using Meridian.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkLib;
using VkLib.Core.Audio;

namespace Meridian.Services.Discovery
{
    public class MusicResolveService
    {
        private readonly Vk _vk;

        public MusicResolveService(Vk vk)
        {
            _vk = vk;
        }

        public async Task<AudioVk> ResolveTrack(string title, string artist, TimeSpan duration, CancellationToken cancellationToken)
        {
            var audios = await SearchAudio(artist + " - " + title, 50, 0);

            if (cancellationToken.IsCancellationRequested)
                return null;

            if (audios != null && audios.Count > 0)
            {
                //сначала фильтруем треки по подходящей длительности (погрешность 5 сек)
                audios = audios.Where(x => Math.Abs((x.Duration - duration).TotalSeconds) < 5).ToList();

                var audio = audios.FirstOrDefault(x => String.Equals(x.Title.Trim(), title, StringComparison.OrdinalIgnoreCase) && String.Equals(x.Artist.Trim(), artist, StringComparison.OrdinalIgnoreCase));
                if (audio == null)
                    audio = audios.FirstOrDefault(x => String.Equals(x.Artist.Trim(), artist, StringComparison.OrdinalIgnoreCase) && x.Title.Trim().StartsWith(title, StringComparison.OrdinalIgnoreCase));
                //if (audio == null)
                //    audio = audios.FirstOrDefault(x => String.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));
                //if (audio == null)
                //{
                //    audio = audios.First();
                //}

                if (audio == null)
                {
                    audios = audios.Where(a => a.Title.ToLower().Contains(title.ToLower())).ToList();

                    audio = audios.FirstOrDefault(x => x.Artist.ToLower().Contains(artist.ToLower()));

                    if (audio == null && (artist.Contains(", ") || artist.Contains(" ft. ") || artist.Contains(" ft ") || artist.Contains(" feat ")))
                    {
                        var artists = artist.Split(new[] { ", ", " ft. ", " ft ", " feat " },
                            StringSplitOptions.RemoveEmptyEntries);

                        audio = audios.FirstOrDefault(a => artists.Any(x => a.Artist.Trim().ToLower().Contains(x.Trim().ToLower())));
                    }
                }

                if (audio == null)
                    return null;

                return new AudioVk(audio);
            }
            else
            {
                bool searchAgain = false;
                if (artist.Contains("(") && artist.Contains(")"))
                {
                    artist = artist.Substring(0, artist.IndexOf("(")) + artist.Substring(artist.LastIndexOf(")") + 1);
                    searchAgain = true;
                }

                if (title.Contains("(") && title.Contains(")"))
                {
                    title = title.Substring(0, title.IndexOf("(")) + title.Substring(title.LastIndexOf(")") + 1);
                    searchAgain = true;
                }

                if (artist.Contains(", "))
                {
                    //try looking with first artist
                    var artists = artist.Split(new[] { ", ", " ft. ", " ft ", " feat " },
                            StringSplitOptions.RemoveEmptyEntries);
                    artist = artists[0];
                    searchAgain = true;
                }

                if (searchAgain && !cancellationToken.IsCancellationRequested)
                    return await ResolveTrack(title, artist, duration, cancellationToken);
            }

            return null;
        }

        private async Task<List<VkAudio>> SearchAudio(string query, int count = 0, int offset = 0)
        {
            var vkAudios = await _vk.Audio.Search(query, count, offset, VkAudioSortType.DateAdded, false, false);
            if (vkAudios.Items != null)
            {
                var result = vkAudios.Items;

                return result;
            }

            return null;
        }
    }
}