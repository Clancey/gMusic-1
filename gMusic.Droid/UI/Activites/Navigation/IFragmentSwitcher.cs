using System;
using Android.App;

namespace GoogleMusic
{
	public interface IFragmentSwitcher
	{
		void SwitchContent (Fragment fragment, bool animated, bool removed = false);
	}
}

