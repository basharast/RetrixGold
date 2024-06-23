using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace RetriX.UWP.Converters
{
    public class BoolNegationConverter : IValueConverter
    {
        //These created before major code changes, they are not in use at all
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = !(bool)value ? Visibility.Visible : Visibility.Collapsed;
            return state;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var state = (Visibility)value!= Visibility.Visible ? true : false;
            return state;
        }
    }
}
