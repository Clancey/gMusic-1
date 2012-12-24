using System;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class AlbumViewModel: BaseViewModel<Album>
	{
		
		#if iOS
		
		public AlbumViewModel (IBaseViewController parent) : base(parent)
		{
			
		}
		
		#elif Droid
		public AlbumViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif
		
		#region implemented abstract members of SectionedAdapter
		
		public override void RowSelected (Album item)
		{
			Util.PlaySong (null, item.Id, -1, false);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Album> (section, position);
		}
		
		#endregion
	}
}
