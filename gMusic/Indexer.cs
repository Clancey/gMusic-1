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
using System.Threading;
using MonoTouch.Foundation;
using System.Linq;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace gMusic
{
	public static class Indexer
	{
		static bool isRunning;

		public static void Index ()
		{
			if (isRunning)
				return;
			new Thread (index).Start ();
		}

		static void index ()
		{
			using (new NSAutoreleasePool()) {
				//try {
					var start = DateTime.Now;
					ClearDatabase ();
					Console.WriteLine ("Getting songs");
					
					IndexArtists();
					IndexSongs();
					
					UIApplication.SharedApplication.BeginInvokeOnMainThread (delegate {
						var alert = new UIAlertView ("Indexing complete", "Took : " + (DateTime.Now - start).TotalSeconds, null, "Ok");
						alert.Show ();
					});
				//} catch (Exception ex) {
				//	Console.WriteLine (ex);
				//}
			}
		}
		
		static void IndexSongs ()
		{
			var songs = Database.WebDatabase.Query<Song> ("select * ,lower(trim(name)) NameNorm from songs").OrderBy (s => s.NameNorm.StartsWith ("The ", StringComparison.OrdinalIgnoreCase) ? s.NameNorm.Substring (s.NameNorm.IndexOf (" ") + 1) : s.NameNorm).ToList ();
			int index = 0;
			Console.WriteLine ("Indexing songs");
			foreach (var song in songs) {
				var nameNorm = song.NameNorm;
				song.IndexChar = getIndex (out nameNorm);
				song.NameNorm = nameNorm;
			}
			songs = songs.OrderBy (x => x.IndexChar).ToList ();
			SongGroup songGroup = new SongGroup ();
					
			Console.WriteLine ("grouping songs");
			List<SongGroup> songGroups = new List<SongGroup> ();
			foreach (var song in songs) {
				Console.WriteLine (index);
				song.Order = index;
				Console.WriteLine (song.IndexChar);
				if (song.IndexChar == songGroup.Index)
					songGroup.Count ++;
				else {
					Console.WriteLine ("new group");
					if (!string.IsNullOrEmpty (songGroup.Index))
						songGroups.Add (songGroup);
					songGroup = new SongGroup (){Index = song.IndexChar,Start = index,Count = 1};
				}
				//Database.Main.Insert(song);
					
						
				index ++;
			}
			songGroups.Add (songGroup);
			Database.Main.InsertAll (songs);
			Database.Main.InsertAll (songGroups);
			
			Util.Songs= songs.ToList();
			Util.SongsDict = songs.ToDictionary(x=> x.id,x=> x);
			Util.SongGroups = songGroups;
			UIApplication.SharedApplication.BeginInvokeOnMainThread(delegate{
				AppDelegate.SongVc.TableView.ReloadData();
			});
		}
		
		
		static void IndexArtists ()
		{
			var artists = Database.WebDatabase.Query<Artist> ("select * from artists").OrderBy (s => s.nameNorm.StartsWith ("The ", StringComparison.OrdinalIgnoreCase) ? s.nameNorm.Substring (s.nameNorm.IndexOf (" ") + 1) : s.nameNorm).ToList ();
			int index = 0;
			Console.WriteLine ("Indexing Artists");
			foreach (var artist in artists) {
				var nameNorm = artist.nameNorm;
				artist.IndexChar = getIndex (out nameNorm);
				artist.nameNorm = nameNorm;
			}
			artists = artists.OrderBy (x => x.IndexChar).ToList ();
			ArtistGroup artistGroup = new ArtistGroup ();
					
			Console.WriteLine ("grouping artists");
			List<ArtistGroup> artistGroups = new List<ArtistGroup> ();
			foreach (var artist in artists) {
				//Console.WriteLine (index);
				artist.Order = index;
				//Console.WriteLine (artist.IndexChar);
				if (artist.IndexChar == artistGroup.Index)
					artistGroup.Count ++;
				else {
					//Console.WriteLine ("new group");
					if (!string.IsNullOrEmpty (artistGroup.Index))
						artistGroups.Add (artistGroup);
					artistGroup = new ArtistGroup (){Index = artist.IndexChar,Start = index,Count = 1};
				}
				//Database.Main.Insert(song);
					
						
				index ++;
			}
				
			artistGroups.Add (artistGroup);
			Database.Main.InsertAll (artists);
			Database.Main.InsertAll (artistGroups);
			Util.ArtistGroups = artistGroups;
			Util.Artists= artists.ToList();
			Util.ArtistsDict = artists.ToDictionary(x=> x.id,x=> x);
			
		}
		
		
		static void IndexAlbums ()
		{
			var artists = Database.WebDatabase.Query<Artist> ("select * from artists").OrderBy (s => s.nameNorm.StartsWith ("The ", StringComparison.OrdinalIgnoreCase) ? s.nameNorm.Substring (s.nameNorm.IndexOf (" ") + 1) : s.nameNorm).ToList ();
			int index = 0;
			Console.WriteLine ("Indexing Artists");
			foreach (var artist in artists) {
				var nameNorm = artist.nameNorm;
				artist.IndexChar = getIndex (out nameNorm);
				artist.nameNorm = nameNorm;
			}
			artists = artists.OrderBy (x => x.IndexChar).ToList ();
			ArtistGroup artistGroup = new ArtistGroup ();
					
			Console.WriteLine ("grouping artists");
			List<ArtistGroup> artistGroups = new List<ArtistGroup> ();
			foreach (var artist in artists) {
				Console.WriteLine (index);
				artist.Order = index;
				Console.WriteLine (artist.IndexChar);
				if (artist.IndexChar == artistGroup.Index)
					artistGroup.Count ++;
				else {
					Console.WriteLine ("new group");
					if (!string.IsNullOrEmpty (artistGroup.Index))
						artistGroups.Add (artistGroup);
					artistGroup = new ArtistGroup (){Index = artist.IndexChar,Start = index,Count = 1};
				}
				//Database.Main.Insert(song);
					
						
				index ++;
			}
				
			artistGroups.Add (artistGroup);
			Database.Main.InsertAll (artists);
			Database.Main.InsertAll (artistGroups);
		}
		static void ClearDatabase ()
		{
			Database.Main.Execute ("delete from SongGroup");
			Database.Main.Execute ("delete from Song");
			Database.Main.Execute ("delete from ArtistGroup");
			Database.Main.Execute ("delete from Artist");
		}
		
		static string getIndex (out string name)
		{
			name = name.Trim ();
			if (name.StartsWith ("the "))
				name = name.Replace ("the ", "");
			var firstLetter = (string.IsNullOrEmpty (name)) ? " ".First () : name.First ();
			if (char.IsLetter (firstLetter))
				return firstLetter.ToString ().ToUpper ();
			return "#";
		}
		
	}
}

