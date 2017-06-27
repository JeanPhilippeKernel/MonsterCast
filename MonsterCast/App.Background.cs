using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.System;
using System.Diagnostics;

namespace MonsterCast
{
    sealed partial class App : Application
    {
        private bool _isInBackgroundMode = false;
        public void Construct()
        {
            this.EnteredBackground += App_EnteredBackground;
            this.LeavingBackground += App_LeavingBackground;

            MemoryManager.AppMemoryUsageLimitChanging += MemoryManager_AppMemoryUsageLimitChanging;
            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;
        }


        private void App_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            _isInBackgroundMode = false;
            Debug.WriteLine($"App state : {_isInBackgroundMode}");
			if(Window.Current.Content == null)
            {

            }
        }

        private void App_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            _isInBackgroundMode = true;
            Debug.WriteLine($"App state : {_isInBackgroundMode}");
        }

        private void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            var levelUsage = MemoryManager.AppMemoryUsageLevel;
			if(levelUsage == AppMemoryUsageLevel.High 
				|| levelUsage == AppMemoryUsageLevel.OverLimit)
            {
                ReduceAppMemoryUsage(MemoryManager.AppMemoryUsageLimit);
            }
        }

        private void MemoryManager_AppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs e)
        {
            if (MemoryManager.AppMemoryUsage >= e.NewLimit)
                ReduceAppMemoryUsage(e.NewLimit);
        }

        private void ReduceAppMemoryUsage(ulong newLimit)
        {
            if (_isInBackgroundMode && Window.Current.Content != null)
            {
                Debug.WriteLine("need to reduce memory");
            }
        }
    }
}
