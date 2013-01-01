using System;
using Android.Views;
using Android.App;
using Android.Widget;
using Android.Graphics;
using Xamarin.Tables;
using Android.Content;


namespace GoogleMusic
{
	public partial class Album : ICell
	{
		
		public Android.Views.View GetCell (Android.Views.View convertView, Android.Views.ViewGroup parent, Context context)
		{
			var view = convertView as AlbumCellView; // re-use an existing view, if one is available
			if (view == null) {
				view = new AlbumCellView(context);
			}
			view.Update (this);
			return view;
		}
	}
}

