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
using Microsoft.UI.Xaml.Controls;

using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using MonsterCast.Core.Enumeration;
using MonsterCast.Manager;
using Windows.UI.Xaml.Controls.Primitives;

namespace MonsterCast.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs> _invokedMenuItemCommand = null;
        private readonly RelayCommand<ItemClickEventArgs> _optionalMenuItemCommand = null;

        private readonly RelayCommand<DragStartedEventArgs> _thumbDragStartedCommand = null;
        private readonly RelayCommand<DragDeltaEventArgs> _thumbDragDeltaCommand = null;
        private readonly RelayCommand<DragCompletedEventArgs> _thumbDragCompleteCommand = null;

        private readonly RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs> _backButtonCommand = null;
        private readonly RelayCommand<NavigationEventArgs> _hostedFrameNavigatedCommand = null;
        private readonly RelayCommand<TappedRoutedEventArgs> _soundFontIconTappedCommand = null;
        private readonly RelayCommand<TappedRoutedEventArgs> _playFontIconTappedCommand = null;
        private readonly RelayCommand<TappedRoutedEventArgs> _loopFontIconTappedCommand = null;

        private readonly RelayCommand _playbackBadgeClickCommand = null;
        private readonly RelayCommand _playbackBadgeInfoFontIconTappedCommand = null;
        private readonly RelayCommand _playbackBadgeLoveFontIconTappedCommand = null;  
        
        private IMessenger _messenger = null;       

        private Cast _activeMedia = null;
        private string _currentMediaStartTime = "00:00:00";
        private string _currentMediaEndTime = "00:00:00";
        private double _volume = 0.2;
        private double _positionMax = 0.0;
        private double _positionValue = 0.0;

        private bool _isBufferingProgress = false;

        
        private Microsoft.UI.Xaml.Controls.NavigationView _mainNavigationView = null;
        private Microsoft.UI.Xaml.Controls.NavigationViewItem _currentNavigationViewItem = null;
        private Frame _hostedFrame = null;
        private Button _navigationViewBackButton = null;
        private bool _isBackButtonEnable = false;

        private Slider _playbackTimelineSlider = null;

        private FontIcon _soundFontIcon = null;
        private FontIcon _playFontIcon = null;
        private FontIcon _loopFontIcon = null;


        private Button _playbackBadge = null;
        private FontIcon _playbackBadgeInfoFontIcon = null;
        private FontIcon _playbackBadgeLoveFontIcon = null;

        #endregion

        #region Properties

        public RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs> InvokedMenuItemCommand => _invokedMenuItemCommand;       
        public RelayCommand<ItemClickEventArgs> OptionalMenuItemCommand => _optionalMenuItemCommand;

        public  RelayCommand<DragStartedEventArgs> ThumbDragStartedCommand => _thumbDragStartedCommand;
        public  RelayCommand<DragDeltaEventArgs> ThumbDragDeltaCommand => _thumbDragDeltaCommand;
        public  RelayCommand<DragCompletedEventArgs> ThumbDragCompleteCommand => _thumbDragCompleteCommand;

        public RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs> BackButtonCommand => _backButtonCommand;
        public RelayCommand<NavigationEventArgs> HostedFrameNavigatedCommand => _hostedFrameNavigatedCommand;

        public RelayCommand<TappedRoutedEventArgs> SoundFontIconTappedCommand => _soundFontIconTappedCommand;
        public RelayCommand<TappedRoutedEventArgs> PlayFontIconTappedCommand => _playFontIconTappedCommand;
        public RelayCommand<TappedRoutedEventArgs> LoopFontIconTappedCommand => _loopFontIconTappedCommand;
        public RelayCommand PlaybackBadgeClickCommand => _playbackBadgeClickCommand;
        public RelayCommand PlaybackBadgeInfoFontIconTappedCommand => _playbackBadgeInfoFontIconTappedCommand;
        public RelayCommand PlaybackBadgeLoveFontIconTappedCommand => _playbackBadgeLoveFontIconTappedCommand;
        public List<Microsoft.UI.Xaml.Controls.NavigationViewItem> MenuItemCollection => new List<Microsoft.UI.Xaml.Controls.NavigationViewItem>
        {

            new Microsoft.UI.Xaml.Controls.NavigationViewItem { Icon = new FontIcon() { Glyph= "\uE93C", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Blue) } , Content = "All podcasts", Tag = typeof(DefaultView) },
            //new Microsoft.UI.Xaml.Controls.NavigationViewItem { Icon = new FontIcon() { Glyph= "\uE77B", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets") } , Content ="Artists", Tag = typeof(FavoriteCastView)},
            new Microsoft.UI.Xaml.Controls.NavigationViewItem { Icon = new FontIcon() { Glyph= "\uE728", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Blue) } , Content ="Your favorites", Tag = typeof(FavoriteCastView)},

            new Microsoft.UI.Xaml.Controls.NavigationViewItem { Icon = new FontIcon() { Glyph= "\uE7F6", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Blue) }, Content = "Now Playing", Tag = typeof(NowPlayingView)},
            //new MenuItem {Icon = "ms-appx:///Assets/Menu/live.png", Name ="Live", PageType = typeof(LiveView) },            
        };

        public List<Microsoft.UI.Xaml.Controls.NavigationViewItem> OptionalItemCollection => new List<Microsoft.UI.Xaml.Controls.NavigationViewItem>
        {
            new Microsoft.UI.Xaml.Controls.NavigationViewItem { Icon = new FontIcon(){ Glyph= "\uE946", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets") }, Content= "", Tag= typeof(AboutView)}
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
                
                _messenger.Send(new GenericMessage<double>(_volume), Message.REQUEST_MEDIAPLAYER_UPDATE_VOLUME);
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


        public Microsoft.UI.Xaml.Controls.NavigationView MainNavigationView
        {
            get { return _mainNavigationView; }
            set { Set(() => MainNavigationView, ref _mainNavigationView, value); }
        }
        public Frame HostedFrame
        {
            get { return _hostedFrame; }
            set { Set(() => HostedFrame, ref _hostedFrame, value); }
        }

        public Button NavigationViewBackButton
        {
            get { return _navigationViewBackButton; }
            set { Set(() => NavigationViewBackButton, ref _navigationViewBackButton, value); }
        }

        public FontIcon PlayFontIcon
        {
            get { return _playFontIcon; }
            set { Set(() => PlayFontIcon, ref _playFontIcon, value); }
        }

        public FontIcon SoundFontIcon
        {
            get { return _soundFontIcon; }
            set { Set(() => SoundFontIcon, ref _soundFontIcon, value); }
        }

        public FontIcon LoopFontIcon
        {
            get { return _loopFontIcon; }
            set { Set(() => LoopFontIcon, ref _loopFontIcon, value); }
        }

        public Button PlaybackBadge
        {
            get { return _playbackBadge; }
            set { Set(() => PlaybackBadge, ref _playbackBadge, value); }
        }

        public FontIcon PlaybackBadgeLoveFontIcon
        {
            get { return _playbackBadgeLoveFontIcon; }
            set { Set(() => PlaybackBadgeLoveFontIcon, ref _playbackBadgeLoveFontIcon, value); }
        }

        public FontIcon PlaybackBadgeInfoFontIcon
        {
            get { return _playbackBadgeInfoFontIcon; }
            set { Set(() => PlaybackBadgeInfoFontIcon, ref _playbackBadgeInfoFontIcon, value); }
        }

        public bool IsBackButtonEnable
        {
            get { return _isBackButtonEnable; }
            set { Set(() => IsBackButtonEnable, ref _isBackButtonEnable, value); }
        }

        public Slider PlaybackTimelineSlider
        {
            get { return _playbackTimelineSlider; }
            set { Set(() => PlaybackTimelineSlider, ref _playbackTimelineSlider, value); }
        }

        

        public delegate void NotificationCallback(bool value);
        public NotificationCallback _notificationMessageCallback { get; set; }
        #endregion

        public MainViewModel(IMessenger messenger)
        {            
            _messenger = messenger;
            _notificationMessageCallback = this.MediaPlayerPlaybackCallback;

            _messenger.Register<NotificationMessage<Type>>(this, ViewBuiltNotificationAction);
            _messenger.Register<GenericMessage<Type>>(this, Message.REQUEST_VIEW_NAVIGATION, NavigationViewRequestAction);        
            _messenger.Register<GenericMessage<Cast>>(this, Message.REQUEST_VIEW_UPDATE_PLAYBACK_BADGE, UpdateViewPlaybackBadgeRequestAction);        
            _messenger.Register<NotificationMessage<Cast>>(this, Message.REQUEST_SET_NAVIGATIONVIEW_CONTENTOVERLAY, SetNavigationViewContentOverlay);

            _messenger.Register<NotificationMessage<TimeSpan>>(this, Message.MEDIAPLAYER_MEDIA_OPENED, MediaOpenedAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_MEDIA_ENDED, MediaEndedAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_MEDIA_FAILED, MediaFailedAction);

            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_MEDIA_BUFFERING_STARTED, MediaBufferingStartedAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_MEDIA_BUFFERING_ENDED, MediaBufferingEndedAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_SOURCE_CHANGED, MediaSourceChangedAction);

            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_PLAYBACK_STATE_OPENING, PlaybackStateOpeningAction);
            _messenger.Register<NotificationMessage<TimeSpan>>(this, Message.MEDIAPLAYER_PLAYBACK_STATE_BUFFERING, PlaybackStateBufferingAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_PLAYBACK_STATE_PLAYING, PlaybackStatePlayingAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_PLAYBACK_STATE_PAUSED, PlaybackStatePausedAction);
            _messenger.Register<NotificationMessage<TimeSpan>>(this, Message.MEDIAPLAYER_PLAYBACK_POSITION_CHANGED, PlaybackPositionChangedAction);

            _messenger.Send(new GenericMessage<double>(_volume), Message.REQUEST_MEDIAPLAYER_UPDATE_VOLUME);
            
            _invokedMenuItemCommand = new RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs>(InvokedMenuItemRelayCommand);
            _optionalMenuItemCommand = new RelayCommand<ItemClickEventArgs>(OptionalRelayCommandHandler);

          

            _thumbDragStartedCommand = new RelayCommand<DragStartedEventArgs>(ThumbDragStartedRelayCommand);
            _thumbDragDeltaCommand = new RelayCommand<DragDeltaEventArgs>(ThumbDragDeltaRelayCommand);
            _thumbDragCompleteCommand = new RelayCommand<DragCompletedEventArgs>(ThumbDragCompleteRelayCommand);

            _backButtonCommand = new RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs>(BackButtonRelayCommand);
            _hostedFrameNavigatedCommand = new RelayCommand<NavigationEventArgs>(HostedFrameNavigatedRelayCommand);

            _soundFontIconTappedCommand = new RelayCommand<TappedRoutedEventArgs>(SoundFontIconTappedRelayCommand);
            _playFontIconTappedCommand = new RelayCommand<TappedRoutedEventArgs>(PlayFontIconTappedRelayCommand);
            _loopFontIconTappedCommand = new RelayCommand<TappedRoutedEventArgs>(LoopFontIconTappedRelayCommandAsync);
            _playbackBadgeClickCommand = new RelayCommand(PlaybackBadgeRelayCommandAsync);

            _playbackBadgeLoveFontIconTappedCommand = new RelayCommand(PlaybackBadgeLoveRelayCommandAsync);
            _playbackBadgeInfoFontIconTappedCommand = new RelayCommand(PlaybackBadgeInfoRelayCommandAsync);
        }

        

        private void MediaOpenedAction(NotificationMessage<TimeSpan> args)
        {
            if(args.Notification == Message.MEDIAPLAYER_MEDIA_OPENED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    IsBufferingProgress = true;
                    PositionMax = args.Content.TotalSeconds;
                    CurrentMediaEndTime = args.Content.ToString(@"hh\:mm\:ss");
                });
            }
        }

        private void MediaEndedAction(NotificationMessage args)
        {
            if (args.Notification == Message.MEDIAPLAYER_MEDIA_ENDED)
            {

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {

                    PositionValue = 0;
                    PositionMax = 0;
                    CurrentMediaEndTime = "00:00:00";
                    CurrentMediaStartTime = "00:00:00";
                });

                //_messenger.Send(new NotificationMessage("media ended"), Message.MEDIAPLAYER_PLAY_END_PLAYING);
            }
        }

        private void MediaFailedAction(NotificationMessage args)
        {
            //throw new NotImplementedException();
        }

        private void MediaBufferingStartedAction(NotificationMessage args)
        {
            //throw new NotImplementedException();
            if(args.Notification == Message.MEDIAPLAYER_MEDIA_BUFFERING_STARTED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsBufferingProgress = true);
            }
        }

        private void MediaBufferingEndedAction(NotificationMessage args)
        {
            if (args.Notification == Message.MEDIAPLAYER_MEDIA_BUFFERING_ENDED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsBufferingProgress = false);
            }
            
        }

        private void MediaSourceChangedAction(NotificationMessage args)
        {
            //throw new NotImplementedException();
            if(args.Notification == Message.MEDIAPLAYER_SOURCE_CHANGED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    PositionValue = 0;
                    PositionMax = 0;
                    CurrentMediaEndTime = "00:00:00";
                    CurrentMediaStartTime = "00:00:00";
                });
            }
        }


        private void PlaybackStatePausedAction(NotificationMessage args)
        {
            //throw new NotImplementedException();
            if (args.Notification == Message.MEDIAPLAYER_PLAYBACK_STATE_PAUSED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => PlayFontIcon.Glyph = "\uE768");
            }
        }

        private void PlaybackStatePlayingAction(NotificationMessage args)
        {
            //throw new NotImplementedException();
            if (args.Notification == Message.MEDIAPLAYER_PLAYBACK_STATE_PLAYING)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => PlayFontIcon.Glyph = "\uE769");
            }
            
        }

        private void PlaybackStateBufferingAction(NotificationMessage<TimeSpan> args)
        {
            //throw new NotImplementedException();
            if (args.Notification == Message.MEDIAPLAYER_PLAYBACK_STATE_BUFFERING)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => PositionMax = args.Content.TotalSeconds);
            }
        }

        private void PlaybackStateOpeningAction(NotificationMessage args)
        {
            //throw new NotImplementedException();
            if (args.Notification == Message.MEDIAPLAYER_PLAYBACK_STATE_OPENING)
            {
                //PositionMax = args.Content.TotalSeconds;
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsBufferingProgress = true);
            }

        }

        private void PlaybackPositionChangedAction(NotificationMessage<TimeSpan> args)
        {
           if(args.Notification ==  Message.MEDIAPLAYER_PLAYBACK_POSITION_CHANGED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    PositionValue = args.Content.TotalSeconds;
                    CurrentMediaStartTime = args.Content.ToString(@"hh\:mm\:ss");
                });
            }
        }







        #region Messenger_Method
        private void ViewBuiltNotificationAction(NotificationMessage<Type> args)
        {
            if(args.Notification == Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
               
                var _navigationViewItem = ((IList<Microsoft.UI.Xaml.Controls.NavigationViewItem>)MainNavigationView.MenuItemsSource).Single(e => (Type)e.Tag == args.Content);
                _navigationViewItem.IsSelected = true;

                _currentNavigationViewItem = _navigationViewItem;
                //HostedFrame.Navigate(args.Content);
            }
           
        }

        private void UpdateViewPlaybackBadgeRequestAction(GenericMessage<Cast> args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => ActiveMedia = args.Content);
        }

        //private void PlayRequestAction(GenericMessage<Cast> args)
        //{
        //    ActiveMedia = args.Content;
            
        //    AppConstants.Player.Source = MediaSource.CreateFromUri(new Uri(args.Content.Song, UriKind.Absolute));
        //    AppConstants.Player.Play();
        //}

        //private void PauseRequestAction(GenericMessage<Cast> args)
        //{
        //    if (AppConstants.Player.PlaybackSession.CanPause)
        //        AppConstants.Player.Pause();
        //}

        private void NavigationViewRequestAction(GenericMessage<Type> arg)
        {
            HostedFrame.ForwardStack.Clear();
            HostedFrame.BackStack.Clear();

            HostedFrame.Navigate(arg.Content);

            HostedFrame.ForwardStack.Clear();
            HostedFrame.BackStack.Clear();
        }

        private void SetNavigationViewContentOverlay(NotificationMessage<Cast> args)
        {
            
        }
        #endregion

        #region MediaPlayer_Method

        private async void PlaybackSession_PlaybackStateChangedAsync(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine($"playback changed : {sender.PlaybackState.ToString()}");
            if (sender.PlaybackState == MediaPlaybackState.Buffering || sender.PlaybackState == MediaPlaybackState.Playing)
                await DispatcherHelper.RunAsync(() => PositionMax = sender.NaturalDuration.TotalSeconds);
            
            if (sender.PlaybackState == MediaPlaybackState.Opening)
                await DispatcherHelper.RunAsync(() => IsBufferingProgress = true);
        }

        private async void PlaybackSession_PlaybackStateChangedAsync_2(MediaPlaybackSession sender, object args)
        {
            if (sender.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayFontIcon.Glyph = "\uE769";

                });
            }
            else if (sender.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayFontIcon.Glyph = "\uE768";

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

                //_messenger.Send(new NotificationMessage("media ended"), Message.MEDIAPLAYER_PLAY_END_PLAYING);
            });
        }

        private async void Player_MediaOpenedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                IsBufferingProgress = true;
                PositionMax = sender.PlaybackSession.NaturalDuration.TotalSeconds;
                CurrentMediaEndTime = sender.PlaybackSession.NaturalDuration.ToString(@"hh\:mm\:ss");

                //_messenger.Send(new GenericMessage<Cast>(ActiveMedia), Message.MEDIAPLAYER_PLAY_NOW_PLAYING);
            });
                                           
            sender.PlaybackSession.PositionChanged -= PlaybackSession_PositionChangedAsync;
            sender.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
        }

        private async void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("position : " + sender.Position.ToString(@"hh\:mm\:ss"));
            await DispatcherHelper.RunAsync(() =>
            {
                PositionValue = sender.Position.TotalSeconds;
                CurrentMediaStartTime = sender.Position.ToString(@"hh\:mm\:ss");
            });
               
        }
        #endregion

        #region RelayCommand_Method
        private async void PlaybackBadgeInfoRelayCommandAsync()
        {
            //var placementElement = PlaybackBadge as Windows.UI.Xaml.FrameworkElement;
            //await DispatcherHelper.RunAsync(() => PlaybackBadge.ContextFlyout.ShowAt(placementElement));
        }
        private async void PlaybackBadgeLoveRelayCommandAsync()
        {
            //var placementElement = PlaybackBadge as Windows.UI.Xaml.FrameworkElement;
            //await DispatcherHelper.RunAsync(() => PlaybackBadge.ContextFlyout.ShowAt(placementElement));
        }


        private async void PlaybackBadgeRelayCommandAsync()
        {
            var placementElement = PlaybackBadge as Windows.UI.Xaml.FrameworkElement;
            await DispatcherHelper.RunAsync(() => PlaybackBadge.ContextFlyout.ShowAt(placementElement));
        }

        private void LoopFontIconTappedRelayCommandAsync(TappedRoutedEventArgs args)
        {
            var loopFontIconBrush = LoopFontIcon.Foreground as Windows.UI.Xaml.Media.SolidColorBrush;
            if(loopFontIconBrush.Color == Windows.UI.Colors.White)
            {
                _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_ENABLE_LOOPING), Message.REQUEST_MEDIAPLAYER_ENABLE_LOOPING);
                LoopFontIcon.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Orange);
            }
            else if(loopFontIconBrush.Color == Windows.UI.Colors.Orange)
            {
                _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_DISABLE_LOOPING), Message.REQUEST_MEDIAPLAYER_DISABLE_LOOPING);
                LoopFontIcon.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.White);
            }

                 
        }


        private void PlayFontIconTappedRelayCommand(TappedRoutedEventArgs args)
        {
            if (ActiveMedia != null)
                _messenger.Send(new NotificationMessageWithCallback(Message.IS_MEDIAPLAYER_PLAYBACK_STATE_PLAYING, _notificationMessageCallback), 
                    Message.IS_MEDIAPLAYER_PLAYBACK_STATE_PLAYING);
        }

        public void MediaPlayerPlaybackCallback(bool result)
        {
           if(result)
            {
                _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_PAUSE_SONG), Message.REQUEST_MEDIAPLAYER_PAUSE_SONG);
               DispatcherHelper.CheckBeginInvokeOnUI(() => PlayFontIcon.Glyph = "\uE768");
            }
            else
            {
                _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_RESUME_SONG), Message.REQUEST_MEDIAPLAYER_RESUME_SONG);
                DispatcherHelper.CheckBeginInvokeOnUI(() => PlayFontIcon.Glyph = "\uE769");
            }
        }

        private async void SoundFontIconTappedRelayCommand(TappedRoutedEventArgs args)
        {
            var placementElement = SoundFontIcon as Windows.UI.Xaml.FrameworkElement;
            await DispatcherHelper.RunAsync(() => SoundFontIcon.ContextFlyout.ShowAt(placementElement));
        }

        private void HostedFrameNavigatedRelayCommand(NavigationEventArgs obj)
        {
            if (HostedFrame != null)
            {
                IsBackButtonEnable = HostedFrame.CanGoBack ? true : false;
                NavigationViewBackButton.IsEnabled = IsBackButtonEnable;
            }
        }
                                                                 
      
        private void ThumbDragStartedRelayCommand(DragStartedEventArgs args)
        {
            _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_GET_ACCESS_TO_PLAYBACK_TIMELINE), 
                Message.REQUEST_MEDIAPLAYER_GET_ACCESS_TO_PLAYBACK_TIMELINE);

        }
        private void ThumbDragDeltaRelayCommand(DragDeltaEventArgs args)
        {
            _messenger.Send(new GenericMessage<double>(PlaybackTimelineSlider.Value),
                Message.REQUEST_MEDIAPLAYER_UPDATE_PLAYBACK_TIMELINE_POSITION);
        }

        private void ThumbDragCompleteRelayCommand(DragCompletedEventArgs args)
        {                                           
            _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_RELEASE_ACCESS_TO_PLAYBACK_TIMELINE), 
                Message.REQUEST_MEDIAPLAYER_RELEASE_ACCESS_TO_PLAYBACK_TIMELINE);
        }

        private void BackButtonRelayCommand(Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            if (HostedFrame.CanGoBack)
                HostedFrame.GoBack();
        }

        private void InvokedMenuItemRelayCommand(Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {            
            if(_currentNavigationViewItem != null 
                && (string)_currentNavigationViewItem.Content == (string)args.InvokedItem)
                return;

            var navigationViewItem = MenuItemCollection.Single(e => (string)e.Content == (string)args.InvokedItem);
            var pageType = navigationViewItem.Tag as Type;
            _messenger.Send(new GenericMessage<Type>(pageType), Message.REQUEST_VIEW_NAVIGATION);

            _currentNavigationViewItem = navigationViewItem;

  
        }

        private void OptionalRelayCommandHandler(ItemClickEventArgs args)
        {
            if (_currentNavigationViewItem != null
                && (string)_currentNavigationViewItem.Content == (string)args.ClickedItem)
                return;

           //unselect all items in menuItemCollection
           var selectedMenuItem = ((IList<Microsoft.UI.Xaml.Controls.NavigationViewItem>)MainNavigationView.MenuItemsSource).Single(e => e.IsSelected);
            selectedMenuItem.IsSelected = false;


            var paneFooterCollection = ((Microsoft.UI.Xaml.Controls.NavigationViewList)MainNavigationView.PaneFooter).ItemsSource as IList<Microsoft.UI.Xaml.Controls.NavigationViewItem>;
            var navigationViewItem = paneFooterCollection.Single(e => (string)e.Content == (string)args.ClickedItem);
            navigationViewItem.IsSelected = true;
            var pageType = navigationViewItem.Tag as Type;
            _messenger.Send(new GenericMessage<Type>(pageType), Message.REQUEST_VIEW_NAVIGATION);

            _currentNavigationViewItem = navigationViewItem;
        }
        #endregion


    }
}
