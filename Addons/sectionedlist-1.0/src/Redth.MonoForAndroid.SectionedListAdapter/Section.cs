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

namespace Redth.MonoForAndroid
{
	public class Section
	{
		public string Caption
		{
			get;
			set;
		}

		public BaseAdapter Adapter
		{
			get;
			set;
		}
	}
}

