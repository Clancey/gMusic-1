
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
	[Activity (Label = "Artists")]			
	public class ArtistFragment : ListFragment, IBaseViewController
	{
		ArtistViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			this.ListAdapter = model = new ArtistViewModel (Activity,this.ListView, this);
			model.PrecachData ();
			
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

