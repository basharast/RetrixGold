using RetriX.Shared.ViewModels;
using System;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace RetriX.UWP.Converters
{
    public class HoldingEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = value as HoldingRoutedEventArgs;
            string ObjectType = args.OriginalSource.GetType().Name;
            switch (ObjectType)
            {
                case "Image":
                    return ((Image)args.OriginalSource).DataContext;
                case "TextBlock":
                    return ((TextBlock)args.OriginalSource).DataContext;
                case "Border":
                    return ((Border)args.OriginalSource).DataContext;
                case "Grid":
                    return null;
                case "ListViewItemPresenter":
                    return ((ListViewItemPresenter)args.OriginalSource).DataContext;
                default:
                    return  ((ListViewItem)args.OriginalSource).DataContext;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
