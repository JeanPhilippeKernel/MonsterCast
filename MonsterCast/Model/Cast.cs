using System;
using GalaSoft.MvvmLight.Threading;
using SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterCast.Model
{
    public class Cast
    {      
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Index { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Summary { get; set; }
        public string Address { get; set; }
        public string Song { get; set; }

        [Ignore]
        public BitmapImage Cover { get; set; }
    }
}
