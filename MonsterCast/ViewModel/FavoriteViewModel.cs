using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Core.Database;
using MonsterCast.Helper;
using MonsterCast.Model;
using MonsterCast.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace MonsterCast.ViewModel
{
    public class FavoriteViewModel : ViewModelBase
    {
        #region Fields
        private IMessenger _messenger = null;
        private IMonsterDatabase _dbConn = null;
        private ObservableCollection<Cast> _favoriteCollection = null;
        #endregion

        #region Properties
        public RelayCommand<ItemClickEventArgs> GridViewCommand { get; set; }
        public ObservableCollection<Cast> FavoriteCollection
        {
            get { return _favoriteCollection; }
            set { Set(ref _favoriteCollection, value); }
        }
        #endregion
        public FavoriteViewModel(IMessenger messenger, IMonsterDatabase dbConn)
        {
            _messenger = messenger;
            _dbConn = dbConn;
            
            _messenger.Register<NotificationMessage>(this, ViewBuiltNotificationAction);
            GridViewCommand = new RelayCommand<ItemClickEventArgs>(RelayCommandHandler);

            _dbConn.DatabaseUpdated += OnDatabaseUpdated;
           
        }

        private async void ViewBuiltNotificationAction(NotificationMessage args)
        {
            if (args.Notification == Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
                var datas = await _dbConn.Database.Table<Cast>().ToListAsync();               
                FavoriteCollection = new ObservableCollection<Cast>(datas);
            }
        }

        private async void OnDatabaseUpdated(object sender, DatabaseUpdateEventArgs e)
        {
            var datas = await _dbConn.Database.Table<Cast>().ToListAsync();           
            FavoriteCollection = new ObservableCollection<Cast>(datas);
            RaisePropertyChanged(() => FavoriteCollection);
        }

     
        private void RelayCommandHandler(ItemClickEventArgs e)
        {           
            var clickedCast = e.ClickedItem as Cast;
            var pageType = typeof(CastDetailView);
            _messenger.Send<GenericMessage<Cast>, CastDetailViewModel>(new GenericMessage<Cast>(clickedCast));
            _messenger.Send(new GenericMessage<Type>(pageType), Core.Enumeration.Message.REQUEST_VIEW_NAVIGATION);
            
        }

        
    }
}
