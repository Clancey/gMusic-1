
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
using Android.Graphics;

namespace GoogleMusic
{			
	public class ColorFragment : Fragment
	{
		
		private int mColorRes = -1;
		
		public ColorFragment() : this(Resource.Color.red){ 

		}
		
		public ColorFragment(int colorRes) {
			mColorRes = colorRes;
			RetainInstance = true;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (savedInstanceState != null)
				mColorRes = savedInstanceState.GetInt("mColorRes");
			Color color = Resources.GetColor (mColorRes);
			// construct the RelativeLayout
			RelativeLayout v = new RelativeLayout(Activity);
			v.SetBackgroundColor(color);
			return v;
		}
		public override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			outState.PutInt("mColorRes", mColorRes);
		}
		
	}
}
