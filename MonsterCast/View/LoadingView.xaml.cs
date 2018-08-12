using CommonServiceLocator;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace MonsterCast.View
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class LoadingView : Page
    {
        private ViewModel.LoadingViewModel _loadingVM = null;
        public LoadingView()
        {
            this.InitializeComponent();
            _loadingVM = ServiceLocator.Current.GetInstance<ViewModel.LoadingViewModel>();
        }

        private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e)
        {
            _loadingVM.FetchingAllCasts();
        }
    }
}
