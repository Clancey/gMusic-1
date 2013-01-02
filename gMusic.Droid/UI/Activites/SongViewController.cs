
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
	[Activity (Label = "Songs")]			
	public class SongViewController : BaseViewController , IBaseViewController
	{
		public SongViewController ()
		{
			this.Title = "Songs";
		}
		SongViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			model = new SongViewModel (Activity, this.ListView, this);
			this.ListView.Adapter = model;
			//model.PrecachData ();
		}
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

