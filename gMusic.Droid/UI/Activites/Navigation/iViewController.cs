using System;
using Xamarin.Tables;


#if iOS
using MonoTouch.UIKit;
#endif

namespace GoogleMusic
{
	public interface IViewController
	{
		UINavigationController NavigationController { get;
#if Droid
			set;
#endif
		}
		string Title {get;set;}
	}
}

