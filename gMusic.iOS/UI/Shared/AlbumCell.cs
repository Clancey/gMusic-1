using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using System.Drawing;
using ClanceysLib;
using MonoTouch.Dialog.Utilities;

namespace GoogleMusic
{
	public class AlbumCell : UITableViewCell {
		const int userSize = 14;
		const int textSize = 15;
		const int timeSize = 12;
		
		const int PicSize = 48;
		const int PicXPad = 10;
		const int PicYPad = 5;
		
		const int PicAreaWidth = 2 * PicXPad + PicSize;
		
		const int TextHeightPadding = 4;
		const int TextWidthPadding = 4;
		const int TextYOffset = userSize + 8;
		const int MinHeight = PicSize + 2 * PicYPad;
		const int TimeWidth = 46;
		
		
		static CGGradient bottomGradient, topGradient;
		
		public static int CellStyle;
		
		static AlbumCell ()
		{
			
		}
		
		// Should never happen
		public AlbumCell (IntPtr handle) : base (handle) {
			Console.WriteLine (Environment.StackTrace);
		}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public AlbumCell ( string key, Album album) : base (UITableViewCellStyle.Subtitle, key)
		{
			//SelectionStyle = UITableViewCellSelectionStyle.None;
			UpdateCell (album);
			this.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		Album currentAlbum;
		public void UpdateCell (Album album)
		{
			try
			{
				if(currentAlbum != null)
					currentAlbum.ALbumArtUpdated -= HandleCurrentAlbumALbumArtUpdated;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			currentAlbum = album;
			currentAlbum.ALbumArtUpdated += HandleCurrentAlbumALbumArtUpdated; 
			
			this.TextLabel.BackgroundColor = UIColor.Clear;
			this.TextLabel.Text = album.Name;
			this.ImageView.Image = currentAlbum.AlbumArt(57);
			
			if(DetailTextLabel != null)
			{
				DetailTextLabel.Text = album.Artist;
				DetailTextLabel.BackgroundColor = UIColor.Clear;
			}
			
		}

		void HandleCurrentAlbumALbumArtUpdated (object sender, EventArgs e)
		{
			this.ImageView.Image = currentAlbum.AlbumArt(57);
		}
		string Url {get;set;}

	}
	
	
}

