using System;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class GenreViewModel : BaseViewModel<Genre>
	{
		public static GroupInfo DefaultGroupInfo = new GroupInfo (){Filter = "Name <> ''",OrderBy = "Name", GroupBy = "IndexCharacter",Ignore = false};
		#if iOS
		
		public GenreViewModel (IBaseViewController parent) : base(parent)
		{
			
			GroupInfo = DefaultGroupInfo;
		}
		
		#elif Droid
		public GenreViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
			
			GroupInfo = DefaultGroupInfo;
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
			var groupInfo = new GroupInfo(){Filter = string.Format("GenreId = {0}",item.Id), Ignore = true};
			var artistCount = Database.Main.GetDistinctObjectCount<Song>(groupInfo, "ArtistId");
			if (artistCount == 1) {
				var song = Database.Main.ObjectForRow<Song> (groupInfo, 0, 0);
				var artist = Database.Main.GetObject<Artist> (song.ArtistId);
				Parent.NavigationController.PushViewController (ArtistViewModel.ViewControllerForArtist(artist), true);
			} else {
				groupInfo = new GroupInfo(){Filter = string.Format("Id in (select distinct ArtistId from song where genreid = {0})",item.Id), OrderBy = "NormName", Ignore = true};
				Parent.NavigationController.PushViewController (new ArtistViewController(){GroupInfo = groupInfo}, true);
			}

		}
		public override void LongPressOnItem (Genre item)
		{

		}
		public static BaseViewController GetNextScreen(Genre genre)
		{
			return null;
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Genre> (GroupInfo,section, position);
		}
		
		#endregion
	}
}

