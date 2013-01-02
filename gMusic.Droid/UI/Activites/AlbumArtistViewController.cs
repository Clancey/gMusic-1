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
	public class AlbumArtistViewController: BaseViewController , IBaseViewController, IViewController
	{
		public AlbumArtistViewController()
		{

		}
		GroupInfo GroupInfo;
		public AlbumArtistViewController( string title, GroupInfo groupinfo)
		{
			Title = title;
			GroupInfo = groupinfo;
			
		}
		public Artist Artist{get;set;}
		ArtistAlbumViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			model = new ArtistAlbumViewModel (Activity, this.ListView, this,GroupInfo);
			
			ListView.Adapter = model;
			
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
		
	}
}

