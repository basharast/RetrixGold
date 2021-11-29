namespace RavinduL.LocalNotifications
{
	using System;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Controls;

	/// <summary>
	/// The base class from which all local notifications derive.
	/// </summary>
	/// <seealso cref="Control" />
	[TemplateVisualState(GroupName = GROUP_LOCAL_NOTIFICATION_STATES, Name = STATE_SHOWN)]
	[TemplateVisualState(GroupName = GROUP_LOCAL_NOTIFICATION_STATES, Name = STATE_HIDDEN)]
	[TemplateVisualState(GroupName = GROUP_LOCAL_NOTIFICATION_STATES, Name = STATE_RESTORING)]
	public abstract partial class LocalNotification : Control
	{
		/// <summary>
		/// Occurs when the notification switches to a <see cref="LocalNotificationState"/>.
		/// </summary>
		public event EventHandler<LocalNotificationStateChangedEventArgs> StateChanged;

		/// <summary>
		/// Gets or sets the time span that the notification stays shown on screen.
		/// <para>Set this value to null if notification should be visible on screen until it's manually dismissed.</para>
		/// </summary>
		public TimeSpan? TimeSpan { get; set; }

		/// <summary>
		/// Called when the notification is being shown with transitions.
		/// </summary>
		protected virtual void OnShowing()
		{
		}

		/// <summary>
		/// Called when the notification is being hidden with transitions.
		/// </summary>
		protected virtual void OnHiding()
		{
		}

		/// <summary>
		/// Called when the notification is being restored with transitions.
		/// </summary>
		protected virtual void OnRestoring()
		{
		}

		/// <summary>
		/// Called when the notification switches to a <see cref="LocalNotificationState"/>.
		/// </summary>
		/// <param name="eventArgs">The <see cref="LocalNotificationStateChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnStateChanged(LocalNotificationStateChangedEventArgs eventArgs)
		{
		}

		private LocalNotificationState _state;

		/// <summary>
		/// The state that the notification is currently in.
		/// <para>Setting this property invokes the <see cref="StateChanged"/> event and the <see cref="OnStateChanged(LocalNotificationStateChangedEventArgs)"/> virtual method (in order).</para>
		/// </summary>
		public LocalNotificationState State
		{
			get { return _state; }
			set
			{
				var args = new LocalNotificationStateChangedEventArgs(_state, _state = value);

				StateChanged?.Invoke(this, args);
				OnStateChanged(args);
			}
		}

		/// <summary>
		/// Shows the notification.
		/// </summary>
		/// <param name="useTransitions">If set to <c>true</c>, the <see cref="OnShowing"/> method will be invoked, and visual transitions will be used.</param>
		public void Show(bool useTransitions = true)
		{
			if (useTransitions)
			{
				State = LocalNotificationState.Showing;
				OnShowing();
			}

			VisualStateManager.GoToState(this, STATE_SHOWN, useTransitions);
		}

		/// <summary>
		/// Hides the notification.
		/// </summary>
		/// <param name="useTransitions">If set to <c>true</c>, the <see cref="OnHiding"/> method will be invoked, and visual transitions will be used.</param>
		public void Hide(bool useTransitions = true)
		{
			if (useTransitions)
			{
				State = LocalNotificationState.Hiding;
				OnHiding();
			}

			VisualStateManager.GoToState(this, STATE_HIDDEN, useTransitions);
		}

		/// <summary>
		/// Restores the notification.
		/// <para>The notification switches to the 'Shown' state, without transitions, after restoration.</para>
		/// </summary>
		/// <param name="useTransitions">If set to <c>true</c>, the <see cref="OnRestoring"/> method will be invoked, and visual transitions will be used.</param>
		public void Restore(bool useTransitions = true)
		{
			if (useTransitions)
			{
				State = LocalNotificationState.Restoring;
				OnRestoring();
			}

			VisualStateManager.GoToState(this, STATE_RESTORING, useTransitions);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild(GROUP_LOCAL_NOTIFICATION_STATES) is VisualStateGroup localNotificationStates)
			{
				localNotificationStates.CurrentStateChanged += LocalNotificationStates_CurrentStateChanged;
			}
		}

		private void LocalNotificationStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
		{
			switch (e.NewState.Name)
			{
				case STATE_SHOWN:
					State = LocalNotificationState.Shown;
					break;

				case STATE_RESTORING:
					Show(false);
					break;

				case STATE_HIDDEN:
					State = LocalNotificationState.Hidden;
					break;
			}
		}
	}
}
