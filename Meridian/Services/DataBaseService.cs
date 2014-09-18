using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            var db = new SQLiteAsyncConnection(_dbPath);

            await db.CreateTablesAsync(typeof(Audio), typeof(LocalAudio)).ConfigureAwait(false);

            Debug.WriteLine("Database initialized.");
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
    }
}
