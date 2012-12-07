using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace GoogleMusic
{
	public class LoadingScreen : UIView
	{
		UIActivityIndicatorView progressSpinner;
		UIImageView imageView;
		string lastImage = "";
		bool tvOut;
		public LoadingScreen (bool isTvOut = false) : base()
		{
			tvOut = isTvOut;
			//TODO: fix tvout
			var frame = UIApplication.SharedApplication.KeyWindow.Frame;// isTvOut && TVOutManager.TVOutManager.tvoutWindow != null? TVOutManager.TVOutManager.tvoutWindow.Frame : UIApplication.SharedApplication.KeyWindow.Frame;
			//var statush = UIApplication.SharedApplication.StatusBarFrame;
			if(tvOut)
				Console.WriteLine("no change");
			else if(Util.MainVC.InterfaceOrientation == UIInterfaceOrientation.Portrait || Util.MainVC.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
				frame = new RectangleF(0,0,frame.Width,frame.Height);
			else
				frame = new RectangleF(0,0,frame.Height,frame.Width );
			
			this.Frame = frame;
			progressSpinner = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge);
			SetImages();
			progressSpinner.StartAnimating();
		}
		public override void LayoutSubviews ()
		{ 
			base.LayoutSubviews ();
			SetImages();
			
			progressSpinner.Frame = new RectangleF(new PointF(((this.Frame.Width - 50) /2) + 10,((this.Frame.Height - 50)/2) + 10),progressSpinner.Frame.Size);
		}
		
		public void SetImages()
		{
			var orientation = Util.MainVC.NavigationController.InterfaceOrientation;
			string nextImage = "Default.png";
			if(tvOut)
				nextImage =  "Default-Landscape~ipad.png";
			else if(Util.IsIpad)
			{
				switch(orientation)
				{
				case UIInterfaceOrientation.LandscapeLeft:
				case UIInterfaceOrientation.LandscapeRight:
					nextImage = "Default-Landscape~ipad.png";
					break;
				case UIInterfaceOrientation.Portrait:
				case UIInterfaceOrientation.PortraitUpsideDown:
					nextImage = "Default-Portrait~ipad.png";
					break;
				default:
						nextImage = "Default-Landscape~ipad.png";
					break;
					
				}
			}

			Console.WriteLine(orientation);
			Console.WriteLine(nextImage);
			if(lastImage == nextImage)
				return;			
			lastImage = nextImage;
			imageView = new UIImageView(Util.FromBundle16x9(lastImage));
			var frame = UIApplication.SharedApplication.KeyWindow.Frame;// tvOut && TVOutManager.TVOutManager.tvoutWindow != null ? TVOutManager.TVOutManager.tvoutWindow.Frame : UIApplication.SharedApplication.KeyWindow.Frame;
			//var statush = UIApplication.SharedApplication.StatusBarFrame;
			if(!tvOut &&  (orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight))
				frame = new RectangleF(0,0,frame.Height,frame.Width);	
			this.Frame = frame;
			imageView.Frame = frame;
			progressSpinner.Frame = new RectangleF(new PointF(((this.Frame.Width - 50) /2) + 10,((this.Frame.Height - 50)/2) + 10),progressSpinner.Frame.Size);
			this.AddSubview(imageView);
			this.AddSubview(progressSpinner);
		}
		
		
	}
}

