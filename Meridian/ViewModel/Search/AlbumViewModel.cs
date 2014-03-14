using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using LastFmLib.Core.Album;
using Meridian.Controls;
using Meridian.Extensions;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View.Flyouts;
using Meridian.ViewModel.Flyouts;
using VkLib.Core.Audio;
using VkLib.Error;

namespace Meridian.ViewModel.Search
{
    public class AlbumViewModel : ViewModelBase
    {
        private LastFmAlbum _album;
        private List<Audio> _tracks;
        private ObservableCollection<LastFmAlbum> _artistAlbums;
        private int _selectedAlbumIndex = -1;

        #region Commands

        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand ShareCommand { get; private set; }

        #endregion

        public LastFmAlbum Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public List<Audio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public ObservableCollection<LastFmAlbum> ArtistAlbums
        {
            get { return _artistAlbums; }
            set { Set(ref _artistAlbums, value); }
        }

        public int SelectedAlbumIndex
        {
            get { return _selectedAlbumIndex; }
            set
            {
                if (Set(ref _selectedAlbumIndex, value))
                {
                    Album = ArtistAlbums[value];
                    LoadTracks();
                }
            }
        }

        public AlbumViewModel()
        {
            InitializeCommands();
        }

        public void Activate()
        {
            //LoadTracks();
            LoadArtistInfo();
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            SaveCommand = new RelayCommand(Save);

            ShareCommand = new RelayCommand(() =>
            {
                var shareViewModel = new ShareViewModel();
                if (Tracks != null && Tracks.Count > 0)
                {
                    foreach (var track in Tracks.Take(15))
                    {
                        shareViewModel.Tracks.Add(track);

                    }
                }

                if (File.Exists(App.Root + "/Cache/artists/" + Album.Artist + ".jpg"))
                {
                    shareViewModel.ImagePath = App.Root + "/Cache/artists/" + Album.Artist + ".jpg";
                    shareViewModel.Image = new BitmapImage(new Uri(shareViewModel.ImagePath));
                }

                var flyout = new FlyoutControl();
                flyout.FlyoutContent = new ShareView() { DataContext = shareViewModel };
                flyout.Show();

                shareViewModel.Activate();
            });
        }

        private async void LoadTracks()
        {
            IsWorking = true;
            Tracks = null;

            try
            {
                var info = await DataService.GetAlbumInfo(Album.Mbid, Album.Name, Album.Artist);
                if (info == null)
                    info = await DataService.GetAlbumInfo(Album.Mbid, Album.Name, Album.Artist, true, false); //иногда автокоррекция тупит и альбом не находится, пробуем без нее
                if (info != null)
                {
                    if (info.Tracks != null)
                    {
                        var tracks = new List<Audio>();

                        for (var i = 0; i < info.Tracks.Count; i++)
                        {
                            var track = info.Tracks[i].ToAudio();
                            track.Order = i + 1;
                            tracks.Add(track);
                        }

                        Tracks = tracks;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }


        private async void LoadArtistInfo()
        {
            IsWorking = true;

            try
            {
                var topAlbums = await DataService.GetArtistAlbums(null, Album.Artist, 10);
                if (topAlbums != null)
                {
                    ArtistAlbums = new ObservableCollection<LastFmAlbum>(topAlbums);

                    if (topAlbums.Count > 0)
                    {
                        var album = topAlbums.FirstOrDefault(x => x.Name == Album.Name);
                        if (album != null)
                        {
                            SelectedAlbumIndex = topAlbums.IndexOf(album);
                        }
                        else
                        {
                            ArtistAlbums.Insert(0, Album);
                            SelectedAlbumIndex = 0;
                        }
                    }
                }
                else
                {
                    ArtistAlbums = new ObservableCollection<LastFmAlbum>() { Album };
                    SelectedAlbumIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async void Save()
        {
            var album = new VkAudioAlbum() { Title = _album.Artist + " - " + _album.Name };

            var flyout = new FlyoutControl();
            flyout.FlyoutContent = new EditAlbumView(album);
            var result = await flyout.ShowAsync();
            if ((bool)result)
            {
                try
                {
                    Debug.WriteLine("Creating new album...");


                    NotificationService.NotifyProgressStarted(MainResources.NotificationSaving);

                    var newAlbumId = await ViewModelLocator.Vkontakte.Audio.AddAlbum(album.Title);

                    Debug.WriteLine("Album created. Id: " + newAlbumId);
                    Debug.WriteLine("Gettings audios...");

                    var progress = new Progress<int>(p => NotificationService.NotifyProgressChanged((int)(p / 2.0f)));

                    var audios = await GetAudioList(progress);

                    Debug.WriteLine("Got audios. Count: " + audios.Count);
                    Debug.WriteLine("Saving audios.");

                    int requestsCount = 0;
                    var audioIds = new List<long>();

                    bool captchaNeeded = false;
                    string captchaImg = string.Empty;
                    string captchaSid = string.Empty;
                    string captchaKey = string.Empty;

                    int progressStep = (int)(100.0f / (audios.Count + 1));

                    for (var i = audios.Count - 1; i > 0; i--)
                    {
                        var vkAudio = audios[i];

                        try
                        {
                            var newAudioId = await ViewModelLocator.Vkontakte.Audio.Add(vkAudio.Id, vkAudio.OwnerId, captchaSid: captchaSid, captchaKey: captchaKey);
                            if (newAudioId != 0)
                            {
                                audioIds.Add(newAudioId);

                                captchaNeeded = false;
                                captchaKey = null;
                                captchaSid = null;
                            }
                        }
                        catch (VkCaptchaNeededException ex)
                        {
                            captchaNeeded = true;
                            captchaImg = ex.CaptchaImg;
                            captchaSid = ex.CaptchaSid;
                        }

                        if (captchaNeeded)
                        {
                            flyout = new FlyoutControl();
                            flyout.FlyoutContent = new CaptchaRequestView(captchaSid, captchaImg);
                            result = await flyout.ShowAsync();
                            if (!string.IsNullOrEmpty((string)result))
                            {
                                captchaKey = (string)result;
                                i = i - 1;
                                continue;
                            }
                            else
                            {
                                NotificationService.NotifyProgressFinished();
                                return;
                            }
                        }

                        NotificationService.NotifyProgressChanged((int)(progressStep / 2.0f));

                        requestsCount++;

                        if (requestsCount >= 2) //не больше 2-х запросов в секунду
                        {
                            requestsCount = 0;
                            await Task.Delay(1000);
                        }
                    }

                    Debug.WriteLine("Audios saved. Moving to album...");

                    if (audioIds.Count > 0)
                    {
                        if (await ViewModelLocator.Vkontakte.Audio.MoveToAlbum(newAlbumId, audioIds))
                        {
                            Debug.WriteLine("Album saved!");

                            NotificationService.NotifyProgressFinished(MainResources.NotificationSaved);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }
        }

        private async Task<List<VkAudio>> GetAudioList(IProgress<int> progress)
        {
            if (Tracks == null)
                return null;

            var result = new List<VkAudio>();
            int requestsCount = 0;
            int progressStep = (int)(100.0f / Tracks.Count);
            foreach (var audio in Tracks)
            {
                //if (token.IsCancellationRequested)
                //    return null;


                Audio vkAudio = null;
                try
                {
                    vkAudio = await DataService.GetAudioByArtistAndTitle(audio.Artist, audio.Title);
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }

                if (vkAudio == null)
                {
                    LoggingService.Log("Failed to find audio " + audio.Artist + " - " + audio.Title);
                }
                else
                {
                    result.Add(new VkAudio()
                    {
                        Id = vkAudio.Id,
                        OwnerId = vkAudio.OwnerId,
                        Title = vkAudio.Title,
                        Artist = vkAudio.Artist,
                        Duration = vkAudio.Duration
                    });
                }

                requestsCount++;

                if (requestsCount >= 2) //не больше 2-х запросов в секунду
                {
                    requestsCount = 0;
                    await Task.Delay(1000);
                }

                //if (token.IsCancellationRequested)
                //    return null;

                progress.Report(progressStep);
            }

            return result;
        }

    }
}
