using MonsterCast.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MonsterCast.Model
{
    public static class  AppConstants
    {
        private static MediaPlayer _player = null;
        public static IEnumerable<Cast> PodcastCollection { get; set; } = null;      
        public static MediaPlayer Player => _player ?? new MediaPlayer();
     
        static AppConstants()
        {
            _player = new MediaPlayer();
                     
        }      
    }
}
