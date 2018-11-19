using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MonsterCast.Database;
using MonsterCast.Database.Tables;
using MonsterCast.Model;
using System;
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
        private IMonsterDatabase _dbConn = null;
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
        public CastDetailViewModel(IMessenger messenger, IMonsterDatabase dbConnexion) 
        {
            _messenger = messenger;
            _dbConn = dbConnexion;
            _messenger.Register<NotificationMessage>(this, ViewBuiltNotificationAction);
            _messenger.Register<GenericMessage<Cast>>(this, SetCastInfoAction);

            _playCommand = new RelayCommand(PlayRelayCommand);
            _loveCommand = new RelayCommand(LoveRelayCommand);
        }

        private void SetCastInfoAction(GenericMessage<Cast> args)
        {
            ActiveCast = args.Content;
        }

        private async void ViewBuiltNotificationAction(NotificationMessage args)
        {
            if(args.Notification == Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
                //Check if the cast is already loved                
                var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == ActiveCast.Title);
                if (_isLoved)
                {
                    //Update the Id of Cast                  
                    var castUpdated = await UpdateCastAsync();
                    if (castUpdated)
                    {
                        await DispatcherHelper.RunAsync(() =>
                        {
                            LoveFontIcon.Glyph = "\uEB52";
                            LoveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                            LoveButtonTitle.Text = "You love";
                        });
                    }

                }

                //Check if the cast is currently playing
            }
        }

        private async void LoveRelayCommand()
        {
            
            var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == ActiveCast.Title);
            if(!_isLoved)
            {
                int inserted = await _dbConn.AddAsync(ActiveCast);
                if(inserted > 0)
                {
                    //Update the Id of Cast
                    var castUpdated = await UpdateCastAsync();
                    if(castUpdated)
                    {
                        await DispatcherHelper.RunAsync(() =>
                        {
                            LoveFontIcon.Glyph = "\uEB52";
                            LoveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                            LoveButtonTitle.Text = "You love";
                        });
                    }
                                  
                }
                
            }
            else
            {
                int deleted = await _dbConn.RemoveAsync(ActiveCast);
                if(deleted > 0)
                {
                    //Reset the Id of Cast
                    ActiveCast.Id = 0;
                    await DispatcherHelper.RunAsync(() => {
                        LoveFontIcon.Glyph = "\uEB51";
                        LoveFontIcon.Foreground = new SolidColorBrush(Colors.White);
                        LoveButtonTitle.Text = "Love it";
                    });
                }
            }
                
          
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


        private async Task<bool> UpdateCastAsync()
        {
            var _updated = await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    var _saved = await _dbConn.Database.GetAsync<Cast>(c => c.Title == ActiveCast.Title);
                    ActiveCast.Id = _saved.Id;
                    return true;
                }
                catch (Exception)
                {

                    return false;
                }
            });
            return _updated;
        }
    }
}
