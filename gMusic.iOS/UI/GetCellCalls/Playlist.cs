using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Xamarin.Tables;

namespace GoogleMusic
{
	public partial class Playlist :ICell
	{
		static NSString key = new NSString("songCell");
		public UITableViewCell GetCell (UITableView tableView)
		{
			return GetCell(tableView,false,false);
		}
		public UITableViewCell GetCell (UITableView tableView,bool ispicker,bool darkThemed)
		{
			var cell = tableView.DequeueReusableCell (key);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, key);
			}
			if (!ispicker && !darkThemed)
				cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			else 
				cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(darkThemed ? 30f : 18f);
			cell.TextLabel.TextColor = darkThemed ? UIColor.White: UIColor.Black;
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

