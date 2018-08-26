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

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace MonsterCast.View
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class FavoriteCastView : Page
    {
        private IMessenger _messenger = null;
        public FavoriteCastView()
        {
            this.InitializeComponent();
            _messenger = ServiceLocator.Current.GetInstance<IMessenger>();
        }

        private void FavoritePage_Loaded(object sender, RoutedEventArgs e)
        {
            _messenger.Send<NotificationMessage, FavoriteViewModel>(new NotificationMessage(Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT));
        }
    }
}
