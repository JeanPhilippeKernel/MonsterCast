using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using MonsterCast.Model;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace MonsterCast.ViewModel
{
    public class NowPlayingViewModel : ViewModelBase
    {
        #region Fields
        private Cast _playing = null;
        private IMessenger _messenger = null;
        private Visibility _showMask = Visibility.Visible;
        #endregion

        #region Properties
        public Cast Playing
        {
            get { return _playing; }
            set { Set(ref _playing, value); }
        }

        public Visibility ShowMask
        {
            get { return _showMask; }
            set
            {
                Set(ref _showMask, value);
                RaisePropertyChanged(() => ShowMask);
            }
        }

        #endregion
        public NowPlayingViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<GenericMessage<Cast>>(this, "now_playing", NowPlayingAction);
            _messenger.Register<NotificationMessage>(this, "end_playing", EndPlayingAction);
        }

        private void EndPlayingAction(NotificationMessage obj)
        {
            ShowMask = Visibility.Visible;
            Playing = null;
        }

        private void NowPlayingAction(GenericMessage<Cast> arg)
        {
            ShowMask = Visibility.Collapsed;
            Playing = arg.Content;
            Debug.WriteLine($"[!] Now playing : {Playing.Title}");
        }
    }
}
