using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MonsterCast.Converter
{
    public class SliderToolTipConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double positionValue = (double)value;
            var timeSpan = TimeSpan.FromSeconds(positionValue);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return string.Empty;
        }
    }
}
