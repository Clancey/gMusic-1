
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
	[Activity (Label = "SongActivity", MainLauncher = true)]			
	public class SongActivity : BaseActivity, IBaseViewController
	{
		SongViewModel model;
		protected override void OnCreate (Bundle bundle)
		{
			Database.SetDatabase ("james.clancey@gmail.com");
			ActionFilter = MainService.SongsUpdated;
			Util.MainVC = new MainActivity ();
			base.OnCreate (bundle);
			ListView.FastScrollEnabled = true;
			ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				var item = model[e.Position];
				if(item is Song)
					model.RowSelected(item as Song);
			};
			this.ListAdapter = model = new SongViewModel (this, this);

		}

		public void ReloadData ()
		{
			this.ListAdapter = new SongViewModel (this, this);
		}
		public override void HandleRefresh ()
		{
			RunOnUiThread(ReloadData);
		}
	}
}

