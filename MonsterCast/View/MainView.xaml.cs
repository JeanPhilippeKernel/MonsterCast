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
        private IMessenger _messenger = null;
        private INavigationService _navigationService = null;
        private MainViewModel _mainVM = null;
        #endregion

        public MainView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            _mainVM = ServiceLocator.Current.GetInstance<MainViewModel>();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();
            _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

            _mainVM.MainNavigationView = MainNavigationView;
            _mainVM.HostedFrame = HostedFrame;
                                                                            
            Messenger.Default.Register<MenuItem>(this, MessageAction);
            Messenger.Default.Register<Type>(this, NavRequestAction);
            
            _mainVM.HostedFrame.Navigate(typeof(DefaultView));

            AppConstants.Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync;
        }

        private void NavRequestAction(Type arg)
        {
            _mainVM.HostedFrame.Navigate(arg);
        }

        private void MessageAction(MenuItem arg)
        {
            //if (arg.PageType == typeof(LiveView))
            //    _navigationService.NavigateTo(ViewModelLocator.LiveViewKey);
            //else
                _mainVM.HostedFrame.Navigate(arg.PageType);
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
                    PlayButton.Glyph = "\uE769";
                                    
                });
            }
            else if (sender.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayButton.Glyph = "\uE768";
                  
                });
            }                                                 
        }

        private void PlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
           if(AppConstants.Player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                AppConstants.Player.Play();
                PlayButton.Glyph = "\uE769";
                   
            }
           else if(AppConstants.Player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                AppConstants.Player.Pause();
                PlayButton.Glyph = "\uE768";
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AppConstants.Player.IsLoopingEnabled = AppConstants.Player.IsLoopingEnabled == true ? false : true;

            var icon = sender as FontIcon;
            if (AppConstants.Player.IsLoopingEnabled)
            {                
                icon.Foreground = new SolidColorBrush(Colors.Orange);                
            }
            else
            {
                icon.Foreground = new SolidColorBrush(Colors.White);               
            }
        }
    }
}
