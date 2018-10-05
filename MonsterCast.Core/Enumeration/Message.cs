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
        public const string REQUEST_VIEW_UPDATE_PLAYBACK_BADGE = "request:view:update:playback:badge";
        public const string REQUEST_SET_NAVIGATIONVIEW_CONTENTOVERLAY = "request:view:set_navigationview_contentoverlay";

        public const string REQUEST_FAVORITE_ADD = "request:favorite:add";
        public const string REQUEST_FAVORITE_REMOVE = "request:favorite:remove";

        public const string REQUEST_MEDIAPLAYER_PLAY_SONG = "request:mediaplayer:play_song";
        public const string REQUEST_MEDIAPLAYER_PAUSE_SONG = "request:mediaplayer:pause_song";
        public const string REQUEST_MEDIAPLAYER_RESUME_SONG = "request:mediaplayer:resume_song";
        public const string REQUEST_MEDIAPLAYER_NEXT_SONG = "request:mediaplayer:next_song";
        public const string REQUEST_MEDIAPLAYER_PREVIOUS_SONG = "request:mediaplayer:previous_song";
        public const string REQUEST_MEDIAPLAYER_UPDATE_VOLUME = "request:mediaplayer:update:volume";


        public const string FAVORITE_ADDED = "favorite:added";
        public const string FAVORITE_REMOVED = "favorite:removed";

        //todo : comment these lines
        //public const string MEDIAPLAYER_PLAY_NOW_PLAYING = "mediaplayer:play:now_playing";
        //public const string MEDIAPLAYER_PLAY_PAUSING = "mediaplayer:play:pausing";
        //public const string MEDIAPLAYER_PLAY_END_PLAYING = "mediaplayer:play:end_playing";

        public const string MEDIAPLAYER_MEDIA_OPENED = "mediaplayer:media:opened";
        public const string MEDIAPLAYER_MEDIA_ENDED = "mediaplayer:media:ended";
        public const string MEDIAPLAYER_MEDIA_FAILED = "mediaplayer:media:failed";


        public const string MEDIAPLAYER_SOURCE_CHANGED = "mediaplayer:source:changed";
        public const string MEDIAPLAYER_MEDIA_BUFFERING_STARTED = "mediaplayer:media:buffering:started";
        public const string MEDIAPLAYER_MEDIA_BUFFERING_ENDED = "mediaplayer:media:buffering:ended";

        public const string MEDIAPLAYER_PLAYBACK_STATE_OPENING = "mediaplayer:playback:state:opening";
        public const string MEDIAPLAYER_PLAYBACK_STATE_BUFFERING = "mediaplayer:playback:state:buffering";
        public const string MEDIAPLAYER_PLAYBACK_STATE_PLAYING = "mediaplayer:playback:state:playing";
        public const string MEDIAPLAYER_PLAYBACK_STATE_PAUSED = "mediaplayer:playback:state:paused";
        public const string MEDIAPLAYER_PLAYBACK_POSITION_CHANGED = "mediaplayer:playback:position:changed";

        public const string IS_MEDIAPLAYER_PLAYBACK_STATE_PLAYING = "is:mediaplayer:playback:state:playing";
        public const string IS_MEDIAPLAYER_PLAYBACK_STATE_PAUSED = "is:mediaplayer:playback:state:paused";


        
    }
}
