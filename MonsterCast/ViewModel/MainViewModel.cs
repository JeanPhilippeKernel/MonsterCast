using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MonsterCast.Model;
using MonsterCast.View;
using System.Collections.Generic;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;

namespace MonsterCast.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private RelayCommand<ItemClickEventArgs> _menuItemCommand = null;
        private IMessenger _messenger = null;

        private Cast _activeMedia = null;
        private string _currentMediaStartTime = "00:00:00";
        private string _currentMediaEndTime = "00:00:00";
        private double _volume = 0.2;
        private double _positionMax = 0.0;
        private double _positionValue = 0.0;

        private bool _isBufferingProgress = false;
        #endregion

        #region Properties
        public RelayCommand<ItemClickEventArgs> MenuItemCommand => _menuItemCommand;
        public List<MenuItem> MenuItemCollection => new List<MenuItem>
        {
            new MenuItem {Icon = "ms-appx:///Assets/Menu/nowplaying.png", Name = "Now Playing", PageType = typeof(NowPlayingView)},
            new MenuItem {Icon = "ms-appx:///Assets/Menu/cast.png", Name ="Your casts", PageType = typeof(FavoriteCastView) },
            new MenuItem {Icon = "ms-appx:///Assets/Menu/live.png", Name ="Live", PageType = typeof(LiveView) },            
        };

        public List<MenuItem> OptionalItemCollection => new List<MenuItem>
        {
            new MenuItem {Icon = "ms-appx:///Assets/Menu/info.png", Name ="About", PageType = typeof(AboutView) },            
        };

        public Cast ActiveMedia
        {
            get { return _activeMedia; }
            set { Set(() => ActiveMedia, ref _activeMedia, value); }
        }

        public string CurrentMediaStartTime
        {
            get { return _currentMediaStartTime; }
            set { Set(() => CurrentMediaStartTime, ref _currentMediaStartTime, value); }
        }
        public string  CurrentMediaEndTime
        {
            get { return _currentMediaEndTime; }
            set { Set(() => CurrentMediaEndTime, ref _currentMediaEndTime, value); }
        }

        public double Volume
        {
            get { return (_volume * 100); }
            set
            {
                double newValue = (value / 100.0);
                Set(() => Volume, ref _volume, newValue);
                AppConstants.Player.Volume = _volume;
            }
        }

        public double PositionMax
        {
            get { return _positionMax; }
            set { Set(() => PositionMax, ref _positionMax, value); }
        }
        public double PositionValue
        {
            get { return _positionValue; }
            set { Set(() => PositionValue, ref _positionValue, value); }
        }

        public bool IsBufferingProgress
        {
            get { return _isBufferingProgress; }
            set { Set(() => IsBufferingProgress, ref _isBufferingProgress, value); }
        }
        #endregion

        public MainViewModel(IMessenger messenger)
        {            
            _messenger = messenger;
            _messenger.Register<GenericMessage<Type>>(this, "nav_request", NavRequestAction);
            _messenger.Register<GenericMessage<Cast>>(this, "play_request", PlayRequestAction);

            _menuItemCommand = new RelayCommand<ItemClickEventArgs>(RelayCommandHandler);

            AppConstants.Player.BufferingStarted += Player_BufferingStartedAsync;
            AppConstants.Player.BufferingEnded += Player_BufferingEndedAsync;
            AppConstants.Player.MediaOpened += Player_MediaOpenedAsync;
            AppConstants.Player.MediaEnded += Player_MediaEndedAsync;
            AppConstants.Player.MediaFailed += Player_MediaFailed;
            AppConstants.Player.SourceChanged += Player_SourceChangedAsync;
            AppConstants.Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync;
            AppConstants.Player.Volume = _volume;
        }

        

        private void PlayRequestAction(GenericMessage<Cast> args)
        {
            ActiveMedia = args.Content;
            AppConstants.Player.Source = MediaSource.CreateFromUri(new Uri(args.Content.Song, UriKind.Absolute));           
            AppConstants.Player.Play();
        }


        private async void PlaybackSession_PlaybackStateChangedAsync(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("playback  changed");
            if (sender.PlaybackState == MediaPlaybackState.Buffering
                || sender.PlaybackState == MediaPlaybackState.Playing)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    PositionMax = sender.NaturalDuration.TotalSeconds;
                    //CurrentMediaEndTime = sender.NaturalDuration.ToString(@"hh\:mm\:ss");
                });
            }
        }

        private async void Player_SourceChangedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                PositionValue = 0;
                PositionMax = 0;
                CurrentMediaEndTime = "00:00:00";
                CurrentMediaStartTime = "00:00:00";
            });
        }

        private async void Player_BufferingEndedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() => IsBufferingProgress = false);
        }

        private async void Player_BufferingStartedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() => IsBufferingProgress = true);
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            //todo : show notif !!
            
        }

        private async void Player_MediaEndedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                PositionValue = 0;
                PositionMax = 0;
                CurrentMediaEndTime = "00:00:00";
                CurrentMediaStartTime = "00:00:00";

                _messenger.Send(new NotificationMessage("media ended"), "end_playing");
            });
        }

        private async void Player_MediaOpenedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                IsBufferingProgress = true;
                PositionMax = sender.PlaybackSession.NaturalDuration.TotalSeconds;
                CurrentMediaEndTime = sender.PlaybackSession.NaturalDuration.ToString(@"hh\:mm\:ss");

                _messenger.Send(new GenericMessage<Cast>(ActiveMedia), "now_playing");
            });
            sender.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
        }

        private async void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                PositionValue = sender.Position.TotalSeconds;
                CurrentMediaStartTime = sender.Position.ToString(@"hh\:mm\:ss");
            });
            
        }

        private void NavRequestAction(GenericMessage<Type> arg)
        {
            Messenger.Default.Send(arg.Content);
        }

        //MenuItem click event handler
        private void RelayCommandHandler(ItemClickEventArgs arg)
        {
            var clickedItem = arg.ClickedItem as MenuItem;
            Messenger.Default.Send(clickedItem);
        }
    }
}
