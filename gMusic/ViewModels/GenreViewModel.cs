using System;
using Xamarin.Tables;

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
		
		public override void RowSelected (Genre item)
		{
			Util.PlaySong (null, item.Id, -1, false);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Genre> (section, position);
		}
		
		#endregion
	}
}

