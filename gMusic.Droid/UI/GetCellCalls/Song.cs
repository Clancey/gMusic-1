using System;
using Android.Views;
using Android.App;
using Android.Widget;
using Redth.MonoForAndroid;
using Android.Graphics;

namespace GoogleMusic
{
	public partial class Song : ICell
	{
		public Android.Views.View GetCell (Android.Views.View convertView, Android.Views.ViewGroup parent, LayoutInflater inflater)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = inflater.Inflate (Android.Resource.Layout.SimpleListItem1, null);
			var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			textView.Text = string.Format("{0} {1}",this.Title,this.Artist);
			textView.SetTextColor (Color.White);
			textView.SetBackgroundColor(Color.Transparent);
			
			return view;
		}
		
		public string SortName {
			get{ return this.Title;}
		}
	}
}

