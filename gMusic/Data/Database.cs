// 
//  Copyright 2012  Xamarin Inc  (http://www.xamarin.com)
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Data;

namespace GoogleMusic
{
	public class Database: InstantDatabase
	{
		internal Database (string file) : base (file)
		{
			CreateTable<Song> ();
			MakeClassInstant<Song> ();
			CreateTable<Artist> ();
			MakeClassInstant<Artist> ();
			CreateTable<Album> ();
			MakeClassInstant<Album> ();
			CreateTable<Genre> ();
			MakeClassInstant<Genre> ();
			CreateTable<Playlist> ();
			MakeClassInstant<Playlist> (PlaylistViewModel.PlaylistGroupInfo);
			CreateTable<PlaylistSongs> ();
			CreateTable<SongOfflineClass> ();
			CreateTable<AlbumOfflineClass> ();
			CreateTable<ArtistOfflineClass> ();
			CreateTable<GenreOfflineClass> ();
			CreateTable<PlaylistOfflineClass> ();
			CreateTable<PreviousPlayedCache> ();
			CreateTable<PlaylistSortedCache> ();
			CreateTable<NextSongCache> ();
			CreateTable<SongOfflineClass> ();
#if mp3tunes
			CreateTable<Movie>();
#endif
		}
#if iOS
		public static readonly string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString () + "/Documents/";
#else
		public static readonly string BaseDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
#endif
		static public Database Main { get; private set; }

		static string dbPath;

		public static void SetDatabase (string user)
		{
			//lock (DatabaseLocker) {
			if (!Directory.Exists (Util.MusicDir))
				Directory.CreateDirectory (Util.MusicDir);
			if (string.IsNullOrEmpty (user))
				throw new Exception ("Database user cannot be null");
				
			var db = user + ".db";// "-Databases.db";
			dbPath = Path.Combine (BaseDir, db);
#if Droid
			//dbPath = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,"Download", "test.db");
			//File.Copy (dbPath, outpath);
#endif
			if (CurrentUser == user && Main != null)
				return;
			CurrentUser = user;
			try {
				Main = new Database (dbPath);
			} catch (Exception ex) {
				if (File.Exists (dbPath))
					File.Delete (dbPath);
				Main = new Database (dbPath);

//				if (Settings.lastResyncVersion < Settings.LastVersionUpdateRequired) {
//					Settings.LastUpdateRequest = "";
//					Settings.ContinuationToken = "";
//					if (File.Exists (dbPath + "-Databases.db"))
//						File.Delete (dbPath + "-Databases.db");
//					Main = new Database (dbPath + "-Databases.db");
//					Settings.lastResyncVersion = Util.CurrentVersion;
//				}
			}
		}

		public static string CurrentUser { get; set; }

		public static bool DatabaseExists (string user)
		{
			var db = user + ".db";//"-Databases.db";
			dbPath = Path.Combine (BaseDir, db);
			return File.Exists (dbPath);
		}

		public static void ResetDatabase ()
		{
			if (Main == null)
				return;

			while (File.Exists(dbPath)) {
				try {
					File.Delete (dbPath);
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			}
			Main = new Database (dbPath);
			/*
			Main.Execute("drop table if exists Song");
			Main.Execute("drop table if exists Artist");
			Main.Execute("drop table if exists Album");
			Main.Execute("drop table if exists Genre");	
			Main.Execute("drop table if exists Playlist");	
			Main.Execute("drop table if exists PlaylistSongs");	
			//Main.Execute("drop table if exists SongOfflineClass"); 
			Main.Execute("drop table if exists AlbumOfflineClass"); 
			Main.Execute("drop table if exists ArtistOfflineClass"); 
			Main.Execute("drop table if exists GenreOfflineClass"); 
			Main.Execute("drop table if exists PlaylistOfflineClass"); 
			Main.Execute ("drop table if exists PreviousPlayedCache");
			Main.Execute ("drop table if exists NextSongCache");
			Main.Execute ("drop table if exists PlaylistSortedCache");
			
			Main.CreateTable<Song>();
			Main.CreateTable<Artist>();
			Main.CreateTable<Album>();
			Main.CreateTable<Genre>();
			Main.CreateTable<Playlist>();
			Main.CreateTable<PlaylistSongs>();
			Main.CreateTable<SongOfflineClass>();
			Main.CreateTable<AlbumOfflineClass>();
			Main.CreateTable<ArtistOfflineClass>();
			Main.CreateTable<GenreOfflineClass>();
			Main.CreateTable<PlaylistOfflineClass>();
			Main.CreateTable<PreviousPlayedCache>();
			Main.CreateTable<PlaylistSortedCache>();
			Main.CreateTable<NextSongCache>();
			*/

		}
		
		public void UpdateOffline (Song song, bool isOffline)
		{
			try {
				int count = isOffline ? 1 : -1;
				int artistCount = 0;
				int albumCount = 0;
				int genreCount = 0;
				if (Util.OfflineArtistList.ContainsKey (song.ArtistId)) {
					Util.OfflineArtistList [song.ArtistId] += count; 
					artistCount = Util.OfflineArtistList [song.ArtistId];
				} else {
					if (count > 0)
						artistCount += count;
					Util.OfflineArtistList.Add (song.ArtistId, artistCount);
				}
				if (Util.OfflineAlbumsList.ContainsKey (song.AlbumId)) {
					Util.OfflineAlbumsList [song.AlbumId] += count;
					albumCount = Util.OfflineAlbumsList [song.AlbumId];
				} else {
					if (count > 0)
						albumCount += count;
					Util.OfflineAlbumsList.Add (song.AlbumId, albumCount);
				}
				
				if (Util.OfflineGenreList.ContainsKey (song.GenreId)) {
					Util.OfflineGenreList [song.GenreId] += count;
					genreCount = Util.OfflineGenreList [song.GenreId];
				} else {
					if (count > 0)
						genreCount += count;
					Util.OfflineGenreList.Add (song.GenreId, genreCount);
				}
				
				if (Util.OfflineSongsList.ContainsKey (song.Id))
					Util.OfflineSongsList [song.Id] = isOffline;
				else
					Util.OfflineSongsList.Add (song.Id, isOffline);

					if (!isOffline)
						this.Execute ("update song set ShouldBeLocal = ? where id = ?", false, song.Id);
					this.Execute ("INSERT OR REPLACE into SongOfflineClass (Id,Offline) values(?,?)", song.Id, isOffline);
					this.Execute ("INSERT OR REPLACE into AlbumOfflineClass (Id,OfflineCount) values(?,?)", song.AlbumId, albumCount);
					this.Execute ("INSERT OR REPLACE into ArtistOfflineClass(Id,OfflineCount) values(?,?)", song.ArtistId, artistCount);
					this.Execute ("INSERT OR REPLACE into GenreOfflineClass(Id,OfflineCount) values(?,?)", song.GenreId, genreCount);
					this.Execute ("update playlist set OffineCount = max(OffineCount + ?,0) where ServerId in(select ServerPlaylistId from playlistsongs where songid = ?)", count, song.Id);
					//this.Execute("update Playlist set 

				//:TODO fixe me
				//UpdateOfflineSongs(true,true);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
		
		public void UpdatePlaylistOfflineCount ()
		{
			try {
					this.Execute ("update playlist set OffineCount = ifnull((select count(ps.SongId) from playlistsongs ps inner join SongOfflineClass soc on ps.songid = soc.id where ps.ServerPlaylistId = ServerId group by ps.ServerPlaylistId),0)");
			} catch (Exception ex) {
#if iOS
				FlurryAnalytics.FlurryAnalytics.LogError("UpdatePlaylistOfflineCount",ex.Message,new MonoTouch.Foundation.NSError());
#endif
			}
		}
		
		public void UpdateDeleteOffline (Song song)
		{
			UpdateOffline (song, false);
		}
		
		public void ResetOffline ()
		{
				this.Execute ("delete from SongOfflineClass"); 
				this.Execute ("delete from AlbumOfflineClass"); 
				this.Execute ("delete from ArtistOfflineClass"); 
				this.Execute ("delete from GenreOfflineClass"); 
				this.Execute ("delete from PlaylistOfflineClass"); 
				this.CreateTable<SongOfflineClass> ();
				this.CreateTable<AlbumOfflineClass> ();
				this.CreateTable<ArtistOfflineClass> ();
				this.CreateTable<GenreOfflineClass> ();
				this.CreateTable<PlaylistOfflineClass> ();

			foreach (var file in Directory.EnumerateFiles(Util.MusicDir)) {
				var songId = Path.GetFileNameWithoutExtension (file);
				//if(Util.SongsDict.ContainsKey(songId))
				UpdateOffline (Database.Main.GetObject<Song> (songId), true);
				//else
				//File.Delete(file);
			}
		}
		//static public WebDatabase WebDatabase { get; private set; }

//		public List<Song> GetAlbumSongs (int albumId)
//		{
//			return Main.Table<Song> ().Where (x => x.AlbumId == albumId).ToList ();
//		}

	}
}

