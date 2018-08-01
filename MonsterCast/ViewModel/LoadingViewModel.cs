using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using MonsterCast.Helper;
using MonsterCast.Model;
using MonsterCast_App.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace MonsterCast.ViewModel
{
    public class LoadingViewModel : ViewModelBase
    {
        #region Fields
        private int _min = 0;
        private int _max = 100;
        private int _currentValue = 0;
        private IMessenger Messenger = null;
        private INavigationService NavigationService = null;
        private IDialogService DialogService = null;
        private const string podcastAddress = "https://www.monstercat.com/podcast/feed.xml";
        
        IProgress<int> _progress = null;
        #endregion

        #region Properties
        //public RelayCommand MediaAction { get; set; }
        public int Min
        {
            get { return _min; }
            set { Set(ref _min, value); }
        }
        public int Max
        {
            get { return _max; }
            set { Set(ref _max, value); }
        }
        public int CurrentValue
        {
            get { return _currentValue; }
            set { Set(ref _currentValue, value); }
        }
        #endregion


        public LoadingViewModel(
            IMessenger _messenger, 
            INavigationService _navigationService,
            IDialogService _dialogService)
        {
            this.Messenger = _messenger;
            this.NavigationService = _navigationService;
            this.DialogService = _dialogService;
            _progress = new Progress<int>(ProgressHandler);
            //MediaAction = new RelayCommand(FetchingAllCasts);
        }

        private void ProgressHandler(int value)
        {
            CurrentValue = (value > CurrentValue) ? value : CurrentValue;        
        }

        public async void FetchingAllCasts()
        {
            _progress.Report(0);        
            using (var _httpClient = new HttpClient())
            {       
                try
                {
                    var _httpTask = _httpClient.GetStreamAsync(podcastAddress);
                    AppConstants.PodcastCollection = await await _httpTask.ContinueWith(async (t) =>
                     {
                         _progress.Report(5);
                         var dynamicCollection = await XmlHelper.Parse(t.Result);
                         _progress.Report(5);

                         return await ConvertToCastObject(dynamicCollection);
                     }, TaskContinuationOptions.OnlyOnRanToCompletion);
                
                    Messenger.Send(new NotificationMessage("podcasts collection has set"));
                    NavigationService.NavigateTo(ViewModelLocator.MainViewKey);                   
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Message: {e.Message}");

                    if(e.HResult == -2146233029)
                    {
                        await DialogService.ShowError(
                            "Oop! Something went wrong... click on retry",
                            "Humm... :(", "Retry",
                            () => FetchingAllCasts());
                    }
                    else
                    {
                        await DialogService.ShowError(
                            "Please check your internet connection and click on retry",
                            "No Internet", "Retry",
                            () => FetchingAllCasts());
                    }
                }
            }
        }

        private async Task<IEnumerable<Cast>> ConvertToCastObject(IEnumerable<dynamic> _collection)
        {
            var _castCollection = new List<Cast>();
            int assignedIndex = 1;
            int _progressCount = 1;
            int _collectionCount = _collection.Count();
            int percent = 0;

            return await await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    foreach (var item in _collection)
                    {
                        var newCast = new Cast
                        {
                            Index = $"#{assignedIndex++}",
                            Title = item.title,
                            Subtitle = item.subtitle,
                            Summary = item.summary,
                            Address = item.image["href"],
                            Song = item.enclosure["url"]
                        };
                     
                        _castCollection.Add(newCast);

                        //compute the percentage... and update the progress bar value
                        if ((int)(((double)_progressCount / (double)_collectionCount) * 100) > percent)
                        {
                            percent = (int)(((double)_progressCount / (double)_collectionCount) * 100);
                            _progress.Report(percent);
                        }
                        _progressCount++;
                    }                    
                });
                return _castCollection.AsEnumerable();
            });
        }
    }
}
