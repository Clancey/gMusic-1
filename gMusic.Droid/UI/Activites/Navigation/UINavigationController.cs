using System;
using Android.App;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Widget;
using Android;

namespace GoogleMusic
{
	public class UINavigationController : Fragment
	{
		public List<Fragment> ControllerStack = new List<Fragment> ();
		public IFragmentSwitcher Parent;
		public Button RightButton;
		public Button LeftButton;
		protected TextView TitleTv;
		LinearLayout LeftButtonLayout;
		LinearLayout RightButtonLayout;
		public UINavigationController() : base()
		{

		}
//		public static implicit operator Fragment (UINavigationController nav)
//		{
//			return nav.CurrentFragment;
//		}
		public override View OnCreateView (Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.NavListView, container, false);
			//ListView = v.FindViewById<ListView> (Resource.Id.listView);
			RightButton = v.FindViewById<Button> (Resource.Id.RightBtn);
			LeftButton = v.FindViewById<Button> (Resource.Id.LeftBtn);
			LeftButton.SetTextColor (Android.Graphics.Color.White);
			LeftButton.Click += LeftClicked;
			TitleTv = v.FindViewById<TextView> (Resource.Id.title);
			var curFrag = CurrentFragment;
			if (curFrag != null) {
				try{
					if(!curFrag.IsAdded)
						FragmentManager.BeginTransaction ().Add (Resource.Id.navContent, CurrentFragment).Commit ();
					else
						FragmentManager.BeginTransaction ().Replace (Resource.Id.navContent, CurrentFragment).Commit ();
						
				if(curFrag is IViewController)
					TitleTv.Text = ((IViewController)curFrag).Title;
				}
				catch(Exception ex)
				{
				}
			}
			SetLeftButton ();
			return v;
		}
		public override void OnDetach ()
		{
			var curFrag = CurrentFragment;
			if (curFrag != null) {
				if (curFrag.IsAdded)
				{
				
					try{
					FragmentManager.BeginTransaction ().Remove (CurrentFragment).Commit ();
					}
					catch(Exception ex)
					{
					}
				}
			}
			base.OnDetach ();
		}
		public void SetLeftButton()
		{
			if(ControllerStack.Count > 1)
			{
				LeftButton.SetBackgroundResource(Resource.Drawable.back);
				LeftButton.Text = "Back";

			} else {
				LeftButton.SetBackgroundResource(Resource.Drawable.menuButton);
				LeftButton.Text = "";
			}
		}

		void LeftClicked (object sender, EventArgs e)
		{
			if (ControllerStack.Count > 1) {
				PopViewController (true);
			}else {

					if(Parent is Com.Slidingmenu.Lib.App.SlidingFragmentActivity)
						((Com.Slidingmenu.Lib.App.SlidingFragmentActivity)Parent).SlidingMenu.Toggle();

			}
		}

		public override void OnResume ()
		{
			base.OnResume ();
		}

		public override void OnActivityCreated (Android.OS.Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
		}

		public UINavigationController (Fragment root)
		{
			ControllerStack.Add (root);
			if (root is IViewController) {
				((IViewController)root).NavigationController = this;
				((IViewController)root).HasBackButton = false;
			}
		}
		public Fragment CurrentFragment
		{
			get{ 
				return ControllerStack.LastOrDefault ();
			}
		}
		public void PushViewController(Fragment fragment,bool animated)
		{
			ControllerStack.Add (fragment);
			if (fragment is IViewController)
				((IViewController)fragment).NavigationController = this;
			SwitchContent (CurrentFragment,animated);
		}
		private void SwitchContent (Fragment fragment, bool animated, bool removed = false)
		{
			var ft = FragmentManager.BeginTransaction ();
			if (animated) {
				if(removed)
					ft.SetCustomAnimations (Resource.Animation.slide_in_left, Resource.Animation.slide_out_right);
				else
					ft.SetCustomAnimations (Resource.Animation.slide_in_right, Resource.Animation.slide_out_left);
				
			}
			try{
			ft.Replace (Resource.Id.navContent, fragment).Commit ();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
			SetLeftButton ();
			if (fragment is IViewController) {
				TitleTv.Text = ((IViewController)fragment).Title;
			}
		}
		public bool PopViewController(bool animated)
		{
			if (ControllerStack.Count <= 1)
				return false;
			var last = CurrentFragment;
			ControllerStack.Remove (last);
			if (CurrentFragment is IViewController)
				((IViewController)CurrentFragment).NavigationController = this;
			SwitchContent (CurrentFragment,animated,true);
			return true;
		}
	}
}

