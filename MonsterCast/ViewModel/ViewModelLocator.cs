using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using MonsterCast.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCast.ViewModel
{
    public class ViewModelLocator
    {
        #region ViewKeys
        public const string MainViewKey = "MainView";
        public const string LoadingViewKey = "LoadingView";
        public const string FavoriteCastViewKey = "FavorisView";     
        public const string LiveViewKey = "LiveView";
        public const string AboutViewKey = "AboutView";
        public const string DefaultViewKey = "DefaultView";
        public const string CastDetailKey = "CastDetailView";
        #endregion

        #region ViewModelProperties
        public LoadingViewModel LoadingVM => SimpleIoc.Default.GetInstance<LoadingViewModel>();
        public MainViewModel MainVM => SimpleIoc.Default.GetInstance<MainViewModel>();
        public AboutViewModel AboutVM => SimpleIoc.Default.GetInstance<AboutViewModel>();
        public LiveViewModel LiveVM => SimpleIoc.Default.GetInstance<LiveViewModel>();
        public DefaultViewModel DefaultVM => SimpleIoc.Default.GetInstance<DefaultViewModel>();
        public FavoriteViewModel FavoriteVM => SimpleIoc.Default.GetInstance<FavoriteViewModel>();
        public CastDetailViewModel CastDetailVM => SimpleIoc.Default.GetInstance<CastDetailViewModel>();
        #endregion

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<IMessenger, Messenger>(true);
           
            //Caching ViewModels Class
            SimpleIoc.Default.Register<LoadingViewModel>();
            SimpleIoc.Default.Register<DefaultViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();

            SimpleIoc.Default.Register<AboutViewModel>(true);
            SimpleIoc.Default.Register<LiveViewModel>();
            SimpleIoc.Default.Register<FavoriteViewModel>();
            SimpleIoc.Default.Register<CastDetailViewModel>(true);
        }

        public static void UnregisterAndCleanup<T>()
            where T : ViewModelBase
        {
            var instance = SimpleIoc.Default.GetInstance<T>();
            instance.Cleanup();
            SimpleIoc.Default.Unregister<T>();
            SimpleIoc.Default.Register<T>();
        }
    }
}
