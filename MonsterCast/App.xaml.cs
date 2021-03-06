﻿using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using MonsterCast.View;
using MonsterCast.ViewModel;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MonsterCast.Database;
using MonsterCast.Database.Tables;
using MonsterCast.Core.Manager;

namespace MonsterCast
{
    /// <summary>
    /// Fournit un comportement spécifique à l'application afin de compléter la classe Application par défaut.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initialise l'objet d'application de singleton.  Il s'agit de la première ligne du code créé
        /// à être exécutée. Elle correspond donc à l'équivalent logique de main() ou WinMain().
        /// </summary>
        public  App()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var navigationService = new NavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            SimpleIoc.Default.Register<IMessenger, Messenger>(true);


            navigationService.Configure(ViewModelLocator.MainViewKey, typeof(MainView));
            navigationService.Configure(ViewModelLocator.FavoriteCastViewKey, typeof(FavoriteCastView));
            navigationService.Configure(ViewModelLocator.LiveViewKey, typeof(LiveView));
            navigationService.Configure(ViewModelLocator.LoadingViewKey, typeof(LoadingView));
            navigationService.Configure(ViewModelLocator.AboutViewKey, typeof(AboutView));
            navigationService.Configure(ViewModelLocator.DefaultViewKey, typeof(DefaultView));
            navigationService.Configure(ViewModelLocator.CastDetailKey, typeof(CastDetailView));
            navigationService.Configure(ViewModelLocator.NowPlayingViewKey, typeof(NowPlayingView));
           
            SimpleIoc.Default.Register<IDialogService, DialogService>();

            SimpleIoc.Default.Register<IMonsterDatabase, MonsterDatabase>(true);
            SimpleIoc.Default.Register<MediaPlayerManager>(true);
           

            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            Construct();

            
        }

        
                              
        /// <summary>
        /// Invoqué lorsque l'application est lancée normalement par l'utilisateur final.  D'autres points d'entrée
        /// seront utilisés par exemple au moment du lancement de l'application pour l'ouverture d'un fichier spécifique.
        /// </summary>
        /// <param name="e">Détails concernant la requête et le processus de lancement.</param>
        protected override async void  OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            var dbConn = SimpleIoc.Default.GetInstance<IMonsterDatabase>();
            int con = await dbConn.ConnectAsync();
            if(con == 0)
            {
                var created = await dbConn.Database.CreateTableAsync<Cast>();
            }

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            //viewTitleBar.ButtonForegroundColor = (Windows.UI.Color)Resources["SystemBaseHighColor"];

                                      
            Frame rootFrame = Window.Current.Content as Frame;
                                                          
            // Ne répétez pas l'initialisation de l'application lorsque la fenêtre comporte déjà du contenu,
            // assurez-vous juste que la fenêtre est active
            if (rootFrame == null)
            {
                // Créez un Frame utilisable comme contexte de navigation et naviguez jusqu'à la première page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: chargez l'état de l'application précédemment suspendue
                }

                // Placez le frame dans la fenêtre active
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Quand la pile de navigation n'est pas restaurée, accédez à la première page,
                    // puis configurez la nouvelle page en transmettant les informations requises en tant que
                    // paramètre
                    rootFrame.Navigate(typeof(View.LoadingView), e.Arguments);                   
                }
                // Vérifiez que la fenêtre actuelle est active
                Window.Current.Activate();
               
                //Initialize the DispatcherHelper
                DispatcherHelper.Initialize();
            }
        }

       
        /// <summary>
        /// Appelé lorsque la navigation vers une page donnée échoue
        /// </summary>
        /// <param name="sender">Frame à l'origine de l'échec de navigation.</param>
        /// <param name="e">Détails relatifs à l'échec de navigation</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Appelé lorsque l'exécution de l'application est suspendue.  L'état de l'application est enregistré
        /// sans savoir si l'application pourra se fermer ou reprendre sans endommager
        /// le contenu de la mémoire.
        /// </summary>
        /// <param name="sender">Source de la requête de suspension.</param>
        /// <param name="e">Détails de la requête de suspension.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: enregistrez l'état de l'application et arrêtez toute activité en arrière-plan 
            var dbConn = SimpleIoc.Default.GetInstance<IMonsterDatabase>();
            await dbConn.CloseAsync();
            deferral.Complete();
        }

        private async void OnResuming(object sender, object e)
        {
            var dbConn = SimpleIoc.Default.GetInstance<IMonsterDatabase>();
            await dbConn.ConnectAsync();
        }
    }
}
