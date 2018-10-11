using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MonsterCast.Model;
using MonsterCast.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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

            _mainVM.PlaybackTimelineSlider = SliderElement;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Button navigationBackButton = ((FrameworkElement)VisualTreeHelper.GetChild(MainNavigationView, 0)).FindName("NavigationViewBackButton") as Button;
            Thumb horizontalThumb = ((FrameworkElement)VisualTreeHelper.GetChild(SliderElement, 0)).FindName("HorizontalThumb") as Thumb;

            if (horizontalThumb != null)
            {
                var interactivityBehaviorCollection = new Microsoft.Xaml.Interactivity.BehaviorCollection();


                var trigger_one = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "DragStarted" };
                var trigger_two = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "DragDelta" };
                var trigger_three = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "DragCompleted" };
                
                var action_one = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = _mainVM.ThumbDragStartedCommand };
                var action_two = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = _mainVM.ThumbDragDeltaCommand };
                var action_three = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = _mainVM.ThumbDragCompleteCommand };


                trigger_one.Actions.Add(action_one);
                trigger_two.Actions.Add(action_two);
                trigger_three.Actions.Add(action_three);

                interactivityBehaviorCollection.Add(trigger_one);
                interactivityBehaviorCollection.Add(trigger_two);
                interactivityBehaviorCollection.Add(trigger_three);

                interactivityBehaviorCollection.Attach(horizontalThumb);
            }


            if (navigationBackButton != null)
            {               
                _mainVM.NavigationViewBackButton = navigationBackButton;
                _mainVM.NavigationViewBackButton.Visibility = Visibility.Collapsed;
            }

            _messenger.Send<NotificationMessage<Type>, MainViewModel>(new NotificationMessage<Type>(typeof(DefaultView), Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }
    }
}
