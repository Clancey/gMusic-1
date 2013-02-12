
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
	public class MainActivity : BaseActivity, IMainViewController, IFragmentSwitcher
	{
	
		protected Fragment mContent;
		static bool isInitialized;
		static SlidingMenu Menu;
		public override void OnCreate (Bundle savedInstanceState)
		{
			Util.MainVC = this;
			base.OnCreate (savedInstanceState);
			if (!isInitialized) {
				Images.Init (Resources);
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
				isInitialized = true;
			}
			var MenuItems = new MenuItem[]{
				new MenuItem{
					Title = "Now Playing",
					Content = new NowPlayingViewController(),
				},
				new MenuItem{
					Title = "Songs",
					Content = new UINavigationController(new GoogleMusic.SongViewController()),
				},
				new MenuItem{
					Title = "Artists",
					Content = new UINavigationController(new ArtistViewController()),
				},
				new MenuItem{
					Title = "Albums",
					Content = new UINavigationController(new AlbumViewController()),
				},
				new MenuItem{
					Title = "Genres",
					Content = new UINavigationController(new GenreViewController()),
				},
				new MenuItem{
					Title = "Playlists",
					Content = new UINavigationController(new PlaylistViewController()),
				},
				new MenuItem{
					Title = "Auto Playlists",
					Content = new UINavigationController(new PlaylistViewController(true)),
				},
			};
			if(Menu == null)
				Menu = new SlidingMenu (MenuItems);
			// set the Above View
//			if (savedInstanceState != null)
//				mContent = FragmentManager.GetFragment (savedInstanceState, "mContent");
			if (mContent == null)
				mContent = Menu.MenuItems[Menu.CurrentIndex].Content;	
			
			// set the Above View
			this.SetContentView (Resource.Layout.content_frame);
			FragmentManager.BeginTransaction ().Replace (Resource.Id.content_frame, mContent).Commit ();

			
			// set the Behind View
			SetBehindContentView (Resource.Layout.menu_frame);

			
			FragmentManager.BeginTransaction ().Replace (Resource.Id.menu_frame, Menu).Commit ();


		}
		public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back) {
				if(mContent is IViewController)
				{
					if((mContent as IViewController).NavigationController.PopViewController(true))
						return true;
				}
			}
			return base.OnKeyDown (keyCode, e);
		}
		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			FragmentManager.PutFragment(outState, "mContent", mContent);
//			if (mContent is ListFragment) {
//				((ListFragment)mContent).ListView.FirstVisiblePosition
//			}
		}

		public void SwitchContent (Fragment fragment)
		{
			SwitchContent (fragment, false);
		}

		public void SwitchContent (Fragment fragment, bool animated, bool removed = false)
		{
			mContent = fragment;
			var ft = FragmentManager.BeginTransaction ();
			if (animated) {
				if(removed)
					ft.SetCustomAnimations (Resource.Animation.slide_in_left, Resource.Animation.slide_out_right);
				else
					ft.SetCustomAnimations (Resource.Animation.slide_in_right, Resource.Animation.slide_out_left);

			}
			ft.Replace (Resource.Id.content_frame, fragment).Commit ();
			SlidingMenu.ShowContent ();
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

