namespace RavinduL.LocalNotifications
{
	/// <summary>
	/// Specifies the state of a <see cref="LocalNotification" />.
	/// </summary>
	public enum LocalNotificationState
	{
		/// <summary>
		/// The <see cref="LocalNotification"/> being hidden from view, and the user not being able to interact with it.
		/// </summary>
		Hidden = 0,

		/// <summary>
		/// The <see cref="LocalNotification"/> transitioning to the <see cref="Hidden"/> state.
		/// </summary>
		Hiding,

		/// <summary>
		/// The <see cref="LocalNotification"/> being displayed, awaiting user interaction.
		/// </summary>
		Shown,

		/// <summary>
		/// The <see cref="LocalNotification"/> transitioning to the <see cref="Shown"/> state, from being hidden.
		/// </summary>
		Showing,

		/// <summary>
		/// The <see cref="LocalNotification"/> transitioning to the <see cref="Shown"/> state, but not from being hidden.
		/// </summary>
		Restoring,
	}
}
