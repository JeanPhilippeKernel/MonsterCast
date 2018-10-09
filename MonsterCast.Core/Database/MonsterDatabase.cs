using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MonsterCast.Core.Database
{
    public class MonsterDatabase : IMonsterDatabase
    {
        private SQLiteAsyncConnection _db = null;

        public SQLiteAsyncConnection Database => _db;

        public EventHandler<DatabaseUpdateEventArgs> DatabaseUpdated { get; set; }

        public async Task<int> AddAsync<T>(T item)
        {
            int inserted = await Database.InsertAsync(item);
            if(inserted > 0)
                DatabaseUpdated?.Invoke(this, new DatabaseUpdateEventArgs());
            return inserted;
        }
        public async Task<int> RemoveAsync<T>(T item)
        {
            int deleted = await Database.DeleteAsync(item);
            if(deleted > 0)
                DatabaseUpdated?.Invoke(this, new DatabaseUpdateEventArgs());
            return deleted;
        }

        public async Task<int> CloseAsync()
        {
            int result = await Task.Factory.StartNew(() =>
            {
                try
                {
                    SQLiteConnectionPool.Shared.Reset();
                    return 0;
                }
                catch (Exception)
                {

                    return -1;
                }
            });
            return result;
        }

        public async Task<int> ConnectAsync()
        {
            StorageFolder _folder = null;
            StorageFile _dbFile = null;


            try
            {
                _folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Database");
                _dbFile = await _folder.CreateFileAsync("castdb", CreationCollisionOption.OpenIfExists);               
                _db = new SQLiteAsyncConnection(_dbFile.Path);
                return 0;
            }
            catch (FileNotFoundException)
            {

                _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Database", CreationCollisionOption.OpenIfExists);
                _dbFile = await _folder.CreateFileAsync("castdb", CreationCollisionOption.OpenIfExists);
                _db = new SQLiteAsyncConnection(_dbFile.Path);
                return 0;
            }
            catch(Exception)
            {
                return -1;
            }
        }

        public async Task<bool> IsAlreadyExist<T>(Expression<Func<T, bool>> predicate)
            where T : class, new()
        {
            T _entity = await Database.Table<T>().Where(predicate).FirstOrDefaultAsync();
            if(_entity != null)
                return true;
            return false;
        }

       
    }
}
