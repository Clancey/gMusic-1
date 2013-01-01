using System;
using Android.App;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Widget;

namespace GoogleMusic
{
	public class UINavigationController : Fragment
	{
		public List<Fragment> ControllerStack = new List<Fragment> ();
		public IFragmentSwitcher Parent;
		public Button RightButton;
		public Button LeftButton;
		protected TextView TitleTv;
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
//			if (HasBackButton) {
//				LeftButton.SetBackgroundResource(Resource.Drawable.back);
//				LeftButton.Click += delegate {
//					if(NavigationController != null)
//						NavigationController.PopViewController(true);
//				};
//			} else {
//				LeftButton.SetBackgroundResource(Resource.Drawable.menuButton);
//				LeftButton.Text = "";
//				LeftButton.Click += delegate {
//					if(NavigationController.Parent is Com.Slidingmenu.Lib.App.SlidingFragmentActivity)
//						((Com.Slidingmenu.Lib.App.SlidingFragmentActivity)NavigationController.Parent).SlidingMenu.Toggle();
//				};
//			}
			TitleTv = v.FindViewById<TextView> (Resource.Id.title);
			FragmentManager.BeginTransaction ().Add (Resource.Id.navContent, CurrentFragment).Commit();
			return v;
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
				return ControllerStack.Last ();
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
			ft.Replace (Resource.Id.navContent, fragment).Commit ();
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
