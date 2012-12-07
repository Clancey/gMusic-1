using System;

namespace GoogleMusic
{
	public interface ICell
	{
		MonoTouch.UIKit.UITableViewCell GetCell (MonoTouch.UIKit.UITableView tv);
		string SortName{get;}
	}
}

