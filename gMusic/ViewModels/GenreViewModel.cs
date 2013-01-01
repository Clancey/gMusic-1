using System;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class GenreViewModel : BaseViewModel<Genre>
	{
		
		#if iOS
		
		public GenreViewModel (IBaseViewController parent) : base(parent)
		{
			
		}
		
		#elif Droid
		public GenreViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif
		
		#region implemented abstract members of SectionedAdapter
		public Action<Genre> GenreSelected {get;set;}
		public override void RowSelected (Genre item)
		{
			if (GenreSelected != null) {
				GenreSelected (item);
				return;
			}
			var groupInfo = new GroupInfo(){Filter = string.Format("ArtistId = {0}",item.Id), OrderBy = "NameNorm"};
			var albumCount = Database.Main.GetObjectCount<Album>(groupInfo);
			Console.WriteLine(albumCount);
			//Parent.NavigationController.PushViewController(new AlbumViewController(item){HasBackButton = true},true);

		}
		public static BaseViewController GetNextScreen(Genre genre)
		{
			return null;
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Genre> (section, position);
		}
		
		#endregion
	}
}

