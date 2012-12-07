using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
#if iOS
using MonoTouch.Foundation;
#endif

namespace GoogleMusic
{

	public abstract class Api
	{
		
		public bool IsSignedIn{ get; protected set; }
		
		public Api (string userName)
		{
			user = userName;
			cookiesFilePath = Util.BaseDir + "/Documents/" + userName;
			LoadCookies();
			foreach(var song in Util.Songs.Where(x=> !x.IsLocal && x.ShouldBeLocal).ToList())
			{
				//TODO: downloader
				//Downloader.AddFile(song);
			}
		}
		internal CookieContainer cookies = new CookieContainer ();
		protected string user;
		protected string cookiesFilePath;
		protected BinaryFormatter bf = new BinaryFormatter ();
		protected List<Cookie> CookieList(string url)
		{
			var theCookies = cookies.GetCookies(new Uri(url));
			List<Cookie> foundCookies = new List<Cookie>();
			foreach(System.Net.Cookie c in theCookies)
			{
				foundCookies.Add(c);
			}
			return foundCookies;
		}
		protected virtual void LoadCookies ()
		{
			if (!string.IsNullOrEmpty (cookiesFilePath) && File.Exists (cookiesFilePath)) {				
				try {
					using (var s = File.OpenRead (cookiesFilePath)) {
						var cs = bf.Deserialize (s) as CookieContainer;
						if (cs != null) cookies = cs;
					}					
				} catch (Exception) {
					if(File.Exists(cookiesFilePath))
					   File.Delete(cookiesFilePath);
				}
			}
		}
		
		protected virtual void SaveCookies ()
		{
			
			if (!string.IsNullOrEmpty (cookiesFilePath)) {
				try {
					using (var s = File.OpenWrite (cookiesFilePath)) {
						bf.Serialize (s, cookies);
					}					
				} catch (Exception) {					
				}
			}
		}
		
		public virtual void ClearCookies()
		{
#if iOS
			foreach(var cookie in NSHttpCookieStorage.SharedStorage.Cookies)
				NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);				
#endif
		if(File.Exists(cookiesFilePath))
				File.Delete(cookiesFilePath);
		}
		
		
		public abstract void SignIn (string email, string password, Action<bool> signedIn);

		public abstract void DeleteAlbum (Album album, Action<bool> OnComplete);

		public abstract void DeleteArtist (Artist artist, Action<bool> OnComplete);

		public abstract void DeleteSong (Song song, Action<bool> OnComplete);

		public abstract void DeleteSongFromPlaylist (PlaylistSongs song, Action<bool> OnComplete);

		public abstract void DeletePlaylist (Playlist playlist, Action<bool> OnComplete);

		public abstract void AddToPlaylist (Playlist playlist, Genre genre, Action<bool> OnSuccess);

		public abstract void AddToPlaylist (Playlist playlist, Artist artist, Action<bool> OnSuccess);

		public abstract void AddToPlaylist (Playlist playlist, Album album, Action<bool> OnSuccess);

		public abstract void AddToPlaylist (Playlist playlist, Song[] songs, Action<bool> OnSuccess);
		
		public abstract void MoveSong(PlaylistSongs song,string prev,string next,int position);

		public abstract void CreatePlaylist (string PlaylistName, Genre genre, Action<bool> OnSuccess);

		public abstract void CreatePlaylist (string PlaylistName, Artist artist, Action<bool> OnSuccess);

		public abstract void CreatePlaylist (string PlaylistName, Album album, Action<bool> OnSuccess);

		public abstract void CreatePlaylist (string PlaylistName, Song song, Action<bool> OnSuccess);

		public abstract void CreateMagicPlayList (Song song, Action<bool> OnSuccess);
		
		public abstract bool GetSongsIfNeeded ();
		
		public abstract bool GetSongsIfNeeded (Action Success);
		
		public int NextArtistId = 1;
		public abstract string GetSongUrl (string SongId);
		
		
		public abstract void EditSong (Song song, Action<bool> OnSuccess);
		public abstract void ThumbsUp(Song song, Action<bool> OnSuccess);
		
		public abstract void ThumbsDown(Song song, Action<bool> OnSuccess);
		
		public abstract HttpWebRequest GetSongWebRequest (string url);

		public string getValue (string name, string html)
		{
			
			var regex = "name=\"" + name + "\"[\\s]*value=([\"'])(.*?)\\1";
			Match theMatch = Regex.Match (html, regex);
				
			return theMatch.Groups [2].ToString ();
		}
		
	
	}
}

