namespace RavinduL.LocalNotifications.Notifications
{
	using System;
	using Windows.Foundation;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Controls;
	using Windows.UI.Xaml.Input;
	using Windows.UI.Xaml.Media;

	/// <summary>
	/// A <see cref="LocalNotification"/> that slides in and out from either the top or the bottom of the screen, consisting of some text and optionally, a glyph alongside it.
	/// </summary>
	/// <seealso cref="LocalNotification" />
	[TemplatePart(Name = PART_NOTIFICATION_ROOT, Type = typeof(Grid))]
	[TemplatePart(Name = PART_TRANSFORM, Type = typeof(TranslateTransform))]
	[TemplatePart(Name = PART_TARGET, Type = typeof(Grid))]
	public partial class SimpleNotification : LocalNotification
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleNotification"/> class.
		/// </summary>
		public SimpleNotification()
		{
			DefaultStyleKey = typeof(SimpleNotification);
			RegisterPropertyChangedCallback(VerticalAlignmentProperty, (d, e) => ((SimpleNotification)d).UpdateManipulationPredicates());
		}

		private Func<Point, double, bool> hasExceededHideThreshold;
		private Func<Point, double, bool> hasExceededPullAwayLength;

		private double internalHideThreshold;
		private double internalPullDownLength;

		private FrameworkElement notificationRoot;
		private TranslateTransform translation;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild(PART_NOTIFICATION_ROOT) is FrameworkElement notificationRoot)
			{
				this.notificationRoot = notificationRoot;
				notificationRoot.SizeChanged += (s, e) => OnSizeChanged();

				if (GetTemplateChild(PART_TRANSFORM) is TranslateTransform translation)
				{
					this.translation = translation;
					notificationRoot.ManipulationDelta += NotificationRoot_ManipulationDelta;
					notificationRoot.ManipulationCompleted += NotificationRoot_ManipulationCompleted;
				}
			}

			if (GetTemplateChild(PART_TARGET) is Grid target)
			{
				TappedEventHandler tappedHandler = null;
				tappedHandler += (s, e) =>
				{
					target.Tapped -= tappedHandler;

					Action?.Invoke();
					Hide();
				};

				target.Tapped += tappedHandler;
			}

			if (GetTemplateChild(PART_DISMISS_BUTTON) is Button dismissButton)
			{
				dismissButton.Click += (s, e) => Hide();
			}

			UpdateManipulationPredicates();
		}

		private void OnSizeChanged()
			=> TranslateHeight = (VerticalAlignment == VerticalAlignment.Top) ? -ActualHeight : ActualHeight;

		private void OnHideThresholdChanged()
			=> internalHideThreshold = (VerticalAlignment == VerticalAlignment.Top) ? -HideThreshold : HideThreshold;

		private void OnPullDownLengthChanged()
			=> internalPullDownLength = (VerticalAlignment == VerticalAlignment.Top) ? PullAwayLength : -PullAwayLength;

		private void UpdateManipulationPredicates()
		{
			if (VerticalAlignment != VerticalAlignment.Top && VerticalAlignment != VerticalAlignment.Bottom)
			{
				throw new ArgumentException($"The valid values for {nameof(VerticalAlignment)} of a {nameof(SimpleNotification)} are {nameof(VerticalAlignment.Top)} and {VerticalAlignment.Bottom}");
			}

			if (VerticalAlignment == VerticalAlignment.Top)
			{
				hasExceededHideThreshold = (cumulativeTranslation, max) => cumulativeTranslation.Y < max;
				hasExceededPullAwayLength = (cumulativeTranslation, max) => cumulativeTranslation.Y > max;
			}
			else
			{
				hasExceededHideThreshold = (cumulativeTranslation, max) => cumulativeTranslation.Y > max;
				hasExceededPullAwayLength = (cumulativeTranslation, max) => cumulativeTranslation.Y < max;
			}

			OnHideThresholdChanged();
			OnPullDownLengthChanged();
			OnSizeChanged();
		}

		protected override void OnStateChanged(LocalNotificationStateChangedEventArgs e)
		{
			base.OnStateChanged(e);

			// Ensures that the notification can be manipulated only _after_ it's shown.
			notificationRoot.ManipulationMode = e.NewState == LocalNotificationState.Shown ? ManipulationModes.TranslateY : ManipulationModes.None;
		}

		private void NotificationRoot_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			if (hasExceededHideThreshold(e.Cumulative.Translation, internalHideThreshold))
			{
				Hide();
			}
			else
			{
				Restore();
			}
		}

		private void NotificationRoot_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			translation.Y += e.Delta.Translation.Y;

			if (hasExceededPullAwayLength(e.Cumulative.Translation, internalPullDownLength))
			{
				translation.Y = internalPullDownLength;
			}
		}
	}
}
