
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
	

	[Service]
	[IntentFilter(new String[]{"com.iis.musicService"})]
	public class MainService : IntentService, IMainViewController
	{
		IBinder binder;
		public static bool IsRunning = false;
		public override void OnCreate ()
		{
			base.OnCreate ();
			IsRunning = true;
			Util.MainVC = this;
			Settings.UserName = "james.clancey@gmail.com";
			if (!string.IsNullOrEmpty (Settings.UserName))
				Database.SetDatabase (Settings.UserName);
			Task.Factory.StartNew(delegate{
				Database.SetDatabase ("james.clancey@gmail.com");
				Util.LoadData();
				if (Util.Api == null)
					Util.Api = new GoogleMusicApi ("james.clancey@gmail.com");
				Util.Api.SignIn ("james.clancey@gmail.com", "Tng4life!", (success) =>{
					Settings.UserName = "james.clancey@gmail.com";
					Console.WriteLine(success);


				});
			});
//			if (string.IsNullOrEmpty (Settings.UserName) || string.IsNullOrEmpty(Settings.Key))
//			{
//				ShowLogin();
//				return;
//			}
//			

		}
		internal const string MessageSent = "MessageSent";
		internal const string SongsUpdated = "SongsUpdated";
		public override void OnDestroy ()
		{
			base.OnDestroy ();
			IsRunning = false;
		}
		protected override void OnHandleIntent (Intent intent)
		{
			var stocksIntent = new Intent (MessageSent);
			stocksIntent.PutExtra (MessageSent, "test");
			
			SendOrderedBroadcast (stocksIntent, null);
		}

		void broadcast(string action)
		{
			var intent = new Intent (action);
			SendOrderedBroadcast (intent, null);
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
			broadcast (SongsUpdated);
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
			//throw new NotImplementedException ();
		}
		public void DownloaderUpdated ()
		{
			//throw new NotImplementedException ();
		}
		#endregion
	}
}

