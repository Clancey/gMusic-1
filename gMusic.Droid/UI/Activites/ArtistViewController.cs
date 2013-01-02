
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Data;

namespace GoogleMusic
{
	[Activity (Label = "Artists")]			
	public class ArtistViewController :  BaseViewController , IBaseViewController
	{
		public GroupInfo GroupInfo;
		public ArtistViewController()
		{
			Title = "Artists";
		}
		ArtistViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			model = new ArtistViewModel (Activity,this.ListView, this,GroupInfo);
			ListView.Adapter = model;
			
		}
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

