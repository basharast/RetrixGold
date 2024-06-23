using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace RetriX.UWP.Converters
{
    class StretchConverter : IValueConverter
    {
        //These created before major code changes, they are not in use at all
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (Stretch)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Stretch)value;
        }
    }
}
