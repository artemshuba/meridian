using System;
using Jupiter.Mvvm;
using Meridian.Enum;
using Meridian.Interfaces;
using Meridian.Services;
using System.Collections.Generic;
using System.Linq;
using Jupiter.Utils.Extensions;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Utils.Messaging;
using Meridian.Model;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Meridian.ViewModel
{
    public class PlayerViewModel : ViewModelBase
    {
        private ImageService _imageService;

        private string _currentTitle;
        private string _currentArtist;

        private CachedImage _artistImage;
        private CachedImage _trackImage;

        private CancellationTokenSource _trackImageCancellationToken;
        private CancellationTokenSource _artistImageCancellationToken;

        private bool _vkStatusUpdated;
        private bool _lastFmScrobbled;
        private bool _lastFmNowPlayingUpdated;

        private ScrobblingService _scrobblingService;

        #region Commands

        public DelegateCommand PlayPauseCommand { get; private set; }

        public DelegateCommand SwitchNextCommand { get; private set; }

        public DelegateCommand SwitchPrevCommand { get; private set; }

        /// <summary>
        /// Shuffle specified tracklist
        /// </summary>
        public DelegateCommand<IList<IAudio>> ShuffleTracklistCommand { get; private set; }

        /// <summary>
        /// Play tracklist
        /// </summary>
        public DelegateCommand<IList<IAudio>> PlayTracklistCommand { get; private set; }

        #endregion

        /// <summary>
        /// Is playing
        /// </summary>
        public bool IsPlaying
        {
            get { return AudioService.Instance.IsPlaying; }
            set
            {
                //leave empty to avoid binding errors
            }
        }

        /// <summary>
        /// Current audio
        /// </summary>
        public IAudio CurrentAudio => AudioService.Instance.CurrentPlaylist.CurrentItem;

        public TimeSpan Position => AudioService.Instance.Position;

        public double PositionSeconds
        {
            get { return AudioService.Instance.Position.TotalSeconds; }
            set
            {
                if (AudioService.Instance.Position.TotalSeconds == value)
                    return;

                AudioService.Instance.Seek(TimeSpan.FromSeconds(value));
            }
        }

        public TimeSpan Duration => AudioService.Instance.Duration;

        public double DurationSeconds => AudioService.Instance.Duration.TotalSeconds;

        public double Volume
        {
            get { return AudioService.Instance.Volume * 100f; }
            set
            {
                var v = value / 100f;
                if (AudioService.Instance.Volume == v)
                    return;

                AudioService.Instance.Volume = v;
                AppState.Volume = v;
                RaisePropertyChanged();
            }
        }

        public bool Repeat
        {
            get { return AudioService.Instance.Repeat == RepeatMode.Always; }
            set
            {
                AudioService.Instance.Repeat = value ? RepeatMode.Always : RepeatMode.None;
                AppState.Repeat = AudioService.Instance.Repeat;
            }
        }

        public bool Shuffle
        {
            get { return AudioService.Instance.Shuffle; }
            set
            {
                if (AudioService.Instance.Shuffle == value)
                    return;

                AudioService.Instance.Shuffle = value;
                AppState.Shuffle = value;
            }
        }

        public CachedImage TrackImage
        {
            get { return _trackImage; }
            private set { Set(ref _trackImage, value); }
        }

        public CachedImage ArtistImage
        {
            get { return _artistImage; }
            private set { Set(ref _artistImage, value); }
        }

        public PlayerViewModel()
        {
            _imageService = Ioc.Resolve<ImageService>();
            _scrobblingService = Ioc.Resolve<ScrobblingService>();

            AudioService.Instance.PlayStateChanged += AudioServicePlayStateChanged;
            AudioService.Instance.PositionChanged += AudioServicePositionChanged;
            AudioService.Instance.CurrentAudioChanged += AudioServiceCurrentAudioChanged;

            Messenger.Default.Register<MessageUserAuthChanged>(this, OnMessageUserAuthChanged);
        }

        protected override void InitializeCommands()
        {
            PlayPauseCommand = new DelegateCommand(() =>
            {
                if (!IsPlaying)
                    AudioService.Instance.Play();
                else
                    AudioService.Instance.Pause();
            });

            SwitchNextCommand = new DelegateCommand(() =>
            {
                AudioService.Instance.SwitchNext(skip: true);
            });

            SwitchPrevCommand = new DelegateCommand(() =>
            {
                AudioService.Instance.SwitchPrev();
            });

            ShuffleTracklistCommand = new DelegateCommand<IList<IAudio>>(tracks =>
            {
                var playlist = tracks.ToList();
                playlist.Shuffle();
                AudioService.Instance.PlayAudio(playlist.First(), playlist);
            });

            PlayTracklistCommand = new DelegateCommand<IList<IAudio>>(tracks =>
            {
                AudioService.Instance.PlayAudio(tracks.First(), tracks);
            });
        }

        private void AudioServicePlayStateChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(IsPlaying));

            TileHelper.UpdateIsPlaying(IsPlaying);
        }

        private async void AudioServiceCurrentAudioChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CurrentAudio));

            await UpdateArtistImage();
            await UpdateTrackImage();
        }

        private void AudioServicePositionChanged(object sender, TimeSpan position)
        {
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(PositionSeconds));
            RaisePropertyChanged(nameof(Duration));
            RaisePropertyChanged(nameof(DurationSeconds));

            if (position.TotalSeconds > 3)
            {
                if (!_vkStatusUpdated && AppState.EnableStatusBroadcasting)
                {
                    _vkStatusUpdated = true;

                    try
                    {
                        _scrobblingService.SetMusicStatus(CurrentAudio as AudioVk).ContinueWith(t => { });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Unable to update VK status");
                    }
                }

                if (AppState.EnableScrobbling && !_lastFmNowPlayingUpdated)
                {
                    _lastFmNowPlayingUpdated = true;
                    try
                    {
                        _scrobblingService.UpdateNowPlaying(CurrentAudio).ContinueWith(t => { });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Unable to update now playing on Last.FM");
                    }
                }
            }

            if (AppState.EnableScrobbling && !_lastFmScrobbled && position.TotalSeconds >= CurrentAudio.Duration.TotalSeconds / 3)
            {
                _lastFmScrobbled = true;

                try
                {
                    _scrobblingService.Scrobble(CurrentAudio).ContinueWith(t => { });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to scrobble track to Last.FM");
                }
            }
        }

        private async Task UpdateTrackImage()
        {
            if (CurrentAudio == null)
            {
                TrackImage = null;
                return;
            }

            var audio = CurrentAudio;

            if (_currentTitle == audio.Title && _currentArtist == audio.Artist)
                return;

            _currentTitle = audio.Title;
            _currentArtist = audio.Artist;

            TaskStarted("trackArt");

            try
            {
                if (_trackImageCancellationToken != null)
                    _trackImageCancellationToken.Cancel();

                _trackImageCancellationToken = new CancellationTokenSource();

                var token = _trackImageCancellationToken.Token;

                var image = await _imageService.GetTrackImage(audio, optimalImageWidth: 250);
                if (!token.IsCancellationRequested)
                {
                    TrackImage = image;

                    await TileHelper.UpdateMainTile(audio, IsPlaying);

                    await _imageService.UpdateTransportControlsImage(audio.Artist, audio.Title);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to update track image");
            }
            finally
            {
                TaskFinished("trackArt");
            }
        }

        private async Task UpdateArtistImage()
        {
            if (CurrentAudio == null || !AppState.ShowArtistArt)
            {
                ArtistImage = null;
                return;
            }

            var audio = CurrentAudio;

            if (_currentArtist == audio.Artist)
                return;

            _currentArtist = audio.Artist;

            TaskStarted("artistArt");

            try
            {
                if (_artistImageCancellationToken != null)
                    _artistImageCancellationToken.Cancel();

                _artistImageCancellationToken = new CancellationTokenSource();

                var token = _artistImageCancellationToken.Token;

                var image = await _imageService.GetArtistImage(CurrentAudio.Artist);
                if (!token.IsCancellationRequested)
                {
                    ArtistImage = image;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to update artist image");
            }
            finally
            {
                TaskFinished("artistArt");
            }
        }

        #region Messaging

        private void OnMessageUserAuthChanged(MessageUserAuthChanged message)
        {
            if (!message.IsLoggedIn)
            {
                _currentTitle = null;
                _currentArtist = null;

                ArtistImage = null;
                TrackImage = null;

                AudioService.Instance.Stop();
            }
        }

        #endregion
    }
}