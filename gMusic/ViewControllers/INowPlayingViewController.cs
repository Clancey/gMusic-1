using System;
#if iOS
using MonoTouch.UIKit;
#endif

namespace GoogleMusic
{
	public interface INowPlayingViewController
	{
		void Update ();
	}
}

