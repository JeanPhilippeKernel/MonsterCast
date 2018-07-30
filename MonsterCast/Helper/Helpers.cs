/*
 *    Author :  Jean Philippe KOUASSI
 *    Location :  Côte d'Ivoire
 *    This is an open source helper, feel free to extend or improve it
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using MonsterCast.Model;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace MonsterCast.Helper
{
    public static class Helpers
    {
        private static SQLiteAsyncConnection _db = null;
        public static SQLiteAsyncConnection Database
        {
            get { return _db ?? throw new Exception("You must set database, see Helpers.SetDatabaseAsync()"); }
            set { _db = value; }
        }
       public static event EventHandler DatabaseUpdated;
        public static async void SetDatabaseAsync()
        {
            try
            {
                var _folder = (await ApplicationData.Current.LocalFolder.GetFoldersAsync())
                    .Where(f => f.Name.Contains("Database"))
                    .FirstOrDefault();

                if (_folder == null)
                {
                    _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Database", CreationCollisionOption.OpenIfExists);
                }

                var dbFile = await _folder.CreateFileAsync("castdb", CreationCollisionOption.OpenIfExists);
                Database = new SQLiteAsyncConnection(dbFile.Path);
                await Database.CreateTableAsync<Cast>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[*] Helper : {ex.Message} ");
            }
        }

        public static async void AddCastToDbAsync(Cast item)
        {
            var _exist = await IsCastExistToDbAsync(item);
            if (!_exist)
            {
                await Database.InsertAsync(item);
                DatabaseUpdated(null, EventArgs.Empty);
            }
        }
        public static async void RemoveCastToDbAsync(Cast item)
        {
            var _exist = await IsCastExistToDbAsync(item);
            if (_exist)
            {
                await Database.DeleteAsync(item);
                DatabaseUpdated(null, EventArgs.Empty);
            }
        }
        public static async Task<bool> IsCastExistToDbAsync(Cast item)
        {  
            var castCollection = await Database.Table<Cast>().ToListAsync();
            var result = castCollection.Exists(e => e.Title == item.Title);
            return result;
        }

        public static void CloseDbConnection()
        {
            SQLiteConnectionPool.Shared.Reset();
            Database = null;
        }
        public static IEnumerable<IList<T>> SplitCollection<T>(ref IEnumerable<T> collection, int splitNumber)
        {
            ICollection<IList<T>> _splitedCollection = new List<IList<T>>();
            int start = 0, end = splitNumber;
            int collectionLength = collection.Count();

            int splitCount = (collectionLength / splitNumber);
            int splitOverflow = (collectionLength % splitNumber);

            for (int x = 0; x < splitCount; ++x)
            {
                IList<T> _list = new List<T>();

                for (int y = start; y < end; ++y)
                    _list.Add(collection.ElementAt(y));

                _splitedCollection.Add(_list);
                start = end;
                end += splitNumber;
            }

            if (splitOverflow > 0)
            {
                IList<T> _list = new List<T>();
                for (int x = start; x < collectionLength; ++x)
                    _list.Add(collection.ElementAt(x));

                _splitedCollection.Add(_list);

            }

            start = end;
            start = 0;
            return _splitedCollection;
        }

        public static async Task<bool> CheckFileExistAsync(string fileName)
        {
            try
            {
                var _file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[*] Helper : File not exist on app local folder - Infos : {ex.Message}");
                return false;
            }           
        }

        public static async Task<string> FetchImageAsync(string url, string castIdentifier)
        {
            var _imagePath = string.Empty;
            using (var _httpClient = new HttpClient())
            {
                var fileExtension = Path.GetExtension(url);
                //generate filename by cast's song url
                var generatedFileName = string.Concat(Path.GetFileNameWithoutExtension(castIdentifier), fileExtension);
                var exist = await CheckFileExistAsync(generatedFileName);
                if (!exist)
                {
                    try
                    {
                        var content = await _httpClient.GetByteArrayAsync(url);
                        var imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(generatedFileName, CreationCollisionOption.OpenIfExists);
                        using (var stream = await imageFile.OpenStreamForWriteAsync())
                        {
                            stream.Position = 0;
                            await stream.WriteAsync(content, 0, content.Length);
                            await stream.FlushAsync();
                            _imagePath = imageFile.Path;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[*] Helper : Error while creating file on app local folder - Infos : {ex.Message}");
                    }
                }
                else
                {
                    var f = await ApplicationData.Current.LocalFolder.GetFileAsync(generatedFileName);
                    _imagePath = f.Path;
                }
            }
            return _imagePath;
        }

        public static void FetchImageParallel(ref IEnumerable<Cast> collection)
        {
            Parallel.ForEach(collection, async (item) =>
            {
                var path = await FetchImageAsync(item.Address, item.Song);
                if (!string.IsNullOrEmpty(path))
                    item.Address = path;
            });
        }
    }                                                                   
}
