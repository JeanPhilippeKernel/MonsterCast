using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MonsterCast.Core.Enumeration;
using MonsterCast.Helper;
using MonsterCast.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MonsterCast.Core.Database;

namespace MonsterCast.ViewModel
{
    public class DefaultViewModel : ViewModelBase
    {
        #region Fields
        private readonly RelayCommand<RangeBaseValueChangedEventArgs> _scrollerBarValueChangedCommand = null;
        
        private readonly RelayCommand<PointerRoutedEventArgs> _castItemPointerEnteredCommand = null;
        private readonly RelayCommand<PointerRoutedEventArgs> _castItemPointerExitedCommand = null;

        private readonly RelayCommand _playCommand = null;
        private readonly RelayCommand _loveCommand = null;

        private readonly RelayCommand _playbackPlayCommand = null;
        private readonly RelayCommand _playbackLoveCommand = null;

        private IEnumerable<Cast> _podcastCollection = null;
        private IEnumerable<IList<Cast>> _splitedCollection = null;
        private ObservableCollection<Cast> _showedCollection = null;

        private readonly List<Grid> _alreadyAttachedBehavior = null;

        private int _showedCollectionIndex = 0;
        private int _splitedCollectionLength = 0;
        private Cast _currentCast = null;
        private Cast _overlayedCast = null;
        private IMessenger _messenger = null;
        private Core.Database.IMonsterDatabase _dbConn = null;

        private ScrollViewer _scrollerView = null;
        private GridView _contentRoot = null;
        private ScrollBar _scrollerBar = null;

        private FontIcon _playFontIcon = null;
        private FontIcon _loveFontIcon = null;

        private TextBlock _playButtonTitle = null;
        private TextBlock _loveButtonTitle = null;


        private Grid _playbackBadge = null;

        private Grid _overlayGrid = null;
        #endregion

        #region Properties
        public RelayCommand<RangeBaseValueChangedEventArgs> ScrollerBarValueChangedCommand => _scrollerBarValueChangedCommand;
        
        public  RelayCommand<PointerRoutedEventArgs> CastItemPointerEnteredCommand => _castItemPointerEnteredCommand;
        public RelayCommand<PointerRoutedEventArgs> CastItemPointerExitedCommand => _castItemPointerExitedCommand;

        public RelayCommand PlayCommand => _playCommand;
        public RelayCommand LoveCommand => _loveCommand;

        public RelayCommand PlaybackPlayCommand => _playbackPlayCommand;
        public RelayCommand PlaybackLoveCommand => _playbackLoveCommand;
       
        public IEnumerable<Cast> PodcastCollection
        {
            get { return _podcastCollection; }
            set { Set(ref _podcastCollection, value); }
        }
        public IEnumerable<IList<Cast>> SplitedCollection
        {
            get { return _splitedCollection; }
            set { Set(ref _splitedCollection, value); }
        }

        public ObservableCollection<Cast> ShowedCollection
        {
            get { return _showedCollection; }
            set { Set(() => ShowedCollection, ref _showedCollection, value); }
        }

        public Cast CurrentCast
        {
            get { return _currentCast; }
            set { Set(ref _currentCast, value); }
        }

        public Cast OverlayedCast
        {
            get { return _overlayedCast; }
            set { Set( () => OverlayedCast, ref _overlayedCast, value); }
        }

        public ScrollViewer ScrollerView
        {
            get { return _scrollerView; }
            set { Set(ref _scrollerView, value); }
        }

        public GridView ContentRoot
        {
            get { return _contentRoot; }
            set { Set(ref _contentRoot, value); }
        }

        public ScrollBar ScrollerBar
        {
            get { return _scrollerBar; }
            set { Set(ref _scrollerBar, value); }
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

        public Grid PlaybackBadge
        {
            get { return _playbackBadge; }
            set { Set(ref _playbackBadge, value); }
        }

        public Grid OverlayGrid
        {
            get { return _overlayGrid; }
            set { Set(() => OverlayGrid, ref _overlayGrid, value); }
        }

        public delegate void NotificationCallback(bool value);
        public NotificationCallback _notificationMessageCallback { get; set; }
        #endregion
        public DefaultViewModel(IMessenger messenger, IMonsterDatabase dbConnexion)
        {
            _alreadyAttachedBehavior = new List<Grid>();

            _messenger = messenger;
            _notificationMessageCallback = MediaPlayerPlaybackCallback;

            _dbConn = dbConnexion;
            _showedCollection = new ObservableCollection<Cast>();

            _messenger.Register<NotificationMessage>(this, MessengerAction);
            _messenger.Register<NotificationMessage>(this, ViewBuiltNotificationAction);
            _messenger.Register<GenericMessage<Cast>>(this, Message.REQUEST_VIEW_UPDATE_PLAYBACK_BADGE, UpdateViewPlaybackBadgeRequestAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_PLAYBACK_STATE_PLAYING, PlaybackStatePlayingAction);
            _messenger.Register<NotificationMessage>(this, Message.MEDIAPLAYER_PLAYBACK_STATE_PAUSED, PlaybackStatePausedAction);

            _scrollerBarValueChangedCommand = new RelayCommand<RangeBaseValueChangedEventArgs>(ScrollerBarValueChangedAction);

            _castItemPointerEnteredCommand = new RelayCommand<PointerRoutedEventArgs>(CastItemPointerEnteredAction);
            _castItemPointerExitedCommand = new RelayCommand<PointerRoutedEventArgs>(CastItemPointerExitedAction);

            _playCommand = new RelayCommand(PlayRelayCommand);
            _loveCommand = new RelayCommand(LoveRelayCommand);

            _playbackPlayCommand = new RelayCommand(PlaybackPlayRelayCommand);
            _playbackLoveCommand = new RelayCommand(PlaybackLoveRelayCommand);
        }

        

        private void PlaybackStatePausedAction(NotificationMessage args)
        {
            if (args.Notification == Message.MEDIAPLAYER_PLAYBACK_STATE_PAUSED)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    PlayFontIcon.Glyph = "\uE768";
                    PlayButtonTitle.Text = "Play";
                });
            }
        }

        private void PlaybackStatePlayingAction(NotificationMessage args)
        {
            if (args.Notification == Message.MEDIAPLAYER_PLAYBACK_STATE_PLAYING)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    PlayFontIcon.Glyph = "\uE769";
                    PlayButtonTitle.Text = "Pause";
                });
            }
        }

        private async void UpdateViewPlaybackBadgeRequestAction(GenericMessage<Cast> args)
        {
            await DispatcherHelper.RunAsync(() =>
            {              
                CurrentCast = args.Content;              
                PlaybackBadge.Visibility = Visibility.Visible;
                PlayFontIcon.Glyph = "\uE769";
                PlayButtonTitle.Text = "Pause";
            });
            await Task.Run(async () =>
            {
                var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == CurrentCast.Title);
                if (_isLoved)
                {
                    //Update the Id of Cast                  
                    var castUpdated = await UpdateCastAsync(CurrentCast);
                    if (castUpdated)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            LoveFontIcon.Glyph = "\uEB52";
                            LoveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                            LoveButtonTitle.Text = "You love";
                        });
                    }
                }
                else
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        LoveFontIcon.Glyph = "\uEB51";
                        LoveFontIcon.Foreground = new SolidColorBrush(Colors.White);
                        LoveButtonTitle.Text = "Love it";
                    });
                }                               
            });

                             
        }

        #region Messenger_Method

        private async void ViewBuiltNotificationAction(NotificationMessage args)
        {
            if (args.Notification == Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
                //Todo: Check if the current Cast is love it
                await Task.Run(() =>
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(async () =>
                    {
                        ShowedCollection.Clear();
                        await Task.Delay(150);
                        _showedCollectionIndex = 0;
                        var datas = SplitedCollection.ElementAt(0);
                        int length = datas.Count;
                        
                        for (int i = 0; i < length; i++)
                            ShowedCollection.Add(datas.ElementAt(i));
                    });
                });
                _messenger.Send(new NotificationMessageWithCallback(Message.IS_MEDIAPLAYER_PLAYBACK_STATE_PLAYING, _notificationMessageCallback), Message.IS_MEDIAPLAYER_PLAYBACK_STATE_PLAYING);
                
            }
        }

        private async void MediaPlayerPlaybackCallback(bool result)
        {
            if (result)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    //CurrentCast = args.Content;
                    PlaybackBadge.Visibility = Visibility.Visible;
                    PlayFontIcon.Glyph = "\uE769";
                    PlayButtonTitle.Text = "Pause";
                });
                await Task.Run(async () =>
                {
                    var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == CurrentCast.Title);
                    if (_isLoved)
                    {
                        //Update the Id of Cast                  
                        var castUpdated = await UpdateCastAsync(CurrentCast);
                        if (castUpdated)
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {
                                LoveFontIcon.Glyph = "\uEB52";
                                LoveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                                LoveButtonTitle.Text = "You love";
                            });
                        }
                    }
                    else
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            LoveFontIcon.Glyph = "\uEB51";
                            LoveFontIcon.Foreground = new SolidColorBrush(Colors.White);
                            LoveButtonTitle.Text = "Love it";
                        });
                    }
                });
            }
           
        }

        private  void MessengerAction(NotificationMessage args)
        {
            if (args.Notification == Message.NOTIFICATION_PODCAST_HAS_BEEN_SET)
            {
                PodcastCollection = AppConstants.PodcastCollection;

                SplitedCollection = Core.Helpers.Collection.Spliter(ref _podcastCollection, 20);
                _splitedCollectionLength = SplitedCollection.Count();

                for (int i = 0; i < _splitedCollectionLength; i++)
                {
                    IEnumerable<Cast> _collection = SplitedCollection.ElementAt(i);
                     Helpers.FetchThumbnailAsync(ref _collection);
                }
               
            }         
        }
        #endregion

        #region RelayCommand_Method
        private  void PlaybackPlayRelayCommand()
        {
           
            if(OverlayGrid != null)
            {
                var frameworkElement = OverlayGrid as FrameworkElement;
                var playFontIcon = frameworkElement.FindName("PlaybackPlayButton") as FontIcon;
                if(playFontIcon.Glyph == "\uE768")
                {
                    playFontIcon.Glyph = "\uE769";
                    _messenger.Send(new GenericMessage<Cast>(OverlayedCast), Message.REQUEST_MEDIAPLAYER_PLAY_SONG);
                }

                else if (playFontIcon.Glyph == "\uE769")
                {
                    playFontIcon.Glyph = "\uE768";
                    _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_PAUSE_SONG), Message.REQUEST_MEDIAPLAYER_PAUSE_SONG);
                }
            }
        }

        private async void PlaybackLoveRelayCommand()
        {
            var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == OverlayedCast.Title);
            if (!_isLoved)
            {
                int inserted = await _dbConn.AddAsync(OverlayedCast);
                if (inserted > 0)
                {
                    //Update the Id of Cast
                    var castUpdated = await UpdateCastAsync(OverlayedCast);
                    if (castUpdated)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            var fe = OverlayGrid as FrameworkElement;
                            var loveFontIcon = fe.FindName("PlaybackLoveButton") as FontIcon;
                            loveFontIcon.Glyph = "\uEB52";
                            loveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                            
                        });
                    }

                }

            }
            else
            {
                int deleted = await _dbConn.RemoveAsync(OverlayedCast);
                if (deleted > 0)
                {
                    //Reset the Id of Cast
                    OverlayedCast.Id = 0;
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        var fe = OverlayGrid as FrameworkElement;
                        var loveFontIcon = fe.FindName("PlaybackLoveButton") as FontIcon;
                        loveFontIcon.Glyph = "\uEB51";
                        loveFontIcon.Foreground = new SolidColorBrush(Colors.White);                    
                    });
                }
            }
        }

        private async void LoveRelayCommand()
        {
            var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == CurrentCast.Title);
            if (!_isLoved)
            {
                int inserted = await _dbConn.AddAsync(CurrentCast);
                if (inserted > 0)
                {
                    //Update the Id of Cast
                    var castUpdated = await UpdateCastAsync(CurrentCast);
                    if (castUpdated)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
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
                int deleted = await _dbConn.RemoveAsync(CurrentCast);
                if (deleted > 0)
                {
                    //Reset the Id of Cast
                    CurrentCast.Id = 0;
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        LoveFontIcon.Glyph = "\uEB51";
                        LoveFontIcon.Foreground = new SolidColorBrush(Colors.White);
                        LoveButtonTitle.Text = "Love it";
                    });
                }
            }
        }

        private void PlayRelayCommand()
        {
            if (PlayFontIcon.Glyph == "\uE768")
            {
                _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_RESUME_SONG), Message.REQUEST_MEDIAPLAYER_RESUME_SONG);
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    PlayFontIcon.Glyph = "\uE769";
                    PlayButtonTitle.Text = "Pause";
                });
               
            }
            else
            {
                _messenger.Send(new NotificationMessage(Message.REQUEST_MEDIAPLAYER_PAUSE_SONG), Message.REQUEST_MEDIAPLAYER_PAUSE_SONG);

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    PlayFontIcon.Glyph = "\uE768";
                    PlayButtonTitle.Text = "Play";
                });
               
            }

        }

        private void CastItemPointerExitedAction(PointerRoutedEventArgs args)
        {
            //args.Handled = true;
            //OverlayGrid.Visibility = Visibility.Collapsed;           
        }

        private async  void CastItemPointerEnteredAction(PointerRoutedEventArgs args)
        {
            args.Handled = true;

            var originalSource = args.OriginalSource as FrameworkElement;
            var overlayGrid = originalSource.FindName("OverlayGrid");
            if (overlayGrid != null)
            {
                var overlayCastcontext = originalSource.DataContext as Cast;
                OverlayedCast = overlayCastcontext;


                var grid = overlayGrid as Grid;

                if (OverlayGrid != null)
                {
                    if (ReferenceEquals(grid, OverlayGrid))
                        return;

                    else
                    {
                        OverlayGrid.Visibility = Visibility.Collapsed;
                        var fe = OverlayGrid as FrameworkElement;
                        var playIcon = fe.FindName("PlaybackPlayButton") as FontIcon;
                        if (playIcon != null)
                        {
                            playIcon.Glyph = "\uE768";
                        }
                    }
                }


                grid.Visibility = Visibility.Visible;
                var frameworkElement = grid as FrameworkElement;

                var playFontIcon = frameworkElement.FindName("PlaybackPlayButton") as FontIcon;
                var loveFontIcon = frameworkElement.FindName("PlaybackLoveButton") as FontIcon;
                var infoFontIcon = frameworkElement.FindName("PlaybackInfoButton") as FontIcon;

                var registeredGrid = _alreadyAttachedBehavior.SingleOrDefault(g => ReferenceEquals(g, grid));
                if(registeredGrid == null)
                {

                    var triggerForLoveIcon = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "Tapped" };
                    var triggerForPlayIcon = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "Tapped" };

                    var actionForLoveIcon = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = PlaybackLoveCommand };
                    var actionForPlayIcon = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = PlaybackPlayCommand };

                    triggerForLoveIcon.Actions.Add(actionForLoveIcon);
                    triggerForPlayIcon.Actions.Add(actionForPlayIcon);

                    triggerForLoveIcon.Attach(loveFontIcon);
                    triggerForPlayIcon.Attach(playFontIcon);

                    _alreadyAttachedBehavior.Add(grid);
                }

                var _isLoved = await _dbConn.IsAlreadyExist<Cast>(c => c.Title == OverlayedCast.Title);
                if(_isLoved)
                {
                    var castUpdated = await UpdateCastAsync(OverlayedCast);
                    if (castUpdated)
                    {
                        loveFontIcon.Glyph = "\uEB52";
                        loveFontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                    }                    
                }
                else
                {
                    loveFontIcon.Glyph = "\uEB51";
                    loveFontIcon.Foreground = new SolidColorBrush(Colors.White);
                }
                OverlayGrid = grid;
            }

        }

        private async void ScrollerBarValueChangedAction(RangeBaseValueChangedEventArgs args)
        {
            if (ContentRoot.ItemsSource != null)
            {
                //Todo : send message to change the content overlay of navigationView

                 double result = args.NewValue - ScrollerView.ScrollableHeight;
                if (result >= -200.0)
                {
                 

                    if(_showedCollectionIndex < (_splitedCollectionLength - 1))
                    {
                        ++_showedCollectionIndex;
                        await Task.Run(() =>
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {    
                                var datas = SplitedCollection.ElementAt(_showedCollectionIndex);
                                int length = datas.Count;

                                for (int i = 0; i < length; i++)
                                    ShowedCollection.Add(datas.ElementAt(i));
                            });
                        });
                    }
                  
                }

            }
        }
        #endregion



        private async Task<bool> UpdateCastAsync(Cast castItem)
        {
            var _updated = await Task.Run(async () =>
            {
                try
                {
                    var _saved = await _dbConn.Database.GetAsync<Cast>(c => c.Title == castItem.Title);
                    castItem.Id = _saved.Id;
                    return true;
                }
                catch (Exception)
                {

                    return false;
                }
            });
            return _updated;
        }

        //private UIElement CreateGridChild<T>(T datas, bool withBackground = true, Visibility visibly = Visibility.Visible)
        //{
        //    Grid _grid = new Grid
        //    {
        //        MinHeight = 740,
        //        VerticalAlignment = VerticalAlignment.Stretch,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        Visibility = visibly
        //    };

        //    GridView _gridView = new GridView
        //    {
        //        Foreground = new SolidColorBrush(Colors.Transparent),
        //        IsItemClickEnabled = true,
        //        SelectionMode = ListViewSelectionMode.None,
        //    };

        //    var trigger = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "ItemClick" };
        //    var action = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = CastItemClickCommand };
        //    trigger.Actions.Add(action);
        //    trigger.Attach(_gridView);

        //    _gridView.ItemsPanel = Application.Current.Resources["GridViewItemsPanelTemplate"] as ItemsPanelTemplate;

        //    if (withBackground)
        //    {
        //        _gridView.ItemTemplate = Application.Current.Resources["GridViewItemTemplate"] as DataTemplate;
        //        _gridView.Background = new ImageBrush()
        //        {
        //            ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Backgrounds/boxbg.jpg", UriKind.RelativeOrAbsolute))
        //        };
        //    }
        //    else
        //    {
        //        _gridView.ItemTemplate = Application.Current.Resources["GridViewItemTemplateBlack"] as DataTemplate;
        //        _gridView.Background = new SolidColorBrush(Colors.White);
        //    }

        //    _gridView.ItemsSource = datas;

        //    _grid.Children.Add(_gridView);
        //    return _grid;
        //}

    }
}
