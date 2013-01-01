using System;
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
		bool HasBackButton{ get; set; }
		string Title {get;set;}
	}
}

