using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Dialog.Utilities;
using MonoTouch.CoreAnimation;
using MonoTouch.MediaPlayer;
using ClanceysLib;

namespace GoogleMusic
{
	public class MusicSectionView : UIControl
	{
		bool showState;
		UIImageView leftImage;
		UIButton rightButton;
		public UILabel MainLabel;
		public UILabel MiddleLabel;
		public UILabel BottomLabel;
		bool imageRequested;
		bool UseDisclosure;
		public int Index {get;set;}
		public bool Expanded {get;set;}
		public static UIImage DefaultLeft = UIImage.FromFile ("Images/default_album.png");
		bool useAllLabels = true;
		CAGradientLayer gradientLayer;
		CALayer backgroundLayer;
		
		public MusicSectionView (RectangleF rect, string title,bool darkThemed, Action disclosureClicked) : this(rect,title,null,true,darkThemed,disclosureClicked)
		{
			showState = true;
			imageRequested = true;
		}
		
		public MusicSectionView (RectangleF rect, string title, Album album,bool useDisclosure,bool darkThemed,Action disclosureClicked) :this (rect, title,"","", album,useDisclosure,darkThemed,disclosureClicked)
		{	
			useAllLabels = false;
			MiddleLabel.RemoveFromSuperview();
			BottomLabel.RemoveFromSuperview();
			MiddleLabel = null;
			BottomLabel = null;
		}
		
		public MusicSectionView (RectangleF rect, string title,string title2,string title3, Album album,bool useDisclosure,bool darkThemed,Action disclosureClicked) :base(rect)
		{
			UseDisclosure = useDisclosure;
			//this.BackgroundColor = UIColor.White;
			leftImage = new UIImageView(new RectangleF (0, 0, 75, 75));
			UpdateAlbum(album);
			leftImage.Layer.ShadowOpacity = .25f;
			leftImage.Layer.ShadowOffset = new SizeF(5,5);
			leftImage.Layer.ShadowColor = UIColor.Black.CGColor;
			MainLabel = new UILabel (new RectangleF (0, 0, 100, 20)){Text = title};
			MainLabel.BackgroundColor = UIColor.Clear;
			MainLabel.TextColor = darkThemed ? UIColor.White : UIColor.Black;
			MainLabel.AdjustsFontSizeToFitWidth = true;
			MainLabel.MinimumFontSize = 10f;
			
			MiddleLabel = new UILabel (new RectangleF (0, 50, 100, 20)){Text = title2};
			MiddleLabel.BackgroundColor = UIColor.Clear;
			MiddleLabel.TextColor = darkThemed ? UIColor.White : UIColor.Black;
			MiddleLabel.AdjustsFontSizeToFitWidth = true;
			MiddleLabel.MinimumFontSize = 10f;
			
			BottomLabel = new UILabel (new RectangleF (0, 70, 100, 20)){Text = title3};
			BottomLabel.BackgroundColor = UIColor.Clear;
			BottomLabel.AdjustsFontSizeToFitWidth = true;
			BottomLabel.TextColor = darkThemed ? UIColor.White : UIColor.Black;
			BottomLabel.MinimumFontSize = 10f;
			
			rightButton = UIButton.FromType (UIButtonType.DetailDisclosure);
			rightButton.TouchDown += delegate {
				if (disclosureClicked != null)
					disclosureClicked ();
			};

			this.BackgroundColor = UIColor.Clear;
			if(!darkThemed){
				backgroundLayer = new CALayer();
				backgroundLayer.Frame = Bounds;
				backgroundLayer.BackgroundColor = UIColor.White.CGColor;
				Layer.AddSublayer(backgroundLayer);
				
				gradientLayer = new CAGradientLayer();
				gradientLayer.Frame = Bounds;
				gradientLayer.Colors = new MonoTouch.CoreGraphics.CGColor[] { UIColor.LightGray.ColorWithAlpha(.1f).CGColor, UIColor.LightGray.ColorWithAlpha(.1f).CGColor, UIColor.LightGray.ColorWithAlpha(.3f).CGColor };
				
				Layer.AddSublayer(gradientLayer);
			}
			
			this.AddSubview (leftImage);
			this.AddSubview (MainLabel);
			this.AddSubview (MiddleLabel);
			this.AddSubview(BottomLabel);
			if(useDisclosure)
				this.AddSubview (rightButton);
			
			
		}
		
		const float padding = 10f;
		const float rightPadding = 30f;

		public override void LayoutSubviews ()
		{
			gradientLayer.Frame = this.Bounds;
			backgroundLayer.Frame = this.Bounds;
			var frameH = this.Frame.Height;
			var leftY = (frameH - leftImage.Frame.Height) / 2;
			leftImage.Frame = leftImage.Frame.SetLocation (padding, leftY);
			
			float rightX =  this.Frame.Width;
			if(UseDisclosure)
			{
				var rightY = (frameH - rightButton.Frame.Height) / 2;
				rightX = (this.Frame.Width - rightPadding - rightButton.Frame.Width);
				rightButton.Frame = rightButton.Frame.SetLocation (rightX, rightY);
			}
			
			var labelY = (frameH - MainLabel.Frame.Height) / (useAllLabels ? 4 : 2);
			var labelX = leftImage.Frame.Right + padding;
			var labelW = rightX - padding - labelX;
			MainLabel.Frame = new RectangleF (labelX, labelY, labelW, MainLabel.Frame.Height);
			if(useAllLabels)
			{
				MiddleLabel.Frame = MainLabel.Frame.AddLocation(0,labelY);
				BottomLabel.Frame = MiddleLabel.Frame.AddLocation(0,labelY);
			}
		}
		
		public void ToggleCollapse ()
		{
			if (UseDisclosure) {
			
				UIView.BeginAnimations ("disclosure");
				UIView.SetAnimationDuration (.2);
				Expanded = !Expanded;
				if (!Expanded)
					leftImage.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeIdentity ();
				else
					leftImage.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeRotation ((float)DegreeToRadian (90));
				
				UIView.CommitAnimations ();
			}
		}

		private double DegreeToRadian (double angle)
		{
			return Math.PI * angle / 180.0;
		}

		public void Update (Song currentSong)
		{
			MainLabel.Text = currentSong.Artist;
			MiddleLabel.Text = currentSong.Title;
			BottomLabel.Text = currentSong.Album;
			UpdateAlbum(currentSong.TheAlbum);
		}

		#region IImageUpdated implementation
		Album currentAlbum;
		public void UpdateAlbum(Album theAlbum)
		{
			if(theAlbum == currentAlbum)
				return;
			try
			{
				if(currentAlbum != null)
					currentAlbum.ALbumArtUpdated -= HandleCurrentAlbumALbumArtUpdated;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
			currentAlbum = theAlbum;
			if(leftImage == null)
				return;
			if(theAlbum == null)
			{
				leftImage.Image = Images.DefaultAlbumImage;
				return;
			}
			currentAlbum.ALbumArtUpdated += HandleCurrentAlbumALbumArtUpdated;
			leftImage.Image = currentAlbum.AlbumArt(320);

		}
		void HandleCurrentAlbumALbumArtUpdated (object sender, EventArgs e)
		{
			leftImage.Image = currentAlbum.AlbumArt(320);
			//var mediaArt = new MPMediaItemArtwork (leftImage.Image);
			if(Util.CurrentSong.AlbumId == currentAlbum.Id)
				Util.UpdateMpNowPlaying();
		}
		#endregion
	}


}

