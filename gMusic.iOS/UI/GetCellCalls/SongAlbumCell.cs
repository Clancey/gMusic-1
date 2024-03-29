using System;
using Xamarin.Tables;
using MonoTouch.UIKit;
using System.Drawing;

namespace GoogleMusic
{
	public class SongAlbumCell : ICell
	{
		Song Song;
		public SongAlbumCell (Song song)
		{
			Song = song;
		}

		#region ICell implementation

		public UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell ("SongAlbumCell") as SongAlbumTableViewCell;
			if (cell == null)
				cell = new SongAlbumTableViewCell ("SongAlbumCell", Song);
			cell.UpdateCell (Song);
			return cell;
		}

		#endregion
	}

	
	class SongAlbumTableViewCell : UITableViewCell
	{
		public bool IsDarkThemed = false;
		const int userSize = 15;
		const int textSize = 15;
		const int timeSize = 12;
		const float padding = 10f;
		const float trackWidth = 40;
		const float timeWidth = 50;
		const float levelWidth = 40;
		public const float Height = 50;
		static UIColor timeColor = UIColor.FromRGB (147, 170, 204);
		SongAlbumCellView songCellView;
		public static int CellStyle;
	
		static SongAlbumTableViewCell ()
		{

		}
	
		// Should never happen
		public SongAlbumTableViewCell (IntPtr handle) : base (handle)
		{
			Console.WriteLine (Environment.StackTrace);
		}
	
		public class SongAlbumCellView : UIView
		{
			public bool IsDarkThemed;
			
			UIFont userFont;
			UIFont textFont;
			UIFont timeFont;
			Song song;
			string userText;
	
			public SongAlbumCellView (Song song,bool isDarkThemed) : base ()
			{
				IsDarkThemed = isDarkThemed;
				Update (song);
				BackgroundColor = isDarkThemed ? UIColor.Clear : UIColor.White ;
			}

			UIView levelMeterView;

			public void Update (Song song)
			{
				userFont = UIFont.BoldSystemFontOfSize (IsDarkThemed ? 20 : userSize);
				textFont = UIFont.BoldSystemFontOfSize (IsDarkThemed ? 20 : textSize);
				timeFont = UIFont.BoldSystemFontOfSize (IsDarkThemed ? 17 : timeSize);
				this.song = song;
				userText = song.Title;
				// 
				// For fake UserIDs (returned by the search), we try looking up by screename now
				//
				SetNeedsDisplay ();
			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
	
				if (Util.CurrentSong == song) {
					if (levelMeterView == null)
						levelMeterView = new UIView ();
					levelMeterView.Frame = new RectangleF (Bounds.Width - padding - levelWidth - timeWidth, 0, levelWidth, levelWidth);
					levelMeterView.AddSubview (Util.SongLevelMeter);
					this.AddSubview (levelMeterView);
				} else 
					levelMeterView = null;

			}
	
			int state;

			public override void Draw (RectangleF rect)
			{
				try {
					state = 0;
					if (song == Util.CurrentSong) {
						if (levelMeterView == null)
							levelMeterView = new UIView ();
						if (levelMeterView.Superview != this) {
							levelMeterView.Frame = new RectangleF (Bounds.Width - padding - levelWidth - timeWidth, (Bounds.Height - levelWidth) / 2, levelWidth, levelWidth);
							levelMeterView.AddSubview (Util.SongLevelMeter);
							this.AddSubview (levelMeterView);
						}
					} else if (levelMeterView != null) {
						levelMeterView.RemoveFromSuperview ();
						levelMeterView = null;
					}
		
					RealDraw (rect);
				} catch (Exception e) {
						Console.WriteLine ("State: " + state, e);
				}
			}
		
			void RealDraw (RectangleF rect)
			{
				var context = UIGraphics.GetCurrentContext ();
				context.ClearRect(rect);
				// Superview is the container, its superview the uitableviewcell
				UIColor textColor;

				state = 1;
	
				var bounds = Bounds;
				//var midx = bounds.Width / 2;


				BackgroundColor.SetColor ();
				context.FillRect (bounds);
				state = 2;					
				context.SetStrokeColor (UIColor.LightGray.CGColor);

				// Draw first line
				context.SetLineWidth (0.5f);
				// Translate context for clear line
				context.TranslateCTM (-0.5f, -0.5f);
				context.BeginPath ();
				context.MoveTo (trackWidth, -1);
				context.AddLineToPoint (trackWidth, rect.Height);
				context.StrokePath ();
	
				//Draw second line
				context.BeginPath ();
				context.MoveTo (Bounds.Width - timeWidth, -1);
				context.AddLineToPoint (Bounds.Width - timeWidth, rect.Height);
				context.StrokePath ();
	
				//Draw border;
	
				context.BeginPath ();
				//context.MoveTo (0,0);
				//context.AddLineToPoint (Bounds.Width,0);
				context.MoveTo (bounds.Width, bounds.Height);
				context.AddLineToPoint (0, bounds.Height);
				//context.AddLineToPoint(0,0);
				context.StrokePath ();
	
	
	
				state = 5;
				//context.DrawLinearGradient (bottomGradient, new PointF (midx, bounds.Height-17), new PointF (midx, bounds.Height), 0);
				//context.DrawLinearGradient (topGradient, new PointF (midx, 1), new PointF (midx, 3), 0);
				state = 6;                    
				textColor = IsDarkThemed ? UIColor.White : UIColor.Black;
	
				state = 7;
	
				textColor.SetColor ();
				var trackStringSize = this.StringSize (song.Track.ToString (), userFont);
				var trackX = (trackWidth - trackStringSize.Width) / 2f;
				var trackY = (Height - trackStringSize.Height) / 2f;
				DrawString (song.Track.ToString (), new RectangleF (new PointF (trackX, trackY), trackStringSize), userFont);
	
				state = 8;
				if (!song.IsLocal && !IsDarkThemed)
						UIColor.DarkGray.SetColor ();
				bool isCurrent = song == Util.CurrentSong;
				var titleWidth = Bounds.Width - trackWidth - timeWidth - (padding * 2) - (isCurrent ? levelWidth + padding : 0);
				var titleStringSize = this.StringSize (song.Title, textFont, new SizeF (titleWidth, Height), UILineBreakMode.TailTruncation);
				var titleX = trackWidth + padding;
				var titleY = (Height - titleStringSize.Height) / 2f;
				DrawString (song.Title, new RectangleF (new PointF (titleX, titleY), titleStringSize), textFont, UILineBreakMode.TailTruncation);
	
				state = 9;
				textColor.SetColor();
				//UIColor.DarkGray.SetColor ();
				var timeString = Util.FormatTimeSpan (song.DurationSpan);
				var timeStringSize = this.StringSize (timeString, timeFont);
				var timeX = bounds.Width - timeWidth + (timeStringSize.Width / 2);
				var timeY = (Height - timeStringSize.Height) / 2;
				DrawString (timeString, new RectangleF (new PointF (timeX, timeY), timeStringSize), timeFont);
	
				state = 10;
	
				/*
		float xPic, xText;
		if ((CellStyle & 1) == 0 && isCurrent){
			xText = TextWidthPadding;
			xPic = bounds.Width-PicAreaWidth+PicXPad;
		} else {
			isCurrent = true;
			xText = PicAreaWidth;
			xPic = PicXPad;
		}
		state = 11;
		
		state = 12;
		DrawString (userText, new RectangleF (xText, TextHeightPadding, bounds.Width-PicAreaWidth-TextWidthPadding-TimeWidth, userSize), userFont);
		DrawString (song.Artist, new RectangleF (xText, bounds.Y + TextYOffset, bounds.Width-PicAreaWidth-TextWidthPadding, bounds.Height-TextYOffset), textFont, UILineBreakMode.WordWrap);
		state = 13;
		timeColor.SetColor ();
		//string time = Util.FormatTime (new TimeSpan (DateTime.UtcNow.Ticks - song.CreatedAt));
		state = 14;

		state = 15;
		//DrawString (time, new RectangleF (xText, TextHeightPadding, bounds.Width-PicAreaWidth-TextWidthPadding, timeSize),
		//            timeFont, UILineBreakMode.Clip, UITextAlignment.Right);
		
		state = 17;
		
		state = 20;
		if(isCurrent)
			Util.SongLevelMeter.Draw(Util.SongLevelMeter.Frame.SetLocation(xPic,PicYPad));
		//tweetImage.Draw (new RectangleF (xPic, PicYPad, PicSize, PicSize));
		
		state = 21;
		*/
			}

		}
	
		public class BorderView : UIView
		{
			public bool isDarkThemed;
			public override void Draw (RectangleF rect)
			{
				var context = UIGraphics.GetCurrentContext ();
				context.ClearRect(rect);
				if(!isDarkThemed){
					UIColor.White.SetColor ();
					context.FillRect (Bounds);
				}
	
				context.SetStrokeColor (UIColor.LightGray.CGColor);

				// Draw first line
				context.SetLineWidth (0.5f);
				// Translate context for clear line
				context.TranslateCTM (-0.5f, -0.5f);
				context.BeginPath ();
				context.MoveTo (0, Bounds.Height);
				context.AddLineToPoint (Bounds.Width, Bounds.Height);
				context.StrokePath ();
			}
		}
	
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public SongAlbumTableViewCell (string ident, Song song, bool isDarkThemed = false) : base (UITableViewCellStyle.Default, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
	
			IsDarkThemed = isDarkThemed;
			songCellView = new SongAlbumCellView (song,isDarkThemed);
			UpdateCell (song);
			ContentView.Add (songCellView);
			BackgroundView = new BorderView (){isDarkThemed = IsDarkThemed,BackgroundColor = UIColor.Clear};
			//Layer.bo
			//Layer.BorderColor = UIColor.LightGray.CGColor;
			//Layer.BorderWidth = .501f;
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (Song song)
		{
			Accessory = IsDarkThemed ? UITableViewCellAccessory.None : UITableViewCellAccessory.DetailDisclosureButton;
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

