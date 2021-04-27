using Jupiter.Utils.Extensions;
using Jupiter.Utils.Helpers;
using Meridian.Enum;
using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services.Discovery;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Windows.System;

namespace Meridian.Services
{
    public class AudioService
    {
        //private fields
        private AudioPlaylist _currentPlaylist = new AudioPlaylist();
        private DispatcherTimer _positionTimer;
        private MediaPlayer _mediaPlayer = new MediaPlayer();

        private CancellationTokenSource _resolveCancellationToken;

        //events
        public event EventHandler PlayStateChanged;
        public event EventHandler<TimeSpan> PositionChanged;
        public event EventHandler CurrentAudioChanged;

        //properties

        public static AudioService Instance { get; } = new AudioService();

        /// <summary>
        /// Is playing
        /// </summary>
        public bool IsPlaying => _mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing
            || _mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Opening
            || _mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering;

        /// <summary>
        /// Current playlist
        /// </summary>
        public AudioPlaylist CurrentPlaylist => _currentPlaylist;

        /// <summary>
        /// Current audio position
        /// </summary>
        public TimeSpan Position => _mediaPlayer.PlaybackSession.Position;

        /// <summary>
        /// Current audio duration
        /// </summary>
        public TimeSpan Duration => _currentPlaylist.CurrentItem?.Duration ?? TimeSpan.Zero;

        /// <summary>
        /// Volume
        /// </summary>
        public double Volume
        {
            get { return _mediaPlayer.Volume; }
            set
            {
                if (_mediaPlayer.Volume == value)
                    return;

                _mediaPlayer.Volume = value;
            }
        }

        /// <summary>
        /// Repeat mode
        /// </summary>
        public RepeatMode Repeat
        {
            get { return _currentPlaylist.Repeat; }
            set
            {
                _currentPlaylist.Repeat = value;
            }
        }

        /// <summary>
        /// Shuffle
        /// </summary>
        public bool Shuffle
        {
            get { return _currentPlaylist.Shuffle; }
            set
            {
                _currentPlaylist.Shuffle = value;
            }
        }

        private AudioService()
        {
            //Application.Current.Resuming += AppResuming;
            //Application.Current.Suspending += AppSuspending;

            Initialize();
        }

        /// <summary>
        /// Resume or start playing current track
        /// </summary>
        public void Play()
        {
            if (CurrentPlaylist.CurrentItem == null)
                return;

            if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.None || _mediaPlayer.Source == null) //source missing
                PlayFrom(CurrentPlaylist.CurrentItem.Source);
            else
                _mediaPlayer.Play();
        }

        /// <summary>
        /// Pause current track
        /// </summary>
        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public void Stop()
        {
            //will automatically pause and seek to 0
            CurrentPlaylist.CurrentItem = null;
            CurrentPlaylist.ClearAll();
        }

        /// <summary>
        /// Switch to next track
        /// </summary>
        public void SwitchNext(bool skip = false)
        {
            _currentPlaylist.MoveNext(skip: skip);
        }

        /// <summary>
        /// Switch to previous track
        /// </summary>
        public void SwitchPrev()
        {
            if (Position > TimeSpan.FromSeconds(3))
                PlayAudio(_currentPlaylist.CurrentItem, _currentPlaylist.Items);
            else
                _currentPlaylist.MovePrevious();
        }

        /// <summary>
        /// Seek to position
        /// </summary>
        public void Seek(TimeSpan position)
        {
            Logger.Info("Seeking " + position);

            _mediaPlayer.PlaybackSession.Position = position;
        }

        /// <summary>
        /// Sets current playlist. Used to update UI on the first data load.
        /// </summary>
        public void SetCurrentPlaylist(AudioPlaylist playlist)
        {
            if (_currentPlaylist != null)
                _currentPlaylist.OnCurrentItemChanged -= CurrentPlaylistOnCurrentItemChanged;

            _currentPlaylist = playlist;

            if (_currentPlaylist != null)
            {
                _currentPlaylist.Repeat = Repeat;
                _currentPlaylist.Shuffle = Shuffle;

                _currentPlaylist.OnCurrentItemChanged += CurrentPlaylistOnCurrentItemChanged;

                CurrentAudioChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Play audio with playlist
        /// </summary>
        public void PlayAudio(IAudio audio, IList<IAudio> sourcePlaylist)
        {
            //check if it's a new playlist
            if (!_currentPlaylist.Items.AreSame(sourcePlaylist))
            {
                _currentPlaylist.OnCurrentItemChanged -= CurrentPlaylistOnCurrentItemChanged;

                var shuffle = Shuffle;
                var repeat = Repeat;

                _currentPlaylist = new AudioPlaylist(sourcePlaylist);
                _currentPlaylist.Repeat = repeat;
                _currentPlaylist.Shuffle = shuffle;

                _currentPlaylist.CurrentItem = audio;

                _currentPlaylist.OnCurrentItemChanged += CurrentPlaylistOnCurrentItemChanged;
                CurrentPlaylistOnCurrentItemChanged(this, audio);
            }
            else
            {
                if (_currentPlaylist.CurrentItem == audio)
                    PlayFrom(_currentPlaylist.CurrentItem.Source);
                else
                    _currentPlaylist.CurrentItem = audio;
            }
        }

        /// <summary>
        /// Load state
        /// </summary>
        public async Task LoadState()
        {
            try
            {
                if (!FileStorageHelper.IsFileExists("currentPlaylist.js"))
                    return;

                var json = await FileStorageHelper.ReadText("currentPlaylist.js");
                if (!string.IsNullOrEmpty(json))
                {
                    _currentPlaylist.Deserialize(json);

                    if (_currentPlaylist.CurrentItem != null)
                    {
                        UpdateTransportControl();
                        CurrentAudioChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load AudioService state");
            }
        }

        /// <summary>
        /// Save state
        /// </summary>
        public async Task SaveState()
        {
            try
            {
                var json = _currentPlaylist.Serialize();
                await FileStorageHelper.WriteText("currentPlaylist.js", json);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save AudioService state");
            }
        }

        private void Initialize()
        {
            _mediaPlayer.PlaybackSession.PlaybackStateChanged += MediaPlayerOnCurrentStateChanged;
            _mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            _mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed; ;

            _mediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            _mediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            _mediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
            _mediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            _mediaPlayer.CommandManager.PlayReceived += CommandManager_PlayReceived;
            _mediaPlayer.CommandManager.PauseReceived += CommandManager_PauseReceived;

            _currentPlaylist.OnCurrentItemChanged += CurrentPlaylistOnCurrentItemChanged;

            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += PositionTimerOnTick;

            Volume = AppState.Volume;
            Repeat = AppState.Repeat;
            Shuffle = AppState.Shuffle;
        }

        private void Close()
        {
            _mediaPlayer.PlaybackSession.PlaybackStateChanged -= MediaPlayerOnCurrentStateChanged;
            _mediaPlayer.MediaEnded -= MediaPlayerOnMediaEnded;

            _mediaPlayer.CommandManager.NextReceived -= CommandManager_NextReceived;
            _mediaPlayer.CommandManager.PreviousReceived -= CommandManager_PreviousReceived;
            _mediaPlayer.CommandManager.PlayReceived -= CommandManager_PlayReceived;
            _mediaPlayer.CommandManager.PauseReceived -= CommandManager_PauseReceived;

            _currentPlaylist.OnCurrentItemChanged -= CurrentPlaylistOnCurrentItemChanged;

            _positionTimer.Stop();
            _positionTimer.Tick -= PositionTimerOnTick;
        }

        private void AppResuming(object sender, object e)
        {
            Initialize();
        }

        private void AppSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            Close();
        }

        private async void CurrentPlaylistOnCurrentItemChanged(object sender, IAudio audio)
        {
            Pause();
            Seek(TimeSpan.Zero);

            UpdateTransportControl();

            if (audio == null)
                return;

            if (audio.Source != null)
            {
                PlayFrom(audio.Source);

                (Application.Current as App).CurrentWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    CurrentAudioChanged?.Invoke(this, EventArgs.Empty);
                });
            }
            else
            {
                (Application.Current as App).CurrentWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    CurrentAudioChanged?.Invoke(this, EventArgs.Empty);
                });

                TryResolveTrack(audio);
            }
        }

        private async void TryResolveTrack(IAudio audio)
        {
            if (_resolveCancellationToken != null)
                _resolveCancellationToken.Cancel();

            _resolveCancellationToken = new CancellationTokenSource();

            var token = _resolveCancellationToken.Token;

            try
            {
                var resolvedTrack = await Ioc.Resolve<MusicResolveService>().ResolveTrack(audio.Title, audio.Artist, audio.Duration, token);

                if (resolvedTrack != null && !token.IsCancellationRequested)
                {
                    audio.Source = resolvedTrack.Source;
                    audio.Duration = resolvedTrack.Duration;
                    audio.Id = resolvedTrack.Id;
                    audio.OwnerId = resolvedTrack.OwnerId;

                    PlayFrom(resolvedTrack.Source);

                    UpdateTransportControl();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to resolve track");
            }
        }

        private void PlayFrom(Uri source)
        {
            _mediaPlayer.Source = MediaSource.CreateFromUri(source);
            _mediaPlayer.Play();
        }

        private void PositionTimerOnTick(object sender, object o)
        {
            PositionChanged?.Invoke(this, Position);
        }

        private void UpdateTransportControl()
        {
            _mediaPlayer.SystemMediaTransportControls.PlaybackStatus = IsPlaying ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Stopped;

            var updater = _mediaPlayer.SystemMediaTransportControls.DisplayUpdater;

            if (CurrentPlaylist?.CurrentItem != null)
            {
                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.Title = CurrentPlaylist.CurrentItem.Title;
                updater.MusicProperties.Artist = CurrentPlaylist.CurrentItem.Artist;
                updater.Update();
            }
            else
            {
                updater.ClearAll();
            }
        }

        public void UpdateCover(RandomAccessStreamReference coverRef)
        {
            var updater = _mediaPlayer.SystemMediaTransportControls.DisplayUpdater;
            updater.Thumbnail = coverRef;
            updater.Update();
        }

        private void MediaPlayerOnCurrentStateChanged(MediaPlaybackSession sender, object args)
        {
            Logger.Info(sender.PlaybackState.ToString());

            (Application.Current as App).CurrentWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (sender.PlaybackState == MediaPlaybackState.Playing)
                    _positionTimer.Start();
                else
                    _positionTimer.Stop();

                PlayStateChanged?.Invoke(this, EventArgs.Empty);
            });

            UpdateTransportControl();
        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            SwitchNext();
        }

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            if (args.Error == MediaPlayerError.SourceNotSupported)
            {
                //audio source url may expire
                CurrentPlaylist.CurrentItem.Source = null;
                TryResolveTrack(CurrentPlaylist.CurrentItem);
            }
            else
                Logger.Error(null, "Media failed. " + args.Error + " " + args.ErrorMessage);
        }

        private void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            SwitchNext();
        }

        private void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            SwitchPrev();
        }

        private void CommandManager_PlayReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPlayReceivedEventArgs args)
        {
            Play();
        }

        private void CommandManager_PauseReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPauseReceivedEventArgs args)
        {
            Pause();
        }
    }
}