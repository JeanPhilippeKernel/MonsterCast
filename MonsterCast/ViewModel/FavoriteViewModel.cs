using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Helper;
using MonsterCast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace MonsterCast.ViewModel
{
    public class FavoriteViewModel : ViewModelBase
    {
        #region Fields
        private IMessenger _messenger = null;
        private IEnumerable<Cast> _favoriteCollection = null;
        #endregion

        #region Properties
        public RelayCommand<ItemClickEventArgs> GridViewCommand { get; set; }
        public RelayCommand LoadedCommand { get; set; }
        public IEnumerable<Cast> FavoriteCollection
        {
            get { return _favoriteCollection; }
            set { Set(ref _favoriteCollection, value); }
        }
        #endregion
        public FavoriteViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            GridViewCommand = new RelayCommand<ItemClickEventArgs>(RelayCommandHandler);
            LoadedCommand = new RelayCommand(LoadFavoritesCast);
            _messenger.Register<GenericMessage<Cast>>(this, MessengerAction);

            LoadFavoritesCast();
        }
        public void LoadFavoritesCast()
        {
            var TaskOp = Helpers.Database.Table<Cast>().ToListAsync();
            TaskOp.GetAwaiter().OnCompleted(() =>
            {
                var datas = TaskOp.Result;
                FavoriteCollection = datas.AsEnumerable();
            });
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
