using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MonsterCast.ViewModel
{
    public class LiveViewModel : ViewModelBase
    {
        #region Fields
        private WebView _webContent = null;
        private bool _isProgress = true;
        private IMessenger _messenger = null;
        #endregion

        #region Properties
        public WebView WebContent
        {
            get { return _webContent; }
            set { Set(ref _webContent, value); }
        } 
        public bool IsProgress
        {
            get { return _isProgress; }
            set { Set(ref _isProgress, value); }
        }

        //public RelayCommand<UIElement> PlayerActionCommand { get; set; }
        #endregion

        #region JsCommand
        private bool _isPlayerPaused = false;

        private string _hideAction = @"$('#stream-info').css('display', 'none');
                                        $('.player-buttons-right').css('display', 'none');
                                        $('.player-button.player-button--twitch.js-watch-twitch').css('display', 'none');
                                        window.external.notify('embed player hided');";

        //private string _playAction = @"$('.player-button.player-button--playpause.js-control-playpause-button').get(0).click();
        //                                 window.external.notify('player : false');";

        private string _pauseAction = @"$('.player-button.player-button--playpause.js-control-playpause-button').get(0).click();
                                        $('.player-overlay.player-play-overlay.js-paused-overlay').css('display', 'none');";

        private string _playerEventSubscribe = @"$('.player-video > video').get(0).addEventListener('pause', () => { window.external.notify('player : true')});
                                                 $('.player-video > video').get(0).addEventListener('play', () => { window.external.notify('player : false')});";
        #endregion

        public LiveViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            WebContent = new WebView()
            {
                Source = new Uri("https://player.twitch.tv/?channel=monstercat", UriKind.Absolute),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
            }; 
                     
            WebContent.FrameDOMContentLoaded += WebContent_FrameDOMContentLoaded;
            WebContent.ScriptNotify += WebContent_ScriptNotify;

            _messenger.Register<GenericMessage<string>>(this, messengerAction);
        }

        private async void messengerAction(GenericMessage<string> args)
        {
            if(args.Content == "invoke pause")
            {
                if (_isPlayerPaused)
                    return;
                await DispatcherHelper.RunAsync(async () =>
                {
                    await WebContent.InvokeScriptAsync("eval", new string[] { _pauseAction });
                });
            }
        }



        //private async void PlayerActionHandler(UIElement obj)
        //{
        //    var playerButtonContainer = obj as Grid;
        //    List<Path> playerButtonItems = playerButtonContainer.Children
        //        .Select(e => (Path)e).ToList();

        //    if (_isPlayerPaused)
        //    {
        //        playerButtonItems.Where(e => e.Name == "playingIcon").First()
        //            .Visibility = Visibility.Collapsed;

        //        playerButtonItems.Where(e => e.Name == "pausingIcon").First()
        //            .Visibility = Visibility.Visible;
        //        await WebContent.InvokeScriptAsync("eval", new string[] { _playAction });
        //    }
        //    else
        //    {
        //        playerButtonItems.Where(e => e.Name == "pausingIcon").First()
        //           .Visibility = Visibility.Collapsed;

        //        playerButtonItems.Where(e => e.Name == "playingIcon").First()
        //            .Visibility = Visibility.Visible;
        //        await WebContent.InvokeScriptAsync("eval", new string[] { _pauseAction });
        //    }
        //}

        private async void WebContent_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            IsProgress = true;
            try
            {
                await sender.InvokeScriptAsync("eval", new string[] { _hideAction});
                await sender.InvokeScriptAsync("eval", new string[] { _playerEventSubscribe });

                IsProgress = false;
            }
            catch (Exception ex)
            {                    
                Debug.WriteLine(ex.Message);
            }
        }
        private void WebContent_ScriptNotify(object sender, NotifyEventArgs e)
        {
            switch (e.Value)
            {
                case "player : false":
                    _isPlayerPaused = false;
                    Debug.WriteLine($"[!] - player played");
                    break;

                case "player : true":
                    _isPlayerPaused = true;
                    Debug.WriteLine($"[!] - player paused");
                    break;
                default:
                    Debug.WriteLine($"[!] - {e.Value}");
                    break;

            }

        }

    }
}
