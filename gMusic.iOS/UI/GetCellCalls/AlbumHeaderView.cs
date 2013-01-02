// 
//  Copyright 2011  Clancey
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using MonoTouch.UIKit;
using System.Drawing;
using ClanceysLib;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class AlbumHeaderView : UIView
	{
		StackPanel detailsView;
		Album Album;
		AlbumImageView albumArt;
		UILabel ArtistLabel;
		UILabel AlbumLabel;
		UILabel ReleasedLabel;
		UILabel CopyrightLabel;
		UILabel TotalLength;
		const float padding = 5;
		const float artistFontSize = 12;
		const float albumFontSize = 15;
		const float timeFontSize = 10;
		CAGradientLayer gradientLayer;
		CALayer backgroundLayer;
		CALayer borderLayer;
		UIButton shuffleButton;
		bool IsDarkThemed;
		public AlbumHeaderView (Album album,int songs,int length, bool isDarkThemed) : base (new RectangleF(0,0,320,90))
		{
			IsDarkThemed = isDarkThemed;
			Album = album;
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			albumArt = new AlbumImageView(new RectangleF(5,5,80,80),false);
			detailsView = new StackPanel();
			detailsView.BackgroundColor = UIColor.Clear;
			
			if(!string.IsNullOrEmpty(album.Artist))
			{
				ArtistLabel = new UILabel(new RectangleF(0,0,0,artistFontSize));
				ArtistLabel.Text = album.Artist;
				ArtistLabel.Font = UIFont.BoldSystemFontOfSize(artistFontSize);
				ArtistLabel.BackgroundColor = UIColor.Clear;
				ArtistLabel.TextColor = isDarkThemed? UIColor.White : UIColor.Black;
				detailsView.AddSubview(ArtistLabel);
			}
			
			
			if(!string.IsNullOrEmpty(album.Name))
			{
				AlbumLabel = new UILabel(new RectangleF(0,0,0,albumFontSize));
				AlbumLabel.Text = album.Name;
				AlbumLabel.Font = UIFont.BoldSystemFontOfSize(albumFontSize);
				AlbumLabel.MinimumFontSize = artistFontSize;
				AlbumLabel.TextColor = isDarkThemed? UIColor.White : UIColor.Black;
				AlbumLabel.AdjustsFontSizeToFitWidth = true;
				AlbumLabel.BackgroundColor = UIColor.Clear;
				detailsView.Add(AlbumLabel);
			}
			
			TotalLength = new UILabel(new RectangleF(0,0,0,timeFontSize));
			TotalLength.Text = "" + songs + " Song" + (songs > 0 ? "s" : "") + ", " + Math.Round(TimeSpan.FromSeconds(length).TotalMinutes) + " Mins.";
			TotalLength.Font = UIFont.SystemFontOfSize(timeFontSize);
			TotalLength.TextColor = isDarkThemed? UIColor.White : UIColor.LightGray;
			TotalLength.BackgroundColor = UIColor.Clear;
			detailsView.AddSubview (TotalLength);

			if(!isDarkThemed){
				backgroundLayer = new CALayer();
				backgroundLayer.Frame = Bounds;
				backgroundLayer.BackgroundColor = UIColor.White.CGColor;
				Layer.AddSublayer(backgroundLayer);
				
				gradientLayer = new CAGradientLayer();
				gradientLayer.Frame = Bounds;
				gradientLayer.Colors = new MonoTouch.CoreGraphics.CGColor[] { UIColor.LightGray.ColorWithAlpha(.1f).CGColor, UIColor.LightGray.ColorWithAlpha(.1f).CGColor, UIColor.LightGray.ColorWithAlpha(.3f).CGColor };
				
				Layer.AddSublayer(gradientLayer);
			}
			borderLayer = new CALayer();
			borderLayer.BackgroundColor = UIColor.LightGray.CGColor;
			borderLayer.Frame = new RectangleF(0,Bounds.Height,Bounds.Width,1);
			Layer.AddSublayer(borderLayer);
			
			shuffleButton = new UIButton(new RectangleF(this.Bounds.Width - 91,this.Bounds.Height - 66,81,66));
			shuffleButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;
			shuffleButton.SetImage(UIImage.FromFile("Images/shuffleButton.png"),UIControlState.Normal);
			shuffleButton.TouchDown += delegate {
				//Console.WriteLine("shuffle pressed");
				Util.PlayAlbum(Album.Id,true);
			};
			
			
			this.AddSubview(albumArt);
			this.AddSubview(detailsView);
			this.AddSubview(shuffleButton);
			
			album.ALbumArtUpdated += delegate {
				albumArt.SetImage(album.AlbumArt(320));
			};
			albumArt.SetImage(album.AlbumArt(320));
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			if(backgroundLayer != null)
				backgroundLayer.BackgroundColor = UIColor.White.CGColor;
			if(gradientLayer != null)
				gradientLayer.Frame = Bounds;
			borderLayer.Frame = new RectangleF(0,Bounds.Height,Bounds.Width,1);
			var x = albumArt.Frame.Right + padding;
			var y = albumArt.Frame.Y;
			var width = Bounds.Width - x - padding;
			detailsView.Frame = new RectangleF(x,y,width,Bounds.Height - (2 * padding));
		}
	}

	public class AlbumHeaderCell : Cell {
		NSString key;
		protected AlbumHeaderView View;
		public CellFlags Flags;
		Album album;
		int songs,length;
		bool isDarkThemed;
		public enum CellFlags {
			Transparent = 1,
			DisableSelection = 2
		}

		public AlbumHeaderCell (Album album,int songs,int length, bool isDarkThemed = false) : base(album.Name)
		{
			this.album = album;
			this.songs = songs;
			this.length = length;
			this.isDarkThemed = isDarkThemed;
			this.Flags =  0;
			key = new NSString ("AlbumHeaderCell" + isDarkThemed + album.Id);
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, key);
				if ((Flags & CellFlags.Transparent) != 0){
					cell.BackgroundColor = UIColor.Clear;
					
					// 
					// This trick is necessary to keep the background clear, otherwise
					// it gets painted as black
					//
					cell.BackgroundView = new UIView (RectangleF.Empty) { 
						BackgroundColor = UIColor.Clear 
					};
				}
				if ((Flags & CellFlags.DisableSelection) != 0)
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				View = new AlbumHeaderView(album,songs,length,isDarkThemed);
				var frame = View.Frame;
				frame.Width = tv.Frame.Width;
				View.Frame = frame;
				cell.ContentView.AddSubview (View);
			} 
			return cell;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				if (View != null){
					View.Dispose ();
					View = null;
				}
			}
		}


	}
	

}

