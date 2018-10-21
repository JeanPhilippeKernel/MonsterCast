using MonsterCast.Database.Tables;
using System.Collections.Generic;

namespace MonsterCast.Model
{
    public static class  AppConstants
    {
        //private static MediaPlayer _player = null;
        public static IEnumerable<Cast> PodcastCollection { get; set; } = null;      
        //public static MediaPlayer Player => _player ?? new MediaPlayer();
     
        static AppConstants()
        {
            //_player = new MediaPlayer();
                     
        }      
    }
}
