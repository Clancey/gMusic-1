using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class ArtistViewModel : BaseViewModel<Artist>
	{

		#if iOS
		
		public ArtistViewModel (IBaseViewController parent) : base(parent)
		{

		}
		
		#elif Droid
		public ArtistViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif

		#region implemented abstract members of SectionedAdapter
	
		public override void RowSelected (Artist item)
		{
			Util.PlaySong (null, item.Id, -1, false);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Artist> (section, position);
		}

		#endregion
	}
}

