using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;

namespace Meridian.ViewModel.Main
{
    public class PopularAudioViewModel : ViewModelBase
    {
        private List<VkAudio> _tracks;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }

        #endregion

        public List<VkAudio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public async void Activate()
        {
            if (Tracks == null || Tracks.Count == 0)
                await LoadTracks();
        }

        public PopularAudioViewModel()
        {
            RegisterTasks("audio");

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            RefreshCommand = new RelayCommand(() => LoadTracks());
        }

        private async Task LoadTracks()
        {
            OnTaskStarted("audio");

            try
            {
                var response = await DataService.GetPopularTracks();
                if (response.Items != null && response.Items.Count > 0)
                {
                    Tracks = response.Items;

                    if (AudioService.CurrentAudio == null)
                    {
                        AudioService.CurrentAudio = Tracks.First();
                        AudioService.SetCurrentPlaylist(Tracks);
                    }
                }
                else
                {
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("audio");
        }
    }
}
