using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class PlaylistViewModel : BaseViewModel<Playlist>
	{
		public static GroupInfo GroupInfo = new  GroupInfo(){Filter = "AutoPlaylist = 0",OrderBy = "Name"};
		#if iOS
		public PlaylistViewModel (IBaseViewController parent) : base(parent)
		{
			
		}
		
		#elif Droid
		public PlaylistViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent ) : base (context,listView, parent)
		{
		}
		#endif
		#region implemented abstract members of SectionedAdapter
		
		public override int RowsInSection (int section)
		{
//			if(IsSearching)
//				return SearchResults.Count;
//			else if(Settings.ShowOfflineOnly)
//				return Util.OfflineSongsGrouped[section].Count();
			return Database.Main.RowsInSection<Playlist> (GroupInfo,section);
		}

		public override int NumberOfSections ()
		{
//			if(IsSearching)
//				return 1;
//			if(Settings.ShowOfflineOnly)
//				return Util.OfflineSongsGrouped.Count();
			return Database.Main.NumberOfSections<Playlist> (GroupInfo);
		}

		public override int GetItemViewType (int section, int row)
		{
			throw new NotImplementedException ();
		}

		public override ICell GetICell (int section, int position)
		{
			return ItemFor (section, position);
		}
		string[] array;
		public override string[] SectionIndexTitles ()
		{
			return Database.Main.QuickJump<Playlist> (GroupInfo);
//			if (IsSearching)
//				array = new string[]{};
//			else if (Settings.ShowOfflineOnly)
//				array = Util.OfflineSongsGrouped.Select (x => x.Key).ToArray ();
//			else
//				array = Database.Main.QuickJump<Song> ();
//			return array;
		}
		
		public override string HeaderForSection (int section)
		{
			return "";
//			if(IsSearching)
//				return "";
//			else if(Settings.ShowOfflineOnly)
//				return Util.OfflineSongsGrouped[section].Key;			
//			return Database.Main.SectionHeader<Song> (section);
		}

		public override void RowSelected (Playlist playlist)
		{
			Util.PlayPlaylist (null, playlist);
			//Util.PlaySong(song,song.ArtistId,song.AlbumId,true);
		}

		public override Playlist ItemFor (int section, int row)
		{
			Playlist thesong;
//			if(IsSearching)
//			{
//				if(SearchResults.Count >row)
//					thesong = SearchResults[row];
//				else
//					thesong = new Song();
//			}				
//			else if(Settings.ShowOfflineOnly)
//			{
//				if(Util.OfflineSongsGrouped.Count > section && Util.OfflineSongsGrouped[section].Count() > row)
//					thesong	= Util.OfflineSongsGrouped[section].ElementAt(row);
//				else{
//					thesong = new Song();
//					tableView.ReloadData();
//				}
//				
//			}
//			else
//			{
				thesong = Database.Main.ObjectForRow<Playlist>(GroupInfo,section,row);
//			}
			return thesong;
		}

		#endregion

	}
}

