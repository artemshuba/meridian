using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;

namespace Meridian.ViewModel.Main
{
    public class RecommendationsViewModel : ViewModelBase
    {
        private const int MAX_AUDIO_RECOMMENDATIONS = 100;

        #region Groups

        private readonly RecommendationsCollection _recommendationsCollection = new RecommendationsCollection()
        {
            new Recommendation() { Title = MainResources.RecommendationsMusicGeneral, Key = "vk", Group = MainResources.RecommendationsMusicGroup },
            //new Recommendation() { Title = MainResources.RecommendationsMusicAdvanced, Key = "echonest", Group = MainResources.RecommendationsMusicGroup, GroupOrder = 1},

            new MoodRecommendation() { Title =  MainResources.MoodsVintage, Key = "vintage" },
            new MoodRecommendation() { Title = MainResources.MoodsUrban, Key = "urban" },
            new MoodRecommendation() { Title = MainResources.MoodsSad, Key = "sad" },
            new MoodRecommendation() { Title = MainResources.MoodsCool, Key = "cool" },
            new MoodRecommendation() { Title = MainResources.MoodsDreamy, Key = "dreamy" },
            new MoodRecommendation() { Title = MainResources.MoodsNostalgia, Key = "nostalgia" },
            new MoodRecommendation() { Title = MainResources.MoodsLoneliness, Key = "loneliness" },
            new MoodRecommendation() { Title = MainResources.MoodsOptimism, Key = "optimism" },
            new MoodRecommendation() { Title = MainResources.MoodsPsychedelic, Key = "psychedelic" },
            new MoodRecommendation() { Title = MainResources.MoodsRomantic, Key = "romantic" },
            new MoodRecommendation() { Title = MainResources.MoodsFreedom, Key = "freedom" },
            new MoodRecommendation() { Title = MainResources.MoodsSexy, Key = "sexy" },
            new MoodRecommendation() { Title = MainResources.MoodsNight, Key = "night" },
            new MoodRecommendation() { Title = MainResources.MoodsHappy, Key = "happy" },
            new MoodRecommendation() { Title = MainResources.MoodsExtreme, Key = "extreme" },
            new MoodRecommendation() { Title = MainResources.MoodsEnergy, Key = "energy" },
            new MoodRecommendation() { Title = MainResources.MoodsEpic, Key = "epic" },
            new MoodRecommendation() { Title = MainResources.MoodsSport, Key = "sport" },

            new GenreRecommendation() { Title = "acoustic", Key = "acoustic" },
            new GenreRecommendation() { Title = "alternative", Key = "alternative" },
            new GenreRecommendation() { Title = "ambient", Key = "ambient" },
            new GenreRecommendation() { Title = "blues", Key = "blues" },
            new GenreRecommendation() { Title = "classic", Key = "classic" },
            new GenreRecommendation() { Title = "drum & bass", Key = "drum & bass" },
            new GenreRecommendation() { Title = "dubstep", Key = "dubstep" },
            new GenreRecommendation() { Title = "dance", Key = "dance" },
            new GenreRecommendation() { Title = "electronic", Key = "electronic" },
            new GenreRecommendation() { Title = "folk", Key = "folk" },
            new GenreRecommendation() { Title = "funk", Key = "funk" },
            new GenreRecommendation() { Title = "grunge", Key = "grunge" },
            new GenreRecommendation() { Title = "hard rock", Key = "hard rock" },
            new GenreRecommendation() { Title = "hardcore", Key = "hardcore" },
            new GenreRecommendation() { Title = "hip-hop", Key = "hip-hop" },
            new GenreRecommendation() { Title = "house", Key = "house" },
            new GenreRecommendation() { Title = "indie rock", Key = "indie rock" },
            new GenreRecommendation() { Title = "industrial", Key = "industrial" },
            new GenreRecommendation() { Title = "instrumental", Key = "instrumental" },
            new GenreRecommendation() { Title = "jazz", Key = "jazz" },
            new GenreRecommendation() { Title = "latin", Key = "latin" },
            new GenreRecommendation() { Title = "metal", Key = "metal" },
            new GenreRecommendation() { Title = "old school", Key = "old school" },
            new GenreRecommendation() { Title = "progressive", Key = "progressive" },
            new GenreRecommendation() { Title = "pop", Key = "pop" },
            new GenreRecommendation() { Title = "punk", Key = "punk" },
            new GenreRecommendation() { Title = "r&b", Key = "r&b" },
            new GenreRecommendation() { Title = "rap", Key = "rap" },
            new GenreRecommendation() { Title = "rap core", Key = "rap core" },
            new GenreRecommendation() { Title = "russian rap", Key = "russian rap" },
            new GenreRecommendation() { Title = "russian pop", Key = "russian pop" },
            new GenreRecommendation() { Title = "reggae", Key = "reggae" },
            new GenreRecommendation() { Title = "rock", Key = "rock" },
            new GenreRecommendation() { Title = "soul", Key = "soul" },
            new GenreRecommendation() { Title = "soundtrack", Key = "soundtrack" },
            new GenreRecommendation() { Title = "trip-hop", Key = "trip-hop" },
            new GenreRecommendation() { Title = "trance", Key = "trance" },
            new GenreRecommendation() { Title = "underground", Key = "underground" },
        };

        #endregion

        private Recommendation _selectedRecommendation;
        private int _selectedRecommendationIndex;
        private ObservableCollection<Audio> _tracks;
        private CancellationTokenSource _cancellationToken;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }

        #endregion

        public RecommendationsCollection RecommendationsCollection
        {
            get { return _recommendationsCollection; }
        }

        public Recommendation SelectedRecommendation
        {
            get { return _selectedRecommendation; }
            set
            {
                if (Set(ref _selectedRecommendation, value))
                {
                    CancelAsync();

                    if (value.Group != MainResources.RecommendationsMusicGroup)
                        LoadRecommendationsByTag(value.Key, _cancellationToken.Token);
                    else
                    {
                        if (value.Key == "vk")
                            LoadGeneralRecommendations(_cancellationToken.Token);
                        else
                        {
                            LoadAdvancedRecommendations(_cancellationToken.Token);
                        }
                    }
                }
            }
        }

        public int SelectedRecommendationIndex
        {
            get { return _selectedRecommendationIndex; }
            set { Set(ref _selectedRecommendationIndex, value); }
        }

        public ObservableCollection<Audio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public RecommendationsViewModel()
        {
            _cancellationToken = new CancellationTokenSource();

            RegisterTasks("audio");

            InitializeCommands();
        }

        public void Activate()
        {
            //LoadGeneralRecommendations(new CancellationToken());
        }

        public void Deactivate()
        {
            CancelAsync();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            RefreshCommand = new RelayCommand(() =>
            {
                CancelAsync();

                if (SelectedRecommendation.Group != MainResources.RecommendationsMusicGroup)
                    LoadRecommendationsByTag(SelectedRecommendation.Key, _cancellationToken.Token);
                else
                {
                    if (SelectedRecommendation.Key == "vk")
                        LoadGeneralRecommendations(_cancellationToken.Token);
                    else
                    {
                        LoadAdvancedRecommendations(_cancellationToken.Token);
                    }
                }
            });
        }

        private async void LoadGeneralRecommendations(CancellationToken token)
        {
            OnTaskStarted("audio");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                int offset = 0;
                const int count = 50;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_AUDIO_RECOMMENDATIONS)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("My recommendations cancelled");
                        break;
                    }

                    var a = await DataService.GetRecommendations();
                    if (a == null || a.Count == 0)
                        break;

                    else if (a.Count > 0)
                    {
                        OnTaskFinished("audio");
                    }

                    offset += count;

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("My recommendations cancelled");
                        break;
                    }

                    foreach (var audio in a)
                    {
                        Tracks.Add(audio);
                    }

                    requestsCount++;

                    if (requestsCount >= 2) //не больше 2-х запросов в секунду
                    {
                        requestsCount = 0;
                        await Task.Delay(1000);
                    }

                    Debug.WriteLine("Load more recommendations");
                }

                if ((Tracks == null || Tracks.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            OnTaskFinished("audio");
        }

        private async void LoadAdvancedRecommendations(CancellationToken token)
        {
            IsWorking = true;

            OnTaskStarted("audio");

            try
            {
                var recommendations = await DataService.GetAdvancedRecommendations(100, token);
                if (recommendations != null && recommendations.Count > 0)
                {
                    Tracks = new ObservableCollection<Audio>(recommendations);
                }
                else
                {
                    Tracks = null;

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

        private async void LoadRecommendationsByTag(string tag, CancellationToken token)
        {
            OnTaskStarted("audio");

            Tracks = null;

            try
            {
                var a = await DataService.GetTagTopTracks(tag, 150);

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("Tag recommendations cancelled");
                    return;
                }


                if (a != null && a.Count > 0)
                {
                    Tracks = new ObservableCollection<Audio>(a);
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

        private void CancelAsync()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }
    }
}
