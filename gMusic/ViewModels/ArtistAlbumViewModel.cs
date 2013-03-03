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
		public override void LongPressOnItem (Song item)
		{
			throw new System.NotImplementedException ();
		}
		public override ICell GetICell (int section, int position)
		{
			return new SongAlbumCell(Database.Main.ObjectForRow<Song> (GroupInfo,section, position));
		}
		public override ICell GetHeaderICell (int section)
		{
			int albumId = 0;
			var h = HeaderForSection (section);
			if (!int.TryParse (h,out albumId) || albumId == 0) {
				return  new AlbumHeaderCell(new Album{Name = "Unknown"},1,1);
			}
			return new AlbumHeaderCell(Database.Main.GetObject<Album> (albumId),1,1);
		}
#if iOS
		public override float GetHeightForHeader (MonoTouch.UIKit.UITableView tableView, int section)
		{
//			int albumId = 0;
//			var h = HeaderForSection (section);
//			if (!int.TryParse (h,out albumId)|| albumId == 0) {
//				return 15f;
//			}
			return 50f;
		}
#endif

		public override string[] SectionIndexTitles ()
		{
			return null;
		}
		#endregion
	}
}

