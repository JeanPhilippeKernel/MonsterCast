using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MonsterCast.Model;
using MonsterCast.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
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
            HostedFrame.CacheSize = 1;

            _mainVM = ServiceLocator.Current.GetInstance<MainViewModel>();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();
            
            _mainVM.MainNavigationView = MainNavigationView;
            _mainVM.HostedFrame = HostedFrame;
            _mainVM.PlayFontIcon = PlayButton;
            _mainVM.SoundFontIcon = SoundButton;
            _mainVM.LoopFontIcon = LoopButton;
            _mainVM.PlaybackBadge = playbackBadge;

            _mainVM.PlaybackBadgeLoveFontIcon = PlaybackLoveButton;
            _mainVM.PlaybackBadgeLoveFontIcon = PlaybackInfoButton;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Button navigationBackButton = ((FrameworkElement)VisualTreeHelper.GetChild(MainNavigationView, 0)).FindName("NavigationViewBackButton") as Button;
            if(navigationBackButton != null)
            {               
                _mainVM.NavigationViewBackButton = navigationBackButton;
            }

            _messenger.Send<NotificationMessage<Type>, MainViewModel>(new NotificationMessage<Type>(typeof(DefaultView), Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }
    }
}
