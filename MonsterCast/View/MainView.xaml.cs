using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using MonsterCast.Model;
using MonsterCast.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace MonsterCast.View
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        #region Fields
        private Frame MainFrame = null;
        private SystemNavigationManager _systemNavigationManager = null;
        private IMessenger _messenger = null;
        private INavigationService _navigationService = null;
        #endregion

        public MainView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();
            MainFrame = (Frame)Window.Current.Content;
            MainFrame.ContentTransitions = new TransitionCollection { new EntranceThemeTransition() };

            _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

            Messenger.Default.Register<MenuItem>(this, MessageAction);
            Messenger.Default.Register<Type>(this, NavRequestAction);

            _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            _systemNavigationManager.BackRequested += _systemNavigationManager_BackRequested;

            HostedFrame.Navigated += HostedFrame_Navigated;
            MainFrame.Navigated += MainFrame_Navigated;

            HostedFrame.Navigate(typeof(DefaultView));

            AppConstants.Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync;
        }

        

        private void NavRequestAction(Type arg)
        {
            HostedFrame.Navigate(arg);
        }

        private void MessageAction(MenuItem arg)
        {
            if (arg.PageType == typeof(LiveView))
                _navigationService.NavigateTo(ViewModelLocator.LiveViewKey);
            else
                HostedFrame.Navigate(arg.PageType);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            _systemNavigationManager.AppViewBackButtonVisibility = MainFrame.CanGoBack ?
                 AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void HostedFrame_Navigated(object sender, NavigationEventArgs e)
        {

            _systemNavigationManager.AppViewBackButtonVisibility = HostedFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void _systemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (_navigationService.CurrentPageKey == ViewModelLocator.LiveViewKey)
            {
                if (MainFrame.CanGoBack)
                {
                    MainFrame.GoBack();
                    _messenger.Send(new GenericMessage<string>("invoke pause"));
                }
            }
            else if (HostedFrame.CanGoBack)
            {
                HostedFrame.GoBack();
            }
        }

        private void SoundGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var placementElement = sender as FrameworkElement;
            SoundGrid.ContextFlyout.ShowAt(placementElement);
        }


        private async void PlaybackSession_PlaybackStateChangedAsync(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            if (sender.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayButton.Visibility = Visibility.Collapsed;
                    PauseButton.Visibility = Visibility.Visible;
                });
            }
            else if (sender.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayButton.Visibility = Visibility.Visible;
                    PauseButton.Visibility = Visibility.Collapsed;
                });
            }                                                 
        }

        private void PauseButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(AppConstants.Player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                AppConstants.Player.Pause();
                PauseButton.Visibility = Visibility.Collapsed;
                PlayButton.Visibility = Visibility.Visible;
            }
        }

        private void PlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
           if(AppConstants.Player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                AppConstants.Player.Play();
                PlayButton.Visibility = Visibility.Collapsed;
                PauseButton.Visibility = Visibility.Visible;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AppConstants.Player.IsLoopingEnabled = AppConstants.Player.IsLoopingEnabled == true ? false : true;

            if(AppConstants.Player.IsLoopingEnabled)
            {
                var grid = sender as Grid;
                foreach (var item in grid.Children)
                {
                    var pathObject = item as Windows.UI.Xaml.Shapes.Path;
                    pathObject.Fill = new SolidColorBrush(Colors.Orange);
                }
            }
            else
            {
                var grid = sender as Grid;
                foreach (var item in grid.Children)
                {
                    var pathObject = item as Windows.UI.Xaml.Shapes.Path;
                    pathObject.Fill = new SolidColorBrush(Colors.White);
                }
            }
        }
    }
}
