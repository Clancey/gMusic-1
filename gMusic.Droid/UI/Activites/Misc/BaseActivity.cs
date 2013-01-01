
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Slidingmenu.Lib;
using Com.Slidingmenu.Lib.App;

namespace GoogleMusic
{
	[Android.App.Activity (Label = "BaseActivity")]			
	public class BaseActivity : SlidingFragmentActivity
	{
		public override void OnLowMemory ()
		{
			base.OnLowMemory ();
			Database.Main.ClearMemory ();
		}
		public override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			RequestWindowFeature(WindowFeatures.NoTitle);
			// set the Behind View
			SetBehindContentView(Resource.Layout.menu_frame);
			//FragmentManager.BeginTransaction ().Replace (Resource.Id.menu_frame, new SampleListFragment()).Commit ();


			var sm = SlidingMenu;
			sm.SetShadowWidthRes(Resource.Dimension.shadow_width);
			sm.SetShadowDrawable(Resource.Drawable.shadow);
			sm.SetBehindOffsetRes(Resource.Dimension.slidingmenu_offset);
			sm.SetFadeDegree(0.35f);
			sm.TouchModeAbove = 1;

			//menu.AttachToActivity (this, 1);
			//menu.SetMenu (Resource.Layout.menu);

		}

	}
}

