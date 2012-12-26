
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
	[Activity (Label = "SongActivity")]			
	public class SongFragment : ListFragment, IBaseViewController
	{
		SongViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
		//	ListView.FastScrollEnabled = true;
			this.ListAdapter = model = new SongViewModel (Activity,this.ListView, this);
			model.PrecachData ();

		}

		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
//		public override void HandleRefresh ()
//		{
//			RunOnUiThread(ReloadData);
//		}
	}
}

