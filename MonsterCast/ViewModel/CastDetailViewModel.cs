using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Helper;
using MonsterCast.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCast.ViewModel
{
    public class CastDetailViewModel : ViewModelBase
    {
        #region Fields
        private IMessenger _messenger = null;
        private Cast _activeCast = null;
        private bool _isFavorite = false;
        #endregion

        #region Properties
        public Cast ActiveCast
        {
            get { return _activeCast; }
            set { Set(ref _activeCast, value); }
        }
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { Set(ref _isFavorite, value); }
        }
        public RelayCommand  PlayCommand { get; set; }
        public RelayCommand FavoriteCommand { get; set; }
        #endregion
        public CastDetailViewModel(IMessenger messenger) 
        {
            _messenger = messenger;
            _messenger.Register<GenericMessage<Cast>>(this, "fromFavorite", MessageFavoriteAction);
            _messenger.Register<GenericMessage<Cast>>(this, "fromDefault", MessageDefaultAction);

            PlayCommand = new RelayCommand(PlayCommandHandler);
            FavoriteCommand = new RelayCommand(FavoriteCommandHandlerAsync);
        }

        private async void FavoriteCommandHandlerAsync()
        {
            await Helpers.Database.InsertAsync(ActiveCast);
            Messenger.Default.Send(new NotificationMessage<Cast>(ActiveCast, "castAdded"));
            Debug.WriteLine("[*] cast added..");
        }

        private void PlayCommandHandler()
        {
            _messenger.Send<GenericMessage<Cast>>(new GenericMessage<Cast>(ActiveCast), "play_request");
        }

        private void MessageDefaultAction(GenericMessage<Cast> args)
        {
            ActiveCast = args.Content;
            IsFavorite = false;
        }

        private void MessageFavoriteAction(GenericMessage<Cast> args)
        {
            ActiveCast = args.Content;
            IsFavorite = true;
        }
    }
}
