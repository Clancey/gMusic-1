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
namespace GoogleMusic
{
	[Activity (Label = "Albums")]			
	public class AlbumFragment : ListFragment, IBaseViewController
	{

		AlbumViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			this.ListAdapter = model = new AlbumViewModel (Activity,this.ListView, this);
			model.PrecachData ();
			
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

