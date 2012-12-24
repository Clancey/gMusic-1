using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Xamarin.Tables;

namespace GoogleMusic
{
	public partial class Genre : ICell
	{
		
		
		static NSString key = new NSString("songCell");
		public UITableViewCell GetCell (MonoTouch.UIKit.UITableView tv)
		{
			return GetCell(tv,false);
		}
		public UITableViewCell GetCell (MonoTouch.UIKit.UITableView tableView, bool isTvOut)
		{
			var cell = tableView.DequeueReusableCell (key);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, key);
			}
			
			if(!isTvOut)
				cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			else 
				cell.Accessory = UITableViewCellAccessory.None;
			
			cell.TextLabel.TextColor = isTvOut ? UIColor.White : UIColor.Black;
			cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(isTvOut ? 30f : 18f);
			cell.TextLabel.Text = this.Name;
			cell.TextLabel.BackgroundColor = UIColor.Clear;
			return cell;
		}
		
		public string SortName {
			get {
				return Name;
			}
		}

	}
}

