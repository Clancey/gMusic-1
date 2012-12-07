// 
//  Copyright 2012  James Clancey, Xamarin Inc  (http://www.xamarin.com)
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
using System.Linq;
using MonoTouch.CoreGraphics;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using System.Threading;

public class BlockAlertView : NSObject
{
	UIView view;
	float height;
	bool vignetteBackground;
	public UIImage backgroundImage;
	List<object[]> blocks;

	static UIImage background;
	static UIFont titleFont;
	static UIFont messageFont;
	static UIFont buttonFont;
	static float AlertViewBounce = 20f;
	static float AlertViewBorder = 10f;
	static float AlertViewButtonHeight = 44f;
	static float ButtonPadding = 10f;
	List<UIButton> buttons;
	public bool FixedWidthButtons;

	static BlockAlertView()
	{
		background = UIImage.FromFile("Images/alert-bg.png").StretchableImage(10,10);
		titleFont = UIFont.SystemFontOfSize(20);
		messageFont = UIFont.SystemFontOfSize(18);
		buttonFont = UIFont.BoldSystemFontOfSize(18);
	}
	public enum ButtonColor
	{
		Black,
		Red,
		Green,
	}

	public static BlockAlertView Alert(string title,string message)
	{
		return new BlockAlertView(title,message);
	}
	public BlockAlertView(string title,string message)
	{
		blocks = new List<object[]>();
		buttons = new List<UIButton>();
		var parentView = BlockBackground.SharedInstance;
		var frame = parentView.Bounds;
		frame.X = (float)Math.Floor((frame.Width - background.Size.Width) * .5);
		frame.Width = background.Size.Width;

		view = new UIView(frame);
		height = AlertViewBorder + 6;
		var maxSize = new SizeF(frame.Width - (AlertViewBorder * 2f),1000);
		if(!string.IsNullOrEmpty(title))
		{
			var size = view.StringSize(title,titleFont,maxSize,UILineBreakMode.WordWrap);
			var labelview = new UILabel(new RectangleF(AlertViewBorder,height,maxSize.Width,size.Height)){
				Font = titleFont,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextColor = UIColor.FromWhiteAlpha(244f,1f),
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Center,
				ShadowColor = UIColor.DarkGray,
				ShadowOffset = new SizeF(0,-1),
				Text = title,
			};
			view.AddSubview(labelview);
			height = labelview.Frame.Bottom + AlertViewBorder;

		}

		if(!string.IsNullOrEmpty(message))
		{

			var size = view.StringSize(message,messageFont,maxSize,UILineBreakMode.WordWrap);
			var labelview = new UILabel(new RectangleF(AlertViewBorder,height,maxSize.Width,size.Height)){
				Font = messageFont,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextColor = UIColor.FromWhiteAlpha(244f,1f),
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Center,
				ShadowColor = UIColor.DarkGray,
				ShadowOffset = new SizeF(0,-1),
				Text = message,
			};
			view.AddSubview(labelview);
			height = labelview.Frame.Bottom + AlertViewBorder;
		}
		vignetteBackground = false;
	}
	public void SetDestructiveButton(string title,Action tapped)
	{
		AddButton(title,ButtonColor.Red,tapped);
	}
	public void SetCancelButton(string title, Action tapped)
	{
		AddButton(title,ButtonColor.Red,tapped);
	}
	public void AddButton(string title,Action tapped)
	{
		AddButton(title,ButtonColor.Green,tapped);
	}
	public void AddButton(string title,ButtonColor color,Action tapped)
	{
		var objects = new object[3]{
			title,
			color.ToString().ToLower(),
			tapped
		};
		blocks.Add(objects);
	}
	public void Show()
	{
		//TODO: TVOut fix
		//if(TVOutManager.TVOutManager.tvoutWindow != null)
		//	Show (TVOutManager.TVOutManager.tvoutWindow);
		//else
			Show(UIApplication.SharedApplication.KeyWindow);

	}
	public void Show(UIWindow window)
	{
		if(blocks.Count == 0)
			AddButton("Ok",null);
		BlockBackground.SharedInstance.RootViewController.View.Bounds = window.Bounds;
		float maxWidth = view.Bounds.Width - AlertViewBorder*2;
		bool isSecondButton = false;
		RectangleF frame;
		//int index = 0;
		for(int i = 0;i < blocks.Count;i++)
		{
			object[] block = blocks[i];
			string title = (string)block[0];
			string color = (string)block[1];
			UIImage image = UIImage.FromFile(string.Format("Images/alert-{0}-button.png",color));
			image = image.StretchableImage(10,0);

			float maxHalfWidth = (view.Bounds.Width - AlertViewBorder *3)/2;
			float width = view.Bounds.Width - AlertViewBorder * 2;
			float xOffset = AlertViewBorder;
			if(FixedWidthButtons){
				width = maxWidth;
				if(i > 0)
					height += AlertViewButtonHeight + ButtonPadding;
			}
			else if(isSecondButton)
			{
				width = maxHalfWidth;
				xOffset = width + AlertViewBorder * 2;
				isSecondButton = false;
			}
			else if(i + 1 < blocks.Count)
			{
				var size = view.StringSize(title,buttonFont,maxWidth, UILineBreakMode.Clip);
				if(size.Width < maxHalfWidth - AlertViewBorder)
				{
					//It might fit
					var block2 = blocks[i];
					string title2 = (string)block2[0];
					size = view.StringSize(title2,buttonFont,maxWidth, UILineBreakMode.Clip);
					if(size.Width < maxHalfWidth - AlertViewBorder)
					{
						isSecondButton = true;
						width = maxHalfWidth;
					}
					else
						height += AlertViewButtonHeight + ButtonPadding;
				}

			}
			else if(blocks.Count == 1)
			{
				var size = view.StringSize(title,buttonFont,width, UILineBreakMode.Clip);
				size.Width = Math.Max(size.Width,80);
				if(size.Width + 3 * AlertViewBorder < width)
				{
					width = size.Width + 2 * AlertViewBorder;
					xOffset = (float)Math.Floor((view.Bounds.Width - width) * .5);
				}
			}
			else
			{
				height += AlertViewButtonHeight + ButtonPadding;
			}

			UIButton btn = UIButton.FromType(UIButtonType.Custom);
			btn.Frame = new RectangleF(xOffset,height ,width,AlertViewButtonHeight);
			btn.TitleLabel.Font = buttonFont;
			btn.TitleLabel.MinimumFontSize =10;
			btn.TitleLabel.TextAlignment = UITextAlignment.Center;
			btn.BackgroundColor = UIColor.Clear;
			btn.Tag = i+1;
			buttons.Add(btn);
			btn.SetBackgroundImage(image, UIControlState.Normal);
			btn.SetTitleColor(UIColor.White, UIControlState.Normal);
			btn.SetTitleShadowColor(UIColor.DarkGray, UIControlState.Normal);
			btn.TitleLabel.ShadowOffset = new SizeF(0,-1);
			btn.SetTitle(title, UIControlState.Normal);
			btn.TouchUpInside += delegate(object sender, EventArgs e) {
				ButtonClicked(btn);
			};
			view.AddSubview (btn);
		}

		height  = buttons.Last().Frame.Bottom + AlertViewBorder;

		if(height < background.Size.Height)
		{
			float offset = background.Size.Height - height;
			height = background.Size.Height;

			for(int i = 0; i< blocks.Count; i++)
			{
				var btn = buttons[i];
				frame = btn.Frame;
				frame.Y += offset;
			//	btn.Frame = frame;
			}
		}

		frame = view.Frame;
		frame.Y = -height;
		frame.Height = height;
		frame.X = (float)Math.Floor((BlockBackground.SharedInstance.RootViewController.View.Bounds.Width - frame.Width) * .5) ;
		view.Frame = frame;

		UIImageView modalBackgroundView = new UIImageView(view.Bounds);
		modalBackgroundView.Image = background;
		modalBackgroundView.ContentMode = UIViewContentMode.ScaleToFill;
		view.InsertSubview(modalBackgroundView,0);
		view.Transform = window.Transform;

		if(backgroundImage != null)
		{
			BlockBackground.SharedInstance.BackgroundImage = backgroundImage;
			backgroundImage.Dispose();
			backgroundImage = null;
		}
		BlockBackground.SharedInstance.VignetteBackground = vignetteBackground;
		BlockBackground.SharedInstance.AddView(view,window);

		var center = view.Center;
		center.Y = (float)Math.Floor(BlockBackground.SharedInstance.RootViewController.View.Bounds.Height * .5) + AlertViewBounce;
		UIView.Animate(.4,0.0, UIViewAnimationOptions.CurveEaseOut,delegate{
			BlockBackground.SharedInstance.Alpha = 1f;
			view.Center = center;
		}, delegate{
			UIView.Animate(.1,0, UIViewAnimationOptions.TransitionNone,delegate{
				center.Y -= AlertViewBounce;
				view.Center = center;
			},null);
		});
	}
	public void DismissWithClickedButton(int index, bool animated)
	{
		if(index >= 0 && index < blocks.Count)
		{
			Action tapped = blocks[index][2] as Action;
			if(tapped != null)
				tapped();

		}

		if(animated)
		{
			UIView.Animate(.1,0,0,delegate{
				var center = view.Center;
				center.Y += AlertViewBounce;
				view.Center = center;
			},
			delegate{
				UIView.Animate(.4,0,UIViewAnimationOptions.CurveEaseIn,delegate{
					var frame = view.Frame;
					frame.Y  = -frame.Height;
					view.Frame = frame;
					BlockBackground.SharedInstance.ReduceAlphaIfEmpty();
				},delegate{
					BlockBackground.SharedInstance.RemoveView(view);
					view.Dispose();
					view = null;
				});
			});

		}
		else
		{
			BlockBackground.SharedInstance.RemoveView (view);
			view.Dispose();
			view = null;
		}
	}
	[Export("btnClicked")]
	public void ButtonClicked(UIButton sender)
	{
		DismissWithClickedButton(buttons.IndexOf(sender),true);
	}
}

public class BlockBackground : UIWindow
{
	static BlockBackground sharedInstance;

	public static BlockBackground SharedInstance
	{
		get{
			if(sharedInstance == null)
				sharedInstance = new BlockBackground();
			return sharedInstance;
		}
	}
	UIWindow _previousKeyWindow;
	public UIImage BackgroundImage;
	public bool VignetteBackground;

	public BlockBackground() : this(UIScreen.MainScreen.Bounds)
	{

	}

	public BlockBackground(RectangleF frame) : base (frame)
	{
		WindowLevel = UIWindow.LevelStatusBar;
        Hidden = true;
        UserInteractionEnabled = false;
        BackgroundColor = UIColor.FromWhiteAlpha(0.4f,0.5f);
        VignetteBackground = false;
		RootViewController = new RotatingViewController();
	}

	public void AddView(UIView view)
	{
		AddView(view,UIApplication.SharedApplication.KeyWindow);
	}
	public void AddView(UIView view, UIWindow window)
	{
		this.Transform = window.Transform;
		if(Hidden)
		{
		 	_previousKeyWindow = window;
       		Alpha = 0.0f;
        	Hidden = false;
        	this.UserInteractionEnabled = true;
			this.Screen = window.Screen;
			this.MakeKeyWindow();
		}
		if(RootViewController.View.Subviews.Length > 0)
		{
			RootViewController.View.Subviews.Last().UserInteractionEnabled = false;
		}

		if(this.BackgroundImage != null)
		{
			UIImageView backgroundView = new UIImageView(BackgroundImage);
	        backgroundView.Frame = Bounds;
			backgroundView.ContentMode = UIViewContentMode.ScaleToFill;
			RootViewController.View.AddSubview(backgroundView);
			BackgroundImage = null;
		}
		RootViewController.View.AddSubview(view);

	}
	public void RemoveView(UIView view)
	{
		if(view.Superview != RootViewController.View)
			return;
		view.RemoveFromSuperview();

		if(RootViewController.View.Subviews.Length > 0)
		{
			UIView topView = RootViewController.View.Subviews.Last();
			if(topView is UIImageView)
				topView.RemoveFromSuperview();
		}
		if(RootViewController.View.Subviews.Length == 0)
		{
			this.Hidden = true;
			if(_previousKeyWindow != null){
				_previousKeyWindow.MakeKeyAndVisible();
				_previousKeyWindow = null;
			}
		}
		else
		{
			RootViewController.View.Subviews.Last().UserInteractionEnabled = true;
		}

	}
	public void ReduceAlphaIfEmpty()
	{
		if (RootViewController.View.Subviews.Length == 1 || (RootViewController.View.Subviews.Length == 2 && RootViewController.View.Subviews[0] is UIImageView))
	    {
	        Alpha = 0.0f;
	        UserInteractionEnabled = false;
	    }

	}
	public override void Draw (RectangleF rect)
	{
		if(BackgroundImage != null || VignetteBackground)
			return;
		var context = UIGraphics.GetCurrentContext();
		float[] locations = new float[2]{0,1f};
		float[] colors = new float[8]{0,0,0,0,0,0,0,0.75f};
		var colorSpace = CGColorSpace.CreateDeviceRGB();
		var gradient = new CGGradient(colorSpace,colors,locations);
		colorSpace.Dispose();

		PointF center = new PointF(Bounds.GetMidX(),Bounds.GetMidY());
		float radius = Math.Min(Bounds.Width,Bounds.Height);
		context.DrawRadialGradient(gradient,center,0,center,radius, CGGradientDrawingOptions.DrawsAfterEndLocation);
		gradient.Dispose();
	}
	private class RotatingViewController : UIViewController
	{
		public override bool ShouldAutorotate ()
		{
			return Util.ShouldRotate;
		}

		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);
			foreach(var view in View.Subviews)
			{
				var center = View.Frame.GetCenter();
				if(toInterfaceOrientation ==  UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
					center = new PointF(center.Y,center.X);
				UIView.Animate(duration,delegate{
					view.Center = center;
				});
			}
		}
	}
}
