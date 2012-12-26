
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace GoogleMusic
{
	public class PlaylistFragment : ListFragment, IBaseViewController
	{
		PlaylistViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
			ListView.FastScrollEnabled = true;
			this.ListAdapter = model = new PlaylistViewModel (Activity,this.ListView, this);
			
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
		#region IBaseViewController implementation


		#endregion
	}
}

