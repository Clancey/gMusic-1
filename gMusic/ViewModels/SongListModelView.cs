using System;
using Xamarin.Tables;
using System.Linq;
using Xamarin.Data;

namespace GoogleMusic
{
	public class SongListModelView : BaseViewModel<Song>
	{
		GroupInfo GroupInfo;
		#if iOS
		public SongListModelView (IBaseViewController parent, string filter, string orderby) : base(parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby};
		}
		
		#elif Droid
		public SongListModelView (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, string filter, string orderby ) : base (context,listView, parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby};
		}
		#endif
		#region implemented abstract members of SectionedAdapter
		
		public override int RowsInSection (int section)
		{
			if(IsSearching)
				return SearchResults.Count;
			else if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped[section].Count();
			return Database.Main.RowsInSection<Song> (GroupInfo,section);
		}
		
		public override int NumberOfSections ()
		{
			return 1;
		}
		
		public override int GetItemViewType (int section, int row)
		{
			throw new NotImplementedException ();
		}
		
		public override ICell GetICell (int section, int position)
		{
			return ItemFor (section, position);
		}
		public override string[] SectionIndexTitles ()
		{
			return null;
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
			if(IsSearching)
			{
				if(SearchResults.Count >row)
					thesong = SearchResults[row];
				else
					thesong = new Song();
			}				
			else if(Settings.ShowOfflineOnly)
			{
				if(Util.OfflineSongsGrouped.Count > section && Util.OfflineSongsGrouped[section].Count() > row)
					thesong	= Util.OfflineSongsGrouped[section].ElementAt(row);
				else{
					thesong = new Song();
					tableView.ReloadData();
				}
				
			}
			else
			{
				thesong = Database.Main.ObjectForRow<Song>(GroupInfo,section,row);
			}
			return thesong;
		}
		
		#endregion
		
	}
}

