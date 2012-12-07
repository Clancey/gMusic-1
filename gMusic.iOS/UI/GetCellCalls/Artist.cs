using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace GoogleMusic
{
	public partial class Artist : ICell
	{
		
		static NSString key = new NSString("songCell");
		public UITableViewCell GetCell (UITableView tableView)
		{
			return GetCell(tableView,false);
		}
		public UITableViewCell GetCell (UITableView tableView,bool darkThemed)
		{
			var cell = tableView.DequeueReusableCell (key);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, key);
			}
			if(!darkThemed)
				cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			else 
				cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.TextColor = darkThemed ? UIColor.White : UIColor.Black;
			cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(darkThemed ? 30f : 18f);
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

