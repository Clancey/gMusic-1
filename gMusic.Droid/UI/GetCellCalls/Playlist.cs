using System;
using Xamarin.Tables;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content;

namespace GoogleMusic
{
	public partial class Playlist : ICell
	{
		public Android.Views.View GetCell (Android.Views.View convertView, Android.Views.ViewGroup parent, Context context)
		{
			var inflater = LayoutInflater.FromContext (context);
			View view = convertView; // re-use an existing view, if one is available
			if (view == null || view.Id != Android.Resource.Layout.SimpleListItem1) // otherwise create a new one
				view = inflater.Inflate (Android.Resource.Layout.SimpleListItem1, null);
			var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			textView.Text = this.Name;
			textView.SetTextColor (Color.White);
			textView.SetBackgroundColor(Color.Transparent);
			
			return view;
		}
	}
}

