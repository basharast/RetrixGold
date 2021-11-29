namespace RavinduL.LocalNotifications.Converters
{
	using System;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Data;

	/// <summary>
	/// Converts a <see cref="String"/> to a <see cref="Visibility"/> by checking if it is null, empty, or consists of only white-space characters.
	/// </summary>
	/// <seealso cref="IValueConverter" />
	public sealed class StringToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (value is string s && !String.IsNullOrWhiteSpace(s)) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
