using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class ArtistViewModel : BaseViewModel<Artist>
	{

		#if iOS
		public ArtistViewModel (IBaseViewController parent, string filter, string orderby) : base(parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
		public ArtistViewModel (IBaseViewController parent) : base(parent)
		{

		}
		public ArtistViewModel (IBaseViewController parent, GroupInfo groupInfo) : base(parent)
		{
			GroupInfo = groupInfo;
		}
		
		#elif Droid
		public ArtistViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, string filter, string orderby ) : base (context,listView, parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
		public ArtistViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent , GroupInfo groupInfo) : base (context,listView ,parent)
		{
			GroupInfo = groupInfo;
		}
		public ArtistViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif

		#region implemented abstract members of SectionedAdapter
		public Action<Artist> ArtistSelected {get;set;}
		public override void RowSelected (Artist artist)
		{
			if (ArtistSelected != null) {
				ArtistSelected (artist);
				return;
			}
			Parent.NavigationController.PushViewController(ViewControllerForArtist(artist), true);			
		}
		public static AlbumArtistViewController ViewControllerForArtist(Artist artist)
		{
			
			GroupInfo groupInfo = new GroupInfo{Filter = string.Format ("ArtistId = {0}", artist.Id), GroupBy = "AlbumId", OrderBy =  "UPPER(Album), Disc, Track", Ignore = true};
			
			return new AlbumArtistViewController (artist.Name, groupInfo);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Artist> (GroupInfo,section, position);
		}

		#endregion
	}
}

