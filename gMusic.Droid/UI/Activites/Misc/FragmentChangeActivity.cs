
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace GoogleMusic
{
	[Activity (Label = "gMusic", MainLauncher = true)]			
	public class FragmentChangeActivity : BaseActivity, IMainViewController
	{
	
		protected Fragment mContent;

		public override void OnCreate (Bundle savedInstanceState)
		{
			Util.MainVC = this;
			base.OnCreate (savedInstanceState);

			Settings.UserName = "james.clancey@gmail.com";
			if (!string.IsNullOrEmpty (Settings.UserName))
				Database.SetDatabase (Settings.UserName);

			Task.Factory.StartNew (delegate {
				Database.SetDatabase ("james.clancey@gmail.com");
				Util.LoadData ();
				if (Util.Api == null)
					Util.Api = new GoogleMusicApi ("james.clancey@gmail.com");
				Util.Api.SignIn ("james.clancey@gmail.com", "Tng4life!", (success) => {
					Settings.UserName = "james.clancey@gmail.com";
					Console.WriteLine (success);
			
		
				});
			});

			var menu = new MenuFragment ();
			// set the Above View
			if (savedInstanceState != null)
				mContent = FragmentManager.GetFragment (savedInstanceState, "mContent");
			if (mContent == null)
				mContent = menu.MenuItems[0].Content;	
			
			// set the Above View
			this.SetContentView (Resource.Layout.content_frame);
			FragmentManager.BeginTransaction ().Replace (Resource.Id.content_frame, mContent).Commit ();

			
			// set the Behind View
			SetBehindContentView (Resource.Layout.menu_frame);

			
			FragmentManager.BeginTransaction ().Replace (Resource.Id.menu_frame, menu).Commit ();
		}
		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			FragmentManager.PutFragment(outState, "mContent", mContent);
//			if (mContent is ListFragment) {
//				((ListFragment)mContent).ListView.FirstVisiblePosition
//			}
		}

		public void switchContent (Fragment fragment)
		{
			mContent = fragment;
			FragmentManager.BeginTransaction ().Replace (Resource.Id.content_frame, fragment).Commit ();
			SlidingMenu.ShowContent ();
			//Toggle ();
		}

		#region IMainViewController implementation
		public void ShowStatus (string message)
		{
			//throw new NotImplementedException ();
		}

		public void HideStatus ()
		{
			//throw new NotImplementedException ();
		}

		public void UpdateStatus (float percent)
		{
			//throw new NotImplementedException ();
		}

		public void UpdateSong (Song currentSong)
		{
			//throw new NotImplementedException ();
		}

		public void SetState (bool state)
		{
			//throw new NotImplementedException ();
		}

		public void PlaylistChanged ()
		{
			//throw new NotImplementedException ();
		}

		public void UpdateStatus (string currentTime, string remainingTime, float percent)
		{
			//throw new NotImplementedException ();
		}

		public void SetPlayCount ()
		{
			//throw new NotImplementedException ();
		}

		public void ShowNowPlaying ()
		{
			//throw new NotImplementedException ();
		}

		public void RefreshSongs ()
		{
			//throw new NotImplementedException ();
		}

		public void RefreshArtists ()
		{
			//throw new NotImplementedException ();
		}

		public void RefreshGenre ()
		{
			//throw new NotImplementedException ();
		}

		public void RefreshAlbum ()
		{
			//throw new NotImplementedException ();
		}

		public void RefreshPlaylist ()
		{
			//throw new NotImplementedException ();
		}

		public void UpdateSongProgress (float percent)
		{
			//throw new NotImplementedException ();
		}

		public void UpdatePlaylistProgress (float percent)
		{
			//throw new NotImplementedException ();
		}

		public void UpdateCurrentSongDownloadProgress (float percent)
		{
			//throw new NotImplementedException ();
		}

		public void UpdateMeter ()
		{
			//throw new NotImplementedException ();
		}

		public void GoToArtist (int artistId)
		{
			//throw new NotImplementedException ();
		}

		public void GoToAlbum (int albumId)
		{
			//throw new NotImplementedException ();
		}

		public void GoToGenre (int genreId)
		{
			//throw new NotImplementedException ();
		}

		public void ToggleMenu ()
		{
			Toggle ();
			//throw new NotImplementedException ();
		}

		public void DownloaderUpdated ()
		{
			//throw new NotImplementedException ();
		}
		#endregion
	}
}

