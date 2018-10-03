using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using MonsterCast.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
                    var _parsingTask = await _httpTask.ContinueWith(async t =>
                    {
                        _progress.Report(5);
                        var _stream = await t.ConfigureAwait(false);
                        var dynamicCollection = await Core.Helpers.XmlParser.Parse(_stream);
                        _progress.Report(5);

                        AppConstants.PodcastCollection = await ConvertToCastObject(dynamicCollection);

                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    await _parsingTask.ContinueWith(t =>
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            Messenger.Send<NotificationMessage, DefaultViewModel>(new NotificationMessage(Core.Enumeration.Message.NOTIFICATION_PODCAST_HAS_BEEN_SET));                          
                            NavigationService.NavigateTo(ViewModelLocator.MainViewKey);
                        });
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);                                 
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Message: {e.Message}");

                    if(e.HResult == -2146233029)
                    {
                        await DialogService.ShowError(
                            "Oop! Something went wrong, we were unable to get podcast's data... Click on retry",
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

            return  await Task.Run(() =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    for (int i = 0; i < _collectionCount; i++)
                    {
                        var newCast = new Cast
                        {
                            Index = $"#{assignedIndex++}",
                            Title = _collection.ElementAt(i).title,
                            Subtitle = _collection.ElementAt(i).subtitle,
                            Summary = _collection.ElementAt(i).summary,
                            Address = _collection.ElementAt(i).image["href"],
                            Song = _collection.ElementAt(i).enclosure["url"]
                        };
                        _castCollection.Add(newCast);
                        if ((int)(((double)_progressCount / (double)_collectionCount) * 100) > percent)
                        {
                            percent = (int)(((double)_progressCount / (double)_collectionCount) * 100);
                            _progress.Report(percent);
                        }
                        _progressCount++;
                    }

                    //foreach (var item in _collection)
                    //{
                    //    var newCast = new Cast
                    //    {
                    //        Index = $"#{assignedIndex++}",
                    //        Title = item.title,
                    //        Subtitle = item.subtitle,
                    //        Summary = item.summary,
                    //        Address = item.image["href"],
                    //        Song = item.enclosure["url"]
                    //    };
                     
                    //    _castCollection.Add(newCast);

                    //    //compute the percentage... and update the progress bar value
                    //    if ((int)(((double)_progressCount / (double)_collectionCount) * 100) > percent)
                    //    {
                    //        percent = (int)(((double)_progressCount / (double)_collectionCount) * 100);
                    //        _progress.Report(percent);
                    //    }
                    //    _progressCount++;
                    //}                    
                });
                return _castCollection;
            });
        }
    }
}
