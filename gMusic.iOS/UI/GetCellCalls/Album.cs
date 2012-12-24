using System;
using MonoTouch.UIKit;
using Xamarin.Tables;

namespace GoogleMusic
{
	public partial class Album : ICell
	{
		const string key ="albumCell";
		public UITableViewCell GetCell (UITableView tableView)
		{
			return GetCell(tableView,false);
		}
		public UITableViewCell GetCell (UITableView tableView, bool darkThemed)
		{
			var cell = tableView.DequeueReusableCell (key + darkThemed) as AlbumCell;
			if (cell == null){
				cell = new AlbumCell (key + darkThemed,this);
			}
			else
			{
				cell.UpdateCell(this);
			}
			if(!darkThemed)
				cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			else 
				cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.TextColor = darkThemed ? UIColor.White:UIColor.Black;
			cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(darkThemed ? 30f : 18f);
			cell.TextLabel.Text = this.Name;
			cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			cell.TextLabel.BackgroundColor = UIColor.Clear;
			cell.DetailTextLabel.Text = this.Artist;
			cell.DetailTextLabel.TextColor = darkThemed ? UIColor.White:UIColor.LightGray;
			cell.DetailTextLabel.BackgroundColor = UIColor.Clear;
			return cell;
		}
		
		public string SortName {
			get {
				return NameNorm;
			}
		}
	}
}

