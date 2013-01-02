using System;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class AlbumHeaderCell : ICell
	{
		Album Album;
		public AlbumHeaderCell (Album album,int songs,int length)
		{
			Album = album;
		}

		#region ICell implementation

		public Android.Views.View GetCell (Android.Views.View convertView, Android.Views.ViewGroup parent, Android.Content.Context context)
		{
			return Album.GetCell (convertView, parent, context);
		}

		#endregion
	}
}

