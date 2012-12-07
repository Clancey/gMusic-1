using System;
using System.Collections.Generic;
using System.Linq;


#if Droid
using Redth.MonoForAndroid;
#endif
namespace GoogleMusic
{
	public class SongViewModel :
#if iOS
		TableViewSource<Song>
#else
		SectionedAdapter<Song>
#endif
	{
#if iOS
		
		public SongViewModel (IBaseViewController parent)
		{
			tableView = parent;
		}

#elif Droid
		public SongViewModel (Android.Content.Context context,IBaseViewController parent ) : base (context)
		{
			tableView = parent;
		}
#endif
		#region implemented abstract members of SectionedAdapter
		IBaseViewController tableView;
		public bool IsSearching {get;set;}
		public List<Song> SearchResults = new List<Song>();
		
		public override int RowsInSection (int section)
		{
			if(IsSearching)
				return SearchResults.Count;
			else if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped[section].Count();
			return Util.SongsGrouped[section].Count();
		}

		public override int NumberOfSections ()
		{
			if(IsSearching)
				return 1;
			if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped.Count();
			return Util.SongsGrouped.Count();
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
			if(IsSearching)
				array = new string[]{};
			else if(Settings.ShowOfflineOnly)
				array =Util.OfflineSongsGrouped.Select(x=> x.Key).ToArray();
			else
				array = Util.SongsGrouped.Select(x=> x.Key).ToArray();
			return array;
		}
		
		public override string HeaderForSection (int section)
		{
			if(IsSearching)
				return "";
			else if(Settings.ShowOfflineOnly)
				return Util.OfflineSongsGrouped[section].Key;			
			return Util.SongsGrouped[section].Key;
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
				if(Util.SongsGrouped.Count > section && Util.SongsGrouped[section].Count() > row)
					thesong= Util.SongsGrouped[section].ElementAt(row);
				else{
					thesong = new Song();
					tableView.ReloadData();
				}
			}
			return thesong;
		}

		#endregion

	}
}

