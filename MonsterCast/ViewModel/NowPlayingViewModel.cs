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
using MonsterCast.Core.Enumeration;
using GalaSoft.MvvmLight.Threading;

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
            _messenger.Register<GenericMessage<Cast>>(this, Message.REQUEST_VIEW_UPDATE_PLAYBACK_BADGE, UpdateViewPlaybackBadgeRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_MEDIA_ENDED, MediaEndedAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_MEDIA_FAILED, MediaFailedAction);
        }

       

        private void UpdateViewPlaybackBadgeRequestAction(GenericMessage<Cast> args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ShowMask = Visibility.Collapsed;
                Playing = args.Content;
                Debug.WriteLine($"[!] Now playing : {Playing.Title}");
            });
        }

        private void MediaEndedAction(NotificationMessage args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ShowMask = Visibility.Visible;
                Playing = null;
            });
        }

        private void MediaFailedAction(NotificationMessage args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ShowMask = Visibility.Visible;
                Playing = null;
            });
            
        }
    }
}
