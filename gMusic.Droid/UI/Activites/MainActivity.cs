using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Preferences;

namespace GoogleMusic
{
	[Activity (Label = "GoogleMusic")]
	public class MainActivity : Activity , IMainViewController
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
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


