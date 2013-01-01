
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
	[Activity (Label = "SongListFragment")]			
	public class SongListViewController : BaseFragment , IBaseViewController, IViewController
	{
		public SongListViewController()
		{

		}
		string Filter;
		string OrderBy;
		public SongListViewController(string title,string filter, string orderby)
		{
			Title = title;
			Filter = filter;
			OrderBy = orderby;
		}
		SongViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			model = new SongViewModel (Activity, this.ListView, this,Filter,OrderBy);
			this.ListView.Adapter = model;
			//model.PrecachData ();
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

