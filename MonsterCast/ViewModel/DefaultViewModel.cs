using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MonsterCast.Helper;
using MonsterCast.Model;
using MonsterCast.View;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterCast.ViewModel
{
    public class DefaultViewModel : ViewModelBase
    {
        #region Fields
        private readonly RelayCommand<RangeBaseValueChangedEventArgs> _scrollerBarValueChangedCommand = null;
        private readonly RelayCommand<ItemClickEventArgs> _castItemClickCommand = null;
        private readonly RelayCommand _playCommand = null;
        private readonly RelayCommand _loveCommand = null;
        //private IEnumerable<Cast> _nextCurrentCollection = null;
        private IEnumerable<Cast> _podcastCollection = null;
        private IEnumerable<IList<Cast>> _splitedCollection = null;
        private int _splitedCollectionLength = 0;
        private Cast _currentCast = null;      
        private IMessenger _messenger = null;

        private ScrollViewer _scrollerView = null;
        private StackPanel _contentRoot = null;
        private ScrollBar _scrollerBar = null;

        private FontIcon _playFontIcon = null;
        private FontIcon _loveFontIcon = null;

        private TextBlock _playButtonTitle = null;
        private TextBlock _loveButtonTitle = null;
        

        private bool _isLoading = false;
        #endregion

        #region Properties
        public RelayCommand<RangeBaseValueChangedEventArgs> ScrollerBarValueChangedCommand => _scrollerBarValueChangedCommand;
        public RelayCommand<ItemClickEventArgs> CastItemClickCommand => _castItemClickCommand;
        public RelayCommand PlayCommand => _playCommand;
        public RelayCommand LoveCommand => _loveCommand;
        //public IEnumerable<Cast> NextCurrentCollection
        //{
        //    get { return _nextCurrentCollection; }
        //    set { Set(ref _nextCurrentCollection, value); }
        //}

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

        public Cast CurrentCast
        {
            get { return _currentCast; }
            set { Set(ref _currentCast, value); }
        }

        public ScrollViewer ScrollerView
        {
            get { return _scrollerView; }
            set { Set(ref _scrollerView, value); }
        }

        public StackPanel ContentRoot
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }
        #endregion
        public DefaultViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            messenger.Register<NotificationMessage>(this, MessengerAction);
            messenger.Register<NotificationMessage>(this, ViewBuiltNotificationAction);

            _scrollerBarValueChangedCommand = new RelayCommand<RangeBaseValueChangedEventArgs>(ScrollerBarValueChangedAction);
            _castItemClickCommand = new RelayCommand<ItemClickEventArgs>(CastItemClickAction);
            _playCommand = new RelayCommand(PlayRelayCommand);
            _loveCommand = new RelayCommand(LoveRelayCommand);
        }

       

        #region Messenger_Method

        private void ViewBuiltNotificationAction(NotificationMessage args)
        {
            if(args.Notification == Core.Enumeration.Message.NOTIFICATION_VIEW_HAS_BEEN_BUILT)
            {
                //Todo: Check if the current Cast is love it

                if (ContentRoot.Children.Count == 0)
                {
                    bool _withBg = true;
                                                                        
                    for (int i = 0; i < _splitedCollectionLength; i++)
                    {
                      
                        if ((i == 0) || (i == 1))
                        {
                            ContentRoot.Children.Add(CreateGridChild(SplitedCollection.ElementAt(i), _withBg));                          
                        }

                        else
                        {
                            ContentRoot.Children.Add(CreateGridChild(SplitedCollection.ElementAt(i), _withBg, Visibility.Collapsed));                          
                        }
                        _withBg = _withBg == true ? false : true;
                    }
                    ContentRoot.UpdateLayout();
                }
            }
        }
        private void MessengerAction(NotificationMessage args)
        {
            if(args.Notification == Core.Enumeration.Message.NOTIFICATION_PODCAST_HAS_BEEN_SET)
            {
                PodcastCollection = AppConstants.PodcastCollection;
                CurrentCast = PodcastCollection.Count() > 0 ? PodcastCollection.ElementAt(0) : null;

                SplitedCollection = Helpers.SplitCollection(ref _podcastCollection, 8);
                _splitedCollectionLength = SplitedCollection.Count();

                IsLoading = true;
                for (int i = 0; i < _splitedCollectionLength; i++)
                {
                    IEnumerable<Cast> _collection = SplitedCollection.ElementAt(i);
                    Helpers.FetchImageParallel(ref _collection);
                }
                IsLoading = false;
            }
            
            //var firstFiveElement = PodcastCollection.Take(5);
            //Helpers.FetchImageParallel(ref firstFiveElement);
            //NextCurrentCollection = firstFiveElement;
        }
        #endregion

        #region RelayCommand_Method

        private async void LoveRelayCommand()
        {
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
                _messenger.Send(new GenericMessage<Cast>(_currentCast), Core.Enumeration.Message.REQUEST_MEDIAPLAYER_PLAY_SONG);
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayFontIcon.Glyph = "\uE769";
                    PlayButtonTitle.Text = "Pause";
                });
            }
            else
            {
                _messenger.Send(new GenericMessage<Cast>(_currentCast), Core.Enumeration.Message.REQUEST_MEDIAPLAYER_PAUSE_SONG);
                await DispatcherHelper.RunAsync(() =>
                {
                    PlayFontIcon.Glyph = "\uE768";
                    PlayButtonTitle.Text = "Play";
                });
            }

        }

        private void CastItemClickAction(ItemClickEventArgs e)
        {
            var clickedCast = e.ClickedItem as Cast;
            var pageType = typeof(CastDetailView);
            _messenger.Send(new GenericMessage<Type>(pageType), Core.Enumeration.Message.REQUEST_VIEW_NAVIGATION);

            _messenger.Send<GenericMessage<Cast>, CastDetailViewModel>(new GenericMessage<Cast>(clickedCast));
        }

        private void ScrollerBarValueChangedAction(RangeBaseValueChangedEventArgs args)
        {
            if (ContentRoot.Children.Count > 0)
            {
                if (/*ScrollerView.VerticalOffset*/ args.NewValue >= ScrollerView.ScrollableHeight)
                {
                    var element = ContentRoot.Children
                    .Where(item => item.Visibility == Visibility.Collapsed)
                    .FirstOrDefault();

                    if (element != null)
                        element.Visibility = Visibility.Visible;
                }

            }
        }
        #endregion



        private UIElement CreateGridChild<T>(T datas, bool withBackground = true, Visibility visibly = Visibility.Visible)
        {
            Grid _grid = new Grid
            {
                MinHeight = 740,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Visibility = visibly
            };

            GridView _gridView = new GridView
            {
                Foreground = new SolidColorBrush(Colors.Transparent),
                IsItemClickEnabled = true,
                SelectionMode = ListViewSelectionMode.None,
            };

            var trigger = new Microsoft.Xaml.Interactions.Core.EventTriggerBehavior() { EventName = "ItemClick" };
            var action = new Microsoft.Xaml.Interactions.Core.InvokeCommandAction() { Command = CastItemClickCommand };
            trigger.Actions.Add(action);
            trigger.Attach(_gridView);

            _gridView.ItemsPanel = Application.Current.Resources["GridViewItemsPanelTemplate"] as ItemsPanelTemplate;

            if (withBackground)
            {
                _gridView.ItemTemplate = Application.Current.Resources["GridViewItemTemplate"] as DataTemplate;
                _gridView.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Backgrounds/boxbg.jpg", UriKind.RelativeOrAbsolute))
                };
            }
            else
            {
                _gridView.ItemTemplate = Application.Current.Resources["GridViewItemTemplateBlack"] as DataTemplate;
                _gridView.Background = new SolidColorBrush(Colors.White);
            }

            _gridView.ItemsSource = datas;

            _grid.Children.Add(_gridView);
            return _grid;
        }
    }
}
