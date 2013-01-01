using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class ArtistAlbumViewModel : BaseViewModel<Album>
	{

		#if iOS
		
		public ArtistAlbumViewModel (IBaseViewController parent) : base(parent)
		{

		}
		
		#elif Droid
		public ArtistAlbumViewModel (Android.Content.Context context, Android.Widget.ListView listView ,IBaseViewController parent ) : base (context,listView ,parent)
		{
		}
		#endif

		#region implemented abstract members of SectionedAdapter
		public Action<Album> AlbumSelected {get;set;}
		public override void RowSelected (Album item)
		{
			if (AlbumSelected != null)
				AlbumSelected (item);
		}
		public override ICell GetICell (int section, int position)
		{
			return Database.Main.ObjectForRow<Album> (section, position);
		}

		#endregion
	}
}

