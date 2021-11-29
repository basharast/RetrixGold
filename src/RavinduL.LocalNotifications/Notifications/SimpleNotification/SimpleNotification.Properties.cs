namespace RavinduL.LocalNotifications.Notifications
{
	using System;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Media;

	partial class SimpleNotification
	{
		public Action Action { get; set; }

		/// <summary>
		/// Gets or sets the text to be displayed.
		/// </summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(nameof(Text), typeof(string), typeof(SimpleNotification), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the glyph to be displayed alongside the text.
		/// </summary>
		public string Glyph
		{
			get { return (string)GetValue(GlyphProperty); }
			set { SetValue(GlyphProperty, value); }
		}

		public static readonly DependencyProperty GlyphProperty =
			DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(SimpleNotification), new PropertyMetadata(null));

		public double TranslateHeight
		{
			get { return (double)GetValue(TranslateHeightProperty); }
			private set { SetValue(TranslateHeightProperty, value); }
		}

		public static readonly DependencyProperty TranslateHeightProperty =
			DependencyProperty.Register(nameof(TranslateHeight), typeof(double), typeof(SimpleNotification), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the length that the <see cref="SimpleNotification"/> can be pulled away from its edge of the screen (dictated by <see cref="FrameworkElement.VerticalAlignment"/>).
		/// </summary>
		public double PullAwayLength
		{
			get { return (double)GetValue(PullAwayLengthProperty); }
			set { SetValue(PullAwayLengthProperty, value); }
		}

		public static readonly DependencyProperty PullAwayLengthProperty =
			DependencyProperty.Register(nameof(PullAwayLength), typeof(double), typeof(SimpleNotification), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the length that the <see cref="SimpleNotification"/> can be pulled toward its edge of the screen (dictated by <see cref="FrameworkElement.VerticalAlignment"/>), exceeding which, the notification is hidden.
		/// </summary>
		public double HideThreshold
		{
			get { return (double)GetValue(HideThresholdProperty); }
			set { SetValue(HideThresholdProperty, value); }
		}

		public static readonly DependencyProperty HideThresholdProperty =
			DependencyProperty.Register(nameof(HideThreshold), typeof(double), typeof(SimpleNotification), new PropertyMetadata(null, (d, e) => ((SimpleNotification)d).OnHideThresholdChanged()));

		/// <summary>
		/// Gets or sets the duration of the sliding transitions that the <see cref="SimpleNotification"/> uses to enter and exit the viewport.
		/// </summary>
		public Duration TransitionDuration
		{
			get { return (Duration)GetValue(TransitionDurationProperty); }
			set { SetValue(TransitionDurationProperty, value); }
		}

		public static readonly DependencyProperty TransitionDurationProperty =
			DependencyProperty.Register(nameof(TransitionDuration), typeof(Duration), typeof(SimpleNotification), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets how wide the <see cref="SimpleNotification"/> should be when the viewport is wide (when it ideally shouldn't stretch horizontally).
		/// <para>Set this value to <see cref="Double.NaN"/> if the notification should be horizontally stretched regardless of the width of the viewport.</para>
		/// </summary>
		public double CompactWidth
		{
			get { return (double)GetValue(CompactWidthProperty); }
			set { SetValue(CompactWidthProperty, value); }
		}

		public static readonly DependencyProperty CompactWidthProperty =
			DependencyProperty.Register(nameof(CompactWidth), typeof(double), typeof(SimpleNotification), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the width that, if exceeded, the <see cref="SimpleNotification"/> attains the fixed width defined by <see cref="CompactWidth"/> instead of stretching horizontally.
		/// </summary>
		public double Breakpoint
		{
			get { return (double)GetValue(BreakpointProperty); }
			set { SetValue(BreakpointProperty, value); }
		}

		public static readonly DependencyProperty BreakpointProperty =
			DependencyProperty.Register(nameof(Breakpoint), typeof(double), typeof(SimpleNotification), new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the <see cref="FontFamily"/> used for displaying the <see cref="Glyph"/>.
		/// </summary>
		public FontFamily GlyphFontFamily
		{
			get { return (FontFamily)GetValue(GlyphFontFamilyProperty); }
			set { SetValue(GlyphFontFamilyProperty, value); }
		}

		public static readonly DependencyProperty GlyphFontFamilyProperty =
			DependencyProperty.Register(nameof(GlyphFontFamily), typeof(FontFamily), typeof(SimpleNotification), new PropertyMetadata(null));
	}
}
