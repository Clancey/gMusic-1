using System;
using System.Drawing;
using ClanceysLib;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;

namespace GoogleMusic
{
	public enum Screens
	{
		Artist,
		Albums,
		Playlist,
		Genre,
		Songs,
		NowPlaying,
	}
	public class PopUpView : UIControl
	{
		
		static float AlertViewButtonHeight = 44f;
		MiniPopUp miniView;
		public Action<int> Clicked {get;set;}
		public Action<bool> OfflineToggled {get;set;}
		Screens FromScreen;
		bool IsOffline;
		public PopUpView (RectangleF frame,Screens screen,bool isOffLine) : base(frame)
		{
			IsOffline = isOffLine;
			FromScreen = screen;
			miniView = new MiniPopUp (this);
			miniView.Frame = PopUpFrame;
			//this.AddSubviews (miniView);
			this.AddGestureRecognizer(new UISwipeGestureRecognizer(this,new Selector("AnimateOut")));
			this.TouchUpInside += delegate
			{
				AnimateOut ();		
			};
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
		}

		private RectangleF PopUpFrame {
			get {
				float h = 178;
				if(FromScreen == Screens.Playlist)
					h = 178;
#if gmusic
				else if(FromScreen == Screens.Songs)
					h = 310 - 24;
				else if(FromScreen == Screens.NowPlaying)
					h = 250 - 18;
#endif
				var x = (this.Frame.Width - 200) / 2;
				var y = (this.Frame.Height - h) / 2;
				return new RectangleF (x, y, 200, h);
			}
		}

		UIImageView tempImage;
		
		static float AlertViewBounce = 20f;
		public void AnimateIn ()
		{
			var frame = PopUpFrame;
			var center = new PointF(frame.GetMidX(),frame.GetMidY() + AlertViewBounce);
			frame.Y = -frame.Height;
			miniView.Frame = frame;
			this.AddSubview (miniView);
			UIView.Animate(.4,0.0, UIViewAnimationOptions.CurveEaseOut,delegate{
				miniView.Center = center;
			}, delegate{
				UIView.Animate(.1,0, UIViewAnimationOptions.TransitionNone,delegate{
					center.Y -= AlertViewBounce;
					miniView.Center = center;
				},null);
			});
			
		}

		[Export("AnimateOut")]
		public void AnimateOut ()
		{
			UIView.Animate(.1,0,0,delegate{
				var center = miniView.Center;
				center.Y += AlertViewBounce;
				miniView.Center = center;
			},
			delegate{
				UIView.Animate(.4,0,UIViewAnimationOptions.CurveEaseIn,delegate{
					var frame = miniView.Frame;
					frame.Y  = -frame.Height;
					miniView.Frame = frame;
				},delegate{
					miniView.RemoveFromSuperview();
					this.RemoveFromSuperview();
				});
			});

		}


		
		class MiniPopUp : UIView
		{
			class availableOffLineView : UIView
			{
				public UISwitch AvailableOffline;
				public UILabel label;
				public availableOffLineView(RectangleF rect) :base( rect)
				{
					AvailableOffline = new UISwitch(rect);
					label = new UILabel(rect.SetHeight(50));
					label.Text = "Offline".Translate();
					label.TextColor = UIColor.White;
					label.Font = UIFont.BoldSystemFontOfSize(17);
					label.BackgroundColor = UIColor.Clear;
					this.AddSubview(label);
					this.AddSubview(AvailableOffline);
				}
				public override void LayoutSubviews ()
				{
					var midY = this.Frame.Height /2;
					AvailableOffline.Frame = AvailableOffline.Frame.SetLocation(this.Frame.Width - 100,midY - (AvailableOffline.Frame.Height / 2));
					label.Frame = label.Frame.SetLocation(10, midY - (label.Frame.Height /2));
				}
			}
			UIButton PlayBtn;
			UIButton PlayNextBtn;
			UIButton GoToArtist;
			UIButton GoToAlbum;
			UIButton AddPlaylist;
			UIButton MagicPlaylist;
			UIButton EditBtn;
			availableOffLineView AvailableOffline;
			UIButton deleteBtn;
			PopUpView Parent;
			UIImage red;
			UIImage black;
			UIImage green;

			
			public MiniPopUp (PopUpView parent): base()
			{
				var image = UIImage.FromFile("Images/alert-red-button.png");
				red  = image.StretchableImage(10,0);
				image = UIImage.FromFile("Images/alert-black-button.png");
				black = image.StretchableImage(10,0);
				image = UIImage.FromFile("Images/alert-green-button.png");
				green = image.StretchableImage(10,0);
				this.BackgroundColor = UIColor.Clear;
				Parent = parent;
				PlayBtn = UIButton.FromType(UIButtonType.Custom);
				PlayBtn.Frame = new RectangleF (10, 10, 180, AlertViewButtonHeight);
				PlayBtn.SetTitle("Play".Translate(), UIControlState.Normal);
				PlayBtn.BackgroundColor = UIColor.Clear;
				PlayBtn.SetBackgroundImage(green, UIControlState.Normal);
				PlayBtn.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(0);
					Parent.AnimateOut ();
				};
				PlayNextBtn = UIButton.FromType(UIButtonType.Custom);
				PlayNextBtn.Frame = (new RectangleF (10, 10, 180, AlertViewButtonHeight));
				PlayNextBtn.SetTitle("Play Next".Translate(), UIControlState.Normal);
				PlayNextBtn.BackgroundColor = UIColor.Clear;
				PlayNextBtn.SetBackgroundImage(green, UIControlState.Normal);
				PlayNextBtn.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(1);
					Parent.AnimateOut ();
				};
				AddPlaylist = UIButton.FromType(UIButtonType.Custom);
				AddPlaylist.Frame = (new RectangleF (10, 10, 180, AlertViewButtonHeight));
				AddPlaylist.SetTitle("Add to Playlist".Translate(), UIControlState.Normal);
				AddPlaylist.BackgroundColor = UIColor.Clear;
				AddPlaylist.SetBackgroundImage(black, UIControlState.Normal);
				AddPlaylist.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(2);
					Parent.AnimateOut ();
				};
				MagicPlaylist = UIButton.FromType(UIButtonType.Custom);
				MagicPlaylist.Frame = (new RectangleF (10, 10, 180, AlertViewButtonHeight));
				var instant = "Make Instant Mix".Translate();
				MagicPlaylist.SetTitle(instant, UIControlState.Normal);
				MagicPlaylist.BackgroundColor = UIColor.Clear;
				MagicPlaylist.SetBackgroundImage(black, UIControlState.Normal);
				MagicPlaylist.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(3);
					Parent.AnimateOut ();
				};
				AvailableOffline = new availableOffLineView(new RectangleF (10, 10, 180, AlertViewButtonHeight));
				AvailableOffline.AvailableOffline.On = Parent.IsOffline;
				AvailableOffline.AvailableOffline.ValueChanged += delegate {
					if(Parent.OfflineToggled != null)
						Parent.OfflineToggled(AvailableOffline.AvailableOffline.On);
				};

				EditBtn = UIButton.FromType(UIButtonType.Custom);
				EditBtn.Frame =  (new RectangleF (10, 10, 180, AlertViewButtonHeight));
				EditBtn.SetTitle("Edit".Translate(), UIControlState.Normal);
				EditBtn.BackgroundColor = UIColor.Clear;
				EditBtn.SetBackgroundImage(black, UIControlState.Normal); 
				EditBtn.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(5);
					Parent.AnimateOut ();
				};
				
				deleteBtn = UIButton.FromType(UIButtonType.Custom);
				deleteBtn.Frame = new RectangleF (10, 10, 180, AlertViewButtonHeight);
				deleteBtn.SetTitle("Delete".Translate(), UIControlState.Normal);
				deleteBtn.BackgroundColor = UIColor.Clear;
				deleteBtn.SetBackgroundImage(black, UIControlState.Normal);
				deleteBtn.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(4);
					Parent.AnimateOut ();
				};
				GoToArtist = UIButton.FromType(UIButtonType.Custom);
				GoToArtist.Frame = (new RectangleF (10, 10, 180, AlertViewButtonHeight));
				GoToArtist.SetTitle("Go to Artist".Translate(), UIControlState.Normal);
				GoToArtist.BackgroundColor = UIColor.Clear;
				GoToArtist.SetBackgroundImage(black, UIControlState.Normal);
				GoToArtist.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(6);
					Parent.AnimateOut ();
				};

				GoToAlbum = UIButton.FromType(UIButtonType.Custom);
				GoToAlbum.Frame = (new RectangleF (10, 10, 180, AlertViewButtonHeight));
				GoToAlbum.SetTitle("Go to Album".Translate(), UIControlState.Normal);
				GoToAlbum.BackgroundColor = UIColor.Clear;
				GoToAlbum.SetBackgroundImage(black, UIControlState.Normal);
				GoToAlbum.TouchDown += delegate {
					if(Parent.Clicked != null)
						Parent.Clicked(7);
					Parent.AnimateOut ();
				};
				
				
				this.AddSubviews (Contents());
			}
			
			UIView[] Contents ()
			{
				if(Parent.FromScreen == Screens.Artist)
				{
					return new UIView[]{
						PlayBtn,
						AddPlaylist,
						AvailableOffline,
						//MagicPlaylist,
						//deleteBtn,
					};	
				}
				else if(Parent.FromScreen == Screens.Albums)
				{
					return new UIView[]{
						PlayBtn,
						AddPlaylist,
						AvailableOffline,
						//MagicPlaylist,
						//deleteBtn,
					};	
				}
				else if(Parent.FromScreen == Screens.Songs)
				{
					return new UIView[]{
						PlayNextBtn,
						AddPlaylist,
#if gmusic
						MagicPlaylist,
#endif
						AvailableOffline,
						EditBtn,
						//deleteBtn,
					};	
				}
				else if(Parent.FromScreen == Screens.Genre)
				{
					return new UIView[]{
						PlayBtn,
						AddPlaylist,
						AvailableOffline,
						//deleteBtn,
					};		
				}
				else if(Parent.FromScreen == Screens.Playlist)
				{
					return new UIView[]{
						PlayBtn,
						AvailableOffline,
						deleteBtn,
					};		
				}
				else if(Parent.FromScreen == Screens.NowPlaying)
				{
					return new UIView[]{
						AddPlaylist,
#if gmusic
						MagicPlaylist,
#endif
						AvailableOffline,
						EditBtn,
						//deleteBtn,
					};	
				}
				return new UIView[0];
			}
			public override void LayoutSubviews ()
			{
				float y = 10f;
				foreach(var view in Contents())
				{
					view.Frame = view.Frame.SetLocation(10,y);
					y += view.Frame.Height + 10f;
				}
			}
			
			private float Width
			{
				get{return Frame.Width;}	
			}
			
			private float Height
			{
				get{return Frame.Height;}	
			}
			
			float XOffset = 0.0f;
			float YOffset = 0.0f;


			public override void Draw (RectangleF rect)
			{
				// Center HUD
				RectangleF allRect = this.Bounds;
				// Draw rounded HUD bacgroud rect
				RectangleF boxRect = new RectangleF (((allRect.Size.Width - this.Width) / 2) + this.XOffset, ((allRect.Size.Height - this.Height) / 2) + this.YOffset, this.Width, this.Height);
				CGContext ctxt = UIGraphics.GetCurrentContext ();
				this.FillRoundedRect (boxRect, ctxt);
				base.Draw (rect);
			}

			void FillRoundedRect (RectangleF rect, CGContext context)
			{
				float radius = 10.0f;
				context.BeginPath ();
				//context.SetGrayFillColor (0.0f, this.Opacity);
				context.MoveTo (rect.GetMinX () + radius, rect.GetMinY ());
				context.AddArc (rect.GetMaxX () - radius, rect.GetMinY () + radius, radius, (float)(3 * Math.PI / 2), 0f, false);
				context.AddArc (rect.GetMaxX () - radius, rect.GetMaxY () - radius, radius, 0, (float)(Math.PI / 2), false);
				context.AddArc (rect.GetMinX () + radius, rect.GetMaxY () - radius, radius, (float)(Math.PI / 2), (float)Math.PI, false);
				context.AddArc (rect.GetMinX () + radius, rect.GetMinY () + radius, radius, (float)Math.PI, (float)(3 * Math.PI / 2), false);
				context.ClosePath ();
				context.SetFillColor (UIColor.Black.ColorWithAlpha(.75f).CGColor);		
				context.FillPath ();
			
				context.SetStrokeColor (UIColor.White.CGColor);	
				context.BeginPath ();
				//context.SetGrayFillColor (0.0f, this.Opacity);
				context.MoveTo (rect.GetMinX () + radius, rect.GetMinY ());
				context.AddArc (rect.GetMaxX () - radius, rect.GetMinY () + radius, radius, (float)(3 * Math.PI / 2), 0f, false);
				context.AddArc (rect.GetMaxX () - radius, rect.GetMaxY () - radius, radius, 0, (float)(Math.PI / 2), false);
				context.AddArc (rect.GetMinX () + radius, rect.GetMaxY () - radius, radius, (float)(Math.PI / 2), (float)Math.PI, false);
				context.AddArc (rect.GetMinX () + radius, rect.GetMinY () + radius, radius, (float)Math.PI, (float)(3 * Math.PI / 2), false);
				context.ClosePath ();
		
				context.StrokePath ();
			}
		
				
		}
	}
}

