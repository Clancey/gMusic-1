using System;
using Xamarin.Tables;
using System.Linq;
using Xamarin.Data;

namespace GoogleMusic
{
	public class SongViewModel : BaseViewModel<Song>
	{
		GroupInfo GroupInfo;
		#if iOS
		public SongViewModel (IBaseViewController parent, string filter, string orderby) : base(parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
		public SongViewModel (IBaseViewController parent) : base(parent)
		{
			
		}
		#elif Droid
		public SongViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, string filter, string orderby ) : base (context,listView, parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
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
			return Database.Main.SectionHeader<Song> (GroupInfo,section);
		}
		
		public override void RowSelected (Song song)
		{
			Util.PlaySong(song,song.ArtistId,song.AlbumId,true);
		}
		
		public override Song ItemFor (int section, int row)
		{
			Song thesong = Database.Main.ObjectForRow<Song>(GroupInfo,section,row);

			return thesong;
		}
		
		#endregion
		
	}
}

