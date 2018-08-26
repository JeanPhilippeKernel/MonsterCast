﻿using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MonsterCast.Model;
using MonsterCast.ViewModel;
using System;
using Windows.UI.Xaml.Controls;
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
        private MainViewModel _mainVM = null;
        #endregion

        public MainView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            _mainVM = ServiceLocator.Current.GetInstance<MainViewModel>();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();
            
            _mainVM.MainNavigationView = MainNavigationView;
            _mainVM.HostedFrame = HostedFrame;
            _mainVM.PlayFontIcon = PlayButton;
            _mainVM.SoundFontIcon = SoundButton;
            _mainVM.LoopFontIcon = LoopButton;
            _mainVM.PlaybackBadge = playbackBadge;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _messenger.Send<NotificationMessage<Type>, MainViewModel>(new NotificationMessage<Type>(typeof(DefaultView), Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }
    }
}
