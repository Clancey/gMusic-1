using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Threading;
using MonoTouch.Foundation;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.ObjCRuntime;
using Pioneer;

namespace GoogleMusic
{
	public class BaseDialogViewController : DialogViewController
	{
		//protected SonglistViewController CurrentSongListViewController;
		protected EditSongViewController CurrentSongEditor;
		PopUpView popUpView;
		public bool DarkThemed;
		public BaseDialogViewController(IntPtr handle): base(handle){}
		public BaseDialogViewController () : this (true,true)
		{
			
			this.Style = UITableViewStyle.Plain;
		}
		public BaseDialogViewController (bool pushing) : this (pushing,true)
		{

		}

		public BaseDialogViewController (bool pushing,bool enableSearch) : base (null,pushing)
		{
			this.Style = UITableViewStyle.Plain;
			this.EnableSearch = enableSearch;
			this.SearchBarTintColor = UIColor.Black;
			this.View.AddGestureRecognizer (new UISwipeGestureRecognizer (this, new Selector ("swiperight")){Direction = UISwipeGestureRecognizerDirection.Right});
			
		}
		
		[Export("swiperight")]
		public void Swipped (UISwipeGestureRecognizer sender)
		{
			Util.MainVC.ToggleMenu ();
		}
		
		[Export("handlePPanGesture")]
		public void handlePPanGesture (PUIPanGestureRecognizer sender)
		{
			UIScrollView targetView = (UIScrollView)this.TableView;
			
			if (sender.State == UIGestureRecognizerState.Began) {
			}
			else if (sender.State == UIGestureRecognizerState.Changed) {
				var receiveOffset = sender.TranslationInView(targetView);
				
				if(targetView.ContentOffset.X<0)receiveOffset.X = receiveOffset.X/2.0f;
				if(targetView.ContentOffset.Y<0)receiveOffset.Y = receiveOffset.Y/2.0f;
				if(targetView.ContentSize.Width - targetView.Frame.Size.Width < targetView.ContentOffset.X)receiveOffset.X = receiveOffset.X/2.0f;
				if(targetView.ContentSize.Height - targetView.Frame.Size.Height < targetView.ContentOffset.Y)receiveOffset.Y = receiveOffset.Y/2.0f;
				
				var currentOffset = new PointF(targetView.ContentOffset.X - receiveOffset.X, targetView.ContentOffset.Y - receiveOffset.Y);
				
				currentOffset.X = 0;
				
				targetView.ContentOffset = currentOffset;
			}
			else if (sender.State == UIGestureRecognizerState.Ended) {
				sender.TimerScrollingInStateEnded(targetView);
			}
			
		}


		public void ShowPopUp (PointF startPoint, Screens screen, bool shouldBeLocal, Action<int> clickedIndex, Action<bool> offLineChanged)
		{
			popUpView = new PopUpView (Util.BaseView.Frame, screen, shouldBeLocal);
			popUpView.Clicked = clickedIndex;
			popUpView.OfflineToggled = offLineChanged;
			Util.BaseView.Superview.AddSubview (popUpView);
			popUpView.AnimateIn ();
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var gesture = new PUIPanGestureRecognizer();
			gesture.AddTarget((sender)=>{
				handlePPanGesture(gesture);
			});
			if(DarkThemed)
				this.View.AddGestureRecognizer( gesture);
		}

		public override void ViewWillAppear (bool animated)
		{
			this.CreateSizingSource(true);
			base.ViewWillAppear (animated);
			if(this.NavigationController != null)
			{
			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
				try{
					if(DarkThemed){
						
						float[] colorMask = new float[6]{222, 255, 0, 255, 222, 255};
						UIImage img = UIImage.FromFile("Images/menubar.png");
						UIImage maskedImage = UIImage.FromImage(img.CGImage.WithMaskingColors(colorMask));
						
						this.NavigationController.NavigationBar.SetBackgroundImage( maskedImage,UIBarMetrics.Default);
					}
				}
				catch{
				}
			}
			if (!DarkThemed && Util.CurrentSong != null && Util.IsIphone) {
				var nowPlayingBtn = BaseViewController.CreateNowPlayingButton();
				this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (nowPlayingBtn);// new UIBarButtonItem (UIImage.FromFile ("Images/nowPlaying.png"), UIBarButtonItemStyle.Bordered, delegate {

			} else
				this.NavigationItem.RightBarButtonItem = null;
		}

		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			FlurryAnalytics.FlurryAnalytics.LogPageView ();
			UIApplication.SharedApplication.BeginReceivingRemoteControlEvents ();
			this.BecomeFirstResponder ();
			
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			
			//UIApplication.SharedApplication.EndReceivingRemoteControlEvents ();
			base.ViewWillDisappear (animated);
		}

	}
}

