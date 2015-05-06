using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Meridian.Model;
using SQLite;

namespace Meridian.Services
{
    public class DataBaseService
    {
        private const string DbName = "music.library";
        private readonly string _dbPath;


        public DataBaseService()
        {
            _dbPath = System.IO.Path.Combine(App.Root, DbName);
        }

        public async Task Initialize()
        {
            var db = new SQLiteAsyncConnection(_dbPath, caseSensitive: false);

            await db.CreateTablesAsync(
                typeof(Audio), 
                typeof(LocalAudio),
                typeof(AudioAlbum), 
                typeof(AudioArtist)).ConfigureAwait(false);

            Debug.WriteLine("Database initialized.");
        }

        public async Task Clear<T>() where T : new()
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            await db.DeleteAllAsync<T>();
        }

        public async Task SaveItems<T>(IEnumerable<T> items) where T : new()
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            await db.InsertOrIgnoreAllAsync(items).ConfigureAwait(false);
        }

        public async Task SaveItem<T>(T item) where T : new()
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            await db.InsertOrIgnoreAsync(item).ConfigureAwait(false);
        }

        public Task<List<T>> GetItems<T>() where T : new()
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            return db.Table<T>().ToListAsync();
        }

        public async Task UpdateItems(IEnumerable items)
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            await db.UpdateAllAsync(items);
        }

        public async Task DeleteItems(IEnumerable items)
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            foreach (var item in items)
            {
                await db.DeleteAsync(item);
            }
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            var db = new SQLiteAsyncConnection(_dbPath, caseSensitive: false);

            return await db.Table<T>().Where(predicate).ToListAsync();
        }

        //albums
        public async Task<List<LocalAudio>> GetLocalAlbumTracks(string albumId)
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            var tracks = await db.Table<LocalAudio>().Where(track => track.AlbumId == albumId).ToListAsync();

            return tracks;
        }

        public async Task<List<AudioAlbum>> GetLocalArtistAlbums(string artistId)
        {
            var db = new SQLiteAsyncConnection(_dbPath);
            var a = await db.Table<AudioAlbum>().ToListAsync();

            var albums = await db.Table<AudioAlbum>().Where(album => album.ArtistId == artistId).ToListAsync();

            return albums;
        }

        public async Task<List<LocalAudio>> GetLocalArtistUnsortedTracks(string artistId)
        {
            var db = new SQLiteAsyncConnection(_dbPath);

            var tracks = await db.Table<LocalAudio>().Where(track => track.ArtistId == artistId && (track.AlbumId == null || track.AlbumId == string.Empty)).ToListAsync();

            return tracks;
        }
    }
}
