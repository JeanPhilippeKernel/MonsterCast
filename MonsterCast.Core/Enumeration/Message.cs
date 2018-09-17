using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCast.Core.Enumeration
{
    public class Message
    {
        public const string NOTIFICATION_PODCAST_HAS_BEEN_SET = "notification:podcast_has_been_set";
        public const string NOTIFICATION_VIEW_HAS_BEEN_BUILT = "notification:view_has_been_built";

        public const string REQUEST_VIEW_NAVIGATION = "request:view:navigation";
        public const string REQUEST_SET_NAVIGATIONVIEW_CONTENTOVERLAY = "request:view:set_navigationview_contentoverlay";
        public const string REQUEST_FAVORITE_ADD = "request:favorite:add";
        public const string REQUEST_FAVORITE_REMOVE = "request:favorite:remove";
        public const string REQUEST_MEDIAPLAYER_PLAY_SONG = "request:mediaplayer:play_song";
        public const string REQUEST_MEDIAPLAYER_PAUSE_SONG = "request:mediaplayer:pause_song";


        public const string FAVORITE_ADDED = "favorite:added";
        public const string FAVORITE_REMOVED = "favorite:removed";

        public const string MEDIAPLAYER_PLAY_NOW_PLAYING = "mediaplayer:play:now_playing";
        public const string MEDIAPLAYER_PLAY_PAUSING = "mediaplayer:play:pausing";
        public const string MEDIAPLAYER_PLAY_END_PLAYING = "mediaplayer:play:end_playing";
        
    }
}
