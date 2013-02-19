using System;
using Xamarin.Tables;


#if iOS
using MonoTouch.UIKit;
#endif

namespace GoogleMusic
{
	public interface IBaseViewController
	{
		void ReloadData ();
		UINavigationController NavigationController { get;
			#if Droid
			set;
			#endif
		}
	}
}

