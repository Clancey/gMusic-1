using System;
using MonoTouch.UIKit;
using System.Drawing;
using ClanceysLib;
using MonoTouch.Foundation;

namespace gMusic
{
	public partial class Song : ICell
	{
		static NSString key = new NSString("songCell");
		static UIImage emptyImage = UIImage.FromFile("Images/empty.png");
		public UITableViewCell GetCell (MonoTouch.UIKit.UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, key);
				cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			}
			//else
			//	cell.UpdateCell(this);
			//cell.Opaque =true;
			;//.Where(x=> x.IndexCharacter == headers[indexPath.Section]).ToArray()[indexPath.Row];
			//cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = this.name;
			//if(Util.IsIpad)
			//{
			//	cell.ImageView.Image = emptyImage;
			/*
			if(Util.CurrentSong != null &&  Util.CurrentSong.Id == Id)
			{
				cell.ImageView.Image = emptyImage;
				cell.ImageView.AddSubview(Util.SongLevelMeter);
			}
			else
				cell.ImageView.Image = null;
			*/
			//cell.TextLabel.TextColor = this.IsLocal ? UIColor.Black : UIColor.DarkGray;
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = this.Artist.name;
			return cell;
		}
		
	}
	
	
	public class SongCell : UITableViewCell {
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
		
		static UIFont userFont = UIFont.BoldSystemFontOfSize (userSize);
		static UIFont textFont = UIFont.SystemFontOfSize (textSize);
		static UIFont timeFont = UIFont.SystemFontOfSize (timeSize);
		static UIColor timeColor = UIColor.FromRGB (147, 170, 204);
		
		SongCellView songCellView;
		
		public static int CellStyle;
		
		static SongCell ()
		{
			/*
			using (var rgb = CGColorSpace.CreateDeviceRGB()){
				float [] colorsBottom = {
					1, 1, 1, .5f,
					0.93f, 0.93f, 0.93f, .5f
				};
				bottomGradient = new CGGradient (rgb, colorsBottom, null);
				float [] colorsTop = {
					0.93f, 0.93f, 0.93f, .5f,
					1, 1, 1, 0.5f
				};
				topGradient = new CGGradient (rgb, colorsTop, null);
			}
			*/
		}
		
		// Should never happen
		public SongCell (IntPtr handle) : base (handle) {
			Console.WriteLine (Environment.StackTrace);
		}
		
		public class SongCellView : UIView{
			
			Song song;
			string userText;
			
			public SongCellView (Song song) : base ()
			{
				Update (song);
				Opaque = true;
				BackgroundColor = UIColor.White;
			}

			public void Update (Song song)
			{
				this.song = song;
				userText = song.name;
	
				// 
				// For fake UserIDs (returned by the search), we try looking up by screename now
				//


				SetNeedsDisplay ();
			}
			
			int state;
			public override void Draw (RectangleF rect)
			{
				try {
					state = 0;
					RealDraw (rect);
				} catch (Exception e) {
					Console.WriteLine("State: " + state, e);
				}
			}
			
			void RealDraw (RectangleF rect)
			{
				var context = UIGraphics.GetCurrentContext ();

				// Superview is the container, its superview the uitableviewcell
				UIColor textColor;
	
				state = 1;
				var bounds = Bounds;
				var midx = bounds.Width/2;
					UIColor.White.SetColor ();
					context.FillRect (bounds);
					state = 5;
					//context.DrawLinearGradient (bottomGradient, new PointF (midx, bounds.Height-17), new PointF (midx, bounds.Height), 0);
					//context.DrawLinearGradient (topGradient, new PointF (midx, 1), new PointF (midx, 3), 0);
					               state = 6;                    
					textColor = UIColor.Black;
				
				
				state = 10;
				float xPic, xText;
				/*
				bool isCurrent = Util.CurrentSong == null ? true: song.Id != Util.CurrentSong.Id;
				if ((CellStyle & 1) == 0 && isCurrent){
					xText = TextWidthPadding;
					xPic = bounds.Width-PicAreaWidth+PicXPad;
				} else {
					isCurrent = true;
					xText = PicAreaWidth;
					xPic = PicXPad;
				}
				*/
				xText = PicAreaWidth;
					xPic = PicXPad;
				state = 11;
				
				state = 12;
				textColor.SetColor ();
				DrawString (userText, new RectangleF (xText, TextHeightPadding, bounds.Width-PicAreaWidth-TextWidthPadding-TimeWidth, userSize), userFont);
				DrawString (song.Artist.name, new RectangleF (xText, bounds.Y + TextYOffset, bounds.Width-PicAreaWidth-TextWidthPadding, bounds.Height-TextYOffset), textFont, UILineBreakMode.WordWrap);
				state = 13;
				timeColor.SetColor ();
				//string time = Util.FormatTime (new TimeSpan (DateTime.UtcNow.Ticks - song.CreatedAt));
				state = 14;

				state = 15;
				//DrawString (time, new RectangleF (xText, TextHeightPadding, bounds.Width-PicAreaWidth-TextWidthPadding, timeSize),
				//            timeFont, UILineBreakMode.Clip, UITextAlignment.Right);
				
				state = 17;
				
				state = 20;
				//if(isCurrent)
				//	Util.SongLevelMeter.Draw(Util.SongLevelMeter.Frame.SetLocation(xPic,PicYPad));
				//tweetImage.Draw (new RectangleF (xPic, PicYPad, PicSize, PicSize));
				
				state = 21;
			}

		}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public SongCell (UITableViewCellStyle style, NSString ident, Song song) : base (style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
			
			songCellView = new SongCellView (song);
			UpdateCell (song);
			ContentView.Add (songCellView);
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (Song song)
		{
			songCellView.Update (song);
			SetNeedsDisplay ();
		}


		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			songCellView.Frame = ContentView.Bounds;
			songCellView.SetNeedsDisplay ();
		}
	}
}

