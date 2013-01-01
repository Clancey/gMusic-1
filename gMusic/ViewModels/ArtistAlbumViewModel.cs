using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class ArtistAlbumViewModel : BaseViewModel<Song>
	{

		#if iOS

		public ArtistAlbumViewModel (IBaseViewController parent, GroupInfo groupinfo) : base(parent)
		{
			GroupInfo = groupinfo;
		}
		#elif Droid
		public ArtistAlbumViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, GroupInfo groupinfo ) : base (context,listView, parent)
		{
			GroupInfo = groupinfo;
		}
		#endif

		#region implemented abstract members of SectionedAdapter
		public override void RowSelected (Song song)
		{
//			if (AlbumSelected != null)
//				AlbumSelected (item);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Song> (GroupInfo,section, position);
		}
		public override ICell GetHeaderICell (int section)
		{
			int albumId = 0;
			if (!int.TryParse (HeaderForSection (section),out albumId)) {
				return new StringCell("Unknown Album".Translate());
			}
			return new StringCell (Database.Main.GetObject<Album> (albumId).Name);
		}

		#endregion
	}
}

