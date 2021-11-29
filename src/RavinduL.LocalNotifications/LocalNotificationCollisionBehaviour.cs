namespace RavinduL.LocalNotifications
{
	/// <summary>
	/// Specifies the way in which a collision between two notifications (i.e. attempting to show one while another still persists) is handled.
	/// </summary>
	public enum LocalNotificationCollisionBehaviour
	{
		/// <summary>
		/// The new notification gets delayed until dismissal of the active notification.
		/// </summary>
		Wait = 0,

		/// <summary>
		/// The active notification gets hidden immediately, after which the new notification will be shown.
		/// </summary>
		Replace,
	}
}
