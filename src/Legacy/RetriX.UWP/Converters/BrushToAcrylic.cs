using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WinUniversalTool.Models;

namespace WinUniversalTool.Converters
{
    class BrushToAcrylic : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                return (value as AcrylicBrush);
            }
            else
            {
                return (value as SolidColorBrush);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                return (value as AcrylicBrush);
            }
            else
            {
                return (value as SolidColorBrush);
            }
        }
    }
}
