using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Helper;
using MonsterCast.Model;
using MonsterCast.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
           Scroller.ViewChanged += Scroller_ViewChanged;

            _defaultVM = ServiceLocator.Current.GetInstance<DefaultViewModel>();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();

            //var d = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "ViewChanged" };
            //var action = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = _defaultVM.ScrollerViewChangedCommand };
            //d.Actions.Add(action);
            //d.Attach(Scroller);

            _defaultVM.ScrollerView = Scroller;
            _defaultVM.ContentRoot = ContentRoot;

            _messenger.Send<NotificationMessage, DefaultViewModel>(new NotificationMessage(Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }

        private void Scroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (ContentRoot.Children.Count > 0)
            {
                if (Scroller.VerticalOffset >= Scroller.ScrollableHeight)
                {
                    var element = ContentRoot.Children
                    .Where(item => item.Visibility == Visibility.Collapsed)
                    .FirstOrDefault();

                    if (element != null)
                        element.Visibility = Visibility.Visible;
                }

            }
        }

        //private UIElement CreateFlipViewChild<T>(T datas)
        //{
        //    FlipView _flipView = new FlipView { HorizontalAlignment = HorizontalAlignment.Stretch };
        //    _flipView.ItemTemplate = Application.Current.Resources["FlipViewItemTemplate"] as DataTemplate;
        //    _flipView.ItemsSource = datas;
        //    _flipView.SelectedIndex = 1;
        //    return _flipView;
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
            base.OnNavigatedTo(e);         
        }
    }
}
