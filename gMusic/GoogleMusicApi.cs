using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

//using System.Json;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using SQLite;
// using ClanceysLib;
#if iOS
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
#endif

using System.Threading;

//using ServiceStack.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Json;
using System.Diagnostics;

namespace GoogleMusic
{
	public class GoogleMusicApi : Api
	{
		string sCookie = "";
		string sessionId;
		public GoogleMusicApi (string userName) : base (userName)
		{
			serviceRoot = GoogleServiceConfig.Default.ServiceRoot;
		}
	
		string serviceRoot = "https://play.google.com/music/services/";
		object locker = new object ();

		public override void ClearCookies ()
		{
			sCookie = "";
			base.ClearCookies ();
		}
		
		#region implemented abstract members of GoogleMusic.Api
		public override void SignIn (string email, string password, Action<bool> signedIn)
		{
			Util.PushNetworkActive ();
			//try {
				var success = signIn (email, password);
				Util.PopNetworkActive ();	
				if (signedIn != null)
					signedIn (success);
			//} catch (Exception ex) {
				Util.PopNetworkActive ();	
			//	throw ex;
			//}
		}

		bool authHtml;

		public bool signIn (string email, string password)
		{
			Database.SetDatabase (email);

			//return;
			if (HasCookie () && !string.IsNullOrEmpty (Settings.Auth)) {
				try {
					
					Requests.HttpsGet (GoogleServiceConfig.Default.MainUrl, "", new Dictionary<string,string> (){
						{"Authorization","GoogleLogin auth=" + Settings.Auth}});
					bool isLoggedIn = GetSongsIfNeeded ();
					if (isLoggedIn) {
						authHtml = true;
						Util.SetCrashlyticsData();
						return true;
					} else {
						ClearCookies ();
						SaveCookies ();
					}
				} catch (Exception ex) {
					Console.WriteLine(ex);
					ClearCookies ();				
				}
			}
			Requests.HttpsGet (GoogleServiceConfig.Default.MainUrl, "", null);

	
			var loginDictionary = GoogleServiceConfig.Default.LoginParameters.ToDictionary (x => x.Key, x => x.Value);
			loginDictionary.Add ("Email", email);
			loginDictionary.Add ("Passwd", password);
			var html = Requests.HttpsPost (GoogleServiceConfig.Default.LoginUrl, loginDictionary, Requests.DefaultForm (""));
			if (html.Contains ("Error")) {
				var err = getHtmlValue (html, "Error");
				if (err.Contains ("CaptchaRequired")) {
					Util.OpenUrl ("https://www.google.com/accounts/DisplayUnlockCaptcha");
					return false;
				} else
					throw new Exception (err);
			}
			Settings.Auth = getHtmlValue (html, "Auth");

			var sid = getHtmlValue (html, "SID");
			cookies.Add(new System.Net.Cookie("SID", sid, "/music/", "play.google.com"));
			Requests.HttpsGet (GoogleServiceConfig.Default.MainUrl, "", new Dictionary<string,string> (){
				{"Authorization","GoogleLogin auth=" + Settings.Auth}});
			
			//sCookie = GetSessionCookie (GoogleServiceConfig.Default.MainUrl);
			
			IsSignedIn = HasCookie ();
			if (IsSignedIn) {
				authHtml = true;
				user = email;
				SaveCookies ();
			}
			Console.WriteLine (IsSignedIn);
			IsSignedIn = GetSongsIfNeeded ();
			Util.SetCrashlyticsData();
			return IsSignedIn;
			
		}
		string GetSessionCookie (string url)
		{
			var req = Requests.CreateRequest (url, "");
			var resp = (HttpWebResponse)req.GetResponse ();
			foreach (var cookie in resp.Cookies) {
				var cookieString = cookie.ToString ();
				if (cookieString.Contains ("xt="))
					return cookieString;
			}
			return "";
		}
		private void checkAuthHtml ()
		{
			if (authHtml)
				return;
			Requests.HttpsGet (GoogleServiceConfig.Default.MainUrl, "", new Dictionary<string,string> (){
						{"Authorization","GoogleLogin auth=" + Settings.Auth}});
			authHtml = true;
		}

		private string getHtmlValue (string html, string name)
		{
			try {
				var strings = html.Split (new string[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var theString in strings) {
					var NameValue = theString.Split (new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
					if (NameValue [0] == name)
						return NameValue [1];
				}
				return "";
			} catch (Exception ex) {
				Console.WriteLine(ex);
				return "";
			}
		}
		
		private bool HasCookie ()
		{
			try {

				var foundCookies = cookies.Cookies().Where (x => x.Name == "xt").ToList ();
				if (foundCookies.Count == 0)
					return false;
				string cookie;
				var mcookie = foundCookies.Where (x => x.Path == "/music/services").FirstOrDefault () ?? foundCookies[0];
				cookie = "xt=" + mcookie.Value;
				if (!string.IsNullOrEmpty (cookie)) {
					sCookie = cookie;
					SetSessionID();
					return true;
				}
				
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			
			return false;
		}
		void SetSessionID()
		{
			var guid = Guid.NewGuid ();
			sessionId  = guid.ToString ().Split (new char[]{"-".First()}).Last ();
			Console.WriteLine (sessionId);
		}
		
		private void SetAuthCookie ()
		{
			var cookies = CookieList(GoogleServiceConfig.Default.ServiceRoot).Where (x => x.Name == "SID").ToList ();
			foreach (var cookie in cookies) {
				cookies.Add(new Cookie("SID", cookie.Value, "/music/listen", "play.google.com"));
				cookies.Add(new Cookie("SID", cookie.Value, "/music/services", "play.google.com"));
			}
			
		}

		private string GetCookie (string cookieName)
		{
			try {
				var cookies =  CookieList(GoogleServiceConfig.Default.ServiceRoot).Where (x => x.Name == cookieName).ToList ();
				if (cookies.Count == 0)
					return "";
				return cookies [0].Value;
				
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return "";
			}
			
		}
		
		public override bool GetSongsIfNeeded ()
		{
			return GetSongsIfNeeded (null);
		}

		private bool isGettingSongs;

		public override bool GetSongsIfNeeded (Action Success)
		{
			lock (locker) {
				if (isGettingSongs) {
					if (Success != null)
						Success ();
					return true;
				}
			}
			
			if (gettingSongsTask == 0) {
				gettingSongsTask = Util.BeginBackgroundTask (delegate {
					Console.WriteLine ("Out of time getting songs");
				});
			}
			Console.WriteLine ("Get Songs if Needed");
			Util.PushNetworkActive ();
			string status = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.StatusPath + sCookie, "{\"sessionId\":\""+ sessionId + "\"}", Requests.DefaultHeaders ());
			//Console.WriteLine(status);
			//string lastUpdate = Requests.HttpsPost (serviceRoot + "loadalltracks?u=0&" + sCookie, "json={\"lastUpdated\":1330392568040000}", Requests.DefaultHeaders ());
			Util.PopNetworkActive ();
			try {

				var jo = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (status);
				if (jo.ContainsKey (GoogleServiceConfig.Default.StatusRoot) && !jo [GoogleServiceConfig.Default.StatusRoot]) {
					if (gettingSongsTask != 0)
						Util.EndBackgroundTask (gettingSongsTask);
					gettingSongsTask = 0;
					return false;
				}	
				if (jo.ContainsKey (GoogleServiceConfig.Default.StatusAvailableTracks))
					Settings.AvailableSongs = jo [GoogleServiceConfig.Default.StatusAvailableTracks];
				else {
					if (gettingSongsTask != 0)
						Util.EndBackgroundTask (gettingSongsTask);
					gettingSongsTask = 0;
					return false;
				}
				//Console.WriteLine("status: " + jo);
				IsSignedIn = true;
				if (Settings.AvailableSongs != Settings.SongsCount || Settings.OverrideSync) {
					Console.WriteLine ("I need to get songs!");
					Console.WriteLine ("Available Songs :" + Settings.AvailableSongs);
					Console.WriteLine ("Sounds Count :" + Settings.SongsCount);
					Console.WriteLine ("Override :" + Settings.OverrideSync);
					
					Settings.OverrideSync = false;
					GetSongs (Success);
					Settings.lastResyncVersion = Util.CurrentVersion;
				} else					
					GetPlaylists (Success);
				if (gettingSongsTask != 0)
					Util.EndBackgroundTask (gettingSongsTask);
				gettingSongsTask = 0;
				return true;
			} catch (Exception ex) {
				if (Success != null)
					Success ();
				Console.WriteLine (ex);
			}
			return false;
		}

		Action getSongsComplete;
		int gettingSongsTask = 0;

		private void GetSongs (Action Success)
		{
			if (isGettingSongs) {
				if (Success != null)
					Success ();
				return;
			}
			getSongsComplete = Success;
			isGettingSongs = true;
			if (gettingSongsTask == 0)
				gettingSongsTask = Util.BeginBackgroundTask (delegate {
					Console.WriteLine ("Syncing out of time.");	
				});
			new Thread (getSongs).Start ();
		}
		
		List<Artist> ArtistsToInsert = new List<Artist> ();
		List<Album> AlbumsToInsert = new List<Album> ();
		int NextAlbumId = 1;
		List<Song> SongsToInsert = new List<Song> ();
		int NextGenreId = 1;
		List<Genre> GenresToInsert = new List<Genre> ();
		bool showStatus = false;

		int songsImported = 0;
		float progress = 1;

		private void getSongs ()
		{
			try {
				songsImported = 0;
				NextAlbumId = Util.AlbumsIdsDict.Count == 0 ? 1 : Util.AlbumsIdsDict.Max (x => x.Value) + 1;
				NextArtistId = Util.ArtistIdsDict.Count == 0 ? 1 : Util.ArtistIdsDict.Max (x => x.Value) + 1;
				NextGenreId = Util.GenresIdsDict.Count == 0 ? 1 : Util.GenresIdsDict.Max (x => x.Value) + 1;
				Settings.CurrentSyncSong = 0;
				if (Settings.SongsCount == 0)
					showStatus = true;
				if (showStatus) {
					Util.MainVC.ShowStatus("Loading Library".Translate());
				}
				FlurryAnalytics.FlurryAnalytics.LogEvent ("Importing Songs", true);
				var start = DateTime.Now;
				while (GetMoreSongs())
					Console.WriteLine ("Getting songs");
				var ended = DateTime.Now - start;
				Util.MainVC.UpdateSongProgress (1f);
				//Util.Util.EnsureInvokedOnMainThread (delegate{
				//	var alert = new UIAlertView ("Completed", "Finished parsing in " + ended.TotalSeconds, null, "Ok");
				//	alert.Show ();
				//});
				
				getPlaylists();
				Console.WriteLine("**************************************");
				Console.WriteLine("**************************************");
				Console.WriteLine("**************************************");
				Console.WriteLine("**************************************");
				Console.WriteLine (string.Format("Finished parsing in {0} seconds",ended.TotalSeconds));
				Console.WriteLine("**************************************");
				Console.WriteLine("**************************************");
				Console.WriteLine("**************************************");
				Console.WriteLine("**************************************");
				//FlurryAnalytics.FlurryAnalytics.EndTimedEvent ("Importing Songs", NSDictionary.FromObjectsAndKeys (new string[]{"Number of Songs"}, new string[]{ ended.TotalSeconds.ToString ()}));
				Util.LoadData ();
				if (showStatus)
					Database.Main.ResetOffline ();
				isGettingSongs = false;
				bool hasGottenSongs = false;
				if (Settings.AvailableSongs > Settings.SongsCount && !hasGottenSongs) {
					Settings.LastUpdateRequest = "";
					if (showStatus) {
						Util.MainVC.HideStatus();
						showStatus = false;
					}
					hasGottenSongs = true;
					GetSongs (getSongsComplete);
					return;
				}
				getPlaylists();
				if (getSongsComplete != null)
					getSongsComplete ();
				if (showStatus) {
					Util.MainVC.HideStatus();
				}
				Util.EnsureInvokedOnMainThread(delegate{
					if(showStatus)
					{
						Util.MainVC.HideStatus();
					}
					Util.MainVC.UpdateSongProgress (1f);
				});
				showStatus = false;
				if (gettingSongsTask != 0) {
					Util.EndBackgroundTask (gettingSongsTask);
					gettingSongsTask = 0;
				}
			} catch (Exception ex) {
				Util.MainVC.HideStatus();
				Console.WriteLine (ex);
				isGettingSongs = false;
				if (gettingSongsTask != 0) {
					Util.EndBackgroundTask (gettingSongsTask);
					gettingSongsTask = 0;
				}
			}
		}

		object songsLocker = new object ();
		bool isLoadingSongJson = false;

		private bool GetMoreSongs ()
		{
			try {
				string continuation = Settings.ContinuationToken;
				Util.PushNetworkActive ();
				string json = "{}";
				if (!string.IsNullOrEmpty (continuation))
					json = "{\"continuationToken\":" + continuation + "}";
				else if (!string.IsNullOrEmpty (Settings.LastUpdateRequest))
					json = "{\"lastUpdated\":" + Settings.LastUpdateRequest + "}";
				//else
				//	json = "{}";
				
				var newHtml = "";
				if (!isLoadingSongJson) {
					newHtml = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.TracksPath + sCookie, new Dictionary<string,string> (){{"json" , json}}
					, Requests.DefaultHeaders ());
				} else {
					lock (songsLocker) {
						if (string.IsNullOrEmpty (nextJson))
							Monitor.Wait (songsLocker);
						newHtml = nextJson;
						nextJson = "";
					}
				}
				Console.WriteLine ("Starting to parse");
				var o = JsonObject.Parse (newHtml);
				string continuationToken = "";
				if (o.ContainsKey (GoogleServiceConfig.Default.TrackContinuationToken)) {
					continuationToken = o [GoogleServiceConfig.Default.TrackContinuationToken].ToString ();
					isLoadingSongJson = true;
					new Thread (new ThreadStart (delegate {
						getSongJson (continuationToken);
					})).Start ();
				} else {
					isLoadingSongJson = false;
				}
				if (o.ContainsKey (GoogleServiceConfig.Default.TrackRequestTime))
					Settings.LastUpdateRequest = o [GoogleServiceConfig.Default.TrackRequestTime].ToString ();
					
				//var array = JsonArrayObjects.Parse (o ["playlist"]);
				if (o.ContainsKey (GoogleServiceConfig.Default.TrackRoot))
					foreach (var key in o[GoogleServiceConfig.Default.TrackRoot]) {
						songsImported ++;
						ParseSong ((JsonObject)key);	
					}
				Console.WriteLine ("inserting to database");
				lock (Database.DatabaseLocker) {
					Database.Main.InsertAll (ArtistsToInsert, "OR REPLACE");
					Database.Main.InsertAll (AlbumsToInsert, "OR REPLACE");
					Database.Main.InsertAll (SongsToInsert, "OR REPLACE");
					Database.Main.InsertAll (GenresToInsert, "OR REPLACE");
				}
				Console.WriteLine ("inserting to database complete");
				ArtistsToInsert.Clear ();
				AlbumsToInsert.Clear ();
				SongsToInsert.Clear ();	
				GenresToInsert.Clear ();
				
				Settings.ContinuationToken = continuationToken;
				Util.PopNetworkActive ();
				Util.EnsureInvokedOnMainThread(delegate{
					//if(showStatus)
						Util.MainVC.UpdateStatus(progress);
					//Util.MainVC.UpdateSongProgress (progress);
				});
			
				return !string.IsNullOrEmpty (continuationToken);
			} catch (Exception ex) {
				Console.WriteLine (ex);	
				
				Util.PopNetworkActive ();
				
				return true;
				//return Settings.ContinuationToken;
			}
		}

		string nextJson = "";

		private void getSongJson (string contToken)
		{
			lock (songsLocker) {
				string json = "{}";
				if (!string.IsNullOrEmpty (contToken))
					json = "{\"continuationToken\":" + contToken + "}";
				nextJson = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.TracksPath + sCookie, new Dictionary<string,string> (){{"json" , json}}
					, Requests.DefaultHeaders ());
				Monitor.Pulse (songsLocker);
				//isLoadingSongJson = false;
			}
		}
		
		public void ParseSong (JsonObject d)
		{
			try {
				string genreName = "";
				string albumArtist = "";
				string artistName = "";
				string songId = "";
				string artistNorm = "";
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackId))
					songId = d [GoogleServiceConfig.Default.TrackId];	
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackDeleted)) {
					if ((bool)d [GoogleServiceConfig.Default.TrackDeleted]) {
						lock (Database.DatabaseLocker) {
							var deleted = Database.Main.Execute ("delete from Song where Id = '" + songId + "'");
							Console.WriteLine (deleted);
						}
						return;
					}
				}
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackAlbumArtist))
					albumArtist = ((string)d [GoogleServiceConfig.Default.TrackAlbumArtist]).Trim ();
				string albumArtistNameNorm = "";
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackAlbumArtistNorm))
					albumArtistNameNorm = ((string)d [GoogleServiceConfig.Default.TrackAlbumArtistNorm]).Trim ();
				
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackArtist))
					artistName = ((string)d [GoogleServiceConfig.Default.TrackArtist]).Trim ();
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackArtistNorm))
					artistNorm = ((string)d [GoogleServiceConfig.Default.TrackArtistNorm]).Trim ();
				if (string.IsNullOrEmpty (albumArtist)) {
					albumArtist = artistName;
					albumArtistNameNorm = artistNorm;
				}
				albumArtistNameNorm = albumArtistNameNorm.Replace ("the ", "");
				artistNorm = artistNorm.Replace ("the ", "");
				string albumName = "";
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackAlbum))
					albumName = ((string)d [GoogleServiceConfig.Default.TrackAlbum]).Trim ();
				string albumNameNorm = "";
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackAlbumNorm))
					albumNameNorm = ((string)d [GoogleServiceConfig.Default.TrackAlbumNorm]).Trim ();
				albumNameNorm = albumNameNorm.Replace ("the ", "");
				string albumUrl = "";
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackGenre))
					genreName = ((string)d [GoogleServiceConfig.Default.TrackGenre]).Trim ();	
				
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackAlbumArtUrl))
					albumUrl = d [GoogleServiceConfig.Default.TrackAlbumArtUrl];
				Song song = new Song ();
				if (Util.ArtistIdsDict.ContainsKey (albumArtistNameNorm)) {
					song.ArtistId = Util.ArtistIdsDict [albumArtistNameNorm];
				} else {
					var artist = new Artist{Id = NextArtistId,Name = albumArtist,NormName = albumArtistNameNorm,IndexCharacter = Util.GetIndexChar (albumArtistNameNorm)};
					song.ArtistId = NextArtistId;
					Util.ArtistIdsDict.Add (albumArtistNameNorm, song.ArtistId);
					NextArtistId ++;
					ArtistsToInsert.Add (artist);
					//lock(Database.DatabaseLocker)Database.Main.Insert(artist,"OR REPLACE");
				}
				//if(string.IsNullOrEmpty(albumNameNorm))
				//	Console.WriteLine("test");
				var albumTuple = new Tuple<int,string> (song.ArtistId, albumNameNorm);
				if (Util.AlbumsIdsDict.ContainsKey (albumTuple)) {
					song.AlbumId = Util.AlbumsIdsDict [albumTuple];
				} else if (!string.IsNullOrEmpty (albumNameNorm)) {
					var album = new Album{Id = NextAlbumId,ArtistId = song.ArtistId,Name = albumName,NameNorm = albumNameNorm,Url = albumUrl,IndexCharacter = Util.GetIndexChar (albumNameNorm)};
					song.AlbumId = album.Id;
					AlbumsToInsert.Add (album);
					//lock(Database.DatabaseLocker)Database.Main.Insert (album,"OR REPLACE");
					Util.AlbumsIdsDict.Add (albumTuple, album.Id);
					NextAlbumId ++;
				}
				if (Util.GenresIdsDict.ContainsKey (genreName)) {
					song.GenreId = Util.GenresIdsDict [genreName];
				} else {
					var genre = new Genre{Id = NextGenreId,Name = genreName,IndexCharacter = Util.GetIndexChar (genreName)};
					song.GenreId = NextGenreId;
					Util.GenresIdsDict.Add (genreName, NextGenreId);
					NextGenreId ++;
					GenresToInsert.Add (genre);
					//lock(Database.DatabaseLocker)Database.Main.Insert (genre,"OR REPLACE");
				}
				song.Id = songId;
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackName)) {
					song.Title = ((string)d [GoogleServiceConfig.Default.TrackName]).Trim ();
					song.TitleNorm = song.Title.ToLower ().Replace ("the ", "");
					song.IndexCharacter = Util.GetIndexChar (song.TitleNorm);
				}
				song.AlbumUrl = albumUrl;
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackDisc))
					song.Disc = d [GoogleServiceConfig.Default.TrackDisc];
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackTrack))
					song.Track = d [GoogleServiceConfig.Default.TrackTrack];
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackDuration))
					song.Duration = d [GoogleServiceConfig.Default.TrackDuration];
				if (d.ContainsKey (GoogleServiceConfig.Default.TrackRating))
					song.Rating = d [GoogleServiceConfig.Default.TrackRating];
				song.Artist = artistName;
				song.Album = albumName;
				SongsToInsert.Add (song);
				//lock(Database.DatabaseLocker)Database.Main.Insert (song,"OR REPLACE");
				Settings.CurrentSyncSong ++;
				progress = (float)((double)Settings.CurrentSyncSong / (double)Settings.AvailableSongs);
				//Util.AppDelegate.MainVC.UpdateSongProgress (progress);
				//if (showStatus)
				//	statusHud.Progress = progress;
			} catch (Exception ex) {
				Console.WriteLine (ex);	
			}

		}
				
		public static string getValue (string name, string html)
		{
			var dict = html.Split (new string[]{"\n","="}, StringSplitOptions.None).ToList ();
			return dict [dict.IndexOf (name) + 1];
		}
		
		public override HttpWebRequest GetSongWebRequest (string url)
		{
			return new HttpWebRequest (new Uri (url));
		}
		#endregion

		private void GetPlaylists ()
		{
			GetPlaylists (null);	
		}

		bool isGettingPlaylist;
		Action getPlaylistCompleted;

		private void GetPlaylists (Action completed)
		{
			lock (locker) {
				if (isGettingPlaylist) {
					if (completed != null)
						completed ();
					return;
				}
				isGettingPlaylist = true;	
			}
			
			getPlaylistCompleted = completed;
			Task.Factory.StartNew(getPlaylists);
		}
		
		private void getPlaylists ()
		{

				try {
					Util.PushNetworkActive ();
					//Util.Util.EnsureInvokedOnMainThread (delegate {
					FlurryAnalytics.FlurryAnalytics.LogEvent ("Get Playlists", true);
					//});
					//lock (Database.Main)
					lock (Database.DatabaseLocker) {
						Database.Main.Execute ("Update Playlist set Deleted = 'true'");
						//Database.Main.Execute ("Update PlaylistSongs set Deleted = 'true'");
					}
					//GetMorePlaylist ("");
					GetRecentTracks ();
					GetFreePlaylist ();
					GetThumbsUp ();
					
					string playlistCont = "-1";
					while (!string.IsNullOrEmpty(playlistCont)) {
						Console.WriteLine ("continuation : " + playlistCont);
						playlistCont = GetMorePlaylist (playlistCont);
					}
					lock (Database.DatabaseLocker) {
						//Database.Main.Execute ("Delete from PlaylistSongs where Deleted = 'true'");
						Database.Main.Execute ("Delete from Playlist where Deleted = 'true'");
					}
					Database.Main.UpdatePlaylistOfflineCount ();
					Util.UpdatePlaylist (true);
				} catch (Exception ex) {
					Console.WriteLine (ex);
				} finally {
					//Util.Util.EnsureInvokedOnMainThread (delegate {
					//FlurryAnalytics.FlurryAnalytics.EndTimedEvent ("Get Playlists", NSDictionary.FromObjectAndKey (new NSString (Util.PlaylistsList.Count ().ToString ()), new NSString ("Total Playlists")));
					//});
					isGettingPlaylist = false;
					//Util.UpdatePlaylist (true);
					if (getPlaylistCompleted != null)
						getPlaylistCompleted ();
					Util.PopNetworkActive ();	
				}

		}

		private string GetMorePlaylist (string continuation)
		{
			continuation = continuation == "-1" ? "" : continuation;

			string json = string.Empty;
			if (!string.IsNullOrEmpty (continuation))
				json = "{\"continuationToken\":\"" + continuation + "\"}";
			//else if (!string.IsNullOrEmpty (Settings.LastUpdateRequest))
			//	json = "{\"lastUpdated\":" + Settings.LastUpdateRequest + "}";
			//else
			//	json = "{\"lastUpdated\":1330392568040000}";
			
			var newHtml = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.PlaylistPath + sCookie, ""
				, Requests.DefaultHeaders ());
			
			
			string continuationToken = "";
			var d = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (newHtml);

			if (d.ContainsKey (GoogleServiceConfig.Default.PlaylistContinuation))
				continuationToken = d [GoogleServiceConfig.Default.PlaylistContinuation];
			
			if (d.ContainsKey (GoogleServiceConfig.Default.PlaylistMagicRoot)) {
				foreach (var key in d[GoogleServiceConfig.Default.PlaylistMagicRoot]) {
					var p = (System.Json.JsonObject)key;
					ParsePlaylist (p, false, false);
				}
			}
			if (d.ContainsKey (GoogleServiceConfig.Default.PlaylistRoot)) {
				foreach (var key in d[GoogleServiceConfig.Default.PlaylistRoot]) {
					var p = (System.Json.JsonObject)key;
					ParsePlaylist (p, true, false);
				}
			}	
			return continuationToken;
		}
		
		private void GetRecentTracks ()
		{
			GetPlaylistsByID ("Last added".Translate(), GoogleServiceConfig.Default.PlaylistLastLoaded);
		}
		
		private void GetFreePlaylist ()
		{
			GetPlaylistsByID ("Free and purchased".Translate(), GoogleServiceConfig.Default.PlaylistFree);
		}
		
		private void GetThumbsUp ()
		{
			GetPlaylistsByID ("Thumbs up".Translate(), GoogleServiceConfig.Default.PlaylistThumbsUp);
		}
		
		private void GetPlaylistsByID (string name, string id)
		{
			string json = "{\"id\":\"" + id + "\"}";
			
			var newHtml = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.PlaylistPath + sCookie, new Dictionary<string,string> () {
					{"json",json}
				}, Requests.DefaultHeaders ());
			JsonObject d = (JsonObject)System.Json.JsonObject.Parse (newHtml);
			ParsePlaylist (d, false, true, name);
		}
		
		public override void DeleteAlbum (Album album, Action<bool> OnComplete)
		{
			/*
			var artistId = album.ArtistId;
			var albumId = album.Id;
			Song[] songs;
			lock (Database.Main)
				songs = Database.Main.Table<Song> ().Where (x => x.AlbumId == albumId).ToArray ();
			DeleteSongs (songs, (complete) => { 
				//if (complete)
				//	Util.UpdateAlbums (false, true);				
				Util.EnsureInvokedOnMainThread (delegate {
					if (OnComplete != null)							
						OnComplete (complete);
				});
			
			});
			*/
		}
		
		public override void DeleteArtist (Artist artist, Action<bool> OnComplete)
		{
			/*
			var artistId = artist.Id;
			Song[] songs;
			lock (Database.Main)
				songs = Database.Main.Table<Song> ().Where (x => x.ArtistId == artistId).ToArray ();
			DeleteSongs (songs, (complete) => { 
				//if (complete)
				//	Util.UpdateArtist (false, true);
				
				Util.EnsureInvokedOnMainThread (delegate {
					if (OnComplete != null)							
						OnComplete (complete);
				});
			
			});
			*/
		}
		
		private void DeleteSongs (Song[] songs, Action<bool> OnComplete)
		{
			/*
			Util.ShowBlockAlert ("Deleting songs".Translate());
			ThreadPool.QueueUserWorkItem (delegate{
				bool succes;
				try {
					
					var songList = string.Join (",", songs.Select (x => "\"" + x.Id + "\"").ToArray ());
					
					//var old = "{\"songIds\":[\"5cb3bc92-e5a1-3213-977d-be99050052af\"],\"entryIds\":[\"\"],\"listId\":\"all\"}";
					var json = Requests.HttpsPost (serviceRoot + "deletesong?u=0&" + sCookie, new Dictionary<string,string> () {
						{"json","{\"songIds\":[\"" + songList + "\"],\"entryIds\":[\"\"],\"listId\":\"all\"}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey ("success"))
						succes = p ["success"];
					else if (p.ContainsKey ("deleteIds"))
						succes = true;
					else 
						succes = false;
					if (succes)
						lock (Database.Main) {
							foreach (var song in songs)
								Database.Main.Delete (song);
						}
					
				} catch (Exception ex) {
					Console.WriteLine ("Error removing from playlist:" + ex);
				} finally {
					OnComplete (succes);
					Util.HideBlockAlert();
				}
			});
			 */
		}
		
		public override void DeleteSong (Song song, Action<bool> OnComplete)
		{
			/*
			Util.ShowBlockAlert ("Deleting song".Translate());
			ThreadPool.QueueUserWorkItem (delegate{
				bool succes;
				try {
					
					var old = "{\"songIds\":[\"5cb3bc92-e5a1-3213-977d-be99050052af\"],\"entryIds\":[\"\"],\"listId\":\"all\"}";
					var json = Requests.HttpsPost (serviceRoot + "deletesong?u=0&" + sCookie, new Dictionary<string,string> () {
						{"json","{\"songIds\":[\"" + song.Id + "\"],\"entryIds\":[\"\"],\"listId\":\"all\"}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey ("deleteIds"))
						succes = true;
					else 
						succes = false;
					if (succes)
						lock (Database.Main)
							Database.Main.Delete (song);
					//Util.UpdateSongs (true, true);
				} catch (Exception ex) {
					Console.WriteLine ("Error removing from playlist:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread (delegate {
						if (OnComplete != null)
							OnComplete (succes);
					});
					progress.Hide (true);
				}
			});
			 */
		}
		
		public override void DeleteSongFromPlaylist (PlaylistSongs song, Action<bool> OnComplete)
		{
				//TODO: fix this message
//			MBProgressHUD progress = new MBProgressHUD (Util.WindowFrame);
//			progress.TitleText = "Removing song from playlist".Translate();
//			progress.Show (true);
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					//var old = "{\"songIds\":[\"8198e2f2-0d46-3f62-9854-a70d12e09c8f\"],\"entryIds\":[\"11bd74c1-5374-3bbf-bf47-437bc9adb4cb\"],\"listId\":\"300c8139-0b8c-4a0c-aa82-8dce4173c9db\"}";
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.PlistDeleteSongPath + sCookie, new Dictionary<string,string> () {
						{"json","{\"songIds\":[\"" + song.SongId + "\"],\"entryIds\":[\"" + song.EntryId + "\"],\"listId\":\"" + song.ServerPlaylistId + "\"}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.PlistDeleteRoot)) {
						var delId = (p ["deleteIds"] as JsonArray).First ().ToString ();
						succes = delId.Contains (song.SongId + "_" + song.EntryId);
					} else 
						succes = false;
					if (succes)
						lock (Database.DatabaseLocker) 
							Database.Main.Execute ("delete from PlaylistSongs where EntryId = ?", song.EntryId);
					
				} catch (Exception ex) {
					Console.WriteLine ("Error removing from playlist:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread (delegate {
						if (OnComplete != null)
							OnComplete (succes);
					});
					//progress.Hide (true);
				}
			});
		}
		
		public override void EditSong (Song song, Action<bool> OnSuccess)
		{	
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					//var old = "{\"songIds\":[\"8198e2f2-0d46-3f62-9854-a70d12e09c8f\"],\"entryIds\":[\"11bd74c1-5374-3bbf-bf47-437bc9adb4cb\"],\"listId\":\"300c8139-0b8c-4a0c-aa82-8dce4173c9db\"}";
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.EditSongPath + sCookie, new Dictionary<string,string> () {
						{"json","{\"entries\":[{\"id\":\"" + song.Id + "\"," +
							"\"genre\":\"" + song.Genre + "\"," +
							"\"albumArtist\":\"" + song.AlbumArtist + "\"," +
							"\"albumArtistNorm\":\"" + song.AlbumArtist.ToLower () + "\"," +
							"\"artist\":\"" + song.Artist + "\"," +
							"\"artistNorm\":\"" + song.Artist.ToLower () + "\"," +
							"\"title\":\"" + song.Title + "\"," +
							"\"titleNorm\":\"" + song.Title.ToLower () + "\"," +
							"\"name\":\"" + song.Title + "\"," +
							"\"album\":\"" + song.Album + "\"," +
							"\"albumNorm\":\"" + song.Album.ToLower () + "\"," +
							"\"track\":" + song.Track + "," +
							"\"disc\":\"" + song.Disc + "\"}]}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.EditRoot)) {
						//var delId = (p ["deleteIds"] as JsonArray).First().ToString();
						succes = p [GoogleServiceConfig.Default.EditRoot];
						
						//var artistName = song.Artist.ToLower ().Replace ("the ", "");
						var albumName = song.Album.ToLower ().Replace ("the ", "");
						var albumArtist = song.AlbumArtist.ToLower ().Replace ("the ", "");
						if (Util.ArtistIdsDict.ContainsKey (albumArtist)) {
							song.ArtistId = Util.ArtistIdsDict [albumArtist];
						} else {
							var artist = new Artist{Id = Util.Api.NextArtistId,Name = song.AlbumArtist,NormName = albumArtist,IndexCharacter = Util.GetIndexChar (albumName)};
							song.ArtistId = Util.Api.NextArtistId;
							Util.ArtistIdsDict.Add (albumArtist, song.ArtistId);
							Util.Api.NextArtistId ++;
							lock (Database.DatabaseLocker)
								Database.Main.Insert (artist, "OR REPLACE");
						}
						
						var albumTuple = new Tuple<int,string> (song.ArtistId, albumName);
						if (Util.AlbumsIdsDict.ContainsKey (albumTuple)) {
							song.AlbumId = Util.AlbumsIdsDict [albumTuple];
						} else {
							string albumArtUrl = "";
							if (p.ContainsKey ("albumArtUrl"))
								albumArtUrl = p ["albumArtUrl"];
							var album = new Album{Id = NextAlbumId,ArtistId = song.ArtistId,Name = song.Album,NameNorm = albumName,Url = albumArtUrl,IndexCharacter = Util.GetIndexChar (albumName)};
							song.AlbumId = album.Id;
							//AlbumsToInsert.Add (album);
							lock (Database.DatabaseLocker)
								Database.Main.Insert (album, "OR REPLACE");
							Util.AlbumsIdsDict.Add (albumTuple, album.Id);
							NextAlbumId ++;
						}
						
						if (Util.GenresIdsDict.ContainsKey (song.Genre)) {
							song.GenreId = Util.GenresIdsDict [song.Genre];
						} else {
							var genre = new Genre{Id = NextGenreId,Name = song.Genre,IndexCharacter = Util.GetIndexChar (song.Genre)};
							song.GenreId = NextGenreId;
							Util.GenresIdsDict.Add (song.Genre, NextGenreId);
							NextGenreId ++;
							//GenresToInsert.Add (genre);
							lock (Database.DatabaseLocker)
								Database.Main.Insert (genre, "OR REPLACE");
						}
						
						
						
						
					} else 
						succes = false;
					
					
				} catch (Exception ex) {
					Console.WriteLine ("Error thumbs up:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread (delegate {
						if (OnSuccess != null)
							OnSuccess (succes);
					});
				}
			});
		}
		
		public override void ThumbsUp (Song song, Action<bool> OnSuccess)
		{	
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					//var old = "{\"songIds\":[\"8198e2f2-0d46-3f62-9854-a70d12e09c8f\"],\"entryIds\":[\"11bd74c1-5374-3bbf-bf47-437bc9adb4cb\"],\"listId\":\"300c8139-0b8c-4a0c-aa82-8dce4173c9db\"}";
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.EditSongPath + sCookie, new Dictionary<string,string> () {
						{"json","{\"entries\":[{\"id\":\"" + song.Id + "\",\"rating\":5,\"L\":\"" + song.Id + "\"}]}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.EditRoot)) {
						//var delId = (p ["deleteIds"] as JsonArray).First().ToString();
						succes = p [GoogleServiceConfig.Default.EditRoot];
					} else 
						succes = false;
					song.Rating = 5;
					
				} catch (Exception ex) {
					Console.WriteLine ("Error thumbs up:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread (delegate {
						if (OnSuccess != null)
							OnSuccess (succes);
					});
				}
			});
		}
		
		public override void ThumbsDown (Song song, Action<bool> OnSuccess)
		{
		
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					var values = "{\"entries\":[{\"id\":\"" + song.Id + "\",\"rating\":0,\"L\":\"" + song.Id + "\"}]}";
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.EditSongPath + sCookie, new Dictionary<string,string> () {
						{"json",values}
					}, Requests.DefaultHeaders ());
					
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.EditRoot)) {
						//var delId = (p ["deleteIds"] as JsonArray).First().ToString();
						succes = p [GoogleServiceConfig.Default.EditRoot];
					} else 
						succes = false;
					
				} catch (Exception ex) {
					Console.WriteLine ("Error thumbs down:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread(delegate {
						if (OnSuccess != null)
							OnSuccess (succes);
					});
				}
			});
		}
		
		public override void DeletePlaylist (Playlist playlist, Action<bool> OnComplete)
		{
							
//			MBProgressHUD progress = new MBProgressHUD (Util.WindowFrame);
//			progress.TitleText = "Deleting Playlist".Translate();
//			progress.Show (true);
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					//var old = "{\"songIds\":[\"8198e2f2-0d46-3f62-9854-a70d12e09c8f\"],\"entryIds\":[\"11bd74c1-5374-3bbf-bf47-437bc9adb4cb\"],\"listId\":\"300c8139-0b8c-4a0c-aa82-8dce4173c9db\"}";
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.PlaylistDeletePath + sCookie, new Dictionary<string,string> () {
						{"json","{\"id\":\"" + playlist.ServerId + "\"}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistDeletePathRoot))
						succes = p [GoogleServiceConfig.Default.PlaylistDeletePathRoot] == playlist.ServerId;
					else 
						succes = false;
					
					if (succes) {
						lock (Database.DatabaseLocker)
							Database.Main.Delete (playlist);
						Util.UpdatePlaylist (false);
					}
					
					
				} catch (Exception ex) {
					Console.WriteLine ("Error removing from playlist:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread (delegate {
						if (OnComplete != null)
							OnComplete (succes);
					});
//					progress.Hide (true);
				}
			});

		}
				
		public override void  AddToPlaylist (Playlist playlist, Genre genre, Action<bool> OnSuccess)
		{
			if (string.IsNullOrEmpty (playlist.ServerId)) {
				CreatePlaylist (playlist.Name, genre, OnSuccess);
				return;
			}
			
			var genreId = genre.Id;
			Song[] songs = null;
			lock (Database.DatabaseLocker) 
				songs = Database.Main.Query<Song> ("select id from song where GenreId = ?", genreId).ToArray ();	
			AddToPlaylist (playlist, songs, OnSuccess);
		}
		
		public override void  AddToPlaylist (Playlist playlist, Artist artist, Action<bool> OnSuccess)
		{
			if (string.IsNullOrEmpty (playlist.ServerId)) {
				CreatePlaylist (playlist.Name, artist, OnSuccess);
				return;
			}
			var artistId = artist.Id;
			Song[] songs = null;
			lock (Database.DatabaseLocker)
				songs = Database.Main.Query<Song> ("select id from song where ArtistId = ?", artistId).ToArray ();
			AddToPlaylist (playlist, songs, OnSuccess);
		}
		
		public override void AddToPlaylist (Playlist playlist, Album album, Action<bool> OnSuccess)
		{
			if (string.IsNullOrEmpty (playlist.ServerId)) {
				CreatePlaylist (playlist.Name, album, OnSuccess);
				return;
			}
			var artistId = album.ArtistId;
			var albumId = album.Id;
			Song[] songs;
			lock (Database.DatabaseLocker) {
				if (albumId == -1)
					songs = Database.Main.Query<Song> ("select id from song where artistId = ? order by track, disc", artistId).ToArray ();
				else
					songs = Database.Main.Query<Song> ("select id from song where AlbumId = ? order by track, disc", albumId).ToArray ();
			}
			AddToPlaylist (playlist, songs, OnSuccess);
		}
		

	

		public override void AddToPlaylist (Playlist playlist, Song[] songs, Action<bool> OnSuccess)
		{
			if (string.IsNullOrEmpty (playlist.ServerId)) {
				CreatePlaylist (playlist.Name, songs, OnSuccess);
				return;
			}
//			MBProgressHUD progress = new MBProgressHUD (Util.WindowFrame);
//			progress.TitleText = "Adding to Playlist".Translate();
//			progress.Show (true);
			ThreadPool.QueueUserWorkItem (delegate {

				bool succes = AddToPlaylist (playlist, songs.Select (x => x.Id).ToArray ());

				Util.EnsureInvokedOnMainThread (delegate {
					if (OnSuccess != null)
						OnSuccess (succes);
				});
//				progress.Hide (true);
			});
		}
		public bool AddToPlaylist(Playlist playlist,string[] songs)
		{
			bool succes = false;
			try {
				var newList = songs.ToList ();
				while (newList.Count > 0) {
					var smallSongList = newList.Take (25).ToList ();
					var songList = string.Join (",", smallSongList.Select (x => "\"" + x + "\"").ToArray ());
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.PlaylistAddPath + sCookie, new Dictionary<string,string> () {
						{"json","{\"playlistId\":\"" + playlist.ServerId + "\",\"songIds\":[" + songList + " ]}"}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistAddSuccess))
						succes = p [GoogleServiceConfig.Default.PlaylistAddSuccess];
					else 
						succes = true;
					if (succes) {
						
						if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistAddRoot)) {
							foreach (var key in p[GoogleServiceConfig.Default.PlaylistAddRoot]) {
								var s = (System.Json.JsonObject)key;
								
								
								string songId = "";
								if (s.ContainsKey (GoogleServiceConfig.Default.PlaylistSong))
									songId = s [GoogleServiceConfig.Default.PlaylistSong];
								else {
									succes = false;
									return false;
								}
								
								string plistEntryId = "";					
								if (s.ContainsKey (GoogleServiceConfig.Default.PlaylistEntry))
									plistEntryId = s [GoogleServiceConfig.Default.PlaylistEntry];
								var playlistId = playlist.ServerId;
								var theSong = new PlaylistSongs{ServerPlaylistId = playlistId,SongId = songId,EntryId = plistEntryId};
								lock (Database.DatabaseLocker) 
									Database.Main.Insert (theSong);
							}
						}
					}
					newList.RemoveRange (0, newList.Count >= 10 ? 10 : newList.Count); 
				}
			} catch (Exception ex) {
				Console.WriteLine ("Error adding songs to playlist" + ex);
			}
			return succes;

		}
		
		public override void MoveSong (PlaylistSongs song, string prev, string next, int position)
		{
			Util.ShowBlockAlert ("Moving song".Translate());
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					var values = "{\"playlistId\":\"" + song.ServerPlaylistId + "\",\"movedSongIds\":[\"" + song.SongId + "\"],\"movedEntryIds\":[\"" + song.EntryId + "\"],\"afterEntryId\":\"" + prev + "\",\"beforeEntryId\":\"" + next + "\"}";
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.PlaylistMovePath + sCookie, new Dictionary<string,string> () {
						{"json",values}
					}, Requests.DefaultHeaders ());
					
					var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
					if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistMoveRoot))
						succes = true;
					else 
						succes = false;

					//Util.UpdateSongs (true, true);
				} catch (Exception ex) {
					Console.WriteLine ("Error moving song in playlist:" + ex);
				} finally {
					Util.EnsureInvokedOnMainThread (delegate {
						if (!succes) {
							Util.ShowMessage ("Error".Translate(), "There was an error moving the song".Translate(),"Ok".Translate());
						}
					});
					Util.HideBlockAlert();
				}
			});
		}

		private bool hasNotified = false;
		string failedSongID = "";

		public override string GetSongUrl (string SongId)
		{
			checkAuthHtml ();
			var songUrl = Requests.HttpsGet (GoogleServiceConfig.Default.SongPlayUrl + SongId + GoogleServiceConfig.Default.SongPlayParam, "", Requests.DefaultHeaders ());
			if (string.IsNullOrEmpty (songUrl)) {
				if (!hasNotified && !Settings.ShowOfflineOnly) {
					hasNotified = true;
					Util.EnsureInvokedOnMainThread(delegate {
						Util.ShowMessage ("Error".Translate(), "There was an error communicating with the server. An internet connection is required in order to use gMusic.".Translate(),"Ok".Translate());
					});
				}
			}
			if (songUrl.Contains ("Forbidden") || songUrl.Contains ("Error 404")) {
				if (failedSongID != SongId) {
					Util.EnsureInvokedOnMainThread (delegate {
						string header = songUrl.Contains ("Error 404") ? "Missing Song".Translate() : "Unable to play music".Translate();
						var song = Database.Main.GetObject<Song>(SongId);
						string message = songUrl.Contains ("Error 404") ? "The song could not be located on the server.".Translate() + "\r\n" + song.ToString () : "Your music is temporarily unavailable from this device. Please try again later.".Translate();
						Util.ShowMessage (header, message,"Ok".Translate());
					});
				}
				failedSongID = SongId;
				return "ERROR";	
			}
			var jo = System.Json.JsonObject.Parse (songUrl);
			//Util.AppDelegate.StartPlayback(songUrl,false);
			var url = jo ["url"];
			return url;
		}
		
		public override void CreatePlaylist (string PlaylistName, Album album, Action<bool> OnSuccess)
		{
			Util.ShowBlockAlert ("Creating Playlist".Translate ());
			ThreadPool.QueueUserWorkItem (delegate {
				//bool succes;
				try {
									
					var plist = CreatePlaylist (PlaylistName);
					AddToPlaylist (plist, album, OnSuccess);

						
					
				} catch (Exception ex) {
					Console.WriteLine ("Error creating the playlist" + ex);
				} finally {
					Util.HideBlockAlert();
				}
				
				
			});
		}

		public override void CreatePlaylist (string PlaylistName, Artist artist, Action<bool> OnSuccess)
		{
			Util.ShowBlockAlert ("Creating Playlist".Translate ());
			ThreadPool.QueueUserWorkItem (delegate {
				//bool succes;
				try {
									
					var plist = CreatePlaylist (PlaylistName);
					AddToPlaylist (plist, artist, OnSuccess);
					//succes = true;
						
					
				} catch (Exception ex) {
					Console.WriteLine ("Error creating the playlist" + ex);
				} finally {
					Util.HideBlockAlert();
					//if(OnSuccess !=null)
					//	OnSuccess(succes);
				}
				
				
			});
		}

		public override void CreatePlaylist (string PlaylistName, Genre genre, Action<bool> OnSuccess)
		{
			Util.ShowBlockAlert ("Creating Playlist".Translate());
			ThreadPool.QueueUserWorkItem (delegate {
				//bool succes;
				try {
									
					var plist = CreatePlaylist (PlaylistName);
					AddToPlaylist (plist, genre, OnSuccess);
					
					//succes = true;
					Util.UpdatePlaylist (true);
						
					
				} catch (Exception ex) {
					Console.WriteLine ("Error creating the playlist" + ex);
				} finally {
					Util.HideBlockAlert();
				}
				
				
			});
		}

		public override void CreatePlaylist (string PlaylistName, Song song, Action<bool> OnSuccess)
		{
			Util.ShowBlockAlert("Creating Playlist".Translate());
			ThreadPool.QueueUserWorkItem (delegate {
				//bool succes;
				try {
									
					var plist = CreatePlaylist (PlaylistName);
					AddToPlaylist (plist, new Song[]{song}, OnSuccess);
					//succes = true;
					
						
					Util.UpdatePlaylist (true);
						
					
				} catch (Exception ex) {
					Console.WriteLine ("Error creating the playlist" + ex);
				} finally {
					Util.HideBlockAlert();
				}
				
				
			});

		}
		
	
	
		public void CreatePlaylist (string PlaylistName, Song[] songs, Action<bool> OnSuccess)
		{
			Util.ShowBlockAlert ("Creating Playlist".Translate());
			ThreadPool.QueueUserWorkItem (delegate {
				var success = CreatePlaylist (PlaylistName, songs.Select (x => x.Id).ToArray ());
				Util.EnsureInvokedOnMainThread(delegate{
					Util.HideBlockAlert();
					if(OnSuccess != null)
						OnSuccess(success);
				});
			});
		}
		public bool CreatePlaylist(string PlaylistName, string[] songs)
		{


				bool success = false;
				try {
									
					var plist = CreatePlaylist (PlaylistName);
					success = AddToPlaylist (plist, songs);
					Util.UpdatePlaylist (true);
					
				} catch (Exception ex) {
					Console.WriteLine ("Error creating the playlist" + ex);
				} 
			return success;

		}
		
		public Playlist CreatePlaylist (string PlaylistName)
		{
			var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.CreatePlaylistPath + sCookie, new Dictionary<string,string> () {
				{"json","{\"title\":\"" + PlaylistName + "\"}"}
			}, Requests.DefaultHeaders ());
			bool succes = false;
			var p = (IDictionary<string, JsonValue>)System.Json.JsonObject.Parse (json);
			if (p.ContainsKey (GoogleServiceConfig.Default.CreatePlaylistSuccess))
				succes = p [GoogleServiceConfig.Default.CreatePlaylistSuccess];
			if (succes) {
				
				string playlistId = "";
				if (p.ContainsKey (GoogleServiceConfig.Default.CreatePlaylistId))
					playlistId = p [GoogleServiceConfig.Default.CreatePlaylistId];								
				if (string.IsNullOrEmpty (playlistId)) {
					succes = false;
					throw new Exception ("Error adding playlist");
				}

				
				string playlistName = "";
				if (p.ContainsKey (GoogleServiceConfig.Default.CreatePlaylistTitle))
					playlistName = p [GoogleServiceConfig.Default.CreatePlaylistTitle];
				
				Playlist plist = new Playlist{ServerId = playlistId,Name  = playlistName};
				lock (Database.DatabaseLocker)
					Database.Main.Insert (plist, "OR REPLACE");
				
				return plist;
			}
			return null;

		}
		
		public override void CreateMagicPlayList (Song song, Action<bool> OnSuccess)
		{
			Util.ShowBlockAlert ("Creating Playlist".Translate());
			ThreadPool.QueueUserWorkItem (delegate {
				bool succes = false;
				try {
					var json = Requests.HttpsPost (serviceRoot + GoogleServiceConfig.Default.CreateMagicPlaylistPath + sCookie, 
					                               "[[\"" +sessionId + "\",1],[[[\"" + song.Id + "\",null,1]]]]"
					                               , Requests.DefaultHeaders ());
					List<string> SongIds = new List<string>();
					foreach(var myString in json.Split(new string[] { "[","]" }, StringSplitOptions.RemoveEmptyEntries))
					{
						var songID = ParseMagicPlaylistResponseString(myString);
						if(!string.IsNullOrEmpty(songID))
							SongIds.Add(songID);
					}
					succes = SongIds.Count > 0;

					if (succes) {

						succes = CreatePlaylist(song.Title + " " + "Mix".Translate(),SongIds.ToArray());
						//succes = AddToPlaylist(plist,SongIds.ToArray());


					}
				} catch (Exception ex) {
					Console.WriteLine ("Error creating the magic playlist" + ex);
				} finally {
				
					Util.EnsureInvokedOnMainThread (delegate {
						if (OnSuccess != null)
							OnSuccess (succes);
					});
					Util.HideBlockAlert();
				}
				
				
			});
		}
		string ParseMagicPlaylistResponseString(string line)
		{
			var items = line.Split (new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
			if (items.Length < 4)
				return string.Empty;
			return items [0].Replace("\"","");
		}
		private PlaylistSongs ParseMagicPlaylistSong (JsonObject s, string playlistId, int order)
		{
			string songId = "";
			if (s.ContainsKey (GoogleServiceConfig.Default.MagicPlaylistSongId))
				songId = s [GoogleServiceConfig.Default.MagicPlaylistSongId];
			
			string plistEntryId = "";					
			if (s.ContainsKey (GoogleServiceConfig.Default.MagicPlaylistEntry))
				plistEntryId = s [GoogleServiceConfig.Default.MagicPlaylistEntry];
							
			PlaylistSongs theSong = new PlaylistSongs (){ServerPlaylistId = playlistId,SongId = songId,EntryId = plistEntryId};
							
			theSong.SOrder = order;
			theSong.IsOnServer = true;	
			return theSong;
			
		}
		
		private void ParseNewPlaylistSong (JsonObject s, string playlistId, int order)
		{
			string songId = "";
			if (s.ContainsKey (GoogleServiceConfig.Default.PlaylistSong))
				songId = s [GoogleServiceConfig.Default.PlaylistSong];
			
			string plistEntryId = "";					
			if (s.ContainsKey (GoogleServiceConfig.Default.PlaylistEntry))
				plistEntryId = s [GoogleServiceConfig.Default.PlaylistEntry];
							
			PlaylistSongs theSong = new PlaylistSongs (){ServerPlaylistId = playlistId,SongId = songId,EntryId = plistEntryId};
			
			lock (Database.DatabaseLocker)
				Database.Main.Insert (theSong);
			
		}
		
		private void ParsePlaylist (System.Json.JsonObject p, bool canEdit, bool autoPlaylist)
		{
			ParsePlaylist (p, canEdit, autoPlaylist, "");
		}
		
		private void ParsePlaylist (System.Json.JsonObject p, bool canEdit, bool autoPlaylist, string title)
		{
			
			List<PlaylistSongs> songsToAdd = new List<PlaylistSongs> ();	
			List<Song> songstoUpdate = new List<Song>();
			try {
				string playlistId = "";
				if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistId))
					playlistId = p [GoogleServiceConfig.Default.PlaylistId];
					
				if (string.IsNullOrEmpty (playlistId))
					return;
					
					
				string playlistName = "";
				if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistTitle))
					playlistName = p [GoogleServiceConfig.Default.PlaylistTitle];
				if (!string.IsNullOrEmpty (title))
					playlistName = title;

				Playlist plist;
				lock (Database.DatabaseLocker) {
					plist = Database.Main.Table<Playlist> ().Where (x => x.ServerId == playlistId).FirstOrDefault ();
				}
				if (plist == null) {
					plist = new Playlist (){ServerId = playlistId, Name = playlistName,CanEdit = canEdit,AutoPlaylist = autoPlaylist};
					lock (Database.DatabaseLocker)
						Database.Main.Insert (plist, "OR IGNORE");
				}
				else
				{
					plist.Deleted = false;
					lock (Database.DatabaseLocker)
						Database.Main.Update(plist);
				}
			
				if (p.ContainsKey (GoogleServiceConfig.Default.PlaylistSecondaryRoot)) {
					int order = 0;
					foreach (var key2 in p[GoogleServiceConfig.Default.PlaylistSecondaryRoot]) {
						var s = (System.Json.JsonObject)key2;
							
						string songId = "";
						if (s.ContainsKey (GoogleServiceConfig.Default.PlaylistSongId))
							songId = s [GoogleServiceConfig.Default.PlaylistSongId];
					
						string plistEntryId = "";					
						if (s.ContainsKey (GoogleServiceConfig.Default.PlaylistEntry))
							plistEntryId = s [GoogleServiceConfig.Default.PlaylistEntry];
						if (string.IsNullOrEmpty (plistEntryId))
							plistEntryId = playlistId + order;
						PlaylistSongs theSong = new PlaylistSongs (){ServerPlaylistId = playlistId,SongId = songId,SOrder = order,EntryId = plistEntryId,Deleted = false};
						songsToAdd.Add (theSong);
						order ++;
						if(plist.ShouldBeLocal)
						{

							var song = Database.Main.GetObject<Song>(songId);
							if(!song.ShouldBeLocal)
							{
								song.ShouldBeLocal = true;
								songstoUpdate.Add(song);
								//TODO: Fix downloader
								//Downloader.AddFile(Util.SongsDict[songId]);
							}

						}
							
					}
					if (songsToAdd.Count > 0) {
						lock (Database.DatabaseLocker) {
							Database.Main.Execute ("delete from PlaylistSongs where ServerPlaylistId = ?", playlistId);
							Database.Main.InsertAll (songsToAdd);
							Database.Main.UpdateAll(songstoUpdate);
						}
					}
						
				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}

		}
		
	
	}
}

