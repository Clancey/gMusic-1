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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
//using MonoTouch.UIKit;

namespace GoogleMusic
{
	public class GoogleServiceConfig
	{
		static GoogleServiceConfig _default;
		static string config = Util.DocumentsFolder + "/config";
		public static void LoadFromFile ()
		{
			CreateDefault ();
			return;
			if (!File.Exists (config)) {
				CreateDefault ();
				CheckForUpdate();
				return;
			}
			
			System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer (typeof(GoogleServiceConfig));
			using (var file = File.Open(config,FileMode.Open,FileAccess.Read)) {
				_default = serializer.ReadObject (file) as GoogleServiceConfig;
			}
			
			CheckForUpdate();
		}
		
		private static void CreateDefault ()
		{
			_default = new GoogleServiceConfig (){
						Version = 0,
						ServiceRoot = "https://play.google.com/music/services/",
						MainUrl = "https://play.google.com/music/listen?u=0",
						LoginUrl = "https://www.google.com//accounts/ClientLogin",
						PreLoginParameters = new Dictionary<string,string> (){
							{"service","sj"},
						},
						GalxPattern = "Auth=([a-zA-Z0-9-_\\.]*)",
						SidPattern = "SID=([a-zA-Z0-9-_\\.]*)",
						LoginParameters = new Dictionary<string,string> (){
							{"service","sj"},
						},
						TwoStepPhrase = "smsToken",
						TwoStepAuthUrl = "https://accounts.google.com/SmsAuth?persistent=yes",
						TwoStepAuthParameters = new Dictionary<string,string> (){
							{"timeStmp", ""},
							{"secTok", ""},
							{"smsVerifyPin", "Verify"},
							{"PersistentCookie", "yes"},
						},
						TwoStepLoginParameters = new Dictionary<string,string> (){
							{"dnConn", ""},
							{"smsToken", "es2st"},
							{"PersistentCookie", "yes"},
							{"pstMsg", "1"},
							{"rmShown", "1"},
							{"followup","http://play.google.com/music/listen"},
							{"secTok", ""},
							{"timeStmp", ""},
							{"continue","http://play.google.com/music/listen"},
							{"service", "sj"},
							{"signIn", "Sign in"},
						},
						CookiePath = "/music/services",
			//Status
						StatusPath = "getstatus?u=0&",
						StatusRoot = "status",
						StatusAvailableTracks = "availableTracks",
			//Tracks
						TracksPath = "loadalltracks?u=0&",
						TrackContinuationToken = "continuationToken",
						TrackRequestTime = "requestTime",
						TrackRoot = "playlist",
						TrackId = "id",
						TrackDeleted = "deleted",
						TrackAlbumArtist = "albumArtist",
						TrackAlbumArtistNorm = "albumArtistNorm",
						TrackArtist = "artist",
						TrackArtistNorm = "artistNorm",
						TrackAlbum = "album",
						TrackAlbumNorm = "albumNorm",
						TrackGenre = "genre",
						TrackAlbumArtUrl = "albumArtUrl",
						TrackName = "title",
						TrackDisc = "disc",
						TrackTrack = "track",
						TrackDuration = "durationMillis",
						TrackRating = "rating",
			//Playlists
						PlaylistPath = "loadplaylist?u=0&",
						PlaylistContinuation = "continuationToken",
						PlaylistMagicRoot = "magicPlaylists",
						PlaylistRoot = "playlists",
						PlaylistTitle = "title",
						PlaylistSong = "songId",
						PlaylistEntry = "playlistEntryId",
						PlaylistId = "playlistId",
						PlaylistSecondaryRoot = "playlist",
						PlaylistSongId = "id",
						
						PlaylistLastLoaded = "auto-playlist-recent",
						PlaylistFree = "auto-playlist-promo",
						PlaylistThumbsUp = "auto-playlist-thumbs-up",
						
			//plist edit
						PlistDeleteSongPath = "deletesong?u=0&",
						PlistDeleteRoot = "deleteIds",						
						
			//Edit song
						EditSongPath = "modifyentries?u=0&",
						EditRoot = "success",
						
			//Delete playlist						
						PlaylistDeletePath = "deleteplaylist?u=0&",
						PlaylistDeletePathRoot = "deleteId",
						
			//Add Playlist
						PlaylistAddPath = "addtoplaylist?u=0&",
						PlaylistAddSuccess = "success",
						PlaylistAddRoot = "songIds",
						
						PlaylistMovePath = "changeplaylistorder?u=0&",
						PlaylistMoveRoot = "movedSongIds",
						
						SongPlayUrl = "https://play.google.com/music/play?songid=",
						SongPlayParam = "&pt=e&u=0",
						
						CreatePlaylistPath = "addplaylist?u=0&",
						CreatePlaylistSuccess = "success",
						CreatePlaylistId = "id",
						CreatePlaylistTitle = "title",
						CreatePlaylistRoot = "entries",
						
						MagicPlaylistSongId = "id",
						MagicPlaylistEntry = "playlistEntryId",

						CreateMagicPlaylistPath = "getmixentries?u=0&format=jsarray&",
						
					};
		}
		
		public static void CheckForUpdate()
		{
			if(Util.HasInternet || Settings.ShowOfflineOnly)
			{
				return;
			}
			try
			{
#if DEBUG
				var versionHtml = "http://www.gmusicapp.com/config/version-debug.html";
				var configHtml = "http://www.gmusicapp.com/config/config-debug.html";
#else
				
				var versionHtml = "http://www.gmusicapp.com/config/version.html";
				var configHtml = "http://www.gmusicapp.com/config/config.html";
#endif
				int version = int.Parse( Requests.HttpsGet(versionHtml,"",null));
				if(version == _default.Version)
					return;
				var newConfig = Requests.HttpsGet(configHtml,"",null);
				
				System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer (typeof(GoogleServiceConfig));
				
				_default = serializer.ReadObject (new MemoryStream(System.Text.Encoding.ASCII.GetBytes (newConfig))) as GoogleServiceConfig;
				
				using(StreamWriter ws = new StreamWriter(config))
				{
					ws.Write(newConfig);
				}
				
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		
		public static GoogleServiceConfig Default {
			get {
				if (_default == null) {
					LoadFromFile ();
					//Use to write config.
					//DataContractJsonSerializer serializer = new DataContractJsonSerializer(_default.GetType());
					//var fw = File.OpenWrite(Util.DocumentsFolder + "/details.txt");
					//serializer.WriteObject(fw,_default);
				}
				return _default;
			}
		}

		public int Version { get; set; }

		public string ServiceRoot { get; set; }

		public string MainUrl { get; set; }

		public string LoginUrl { get; set; }

		public Dictionary<string,string> PreLoginParameters{ get; set; }

		public Dictionary<string,string> LoginParameters{ get; set; }

		public string GalxPattern { get; set; }
		public string SidPattern { get; set; }
		
		//TwoStep
		public string TwoStepPhrase { get; set; }

		public string TwoStepAuthUrl { get; set; }

		public Dictionary<string,string> TwoStepAuthParameters{ get; set; }

		public Dictionary<string,string> TwoStepLoginParameters{ get; set; }

		public string CookiePath { get; set; }
		
		//Status parsing
		public string StatusPath { get; set; }

		public string StatusRoot { get; set; }

		public string StatusAvailableTracks { get; set; }
		
		//SongParsing
		public string TracksPath { get; set; }

		public string TrackContinuationToken { get; set; }

		public string TrackRequestTime { get; set; }

		public string TrackRoot { get; set; }

		public string TrackId { get; set; }

		public string TrackDeleted { get; set; }

		public string TrackAlbumArtist { get; set; }

		public string TrackAlbumArtistNorm { get; set; }

		public string TrackArtist { get; set; }

		public string TrackArtistNorm { get; set; }

		public string TrackAlbum { get; set; }

		public string TrackAlbumNorm { get; set; }

		public string TrackGenre { get; set; }

		public string TrackAlbumArtUrl { get; set; }

		public string TrackName { get; set; }

		public string TrackDisc { get; set; }

		public string TrackTrack { get; set; }

		public string TrackDuration { get; set; }

		public string TrackRating { get; set; }
		
		
		//Playlist
		public string PlaylistPath { get; set; }

		public string PlaylistContinuation { get; set; }

		public string PlaylistMagicRoot { get; set; }

		public string PlaylistRoot { get; set; }

		public string PlaylistSecondaryRoot { get; set; }

		public string PlaylistId { get; set; }

		public string PlaylistTitle { get; set; }

		public string PlaylistSong { get; set; }

		public string PlaylistSongId { get; set; }

		public string PlaylistEntry { get; set; }
		
		//AutoPlaylists
		public string PlaylistLastLoaded { get; set; }

		public string PlaylistFree { get; set; }

		public string PlaylistThumbsUp { get; set; }
		
		//Playlist Editing
		public string PlistDeleteSongPath { get; set; }

		public string PlistDeleteRoot { get; set; }
		
		//Edit track
		public string EditSongPath { get; set; }

		public string EditRoot { get; set; }
		
		//Delete Playlist
		public string PlaylistDeletePath { get; set; }

		public string PlaylistDeletePathRoot{ get; set; }
		
		//Add Playlist
		public string PlaylistAddPath { get; set; }

		public string PlaylistAddSuccess { get; set; }

		public string PlaylistAddRoot { get; set; }
		
		//Move song in playlist
		public string PlaylistMovePath { get; set; }

		public string PlaylistMoveRoot { get; set; }
		
		public string SongPlayUrl { get; set; }

		public string SongPlayParam { get; set; }
		
		public string CreatePlaylistPath { get; set; }
		
		public string CreateMagicPlaylistPath { get; set; }

		public string CreatePlaylistSuccess { get; set; }

		public string CreatePlaylistId { get; set; }

		public string CreatePlaylistTitle { get; set; }

		public string CreatePlaylistRoot { get; set; }
		
		public string MagicPlaylistSongId { get; set; }

		public string MagicPlaylistEntry { get; set; }
	}
}

