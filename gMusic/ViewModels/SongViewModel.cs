using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class SongViewModel : BaseViewModel<Song>
	{
		#if iOS
		
		public SongViewModel (IBaseViewController parent) : base(parent)
		{
			
		}
		
		#elif Droid
		public SongViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent ) : base (context,listView, parent)
		{
		}
		#endif

		#region implemented abstract members of SectionedAdapter
		
		public override int RowsInSection (int section)
		{
			if(IsSearching)
				return SearchResults.Count;
			else if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped[section].Count();
			return Database.Main.RowsInSection<Song> (section);
		}

		public override int NumberOfSections ()
		{
			if(IsSearching)
				return 1;
			if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped.Count();
			return Database.Main.NumberOfSections<Song> ();
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
			if (IsSearching)
				array = new string[]{};
			else if (Settings.ShowOfflineOnly)
				array = Util.OfflineSongsGrouped.Select (x => x.Key).ToArray ();
			else
				array = Database.Main.QuickJump<Song> ();
			return array;
		}
		
		public override string HeaderForSection (int section)
		{
			if(IsSearching)
				return "";
			else if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped[section].Key;			
			return Database.Main.SectionHeader<Song> (section);
		}

		public override void RowSelected (Song song)
		{
			Util.PlaySong(song,song.ArtistId,song.AlbumId,true);
		}

		public override Song ItemFor (int section, int row)
		{
			Song thesong;
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
				thesong = Database.Main.ObjectForRow<Song>(section,row);
//			}
			return thesong;
		}

		#endregion

	}
}

