using System;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class AlbumViewModel: BaseViewModel<Album>
	{
		
		#if iOS
		public AlbumViewModel (IBaseViewController parent, string filter, string orderby) : base(parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
		public AlbumViewModel (IBaseViewController parent) : base(parent)
		{
			
		}
		
		#elif Droid
		public AlbumViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, string filter, string orderby ) : base (context,listView, parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
		public AlbumViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif
		
		#region implemented abstract members of SectionedAdapter
		public Action<Album> AlbumSelected { get; set; }
		public override void RowSelected (Album item)
		{
			if (AlbumSelected != null) {
				AlbumSelected (item);
				return;
			}
			Parent.NavigationController.PushViewController(new SongListViewController(item.Name,string.Format("AlbumId = {0}",item.Id),"Disc, Track"){HasBackButton = true},true);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Album> (GroupInfo,section, position);
		}
		
		#endregion
	}
}
