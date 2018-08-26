using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MonsterCast.Helper;
using MonsterCast.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MonsterCast.ViewModel
{
    public class CastDetailViewModel : ViewModelBase
    {
        #region Fields
        private IMessenger _messenger = null;
        private Cast _activeCast = null;
        private bool _isFavorite = false;

        private FontIcon _playFontIcon = null;
        private FontIcon _loveFontIcon = null;

        private TextBlock _playButtonTitle = null;
        private TextBlock _loveButtonTitle = null;

        private readonly RelayCommand _playCommand = null;
        private readonly RelayCommand _loveCommand = null;
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

        public FontIcon PlayFontIcon
        {
            get { return _playFontIcon; }
            set { Set(ref _playFontIcon, value); }
        }

        public FontIcon LoveFontIcon
        {
            get { return _loveFontIcon; }
            set { Set(ref _loveFontIcon, value); }
        }

        public TextBlock PlayButtonTitle
        {
            get { return _playButtonTitle; }
            set { Set(ref _playButtonTitle, value); }
        }

        public TextBlock LoveButtonTitle
        {
            get { return _loveButtonTitle; }
            set { Set(ref _loveButtonTitle, value); }
        }

        public RelayCommand PlayCommand => _playCommand;
        public RelayCommand LoveCommand => _loveCommand;
        #endregion
        public CastDetailViewModel(IMessenger messenger) 
        {
            _messenger = messenger;                                     
            _messenger.Register<NotificationMessage>(this, ViewBuiltNotificationAction);
            _messenger.Register<GenericMessage<Cast>>(this, SetCastInfoAction);

            _playCommand = new RelayCommand(PlayRelayCommand);
            _loveCommand = new RelayCommand(LoveRelayCommand);
        }

        private void SetCastInfoAction(GenericMessage<Cast> args)
        {
            ActiveCast = args.Content;
        }

        private void ViewBuiltNotificationAction(NotificationMessage args)
        {
            if(args.Notification == Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
                //Check if the cast is already loved
                //Check if the cast is currently playing
            }
        }

        private async void LoveRelayCommand()
        {
            Helpers.AddCastToDbAsync(ActiveCast);
            Debug.WriteLine("[*] cast added..");

            await DispatcherHelper.RunAsync(() =>
            {
                var _currentBrush = (SolidColorBrush)LoveFontIcon.Foreground;
                if (_currentBrush.Color == Colors.White)
                {
                    LoveFontIcon.Glyph = "\uEB52";
                    LoveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                    LoveButtonTitle.Text = "You love";
                }
                else
                {
                    LoveFontIcon.Glyph = "\uEB51";
                    LoveFontIcon.Foreground = new SolidColorBrush(Colors.White);
                    LoveButtonTitle.Text = "Love it";
                }
            });
          
        }

        private async void PlayRelayCommand()
        {

            if (PlayFontIcon.Glyph == "\uE768")
            {
                _messenger.Send(new GenericMessage<Cast>(ActiveCast), Core.Enumeration.Message.REQUEST_MEDIAPLAYER_PLAY_SONG);
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayFontIcon.Glyph = "\uE769";
                    PlayButtonTitle.Text = "Pause";
                });
            }
            else
            {
                _messenger.Send(new GenericMessage<Cast>(ActiveCast), Core.Enumeration.Message.REQUEST_MEDIAPLAYER_PAUSE_SONG);
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayFontIcon.Glyph = "\uE768";
                    PlayButtonTitle.Text = "Play";
                });
            }
        }
    }
}
