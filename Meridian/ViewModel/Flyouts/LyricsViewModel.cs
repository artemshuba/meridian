using System;
using Meridian.Model;
using Meridian.Services;

namespace Meridian.ViewModel.Flyouts
{
    public class LyricsViewModel : ViewModelBase
    {
        private VkAudio _track;
        private string _lyrics;

        public VkAudio Track
        {
            get { return _track; }
            set
            {
                if (Set(ref _track, value))
                    LoadLyrics();
            }
        }

        public string Lyrics
        {
            get { return _lyrics; }
            set { Set(ref _lyrics, value); }
        }

        private async void LoadLyrics()
        {
            if (Track == null || !Track.HasLyrics)
                return;

            IsWorking = true;


            try
            {
                Lyrics = await DataService.GetLyrics(Track.LyricsId.ToString());
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }
    }
}
