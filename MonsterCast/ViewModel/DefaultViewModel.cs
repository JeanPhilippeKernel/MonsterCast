using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Helper;
using MonsterCast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace MonsterCast.ViewModel
{
    public class DefaultViewModel : ViewModelBase
    {
        #region Fields
        private IEnumerable<Cast> _nextCurrentCollection = null;
        private IEnumerable<Cast> _podcastCollection = null;
        private Cast _currentCast = null;      
        private IMessenger _messenger = null;
        #endregion

        #region Properties
        public IEnumerable<Cast> NextCurrentCollection
        {
            get { return _nextCurrentCollection; }
            set { Set(ref _nextCurrentCollection, value); }
        }

        public IEnumerable<Cast> PodcastCollection
        {
            get { return _podcastCollection; }
            set { Set(ref _podcastCollection, value); }
        }

        public Cast CurrentCast
        {
            get { return _currentCast; }
            set { Set(ref _currentCast, value); }
        }
        #endregion
        public DefaultViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            messenger.Register<NotificationMessage>(this, MessengerAction);
        }

        private void MessengerAction(NotificationMessage args)
        {
            PodcastCollection = AppConstants.PodcastCollection;
            CurrentCast = PodcastCollection.Count() > 0 ? PodcastCollection.First() : null;

            var firstFiveElement = PodcastCollection.Take(5);
            Helpers.FetchImageParallel(ref firstFiveElement);
            NextCurrentCollection = firstFiveElement;
        }

        public void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedCast = e.ClickedItem as Cast;
            var pageType = Type.GetType("MonsterCast.View.CastDetailView");
            _messenger.Send(new GenericMessage<Type>(pageType), "nav_request");

            _messenger.Send(new GenericMessage<Cast>(clickedCast), "fromDefault");
        }
    }
}
