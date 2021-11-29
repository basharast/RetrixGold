namespace RavinduL.LocalNotifications
{
	using System;

	/// <summary>
	/// Contains information about a setting of <see cref="LocalNotification.State"/>.
	/// </summary>
	/// <seealso cref="EventArgs" />
	public sealed class LocalNotificationStateChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the state that was set from.
		/// </summary>
		public LocalNotificationState OldState { get; private set; }

		/// <summary>
		/// Gets the state that was set to.
		/// </summary>
		public LocalNotificationState NewState { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the value changed during the setting.
		/// </summary>
		public bool ValueChanged => OldState != NewState;

		internal LocalNotificationStateChangedEventArgs(LocalNotificationState oldState, LocalNotificationState newState)
		{
			OldState = oldState;
			NewState = newState;
		}
	}
}
