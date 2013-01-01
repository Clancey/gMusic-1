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
		
		#elif Droid
		public ArtistViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, string filter, string orderby ) : base (context,listView, parent)
		{
			GroupInfo = new GroupInfo (){Filter = filter,OrderBy = orderby,Ignore = true};
		}
		public ArtistViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif

		#region implemented abstract members of SectionedAdapter
		public Action<Artist> ArtistSelected {get;set;}
		public override void RowSelected (Artist item)
		{
			if (ArtistSelected != null)
				ArtistSelected (item);
			else {
				var groupInfo = new GroupInfo(){Filter = string.Format("ArtistId = {0}",item.Id), OrderBy = "NameNorm"};
				var albumCount = Database.Main.GetObjectCount<Album>(groupInfo);
				Console.WriteLine(albumCount);
				Parent.NavigationController.PushViewController(new AlbumViewController(item){HasBackButton = true},true);
				}			
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Artist> (section, position);
		}

		#endregion
	}
}

