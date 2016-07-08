using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.Services.Music.Repositories;
using Microsoft.WindowsAPICodePack.Shell;

namespace Meridian.Services.Music
{
    public class LocalMusicService
    {
        private CancellationTokenSource _scanCancellationToken = new CancellationTokenSource();
        private readonly LocalTracksRepository _tracksRepository;
        private readonly LocalArtistsRepository _artistsRepository;
        private readonly LocalAlbumsRepository _albumsRepository;

        public LocalMusicService()
        {
            _tracksRepository = new LocalTracksRepository();
            _artistsRepository = new LocalArtistsRepository();
            _albumsRepository = new LocalAlbumsRepository();
        }

        public async Task ScanMusic(IProgress<double> progress)
        {
            LoggingService.Log("Music scan started");

            try
            {
                int count = 0;

                var tracks = new List<LocalAudio>();


                await Task.Run(async () =>
                {
                    var musicFiles = FilesHelper.GetMusicFiles();

                    double totalCount = musicFiles.Count;

                    //not cool but ¯\_(ツ)_/¯

                    var albums = new Dictionary<string, AudioAlbum>();
                    var artists = new Dictionary<string, AudioArtist>();

                    foreach (var filePath in musicFiles)
                    {
                        TagLib.File audioFile = null;

                        try
                        {
                            audioFile = TagLib.File.Create(filePath);
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Log(ex);
                            continue;
                        }

                        var track = new LocalAudio();
                        track.Id = Md5Helper.Md5(filePath);
                        if (!string.IsNullOrEmpty(audioFile.Tag.Title))
                            track.Title = StringHelper.ToUtf8(audioFile.Tag.Title);
                        else
                            track.Title = Path.GetFileNameWithoutExtension(filePath);

                        var artist = audioFile.Tag.FirstPerformer;
                        if (string.IsNullOrEmpty(artist))
                            artist = audioFile.Tag.FirstAlbumArtist;

                        track.Artist = StringHelper.ToUtf8(artist);
                        if (!string.IsNullOrEmpty(track.Artist))
                            track.Artist = track.Artist.Trim();
                        else
                            track.Artist = string.Empty;

                        track.Duration = audioFile.Properties.Duration;
                        track.Source = filePath;

                        if (!string.IsNullOrWhiteSpace(audioFile.Tag.Album))
                        {
                            track.AlbumId = Md5Helper.Md5(track.Artist.Trim().ToLower() + "_" + StringHelper.ToUtf8(audioFile.Tag.Album).Trim());
                            track.Album = StringHelper.ToUtf8(audioFile.Tag.Album).Trim();
                            if (!albums.ContainsKey(track.AlbumId))
                                albums.Add(track.AlbumId, new AudioAlbum() { Id = track.AlbumId, Artist = track.Artist, ArtistId = !string.IsNullOrEmpty(track.Artist) ? Md5Helper.Md5(track.Artist.Trim().ToLower()) : null, Title = StringHelper.ToUtf8(audioFile.Tag.Album), Year = (int)audioFile.Tag.Year });
                            else
                            {
                                if (string.IsNullOrEmpty(albums[track.AlbumId].CoverPath) && audioFile.Tag.Pictures != null && audioFile.Tag.Pictures.Length > 0)
                                {
                                    albums[track.AlbumId].CoverPath = filePath;
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(track.Artist))
                        {
                            track.ArtistId = Md5Helper.Md5(track.Artist.Trim().ToLower());
                            track.Artist = track.Artist.Trim();
                            if (!artists.ContainsKey(track.ArtistId))
                                artists.Add(track.ArtistId, new AudioArtist() { Id = track.ArtistId, Title = track.Artist });
                        }

                        tracks.Add(track);

                        count++;

                        progress.Report(count / totalCount * 100);
                        audioFile.Dispose();
                    }

                    await ServiceLocator.DataBaseService.SaveItems(tracks);
                    await ServiceLocator.DataBaseService.SaveItems(albums.Values);
                    await ServiceLocator.DataBaseService.SaveItems(artists.Values);

                    LoggingService.Log("Music scan finished. Found " + count + " tracks; " + albums.Count + " albums; " + artists.Count + " artists");

                    tracks.Clear();
                    albums.Clear();
                    artists.Clear();

                }, _scanCancellationToken.Token);
            }
            catch (Exception ex)
            {
                LoggingService.Log("Music scan error. " + ex);
            }
        }

        public void ScanMusicCancel()
        {
            _scanCancellationToken.Cancel();

            _scanCancellationToken = new CancellationTokenSource();
        }

        public async Task<List<LocalAudio>> GetTracks()
        {
            return await _tracksRepository.GetTracks();
        }

        public async Task<List<LocalAudio>> GetAlbumTracks(string albumId)
        {
            return await ServiceLocator.DataBaseService.GetLocalAlbumTracks(albumId);
        }

        public async Task<List<AudioAlbum>> GetAlbums()
        {
            return await _albumsRepository.Get();
        }

        public async Task<List<AudioArtist>> GetArtists()
        {
            return await _artistsRepository.Get();
        }

        public async Task<List<AudioAlbum>> GetArtistAlbums(string artistId)
        {
            return await ServiceLocator.DataBaseService.GetLocalArtistAlbums(artistId);
        }

        public async Task<List<LocalAudio>> GetArtistUnsortedTracks(string artistId)
        {
            return await ServiceLocator.DataBaseService.GetLocalArtistUnsortedTracks(artistId);
        }

        public async Task<List<LocalAudio>> SearchTracks(string query)
        {
            //not good, but sqlite doesn't support case insensitive queries for unicode
            var tracks = await ServiceLocator.DataBaseService.GetItems<LocalAudio>();

            return await Task.Run(() =>
            {
                var result = tracks.Where(a => a.Title != null && a.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
                result.AddRange(tracks.Where(a => a.Artist != null && a.Artist.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList());
                result.AddRange(tracks.Where(a => a.Album != null && a.Album.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList());

                tracks.Clear();
                return result;
            });
        }

        public async Task<List<AudioAlbum>> SearchAlbums(string query)
        {
            //not good, but sqlite doesn't support case insensitive queries for unicode
            var albums = await ServiceLocator.DataBaseService.GetItems<AudioAlbum>();

            return await Task.Run(() =>
            {
                var result = albums.Where(a => a.Title != null && a.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
                result.AddRange(albums.Where(a => a.Artist != null && a.Artist.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList());
                albums.Clear();
                return result;
            });
        }

        public async Task<List<AudioArtist>> SearchArtists(string query)
        {
            //not good, but sqlite doesn't support case insensitive queries for unicode
            var artists = await ServiceLocator.DataBaseService.GetItems<AudioArtist>();

            return await Task.Run(() =>
            {
                var result = artists.Where(a => a.Title != null && a.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
                artists.Clear();
                return result;
            });
        }

        public async Task Clear()
        {
            await ServiceLocator.DataBaseService.Clear<Audio>();
            await ServiceLocator.DataBaseService.Clear<AudioAlbum>();
            await ServiceLocator.DataBaseService.Clear<AudioArtist>();
        }
    }
}
