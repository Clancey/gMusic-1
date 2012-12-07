using System;
using MonoTouch.UIKit;

namespace GoogleMusic
{
	public abstract class TableViewSource<T> : UITableViewSource
	{
		public TableViewSource ()
		{
		}

		
		public abstract int RowsInSection (int section);
		
		public abstract ICell GetICell (int section, int position);
		
		public abstract int NumberOfSections ();
		
		public abstract int GetItemViewType (int section, int row);

		public abstract string[] SectionIndexTitles ();

		public abstract string HeaderForSection(int section);

		public abstract void RowSelected(T item);

		public abstract T ItemFor (int section, int row);

		public override int RowsInSection (UITableView tableview, int section)
		{
			return RowsInSection (section);
		}
		public override int NumberOfSections (UITableView tableView)
		{
			return NumberOfSections ();
		}
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			return GetICell (indexPath.Section, indexPath.Row).GetCell(tableView);
		}

		
		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			RowSelected (ItemFor (indexPath.Section, indexPath.Row));
		}	

		public override string[] SectionIndexTitles (UITableView tableView)
		{
			return SectionIndexTitles ();
		}
		
		
		public override string TitleForHeader (UITableView tableView, int section)
		{
			return HeaderForSection(section);
		}

	}
}

