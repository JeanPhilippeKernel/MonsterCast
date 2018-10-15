/*
 *    Author :  Jean Philippe KOUASSI
 *    Location :  Côte d'Ivoire
 *    This is an open source helper, feel free to extend or improve it
 */
using System;
using System.Collections.Generic;
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

                if (fileExtension != ".jpg")
                {
                    fileExtension = ".jpg";
                    url = Path.ChangeExtension(url, ".jpg");
                }
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
