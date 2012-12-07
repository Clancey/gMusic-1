
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
			ActionFilter = MainService.SongsUpdated;
			Util.MainVC = new MainActivity ();
			base.OnCreate (bundle);
			ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				model.RowSelected( (Song)model[e.Position]);
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

