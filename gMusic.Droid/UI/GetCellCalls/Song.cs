using System;
using Android.Views;
using Android.App;
using Android.Widget;
using Redth.MonoForAndroid;

namespace GoogleMusic
{
	public partial class Song : ICell
	{
		public Android.Views.View GetCell (Android.Views.View convertView, Android.Views.ViewGroup parent, LayoutInflater inflater)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = inflater.Inflate (Android.Resource.Layout.SimpleListItem1, null);
			view.FindViewById<TextView> (Android.Resource.Id.Text1).Text = string.Format("{0} {1}",this.Title,this.Artist);
			return view;
		}
		
		public string SortName {
			get{ return this.Title;}
		}
	}
}

