namespace RavinduL.LocalNotifications.Converters
{
	using System;
	using Windows.UI.Xaml.Data;

	public abstract class ComparatorConverter<TFrom, TTo> : IValueConverter
	{
		public TFrom KnownValue { get; set; }
		public TFrom UnknownValue { get; set; }

		public TTo WhenEqual { get; set; }
		public TTo WhenUnequal { get; set; }

		public ComparatorConverter()
		{
		}

		public ComparatorConverter(TFrom knownValue, TTo whenEqual, TTo whenUnequal, TFrom unknownValue)
		{
			KnownValue = knownValue;
			WhenEqual = whenEqual;
			WhenUnequal = whenUnequal;
			UnknownValue = unknownValue;
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return Object.Equals(value, KnownValue) ? WhenEqual : WhenUnequal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return Object.Equals(value, WhenEqual) ? KnownValue : UnknownValue;
		}
	}
}
