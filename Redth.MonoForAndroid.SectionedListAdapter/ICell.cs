using System;
using Android.Views;

namespace Redth.MonoForAndroid
{
	public interface ICell
	{
		View GetCell (View convertView, ViewGroup parent, LayoutInflater inflater);
		string SortName{get;}
	}
}

