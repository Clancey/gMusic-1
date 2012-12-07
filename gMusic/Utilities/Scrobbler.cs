using System;
//using Lastfm;
//using Lastfm.Scrobbling;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleMusic
{
	public class Scrobbler
	{
		const string API_KEY = "ed2f8853c7675a00d3259318d6e1e3c5";
		const string API_SECRET = "bc852d2a7b618585e02d5c596d7ef063";

		static Scrobbler ()
		{
//			Main = new Scrobbler ();	
//			if (Settings.LastFmEnabled) {
//				Main.session = new Session (API_KEY, API_SECRET, Settings.LastFmSession);
//				Main.connection = new Connection (Main.session);
//				Main.manager = new ScrobbleManager (Main.connection);
//			}
		}

		public static Scrobbler Main;

		public Scrobbler ()
		{
		}

//		Session session;
//		Connection connection;
//		ScrobbleManager manager;

		public bool Login (string username, string md5password)
		{
			return this.Login (username, md5password, true);
		}

		public bool Login (string username, string md5password, bool shouldAlert)
		{
//			try {
//				session = new Session (API_KEY, API_SECRET);
//				session.Authenticate (username, md5password);
//				if (session.Authenticated) {
//					connection = new Connection (session);
//					manager = new ScrobbleManager (connection);
//					Settings.LastFmEnabled = true;
//					Settings.LastFmLoggedIn = true;
//					Settings.LastFmKey = md5password;
//					Settings.LastFmUserName = username;
//					Settings.LastFmSession = session.SessionKey;					
//					Main.session = new Session (API_KEY, API_SECRET, Settings.LastFmSession);
//					Main.connection = new Connection (Main.session);
//					Main.manager = new ScrobbleManager (Main.connection);
//				} else {
//					Settings.LastFmEnabled = false;
//					Settings.LastFmLoggedIn = false;
//					Settings.LastFmKey = "";
//					Settings.LastFmUserName = username;
//					Settings.LastFmSession = "";
//				}
//				return session.Authenticated;
//			} catch (Exception ex) {
//				if (shouldAlert)
//					Util.EnsureInvokedOnMainThread (delegate{
//						Util.ShowMessage("Last.fm", ex.Message,"Ok".Translate());
//					});
//				return false;	
//			}
			return false;
		}
		
		public void NowPlaying (Song song)
		{
//			//Facebook.NowPlaying(song);
//			if (session == null || manager == null)
//				return;
//			Task.Factory.StartNew (delegate {
//				nowPlaying(song);
//			}).Start ();
		}
		void nowPlaying(Song song)
		{
//			try {
//				if(!Main.session.Authenticated)
//					this.Login (Settings.LastFmUserName, Settings.LastFmKey, false);
//				manager.ReportNowplaying (new NowplayingTrack (song.Artist, song.Title, new TimeSpan (0, 0, 0, 0, song.Duration)));
//				}
//			catch (Exception ex) {
//				this.Login (Settings.LastFmUserName, Settings.LastFmKey, false);
//				try{
//					manager.ReportNowplaying (new NowplayingTrack (song.Artist, song.Title, new TimeSpan (0, 0, 0, 0, song.Duration)));
//				}
//				catch(Exception e)
//				{
//					Console.WriteLine (e);	
//				}
//			}
		}

		object locker = new object ();
		Song lastScrobbledSong;

		public void Scrobble (Song song)
		{
//			if (song == lastScrobbledSong)
//				return;
//			lastScrobbledSong = song;
//			if (session == null || manager == null)
//				return;
//		
//			Task.Factory.StartNew (delegate {
//				try {
//					manager.Queue (new Entry (song.Artist, song.Title, song.Album, DateTime.UtcNow, PlaybackSource.User, new TimeSpan (0, 0, 0, 0, song.Duration), ScrobbleMode.Played));
//				} catch (Exception ex) {
//					var auth = this.Login (Settings.LastFmUserName, Settings.LastFmKey, true);
//					Console.WriteLine (ex);	
//				}
//			}).Start ();

		}

		public void LoveSong (Song song)
		{
//			if (session == null || !session.Authenticated)
//				return;
//			Task.Factory.StartNew (delegate {
//				try {
//					var track = new Lastfm.Services.Track (song.Artist, song.Title, session);
//					track.Love ();
//				} catch (Exception ex) {
//					Console.WriteLine (ex);
//					//FlurryAnalytics.FlurryAnalytics.LogError(ex.Source,ex.Message,new NSException());
//				}
//			}).Start ();
		}
		
		public void UnLoveSong (Song song)
		{
//			if (session == null || !session.Authenticated)
//				return;
//			Task.Factory.StartNew (delegate {
//				try {
//					var track = new Lastfm.Services.Track (song.Artist, song.Title, session);
//					track.UnLove ();
//				} catch (Exception ex) {
//					Console.WriteLine (ex);
//					//FlurryAnalytics.FlurryAnalytics.LogError(ex.Source,ex.Message,new NSException());
//				}
//			}).Start ();
		}
		
	}
}

