using System;
using Android.App;
using Android.Widget;
using Android.Views;
using Android.OS;
using Com.Slidingmenu.Lib;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class BaseViewController : Fragment, IViewController
	{
		public UINavigationController NavigationController { get; set; }
		public string Title {get;set;}
		public ListView ListView { get; private set; }
		public BaseViewController ()
		{

		}
		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
			
			//if (this.Activity != null && NavigationController != null)
			//	NavigationController.Parent = this.Activity as MainActivity;
		}
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.list, container, false);
			ListView = v.FindViewById<ListView> (Resource.Id.listView);
			return v;
			
		}
	}
}

