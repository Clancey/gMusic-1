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
	[Activity (Label = "Genres")]			
	public class GenreFragment : BaseViewController, IBaseViewController
	{
		GenreViewModel model;
		public GenreFragment()
		{
			Title = "Genres";
		}
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			model = new GenreViewModel (Activity,this.ListView, this);
			this.ListView.Adapter = model;
			//model.PrecachData ();
			
		}
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

