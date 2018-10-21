using MonsterCast.Database.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MonsterCast.Core.Helpers
{
    public static class Storage
    {
        public static readonly StorageFolder LOCAL_FOLDER = null;
        public static readonly StorageFolder PACKAGE_FOLDER = null;
        static Storage()
        {
            if (LOCAL_FOLDER == null)
                LOCAL_FOLDER = ApplicationData.Current.LocalFolder;

            if (PACKAGE_FOLDER == null)
                PACKAGE_FOLDER = Package.Current.InstalledLocation;
        }

        public static async Task<bool> IsFileExistsLocally(string fileName)
        {
            try
            {
                var _file = await LOCAL_FOLDER.GetFileAsync(fileName);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[*] Helper : File not exist on app local folder - Infos : {ex.Message}");
                return false;
            }
        }

        public static async Task<string> FetchImageAsync(string url, string castIdentifier)
        {
            var _imagePath = string.Empty;
            using (var _httpClient = new HttpClient())
            {
                var fileExtension = Path.GetExtension(url);

                if (fileExtension != ".jpg")
                {
                    fileExtension = ".jpg";
                    url = Path.ChangeExtension(url, ".jpg");
                }
                //generate filename by cast's song url
                var generatedFileName = string.Concat(Path.GetFileNameWithoutExtension(castIdentifier), fileExtension);
                var exist = await IsFileExistsLocally(generatedFileName);
                if (!exist)
                {
                    try
                    {
                        var content = await _httpClient.GetByteArrayAsync(url);
                        var imageFile = await LOCAL_FOLDER.CreateFileAsync(generatedFileName, CreationCollisionOption.OpenIfExists);
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
                        System.Diagnostics.Debug.WriteLine($"[*] Helper : Error while creating file on app local folder - Infos : {ex.Message}");

                    }
                }
                else
                {
                    var f = await LOCAL_FOLDER.GetFileAsync(generatedFileName);
                    _imagePath = f.Path;
                }
            }
            return _imagePath;
        }

        public static void FetchThumbnailAsync(ref IEnumerable<Cast> collection)
        {
            var taskCollection = new List<Task>();

            foreach (var item in collection)
            {
                var action = new Action(async () =>
                {
                    var path = await FetchImageAsync(item.Address, item.Song);
                    if (!string.IsNullOrEmpty(path))
                        item.Address = path;
                });
                var task = Task.Run(action);

                taskCollection.Add(task);
            }
            Task.WaitAll(taskCollection.ToArray());
            //var result  =  Parallel.ForEach(collection, async (item) =>
            //{
            //    var path = await FetchImageAsync(item.Address, item.Song);
            //    if (!string.IsNullOrEmpty(path))
            //        item.Address = path;
            //});

        }
    }
}
