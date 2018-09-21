﻿using System;
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

namespace MonsterCast.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs> _invokedMenuItemCommand = null;
        private readonly RelayCommand<ItemClickEventArgs> _optionalMenuItemCommand = null;
        private readonly RelayCommand<ManipulationCompletedRoutedEventArgs> _thumbManipulationCompletedCommand = null;
        private readonly RelayCommand<ManipulationStartedRoutedEventArgs> _thumbManipulationStartedCommand = null;
        private readonly RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs> _backButtonCommand = null;
        private readonly RelayCommand<NavigationEventArgs> _hostedFrameNavigatedCommand = null;
        private readonly RelayCommand<TappedRoutedEventArgs> _soundFontIconTappedCommand = null;
        private readonly RelayCommand<TappedRoutedEventArgs> _playFontIconTappedCommand = null;
        private readonly RelayCommand<TappedRoutedEventArgs> _loopFontIconTappedCommand = null;
        private readonly RelayCommand _playbackBadgeClickCommand = null;      
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

        private FontIcon _soundFontIcon = null;
        private FontIcon _playFontIcon = null;
        private FontIcon _loopFontIcon = null;
        private Button _playbackBadge = null;
        #endregion

        #region Properties
        public RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs> InvokedMenuItemCommand => _invokedMenuItemCommand;       
        public RelayCommand<ItemClickEventArgs> OptionalMenuItemCommand => _optionalMenuItemCommand;
        public RelayCommand<ManipulationCompletedRoutedEventArgs> ThumbManipulationCompletedCommand => _thumbManipulationCompletedCommand;
        public RelayCommand<ManipulationStartedRoutedEventArgs> ThumbManipulationStartedCommand => _thumbManipulationStartedCommand;
        public RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs> BackButtonCommand => _backButtonCommand;
        public RelayCommand<NavigationEventArgs> HostedFrameNavigatedCommand => _hostedFrameNavigatedCommand;


        public RelayCommand<TappedRoutedEventArgs> SoundFontIconTappedCommand => _soundFontIconTappedCommand;
        public RelayCommand<TappedRoutedEventArgs> PlayFontIconTappedCommand => _playFontIconTappedCommand;
        public RelayCommand<TappedRoutedEventArgs> LoopFontIconTappedCommand => _loopFontIconTappedCommand;
        public RelayCommand PlaybackBadgeClickCommand => _playbackBadgeClickCommand;
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

        public bool IsBackButtonEnable
        {
            get { return _isBackButtonEnable; }
            set { Set(() => IsBackButtonEnable, ref _isBackButtonEnable, value); }
        }
        #endregion

        public MainViewModel(IMessenger messenger)
        {            
            _messenger = messenger;
            _messenger.Register<NotificationMessage<Type>>(this, ViewBuiltNotificationAction);
            _messenger.Register<GenericMessage<Type>>(this, Core.Enumeration.Message.REQUEST_VIEW_NAVIGATION, NavigationViewRequestAction);
            _messenger.Register<GenericMessage<Cast>>(this, Core.Enumeration.Message.REQUEST_MEDIAPLAYER_PLAY_SONG, PlayRequestAction);
            _messenger.Register<GenericMessage<Cast>>(this, Core.Enumeration.Message.REQUEST_MEDIAPLAYER_PAUSE_SONG, PauseRequestAction);
            _messenger.Register<NotificationMessage<Cast>>(this, Core.Enumeration.Message.REQUEST_SET_NAVIGATIONVIEW_CONTENTOVERLAY, SetNavigationViewContentOverlay);

            
            _invokedMenuItemCommand = new RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs>(InvokedMenuItemRelayCommand);
            _optionalMenuItemCommand = new RelayCommand<ItemClickEventArgs>(OptionalRelayCommandHandler);
            _thumbManipulationCompletedCommand = new RelayCommand<ManipulationCompletedRoutedEventArgs>(ThumbManipulationCompletedRelayCommand);
            _thumbManipulationStartedCommand = new RelayCommand<ManipulationStartedRoutedEventArgs>(ThumbManipulationStartedRelayCommand);
            _backButtonCommand = new RelayCommand<Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs>(BackButtonRelayCommand);
            _hostedFrameNavigatedCommand = new RelayCommand<NavigationEventArgs>(HostedFrameNavigatedRelayCommand);

            _soundFontIconTappedCommand = new RelayCommand<TappedRoutedEventArgs>(SoundFontIconTappedRelayCommand);
            _playFontIconTappedCommand = new RelayCommand<TappedRoutedEventArgs>(PlayFontIconTappedRelayCommand);
            _loopFontIconTappedCommand = new RelayCommand<TappedRoutedEventArgs>(LoopFontIconTappedRelayCommandAsync);
            _playbackBadgeClickCommand = new RelayCommand(PlaybackBadgeRelayCommandAsync);

            AppConstants.Player.BufferingStarted += Player_BufferingStartedAsync;
            AppConstants.Player.BufferingEnded += Player_BufferingEndedAsync;
            AppConstants.Player.MediaOpened += Player_MediaOpenedAsync;
            AppConstants.Player.MediaEnded += Player_MediaEndedAsync;
            AppConstants.Player.MediaFailed += Player_MediaFailed;
            AppConstants.Player.SourceChanged += Player_SourceChangedAsync;
            AppConstants.Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync;
            AppConstants.Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync_2;
            AppConstants.Player.Volume = _volume;           
        }

        


        #region Messenger_Method
        private void ViewBuiltNotificationAction(NotificationMessage<Type> args)
        {
            if(args.Notification == Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
               
                var _navigationViewItem = ((IList<Microsoft.UI.Xaml.Controls.NavigationViewItem>)MainNavigationView.MenuItemsSource).Single(e => (Type)e.Tag == args.Content);
                _navigationViewItem.IsSelected = true;

                _currentNavigationViewItem = _navigationViewItem;
                HostedFrame.Navigate(args.Content);
            }
           
        }

        private void PlayRequestAction(GenericMessage<Cast> args)
        {
            ActiveMedia = args.Content;
            AppConstants.Player.Source = MediaSource.CreateFromUri(new Uri(args.Content.Song, UriKind.Absolute));
            AppConstants.Player.Play();
        }

        private void PauseRequestAction(GenericMessage<Cast> args)
        {
            if (AppConstants.Player.PlaybackSession.CanPause)
                AppConstants.Player.Pause();
        }

        private void NavigationViewRequestAction(GenericMessage<Type> arg)
        {
            HostedFrame.Navigate(arg.Content);          
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

                _messenger.Send(new NotificationMessage("media ended"), Core.Enumeration.Message.MEDIAPLAYER_PLAY_END_PLAYING);
            });
        }

        private async void Player_MediaOpenedAsync(MediaPlayer sender, object args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                IsBufferingProgress = true;
                PositionMax = sender.PlaybackSession.NaturalDuration.TotalSeconds;
                CurrentMediaEndTime = sender.PlaybackSession.NaturalDuration.ToString(@"hh\:mm\:ss");

                _messenger.Send(new GenericMessage<Cast>(ActiveMedia), Core.Enumeration.Message.MEDIAPLAYER_PLAY_NOW_PLAYING);
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

        private async void PlaybackBadgeRelayCommandAsync()
        {
            var placementElement = PlaybackBadge as Windows.UI.Xaml.FrameworkElement;
            await DispatcherHelper.RunAsync(() => PlaybackBadge.ContextFlyout.ShowAt(placementElement));
        }

        private async void LoopFontIconTappedRelayCommandAsync(TappedRoutedEventArgs argss)
        {
            AppConstants.Player.IsLoopingEnabled = AppConstants.Player.IsLoopingEnabled == true ? false : true;
            
            if (AppConstants.Player.IsLoopingEnabled)
                await DispatcherHelper.RunAsync(() => LoopFontIcon.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Orange));
            else
                await DispatcherHelper.RunAsync(() => LoopFontIcon.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.White));           
        }


        private async void PlayFontIconTappedRelayCommand(TappedRoutedEventArgs args)
        {
            if (AppConstants.Player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                AppConstants.Player.Play();
                await DispatcherHelper.RunAsync(() => PlayFontIcon.Glyph = "\uE769");
            }
            else if (AppConstants.Player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                AppConstants.Player.Pause();
                await DispatcherHelper.RunAsync(() => PlayFontIcon.Glyph = "\uE768");               
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
                                                                 
        private void ThumbManipulationStartedRelayCommand(ManipulationStartedRoutedEventArgs args)
        {
            Debug.WriteLine("Manipulation started");
            args.Handled = true;
            if (AppConstants.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Paused
                || AppConstants.Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                AppConstants.Player.PlaybackSession.PositionChanged -= PlaybackSession_PositionChangedAsync;
            }
        }

        private void ThumbManipulationCompletedRelayCommand(ManipulationCompletedRoutedEventArgs args)
        {
            Debug.WriteLine("Manipulation completed");
            args.Handled = true;
            var source = args.OriginalSource as Slider;
            AppConstants.Player.PlaybackSession.Position = TimeSpan.FromSeconds(source.Value);          
            AppConstants.Player.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
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
            _messenger.Send(new GenericMessage<Type>(pageType), Core.Enumeration.Message.REQUEST_VIEW_NAVIGATION);

            _currentNavigationViewItem = navigationViewItem;


            //var navItem = obj.SelectedItem as UIElementCollection;
            //var d = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(navItem, 0);
            //var d = f.FindName("SelectionIndicator");

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
            _messenger.Send(new GenericMessage<Type>(pageType), Core.Enumeration.Message.REQUEST_VIEW_NAVIGATION);

            _currentNavigationViewItem = navigationViewItem;
        }
        #endregion


    }
}
