using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Helper;
using MonsterCast.Model;
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
        public FavoriteViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            GridViewCommand = new RelayCommand<ItemClickEventArgs>(RelayCommandHandler);
            _messenger.Register<GenericMessage<Cast>>(this, MessengerAction);

            FavoriteCollection = AppConstants.FavoriteCollection;
            AppConstants.FavoriteCollectionUpdated += AppConstants_FavoriteCollectionUpdated;
        }

        private void AppConstants_FavoriteCollectionUpdated(object sender, EventArgs e)
        {
            FavoriteCollection = AppConstants.FavoriteCollection;
            RaisePropertyChanged(() => FavoriteCollection);
        }

        private void RelayCommandHandler(ItemClickEventArgs e)
        {           
            var clickedCast = e.ClickedItem as Cast;
            var pageType = Type.GetType("MonsterCast.View.CastDetailView");
            _messenger.Send(new GenericMessage<Type>(pageType), "nav_request");
            _messenger.Send(new GenericMessage<Cast>(clickedCast), "fromFavorite");
        }

        private void MessengerAction(GenericMessage<Cast> args)
        {
            //Add the recieved objec to favorite collection
        }
    }
}
