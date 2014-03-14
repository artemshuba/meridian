using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using Meridian.Controls;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.Services;
using Meridian.View.Flyouts;
using Meridian.ViewModel.Messages;
using VkLib.Core.Audio;

namespace Meridian.ViewModel.Main
{
    public class MusicViewModel : ViewModelBase, IDropTarget
    {
        private const int MAX_WALL_AUDIOS = 300;
        private const int MAX_NEWS_AUDIOS = 300;

        private ObservableCollection<VkAudioAlbum> _albums;
        private ObservableCollection<Audio> _tracks;
        private VkAudioAlbum _selectedAlbum;
        private CancellationTokenSource _cancellationToken;
        private int _totalAlbumsCount;

        #region Commands

        /// <summary>
        /// Команда воспроизведения аудиозаписи
        /// </summary>
        public RelayCommand<Audio> PlayAudioCommand { get; private set; }

        /// <summary>
        /// Команда дозагрузки альбомов
        /// </summary>
        public RelayCommand LoadMoreAlbumsCommand { get; private set; }

        /// <summary>
        /// Команда добавления нового альбома
        /// </summary>
        public RelayCommand AddAlbumCommand { get; private set; }

        /// <summary>
        /// Команда редактирования альбома
        /// </summary>
        public RelayCommand<VkAudioAlbum> EditAlbumCommand { get; private set; }

        /// <summary>
        /// Команда удаления альбома
        /// </summary>
        public RelayCommand<VkAudioAlbum> RemoveAlbumCommand { get; private set; }

        /// <summary>
        /// Команда обновления списка аудиозаписей
        /// </summary>
        public RelayCommand RefreshCommand { get; private set; }

        #endregion

        /// <summary>
        /// Список альбомов
        /// </summary>
        public ObservableCollection<VkAudioAlbum> Albums
        {
            get { return _albums; }
            set { Set(ref _albums, value); }
        }

        /// <summary>
        /// Список аудиозаписей
        /// </summary>
        public ObservableCollection<Audio> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        /// <summary>
        /// Выбранный альбом
        /// </summary>
        public VkAudioAlbum SelectedAlbum
        {
            get { return _selectedAlbum; }
            set
            {
                if (Set(ref _selectedAlbum, value))
                {
                    CancelAsync();

                    if (value != null)
                    {
                        switch ((int)value.Id)
                        {
                            case -100:
                                LoadNewsAudios(_cancellationToken.Token);
                                break;
                            case -101:
                                LoadWallAudios(_cancellationToken.Token);
                                break;
                            case -102:
                                LoadFavoritesAudios(_cancellationToken.Token);
                                break;
                            default:
                                LoadTracks(_cancellationToken.Token);
                                break;
                        }
                    }
                }
            }
        }

        public MusicViewModel()
        {
            _cancellationToken = new CancellationTokenSource();

            InitializeCommands();
            InitializeMessageInterception();

            RegisterTasks("audio", "albums");
        }

        public async void Activate()
        {
            if (Albums == null || Albums.Count == 0)
                await LoadAlbums();
        }

        public void Deactivate()
        {
            CancelAsync();

            MessengerInstance.Unregister<UserTracksChangedMessage>(this, OnUserTracksChanged);
        }

        private void InitializeCommands()
        {
            PlayAudioCommand = new RelayCommand<Audio>(audio =>
            {
                AudioService.Play(audio);
                AudioService.SetCurrentPlaylist(Tracks);
            });

            LoadMoreAlbumsCommand = new RelayCommand(() => LoadMoreAlbums());

            RefreshCommand = new RelayCommand(() =>
            {
                CancelAsync();
                LoadTracks(_cancellationToken.Token);
            });

            AddAlbumCommand = new RelayCommand(AddNewAlbum);

            EditAlbumCommand = new RelayCommand<VkAudioAlbum>(EditAlbum);

            RemoveAlbumCommand = new RelayCommand<VkAudioAlbum>(RemoveAlbum);
        }

        private void InitializeMessageInterception()
        {
            MessengerInstance.Register<UserTracksChangedMessage>(this, OnUserTracksChanged);
        }

        private async void OnUserTracksChanged(UserTracksChangedMessage message)
        {
            if (SelectedAlbum.Id == -1)
            {
                CancelAsync();
                await LoadTracks(_cancellationToken.Token);
            }
        }

        private async Task LoadAlbums()
        {
            IsWorking = true;
            OnTaskStarted("albums");

            try
            {
                var response = await DataService.GetUserAlbums();

                var albums = response.Items;

                _totalAlbumsCount = response.TotalCount;

                if (albums == null)
                    albums = new List<VkAudioAlbum>();

                albums.Insert(0, new VkAudioAlbum() { Id = -1, Title = MainResources.MyMusicAllTracks });
                albums.Insert(1, new VkAudioAlbum() { Id = -100, Title = MainResources.MyMusicNews });
                albums.Insert(2, new VkAudioAlbum() { Id = -101, Title = MainResources.MyMusicWall });
                albums.Insert(3, new VkAudioAlbum() { Id = -102, Title = MainResources.MyMusicFavorites });
                albums.Insert(4, new VkAudioAlbum() { Id = int.MinValue }); //separator

                Albums = new ObservableCollection<VkAudioAlbum>(albums);

                SelectedAlbum = albums.First();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("albums", ErrorResources.LoadAlbumsErrorCommon);
            }

            IsWorking = false;
            OnTaskFinished("albums");
        }

        private async Task LoadMoreAlbums()
        {
            if (Albums == null || Albums.Count - 5 >= _totalAlbumsCount)
                return;

            IsWorking = true;

            try
            {
                var response = await DataService.GetUserAlbums(0, Albums.Count - 5);


                if (response.Items != null)
                {
                    foreach (var album in response.Items)
                    {
                        Albums.Add(album);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            IsWorking = false;
        }

        private async Task LoadTracks(CancellationToken token)
        {
            if (SelectedAlbum == null)
                return;

            IsWorking = true;
            OnTaskStarted("audio");

            try
            {
                var response = await DataService.GetUserTracks(0, 0, SelectedAlbum.Id != 0 ? SelectedAlbum.Id : 0);
                if (response.Items != null && response.Items.Count > 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Tracks load cancelled");
                        return;
                    }

                    Tracks = new ObservableCollection<Audio>(response.Items);

                    if (AudioService.CurrentAudio == null)
                    {
                        AudioService.SetCurrentPlaylist(Tracks.ToList());
                        AudioService.CurrentAudio = Tracks.First();
                        AudioService.CurrentAudio.IsPlaying = true;
                    }
                }
                else
                {
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
                    Tracks = null;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            IsWorking = false;
            OnTaskFinished("audio");
        }

        private async void LoadNewsAudios(CancellationToken token)
        {
            IsWorking = true;
            OnTaskStarted("audio");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                int offset = 0;
                int count = 50;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_NEWS_AUDIOS)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("News audio cancelled");
                        break;
                    }

                    var a = await DataService.GetNewsAudio(count, offset, token);
                    if (a == null || a.Count == 0)
                        break;
                    else if (a.Count > 0)
                    {
                        OnTaskFinished("audio");
                    }

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("News audio cancelled");
                        break;
                    }

                    offset += count;

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

                    Debug.WriteLine("Loading more audios from news");
                }

                if ((Tracks == null || Tracks.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            IsWorking = false;
            OnTaskFinished("audio");
        }

        private async void LoadWallAudios(CancellationToken token)
        {
            IsWorking = true;
            OnTaskStarted("audio");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                int offset = 0;
                int count = 50;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_WALL_AUDIOS)
                {
                    var a = await DataService.GetWallAudio(count, offset, 0, token);
                    if (a == null || a.Count == 0)
                        break;
                    else if (a.Count > 0)
                    {
                        OnTaskFinished("audio");
                    }

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Wall audios cancelled");
                        break;
                    }

                    offset += count;

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

                    Debug.WriteLine("Loading more audios from wall");
                }

                if ((Tracks == null || Tracks.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
            }
            catch (Exception ex)
            {
                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);

                LoggingService.Log(ex);
            }

            IsWorking = false;
            OnTaskFinished("audio");
        }

        private async void LoadFavoritesAudios(CancellationToken token)
        {
            IsWorking = true;
            OnTaskStarted("audio");

            Tracks = new ObservableCollection<Audio>();

            try
            {
                int offset = 0;
                int count = 50;
                int requestsCount = 0;

                while (Tracks != null && Tracks.Count < MAX_WALL_AUDIOS)
                {
                    var a = await DataService.GetFavoritesAudio(count, offset, 0, token);
                    if (a == null || a.Count == 0)
                        break;
                    else if (a.Count > 0)
                    {
                        OnTaskFinished("audio");
                    }

                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Favorites audios cancelled");
                        break;
                    }

                    offset += count;

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

                    Debug.WriteLine("Loading more audios from favorites");
                }

                if ((Tracks == null || Tracks.Count == 0) && !token.IsCancellationRequested)
                    OnTaskError("audio", ErrorResources.LoadAudiosErrorEmpty);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                OnTaskError("audio", ErrorResources.LoadAudiosErrorCommon);
            }

            IsWorking = false;
            OnTaskFinished("audio");
        }

        private void CancelAsync()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            _cancellationToken = new CancellationTokenSource();
        }

        private async void AddNewAlbum()
        {
            var album = new VkAudioAlbum() { Title = "New album" };

            var flyout = new FlyoutControl();
            flyout.FlyoutContent = new EditAlbumView(album);
            var result = await flyout.ShowAsync();
            if ((bool)result)
            {
                try
                {
                    var newAlbumId = await ViewModelLocator.Vkontakte.Audio.AddAlbum(album.Title);
                    if (newAlbumId != 0)
                    {
                        album.Id = newAlbumId;
                        album.OwnerId = ViewModelLocator.Vkontakte.AccessToken.UserId;
                        Albums.Insert(5, album);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }
        }

        private async void EditAlbum(VkAudioAlbum album)
        {
            var flyout = new FlyoutControl();
            flyout.FlyoutContent = new EditAlbumView(album);

            var result = await flyout.ShowAsync();
            if (result != null && (bool)result)
            {
                try
                {
                    if (await ViewModelLocator.Vkontakte.Audio.EditAlbum(album.Id.ToString(), album.Title))
                    {
                        Albums[Albums.IndexOf(album)].Title = album.Title;
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                }
            }
        }

        private async void RemoveAlbum(VkAudioAlbum album)
        {
            try
            {
                var result = await ViewModelLocator.Vkontakte.Audio.DeleteAlbum(album.Id);
                if (result)
                {
                    Albums.Remove(Albums.FirstOrDefault(a => a.Id == album.Id));

                    SelectedAlbum = Albums.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Audio && !ViewModelLocator.Main.ShowShareBar)
            {
                if (dropInfo.TargetCollection == Tracks)
                {
                    dropInfo.Effects = DragDropEffects.Move;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                }
                else if (dropInfo.TargetCollection == Albums && (dropInfo.TargetItem is VkAudioAlbum && ((VkAudioAlbum)dropInfo.TargetItem).Id >= -1))
                {
                    dropInfo.Effects = DragDropEffects.Move;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                }
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Audio)
            {
                var source = (Audio)dropInfo.Data;
                if (dropInfo.TargetCollection == Tracks)
                {
                    var target = (Audio)dropInfo.TargetItem;
                    if (source == target)
                        return;
                    long afterAid, beforeAid;

                    int index = Tracks.IndexOf(target);
                    if (index > 0)
                    {
                        //проверяем куда вставлять: после taget или перед
                        if (Tracks[index - 1] == source)
                        {
                            afterAid = target.Id;
                            beforeAid = index == Tracks.Count - 1 ? 0 : Tracks[index + 1].Id;
                        }
                        else
                        {
                            afterAid = Tracks[index - 1].Id;
                            beforeAid = target.Id;
                        }
                    }
                    else if (index == 0)
                    {
                        afterAid = 0;
                        beforeAid = target.Id;
                    }
                    else
                        return;

                    if (await ViewModelLocator.Vkontakte.Audio.Reorder(source.Id, afterAid, beforeAid))
                    {
                        ((IList)dropInfo.DragInfo.SourceCollection).Remove(source);
                        ((IList)dropInfo.TargetCollection).Insert(index, source);
                    }
                    else
                    {
                        LoggingService.Log("Unable to reorder tracks.");
                    }
                }
                else
                {
                    var target = (VkAudioAlbum)dropInfo.TargetItem;
                    long targetId = (long)Math.Abs(target.Id);
                    //перемещаем в альбом
                    try
                    {
                        if (await ViewModelLocator.Vkontakte.Audio.MoveToAlbum(targetId,
                            new List<long>() { source.Id }))
                        {
                            if (SelectedAlbum.Id > 0)
                                ((IList)dropInfo.DragInfo.SourceCollection).Remove(source);
                        }
                        else
                        {
                            LoggingService.Log("Unable to move track to album.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Log(ex);
                    }

                }
            }
        }
    }
}
