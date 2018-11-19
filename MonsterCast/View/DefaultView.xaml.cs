using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace MonsterCast.View
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class DefaultView : Page
    {
        private DefaultViewModel _defaultVM = null;
         
        private IMessenger _messenger = null;

        
        public DefaultView()
        {
            this.InitializeComponent();
            PlaybackBadge.Visibility = Visibility.Collapsed;
           //Scroller.ViewChanged += Scroller_ViewChanged;

            _defaultVM = ServiceLocator.Current.GetInstance<DefaultViewModel>();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();

           
            _defaultVM.ScrollerView = Scroller;
            _defaultVM.ContentRoot = ContentRoot;
            _defaultVM.PlayFontIcon = PlayButton;
            _defaultVM.LoveFontIcon = LoveButton;
            _defaultVM.PlayButtonTitle = PlayButtonTitle;
            _defaultVM.LoveButtonTitle = LoveButtonTitle;
            _defaultVM.PlaybackBadge = PlaybackBadge;
            
        }

        //private void Scroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        //{
        //    if (ContentRoot.Children.Count > 0)
        //    {
        //        if (Scroller.VerticalOffset >= Scroller.ScrollableHeight)
        //        {
        //            var element = ContentRoot.Children
        //            .Where(item => item.Visibility == Visibility.Collapsed)
        //            .FirstOrDefault();

        //            if (element != null)
        //                element.Visibility = Visibility.Visible;
        //        }

        //    }
        //}

        //private UIElement CreateFlipViewChild<T>(T datas)
        //{
        //    FlipView _flipView = new FlipView { HorizontalAlignment = HorizontalAlignment.Stretch };
        //    _flipView.ItemTemplate = Application.Current.Resources["FlipViewItemTemplate"] as DataTemplate;
        //    _flipView.ItemsSource = datas;
        //    _flipView.SelectedIndex = 1;
        //    return _flipView;
        //}
        private void Scroller_Loaded(object sender, RoutedEventArgs e)
        {

            ScrollBar scrollBar = ((FrameworkElement)VisualTreeHelper.GetChild(Scroller, 0)).FindName("VerticalScrollBar") as ScrollBar;
            if(scrollBar != null)
            {                
                var trigger = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "ValueChanged" };
                var action = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = _defaultVM.ScrollerBarValueChangedCommand };
                trigger.Actions.Add(action);
                trigger.Attach(scrollBar);

                _defaultVM.ScrollerBar = scrollBar;
            }

            _messenger.Send<NotificationMessage, DefaultViewModel>(new NotificationMessage(Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }

       
    }
}
