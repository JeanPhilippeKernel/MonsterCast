using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using MonsterCast.Core.Enumeration;
using GalaSoft.MvvmLight;
using MonsterCast.ViewModel;
using System.Diagnostics;

namespace MonsterCast.Manager
{
    public class MediaPlayerManager
    {
        private readonly MediaPlayer _mediaPlayer = null;
       
        private readonly MediaPlaybackList _mediaPlaybackList = null;
        private IMessenger _messenger = null;
        private Cast _currentPlayingCast = null;

        private bool _isLoopingEnabled = false;

        public MediaPlayerManager(IMessenger messenger)
        {
            _messenger = messenger;
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.CommandManager.IsEnabled = true;
            
            
            _messenger.Register<GenericMessage<Cast>>(this, Message.REQUEST_MEDIAPLAYER_PLAY_SONG, PlayRequestAction);                                             
            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_PAUSE_SONG, NotificationRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_RESUME_SONG, NotificationRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_PREVIOUS_SONG, NotificationRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_NEXT_SONG, NotificationRequestAction);

            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_ENABLE_LOOPING, NotificationRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_DISABLE_LOOPING, NotificationRequestAction);

            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_GET_ACCESS_TO_PLAYBACK_TIMELINE, NotificationRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.REQUEST_MEDIAPLAYER_RELEASE_ACCESS_TO_PLAYBACK_TIMELINE, NotificationRequestAction);

            _messenger.Register<GenericMessage<double>>(this, Message.REQUEST_MEDIAPLAYER_UPDATE_VOLUME, UpdateVolumeRequestAction);
            _messenger.Register<GenericMessage<double>>(this, Message.REQUEST_MEDIAPLAYER_UPDATE_PLAYBACK_TIMELINE_POSITION, UpdatePlaybackTimelinePositionAction);

            _messenger.Register<NotificationMessageWithCallback>(this, Message.IS_MEDIAPLAYER_PLAYBACK_STATE_PLAYING, CheckPlaybackPlayingState);

            _mediaPlayer.MediaOpened += MediaOpened;
            _mediaPlayer.MediaEnded += MediaEnded;
            _mediaPlayer.MediaFailed += MediaFailed;

            _mediaPlayer.BufferingStarted += BufferingStarted;
            _mediaPlayer.BufferingEnded += BufferingEnded;
            _mediaPlayer.SourceChanged += SourceChanged;

            _mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackStateChanged;
            _mediaPlayer.PlaybackSession.PositionChanged += PositionChanged;
        }

        

        private void CheckPlaybackPlayingState(NotificationMessageWithCallback args)
        {
            
            var playbackState = _mediaPlayer.PlaybackSession.PlaybackState;
            if (playbackState == MediaPlaybackState.Playing)
                args.Execute(true);
            else
                args.Execute(false);
        }


        #region Messenger
        private void PlayRequestAction(GenericMessage<Cast> args)
        {
            
            if(ReferenceEquals(_currentPlayingCast, args.Content)
                && _mediaPlayer.PlaybackSession.Position != TimeSpan.FromSeconds(0.0))
            {
                //_messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_MEDIA_ENDED), Message.MEDIAPLAYER_MEDIA_ENDED);
                return;
            }

            _currentPlayingCast = args.Content;
            _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(args.Content.Song, UriKind.Absolute));
            _mediaPlayer.Play();
            _messenger.Send(new GenericMessage<Cast>(_currentPlayingCast), Message.REQUEST_VIEW_UPDATE_PLAYBACK_BADGE);
            
        }

        private void NotificationRequestAction(NotificationMessage args)
        {
            if (args.Notification == Message.REQUEST_MEDIAPLAYER_PAUSE_SONG)
            {
                //_messenger.Send<NotificationMessage, ViewModel.MainViewModel>(new NotificationMessage(Message.MEDIAPLAYER_PLAY_PAUSING));
                _mediaPlayer.Pause();
            }
            else if (args.Notification == Message.REQUEST_MEDIAPLAYER_RESUME_SONG)
            {
                // check if the media player hasn't already ended the song...
                if(_mediaPlayer.PlaybackSession.Position == TimeSpan.FromSeconds(0.0))
                {
                    //_mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(0.0);
                    _messenger.Send(new GenericMessage<Cast>(_currentPlayingCast), Message.REQUEST_MEDIAPLAYER_PLAY_SONG);
                }
                else
                {
                    _mediaPlayer.Play();
                }
               
            }
            else if (args.Notification == Message.REQUEST_MEDIAPLAYER_PREVIOUS_SONG)
            {

            }
            else if (args.Notification == Message.REQUEST_MEDIAPLAYER_NEXT_SONG)
            {

            }

            else if(args.Notification == Message.REQUEST_MEDIAPLAYER_DISABLE_LOOPING)
            {
                _isLoopingEnabled = false;
            }
            else if (args.Notification == Message.REQUEST_MEDIAPLAYER_ENABLE_LOOPING)
            {
                _isLoopingEnabled = true;
            }

            else if(args.Notification == Message.REQUEST_MEDIAPLAYER_GET_ACCESS_TO_PLAYBACK_TIMELINE)
            {
                _mediaPlayer.PlaybackSession.PositionChanged -= PositionChanged;
            }
            else if(args.Notification == Message.REQUEST_MEDIAPLAYER_RELEASE_ACCESS_TO_PLAYBACK_TIMELINE)
            {
                _mediaPlayer.PlaybackSession.PositionChanged += PositionChanged;
            }
        }

        private void UpdateVolumeRequestAction(GenericMessage<double> args)
        {
            _mediaPlayer.Volume = args.Content;
        }

        private void UpdatePlaybackTimelinePositionAction(GenericMessage<double> args)
        {
            
            _mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(args.Content);
        }
        #endregion


        #region Media_Player_Events
        private void MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_MEDIA_FAILED), Message.MEDIAPLAYER_MEDIA_FAILED);

        }

        private void MediaEnded(MediaPlayer sender, object args)
        {
            _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_MEDIA_ENDED), Message.MEDIAPLAYER_MEDIA_ENDED);

            
        }

        private void MediaOpened(MediaPlayer sender, object args)
        {
            _messenger.Send(new NotificationMessage<TimeSpan>(sender.PlaybackSession.NaturalDuration, Message.MEDIAPLAYER_MEDIA_OPENED), Message.MEDIAPLAYER_MEDIA_OPENED);

        }

        private void BufferingStarted(MediaPlayer sender, object args)
        {
            _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_MEDIA_BUFFERING_STARTED), Message.MEDIAPLAYER_MEDIA_BUFFERING_STARTED);
        }

        private void BufferingEnded(MediaPlayer sender, object args)
        {
            _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_MEDIA_BUFFERING_ENDED), Message.MEDIAPLAYER_MEDIA_BUFFERING_ENDED);

        }

        private void SourceChanged(MediaPlayer sender, object args)
        {
            _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_SOURCE_CHANGED), Message.MEDIAPLAYER_SOURCE_CHANGED);
        }

        private void PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            switch (sender.PlaybackState)
            {
                case MediaPlaybackState.Opening:
                    _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_PLAYBACK_STATE_OPENING), Message.MEDIAPLAYER_PLAYBACK_STATE_OPENING);
                    break;

                case MediaPlaybackState.Buffering:
                    _messenger.Send(new NotificationMessage<TimeSpan>(sender.NaturalDuration, Message.MEDIAPLAYER_PLAYBACK_STATE_BUFFERING), Message.MEDIAPLAYER_PLAYBACK_STATE_BUFFERING);
                    break;

                case MediaPlaybackState.Playing:
                    _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_PLAYBACK_STATE_PLAYING), Message.MEDIAPLAYER_PLAYBACK_STATE_PLAYING);
                    break;

                case MediaPlaybackState.Paused:
                    _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_PLAYBACK_STATE_PAUSED), Message.MEDIAPLAYER_PLAYBACK_STATE_PAUSED);
                    break;
            } 
        }

        private void PositionChanged(MediaPlaybackSession sender, object args)
        {
            if (sender.Position.TotalSeconds > sender.NaturalDuration.TotalSeconds)
            {
                _mediaPlayer.Pause();
                _mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(-1.0);
                
                _messenger.Send(new NotificationMessage(Message.MEDIAPLAYER_MEDIA_ENDED), Message.MEDIAPLAYER_MEDIA_ENDED);

                if (_isLoopingEnabled)
                {
                    //_mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(-1.0);
                    _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_RESUME_SONG), Message.REQUEST_MEDIAPLAYER_RESUME_SONG);
                }
            }
            else
            {
                _messenger.Send(new NotificationMessage<TimeSpan>(sender.Position, Message.MEDIAPLAYER_PLAYBACK_POSITION_CHANGED), Message.MEDIAPLAYER_PLAYBACK_POSITION_CHANGED);
            }
        }
        #endregion


    }
}
