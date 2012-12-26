using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Preferences;
using Com.Slidingmenu.Lib;

namespace GoogleMusic
{
	[Activity (Label = "GoogleMusic")]
	public class MainActivity : Activity , IMainViewController
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SlidingMenu menu = new SlidingMenu(this);
			menu.Mode = 0;
			menu.TouchModeAbove = 1;
			menu.SetShadowWidthRes (Resource.Dimension.shadow_width);
			menu.SetShadowDrawable (Resource.Drawable.shadow);
			menu.SetBehindOffsetRes (Resource.Dimension.slidingmenu_offset);
			menu.SetFadeDegree (.35f);
			menu.AttachToActivity (this, 1);
			menu.SetMenu (Resource.Layout.menu);


			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};
		}

		#region IMainViewController implementation

		public void ShowStatus (string message)
		{

		}

		public void HideStatus ()
		{

		}

		public void UpdateStatus (float percent)
		{

		}

		public void UpdateSong (Song currentSong)
		{

		}

		public void SetState (bool state)
		{

		}

		public void PlaylistChanged ()
		{

		}

		public void UpdateStatus (string currentTime, string remainingTime, float percent)
		{

		}

		public void SetPlayCount ()
		{

		}

		public void ShowNowPlaying ()
		{

		}

		public void RefreshSongs ()
		{

		}

		public void RefreshArtists ()
		{

		}

		public void RefreshGenre ()
		{

		}

		public void RefreshAlbum ()
		{

		}

		public void RefreshPlaylist ()
		{

		}

		public void UpdateSongProgress (float percent)
		{

		}

		public void UpdatePlaylistProgress (float percent)
		{

		}

		public void UpdateCurrentSongDownloadProgress (float percent)
		{

		}

		public void UpdateMeter ()
		{

		}

		public void GoToArtist (int artistId)
		{

		}

		public void GoToAlbum (int albumId)
		{

		}

		public void GoToGenre (int genreId)
		{

		}

		public void ToggleMenu ()
		{

		}

		public void DownloaderUpdated ()
		{

		}

		#endregion
	}
}


