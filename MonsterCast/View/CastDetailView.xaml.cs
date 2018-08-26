using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MonsterCast.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CastDetailView : Page
    {
        private CastDetailViewModel _castDetailVM = null;
        private IMessenger _messenger = null;
        public CastDetailView()
        {
            this.InitializeComponent();

            _castDetailVM = ServiceLocator.Current.GetInstance<CastDetailViewModel>();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();

            _castDetailVM.PlayFontIcon = PlayButton;
            _castDetailVM.LoveFontIcon = LoveButton;
            _castDetailVM.PlayButtonTitle = PlayButtonTitle;
            _castDetailVM.LoveButtonTitle = LoveButtonTitle;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _messenger.Send<NotificationMessage, CastDetailViewModel>(new NotificationMessage(Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }
    }
}
