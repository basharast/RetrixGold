namespace RavinduL.LocalNotifications
{
	using System;
	using System.Collections.Generic;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Controls;

	/// <summary>
	/// Manages the showing and hiding of <see cref="LocalNotification"/>s.
	/// </summary>
	public sealed class LocalNotificationManager
	{
		private Grid grid;
		private Queue<LocalNotification> q = new Queue<LocalNotification>();
		private LocalNotification current;
		private DispatcherTimer timer;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalNotificationManager" /> class.
		/// </summary>
		/// <param name="grid">The grid (that ideally spans the entire screen) within which all <see cref="LocalNotification" />s managed by this instance of the <see cref="LocalNotificationManager" /> will be added to.</param>
		/// <exception cref="ArgumentNullException">grid</exception>
		public LocalNotificationManager(Grid grid)
		{
			this.grid = grid ?? throw new ArgumentNullException(nameof(grid));
		}

		/// <summary>
		/// Shows the specified <see cref="LocalNotification" />.
		/// </summary>
		/// <param name="notification">The <see cref="LocalNotification" /> to be shown.</param>
		/// <param name="collisionBehaviour">How the notification being shown should behave if an active notification exists.</param>
		/// <exception cref="ArgumentNullException">notification</exception>
		public void Show(LocalNotification notification, LocalNotificationCollisionBehaviour collisionBehaviour = LocalNotificationCollisionBehaviour.Wait)
		{
			if (notification == null)
			{
				throw new ArgumentNullException(nameof(notification));
			}

			if (current == null)
			{
				current = notification;

				grid.Children.Add(current);

				current.LayoutUpdated += Current_LayoutUpdated;

				current.UpdateLayout();
			}
			else
			{
				q.Enqueue(notification);

				if (collisionBehaviour == LocalNotificationCollisionBehaviour.Replace)
				{
					HideCurrent();
				}
			}
		}

		private void Current_LayoutUpdated(object sender, object e)
		{
			current.LayoutUpdated -= Current_LayoutUpdated;

			// When the Hide(false) method is invoked, the state of the notification will change to Hidden, which shouldn't be handled by Current_StateChanged (upon which it gets hidden immediately).
			EventHandler<LocalNotificationStateChangedEventArgs> handler = null;
			handler = (s, a) =>
			{
				if (a.NewState == LocalNotificationState.Hidden)
				{
					current.StateChanged -= handler;
					current.StateChanged += Current_StateChanged;

					current.Show();
				}
			};

			current.StateChanged += handler;

			current.Hide(false);
		}

		private void Current_StateChanged(object sender, LocalNotificationStateChangedEventArgs e)
		{
			switch (e.NewState)
			{
				// When a notification first gets hidden, it will get removed from the visual tree.
				case LocalNotificationState.Hidden:
					{
						current.StateChanged -= Current_StateChanged;

						DisengageTimer();

						grid.Children.Remove(current);

						current = null;

						if (q.Count > 0)
						{
							Show(q.Dequeue());
						}
					}
					break;

				// When a notification gets shown when the timer is null (i.e. when it's first shown), a new timer will be assigned to schedule hiding it.
				// The null check is so that when the notification switches to the 'Shown' state multiple times (e.g. after restoration), a new timer doesn't get assigned to it.
				case LocalNotificationState.Shown:
					{
						if (timer == null)
						{
							timer = new DispatcherTimer
							{
								Interval = current.TimeSpan ?? TimeSpan.Zero,
							};

							if (current.TimeSpan != null)
							{
								timer.Tick += Timer_Tick;
								timer.Start();
							}
						}
					}
					break;
			}
		}

		private void DisengageTimer()
		{
			if (timer != null)
			{
				timer.Stop();
				timer.Tick -= Timer_Tick;
				timer = null;
			}
		}

		private void Timer_Tick(object sender, object e)
		{
			DisengageTimer();
			HideCurrent();
		}

		/// <summary>
		/// Hides the current <see cref="LocalNotification"/> if one exists.
		/// </summary>
		public void HideCurrent()
		{
			current?.Hide();
		}

		/// <summary>
		/// Hides the current <see cref="LocalNotification"/>, and cancels those that are scheduled for the future.
		/// </summary>
		public void HideAll()
		{
			q.Clear();
			HideCurrent();
		}
	}
}
