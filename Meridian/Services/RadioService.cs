using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EchonestApi.Core.Artist;
using EchonestApi.Core.Playlist;
using Meridian.Model;
using Meridian.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Meridian.Services
{
    public static class RadioService
    {
        private const string STATIONS_FILE = "stations.js";

        private static string _sessionId;
        private static RadioStation _currentRadio;
        private static List<EchoSong> _futureSongs;
        private static EchoSong _currentSong;

        public static string SessionId
        {
            get { return _sessionId; }
        }

        public static RadioStation CurrentRadio
        {
            get { return _currentRadio; }
        }


        public static async void PlayRadio(RadioStation station)
        {
            AudioService.Playlist.Clear();

            try
            {
                _currentRadio = station;
                if (!string.IsNullOrEmpty(_sessionId))
                {
                    //удаляем предыдущую сессию, чтобы не засорять Echonest (и потому что есть ограничение в 1000 сессий)
                    await DeleteSession(_sessionId);
                }

                if (station.Songs == null)
                    await CreateArtistsSession(station.Artists.Select(s => s.Name));
                else
                    await CreateSongsSession(station.Songs.Select(s => s.Id));


                if (_futureSongs != null && _futureSongs.Count > 0)
                {
                    AudioService.SetCurrentPlaylist(new List<Audio>(), true);

                    PlaySong(_futureSongs.First());
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public static async Task<EchoArtist> FindArtist(string name)
        {
            try
            {
                var artists = await ViewModelLocator.Echonest.Artist.Search(name);
                if (artists != null && artists.Count > 0)
                {
                    return artists.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }

        public static async void StartRadioFromSong(string title, string artistName)
        {
            try
            {
                var song = await FindSong(title, artistName);
                if (song != null)
                {
                    var r = new RadioStation();
                    r.Title = title;
                    r.Songs = new List<EchoSong>() { song };

                    PlayRadio(r);
                }
                else
                {
                    var artist = await FindArtist(artistName);
                    if (artist == null)
                        return;

                    var r = new RadioStation();
                    r.Title = artistName;
                    r.Artists = new List<EchoArtist>() { artist };

                    PlayRadio(r);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public static async Task<EchoSong> FindSong(string title, string artist)
        {
            try
            {
                var songs = await ViewModelLocator.Echonest.Song.Search(title, artist);
                if (songs != null && songs.Count > 0)
                {
                    return songs.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }

        public static async Task CreateArtistsSession(IEnumerable<string> artists)
        {
            try
            {
                var response = await ViewModelLocator.Echonest.Playlist.DynamicCreate(EchoPlaylistType.ArtistRadio, artists: artists);
                _sessionId = response.Item1;
                if (response.Item2 != null)
                    _futureSongs = new List<EchoSong>() { response.Item2 };
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public static async Task CreateSongsSession(IEnumerable<string> songIds)
        {
            try
            {
                var response = await ViewModelLocator.Echonest.Playlist.DynamicCreate(EchoPlaylistType.SongRadio, songIds: songIds);
                _sessionId = response.Item1;
                if (response.Item2 != null)
                    _futureSongs = new List<EchoSong>() { response.Item2 };
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public static async Task DeleteSession(string sessionId)
        {
            try
            {
                await ViewModelLocator.Echonest.Playlist.DynamicDelete(_sessionId);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            _sessionId = null;
        }

        public static void RestoreSession(string sessionId, RadioStation radio)
        {
            _sessionId = sessionId;
            _currentRadio = radio;
        }

        //Переключиться на следующий трек, обычно вызывается автоматически, при завершении текущего трека
        public static async Task SwitchNext()
        {
            Next();
        }

        //Пропустить текущий трек и перейти к следующему. Обычно вызывается, если пользователь нажал Next
        public static async Task SkipNext()
        {
            try
            {
                if (_currentSong != null)
                    ViewModelLocator.Echonest.Playlist.DynamicFeedback(_sessionId, skipSongs: new[] { _currentSong.Id });
                Next();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

        }

        private static async void Next()
        {
            if (_futureSongs == null || _futureSongs.Count == 0 ||
                _futureSongs.IndexOf(_currentSong) >= _futureSongs.Count - 1)
            {
                try
                {
                    //получаем еще пачку треков
                    _futureSongs = await ViewModelLocator.Echonest.Playlist.DynamicNext(_sessionId, 5);
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);

                    return;
                }
            }

            if (_futureSongs == null)
            {
                await DeleteSession(_sessionId);
                PlayRadio(_currentRadio);
            }
            else
            {
                var currentIndex = _futureSongs.IndexOf(_currentSong);
                currentIndex++;
                PlaySong(_futureSongs[currentIndex]);
            }
        }

        //Вызывается если текущий трек не удалось воспроизвести (не найден ВКонтакте)
        public static async Task InvalidateCurrentSong()
        {
            try
            {
                ViewModelLocator.Echonest.Playlist.DynamicFeedback(_sessionId, invalidateSong: new[] { _currentSong.Id });
                Next();
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }
        }

        public static async void Stop()
        {
            if (string.IsNullOrEmpty(_sessionId))
                return;

            _currentRadio = null;
            await DeleteSession(_sessionId);
        }

        private static void PlaySong(EchoSong song)
        {
            var audio = new VkAudio()
            {
                Title = song.Title,
                Artist = song.ArtistName
            };

            _currentSong = song;

            AudioService.Play(audio);
            AudioService.Playlist.Insert(0, audio);
            //AudioService.SetCurrentPlaylist(new[] { audio }, true);
        }

        public static async Task SaveStations(IEnumerable<RadioStation> stations)
        {
            await Task.Run(() =>
            {
                var settings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var json = JsonConvert.SerializeObject(stations, settings);

                File.WriteAllText(STATIONS_FILE, json);
            });
        }

        public static Task<List<RadioStation>> LoadStations()
        {
            return Task.Run(() =>
            {
                if (!File.Exists(STATIONS_FILE))
                    return null;

                var json = File.ReadAllText(STATIONS_FILE);
                if (string.IsNullOrEmpty(json))
                    return null;

                var stations = JsonConvert.DeserializeObject<List<RadioStation>>(json);
                return stations;
            });
        }
    }
}
