using System;
using Android.Views;
using Android.App;
using Android.Widget;
using Android.Graphics;
using Xamarin.Tables;
using Android.Content;

namespace GoogleMusic
{
	public partial class Song : ICell
	{
		public Android.Views.View GetCell (Android.Views.View convertView, Android.Views.ViewGroup parent, Context context)
		{
			var inflater = LayoutInflater.FromContext (context);
			View view = convertView; // re-use an existing view, if one is available
			if (view == null || view.Id != Resource.Layout.songCell) // otherwise create a new one
				view = inflater.Inflate (Resource.Layout.songCell, null);
			var songTv = view.FindViewById<TextView> (Resource.Id.songTitle);
			songTv.Text = this.Title;
			
			var artistTv = view.FindViewById<TextView> (Resource.Id.artistName);
			artistTv.Text = this.Artist;
			
			return view;
		}
		
		public string SortName {
			get{ return this.Title;}
		}
	}
}

