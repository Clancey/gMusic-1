using System;
using MonoTouch.UIKit;
using System.Drawing;
using ClanceysLib;
using MonoTouch.Foundation;
using Xamarin.Tables;

namespace GoogleMusic
{
	public partial class Song : ICell
	{
		static NSString key = new NSString("songCell");
		public static UIImage emptyImage = UIImage.FromFile("Images/empty.png");
		public UITableViewCell GetCell (MonoTouch.UIKit.UITableView tv)
		{
			return GetCell(tv,false);
		}
		public UITableViewCell GetCell (MonoTouch.UIKit.UITableView tv, bool isTvOut)
		{
			SongCell cell = tv.DequeueReusableCell (key) as SongCell;
			if (cell == null) {
				cell = new SongCell(UITableViewCellStyle.Subtitle, key,this,isTvOut);
			}
			else
			{
				cell.UpdateCell(this,isTvOut);
			}
			
			if(!isTvOut)
				cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			else 
				cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(isTvOut ? 30f : 18f);
			cell.DetailTextLabel.Font = (isTvOut ? UIFont.SystemFontOfSize(18f) : UIFont.BoldSystemFontOfSize( 14f));
			return cell;
		}
		
		public string SortName {
			get{ return this.Title;}
		}
	}
	
	
	public class SongCell : UITableViewCell {
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public Song Song;
		//public UIProgressView progress;
		public SongCell (UITableViewCellStyle style, NSString ident, Song song, bool isDarkThemed) : base (style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
			//progress = new UIProgressView(UIProgressViewStyle.Bar);

				
			UpdateCell (song,isDarkThemed);
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (Song song, bool isDarkThemed)
		{

			Song = song;
			//Song.DownloadProgress += HandleDownloadProgress;
			/*
			if(song.DownloadPercent > 0f && song.DownloadPercent != 1f)
			{
				this.ContentView.AddSubview(progress);
				progress.Progress = song.DownloadPercent;
			}
			*/
			this.TextLabel.Text = Song.Title;

			
			if(Song.Equals(Util.CurrentSong))
			{
				this.ImageView.Image = Song.emptyImage;
				this.ImageView.AddSubview(Util.SongLevelMeter);
			}
			else
				this.ImageView.Image = null;
			
			this.TextLabel.TextColor = isDarkThemed ? UIColor.White : Song.IsLocal ? UIColor.Black : UIColor.DarkGray;
			this.TextLabel.BackgroundColor = UIColor.Clear;
			if (this.DetailTextLabel != null)
			{
				this.DetailTextLabel.TextColor = isDarkThemed ? UIColor.White : UIColor.Gray;
				this.DetailTextLabel.Text = Song.Artist;
				this.DetailTextLabel.BackgroundColor = UIColor.Clear;
			}
			
			//
			SetNeedsDisplay ();
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var frame =  this.ContentView.Bounds;
			frame.Y = frame.Height -10;
			frame.Height = 10;
		}
		
	}
}

