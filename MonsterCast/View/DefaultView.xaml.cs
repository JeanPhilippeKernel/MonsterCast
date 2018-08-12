using CommonServiceLocator;
using MonsterCast.Helper;
using MonsterCast.Model;
using MonsterCast.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace MonsterCast.View
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class DefaultView : Page
    {
        public DefaultViewModel DefaultVM { get; set; }
        
        public DefaultView()
        {
            this.InitializeComponent();
            Scroller.ViewChanged += Scroller_ViewChanged;
           
            DefaultVM = ServiceLocator.Current.GetInstance<DefaultViewModel>();                      
        }

        private void Scroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {            
            if(ContentRoot.Children.Count > 0)
            {
                if (Scroller.VerticalOffset >= Scroller.ScrollableHeight)
                {
                    var element = ContentRoot.Children
                    .Where(item => item.Visibility == Visibility.Collapsed)
                    .FirstOrDefault();

                    if (element != null)
                        element.Visibility = Visibility.Visible;
                }
                
            }
        }

        public void GenerateViewContent()
        {
            if (ContentRoot.Children.Count == 0)
            {
                bool _withBg = true;
                var _podcastCollection = DefaultVM.PodcastCollection;
                ContentRoot.Children.Add(CreateFlipViewChild(DefaultVM.NextCurrentCollection));
                var splitedCollection = Helpers.SplitCollection(ref _podcastCollection, 8);

                IEnumerable<Cast> collection = null;

                for (int i = 0; i < splitedCollection.Count(); i++)
                {
                    collection = splitedCollection.ElementAt(i);
                    Helpers.FetchImageParallel(ref collection);
                    if ((i == 0) || (i == 1))
                    {                       
                        ContentRoot.Children.Add(CreateGridChild(collection, _withBg));
                        _withBg = _withBg == true ? false : true;
                    }

                    else
                    {                       
                        ContentRoot.Children.Add(CreateGridChild(collection, _withBg, Visibility.Collapsed));
                        _withBg = _withBg == true ? false : true;
                    }
                }
                ContentRoot.UpdateLayout();
            }
            
        }

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

            _gridView.ItemClick += DefaultVM.GridView_ItemClick;

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

        private UIElement CreateFlipViewChild<T>(T datas)
        {
            FlipView _flipView = new FlipView { HorizontalAlignment = HorizontalAlignment.Stretch };
            _flipView.ItemTemplate = Application.Current.Resources["FlipViewItemTemplate"] as DataTemplate;
            _flipView.ItemsSource = datas;
            _flipView.SelectedIndex = 1;
            return _flipView;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GenerateViewContent();
            base.OnNavigatedTo(e);         
        }
    }
}
