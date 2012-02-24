using System;
using MonoTouch.UIKit;
using System.Threading;
using MonoTouch.Foundation;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using ClanceysLib;
using MonoTouch.CoreGraphics;
using MonoTouch.ObjCRuntime;

namespace gMusic
{
	public class MySearchBar : UISearchBar
	{
		public MySearchBar (RectangleF rect) : base (rect)
		{
			
		}

		public override void LayoutSubviews ()
		{
			this.Frame = this.Frame.SetWidth(this.Superview.Bounds.Width);
			base.LayoutSubviews ();
			foreach (var view in this.Subviews) {
				if (view is UITextField)
					view.Frame = view.Frame.SetWidth (view.Frame.Width - 25);
				if (view is UIButton)
					view.Frame = view.Frame.AddLocation (-25, 0);
			}
		}
	}

	public abstract class BaseViewController : UITableViewController
	{
		//PopUpView popUpView;
		//LoadingScreen loadingScreen;
		protected bool reloading;
		public UISearchBar searchBar;
		public UIProgressView progressView;
		protected UIView headerView;
		protected bool hasShuffle;
		//TapableView shuffleView;
		public Action ShuffleClicked {get;set;}

		public BaseViewController (UITableViewStyle style, bool hasShuffle) : base (style)
		{	

			setupTable ();
			//this.View.AddGestureRecognizer(new UISwipeGestureRecognizer(this,new Selector("swiperight")){Direction = UISwipeGestureRecognizerDirection.Right});
		}
		
		
		public virtual void Refresh()
		{
			
		}
		
		bool progressShown;

		protected virtual void setupTable ()
		{
			TableView.SectionIndexMinimumDisplayRowCount = 20;
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			searchBar.Frame = new RectangleF (0, 0, TableView.Bounds.Width, 44);
			base.DidRotate (fromInterfaceOrientation);
		}

		public void ActivateController (UIViewController controller)
		{
			//dirty = true;
			
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			// We can not push a nav controller into a nav controller
			if (nav != null && !(controller is UINavigationController))
				nav.PushViewController (controller, true);
			else
				PresentModalViewController (controller, true);
		}
		
		public void DeactivateController (bool animated)
		{
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			if (nav != null)
				nav.PopViewControllerAnimated (animated);
			else
				DismissModalViewControllerAnimated (animated);
		}
		
		EventHandler refreshRequested;
		/// <summary>
		/// If you assign a handler to this event before the view is shown, the
		/// DialogViewController will have support for pull-to-refresh UI.
		/// </summary>
		public event EventHandler RefreshRequested {
			add {
				refreshRequested += value; 
			}
			remove {
				refreshRequested -= value;
			}
		}
			
		public void ReloadComplete ()
		{
			this.TableView.ReloadData();
		
		}
		
		public void HandleUtilSongsCollectionChanged ()
		{
			this.InvokeOnMainThread (delegate {
				try {
					this.TableView.ReloadData ();
				} catch {
					
				}
				HasRefreshed = true;
				ReloadComplete ();
			});
		}

		protected static bool ShouldLoadSongs = true;
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
		}
		
		
		static bool DataLoaded;
		static bool isLoading;
		protected bool HasRefreshed = false;

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
				
			//Flurry.FlurryAnalytics.LogPageView();
			UIApplication.SharedApplication.BeginReceivingRemoteControlEvents ();
			this.BecomeFirstResponder ();
			
		}

		public override void LoadView ()
		{
			base.LoadView ();
		}

		private void loadMusic ()
		{
			
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			//UIApplication.SharedApplication.EndReceivingRemoteControlEvents ();
			base.ViewWillDisappear (animated);
		}
		
		public override void DidMoveToParentViewController (UIViewController parent)
		{
			Console.WriteLine("new parent");
			base.DidMoveToParentViewController (parent);
		}
		
		#region search
		
		#endregion

	}
}

