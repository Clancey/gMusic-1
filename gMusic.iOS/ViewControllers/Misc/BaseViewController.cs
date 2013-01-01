using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Threading;
using MonoTouch.Foundation;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using ClanceysLib;
using MonoTouch.CoreGraphics;
using MonoTouch.ObjCRuntime;
using Pioneer;
using System.Threading.Tasks;

namespace GoogleMusic
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

	public abstract class BaseViewController : UITableViewController, ISearchable, IBaseViewController, IViewController
	{

		public Screens Screen { get; protected set; }
		public bool HasBackButton { get; set; }
		//protected SonglistViewController CurrentSongListViewController;
		protected EditSongViewController CurrentSongEditor;
		protected PopUpView popUpView;
		protected bool reloading;
		public RefreshTableHeaderView refreshView;
		public UISearchBar searchBar;
		public UIProgressView progressView;
		protected UIView headerView;
		protected bool hasShuffle;
		public bool HasSearch;
		UIControl shuffleView;
		protected UILabel shuffleLabel;
		public bool DarkThemed;
		public Action ShuffleClicked {get;set;}
		UIImageView shuffleimage;
		public void ReloadData()
		{
			TableView.ReloadData ();
		}
		public bool ShowMenu;
		public BaseViewController (UITableViewStyle style, bool hasShuffle) : this(style,hasShuffle,true)
		{
			
		}

		public BaseViewController (UITableViewStyle style, bool hasShuffle,bool hasSearch) : base (style)
		{	
			this.TableView.ScrollsToTop = true;
			this.HasSearch = hasSearch;
			this.hasShuffle = hasShuffle;
			//var bounds = View.Bounds;
			setupRefresh();
			if(UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) 
				RefreshRequested += delegate {
					Refresh();
				};
			
			progressView = new UIProgressView (UIProgressViewStyle.Bar);
			progressView.BackgroundColor = UIColor.Black;
			searchBar = new MySearchBar (new RectangleF (0, 0, TableView.Bounds.Width, 44)) {
					Delegate = new SearchDelegate (this),
					TintColor = UIColor.Black
				};
			//searchBar.
			
			headerView = new UIView (new RectangleF (0, 0, TableView.Bounds.Width, ((HasSearch && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && hasSearch && !DarkThemed) ? 44 : 0) + (hasShuffle ? 50: 0)));
			//headerView.AddSubview(toolbar);
			
			if(HasSearch && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
				headerView.AddSubview (searchBar);
			if(hasShuffle)
			{
				shuffleView = new UIControl(new RectangleF(0, ((UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) ? 44 : 0), TableView.Bounds.Width,50));
				shuffleLabel = new UILabel(new RectangleF(10,0,100,50));
				shuffleLabel.Text = "Shuffle";
				shuffleLabel.TextColor = UIColor.Black;
				shuffleLabel.Font = UIFont.BoldSystemFontOfSize(17f);
				shuffleLabel.BackgroundColor = UIColor.Clear;
				shuffleView.AddSubview(shuffleLabel);
				shuffleimage = new UIImageView(UIImage.FromFile("Images/shuffleGray.png"));
				shuffleimage.Frame = shuffleimage.Frame.SetLocation(80,10);
				shuffleView.AddSubview(shuffleimage);
				shuffleView.TouchDown += delegate{
					if(ShuffleClicked != null)
						ShuffleClicked();
				};
					
				headerView.AddSubview(shuffleView);	
			}
			//setupTable ();
			//this.View.AddGestureRecognizer(new UISwipeGestureRecognizer(this,new Selector("swiperight")){Direction = UISwipeGestureRecognizerDirection.Right});
		}
		private void setupRefresh()
		{
			if(Util.IsIos6)
			{
				RefreshControl = new UIRefreshControl();
				RefreshControl.AttributedTitle = new NSAttributedString("Pull down to refresh...".Translate());
				RefreshControl.ValueChanged += delegate {
					RefreshControl.AttributedTitle = new NSAttributedString("Loading...".Translate());
					Refresh();
				};
				return;
			}
			
			var bounds = View.Bounds;
			if(UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) 
			refreshView = new RefreshTableHeaderView (new RectangleF (0, -bounds.Height, bounds.Width, bounds.Height));
			if (reloading)
				refreshView.SetActivity (true);
		}
		[Export("swiperight")]
		public void Swipped(UISwipeGestureRecognizer sender)
		{
			Util.MainVC.ToggleMenu();
		}
		[Export("handlePPanGesture")]
		public void handlePPanGesture (PUIPanGestureRecognizer sender)
		{
			UIScrollView targetView = (UIScrollView)this.TableView;
			if (sender.State == UIGestureRecognizerState.Began) {
			}
			else if (sender.State == UIGestureRecognizerState.Changed) {

				var receiveOffset = sender.TranslationInView(targetView);
				var sectionOffset = (targetView.Frame.Width - 44);
				if(receiveOffset.X > (targetView.Frame.Width - 44))
				{
					return;
				}
				
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


		public virtual void Refresh()
		{
			ThreadPool.QueueUserWorkItem (delegate{
				Util.Api.GetSongsIfNeeded (delegate {
					this.BeginInvokeOnMainThread (delegate{
						ReloadComplete ();	
					});
				});
			});
		}
		
		bool progressShown;

		public void UpdateProgress (float progress)
		{
			
			Util.EnsureInvokedOnMainThread (delegate {
				progressShown = progressView.Superview == this.headerView;
				int progressH = 10;
				if (progress < 1f) {
					ReloadComplete();
					if (!progressShown) {
						this.headerView.AddSubview (progressView);
						progressView.Frame = new RectangleF (0, 0, headerView.Frame.Width, progressH);
						this.searchBar.Frame = this.searchBar.Frame.AddLocation (0, progressH);
						if(hasShuffle && shuffleView != null)
							this.shuffleView.Frame = this.shuffleView.Frame.AddLocation(0,progressH);
						this.headerView.Frame = this.headerView.Frame.Add (new SizeF (0, progressH));
					}
				} else {
					progressView.RemoveFromSuperview ();
					this.searchBar.Frame = this.searchBar.Frame.SetLocation(0,0);
					float headerHeight = DarkThemed ? 80 : 44;
					if(hasShuffle && shuffleView != null)
						this.shuffleView.Frame = this.shuffleView.Frame.SetLocation(0,(UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? headerHeight : 0));
					this.headerView.Frame = this.headerView.Frame.SetHeight( (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? headerHeight : 0) + (hasShuffle ? 50: 0));
				}
				progressView.Progress = progress;
			});
		}
		
		protected virtual void setupTable ()
		{
			if(UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && HasSearch && !Util.IsIos6) 
				TableView.AddSubview (refreshView);
			TableView.TableHeaderView = headerView;

			if(DarkThemed){
				this.View.BackgroundColor = UIColor.Clear;
				TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
				TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
			else
				TableView.BackgroundColor = UIColor.White;
			
			TableView.SectionIndexMinimumDisplayRowCount = 20;
			var gesture = new PUIPanGestureRecognizer();
			gesture.AddTarget((sender)=>{
				handlePPanGesture(gesture);
			});
			if(DarkThemed)
				this.View.AddGestureRecognizer( gesture);
		}
		[Obsolete ("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return Util.IsIpad;
		}
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			Util.LastOrientation = this.InterfaceOrientation;
			searchBar.Frame = new RectangleF (0, 0, TableView.Bounds.Width, 44);
			base.DidRotate (fromInterfaceOrientation);
			Util.MainVC.UpdateLoadingScreen();
			if(sectionOverlay != null)
			{
				var frame = TableView.Frame;
				frame.X = frame.Width - 44;
				frame.Width = 44;
				sectionOverlay.Frame = frame;
			}
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
				PresentViewController(controller, true,null);
		}
		
		public void DeactivateController (bool animated)
		{
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			if (nav != null)
				nav.PopViewControllerAnimated (animated);
			else
				DismissViewController (animated,null);
		}

		public void ShowPopUp ( bool isOffline, Action<int> clickedIndex, Action<bool> offlineSwitched)
		{
			try{
			this.searchBar.ResignFirstResponder(); 
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
			if(popUpView != null && popUpView.Superview == Util.BaseView)
				return;
			popUpView = new PopUpView (Util.BaseView.Frame, Screen, isOffline);
			popUpView.Clicked = clickedIndex;
			popUpView.OfflineToggled = offlineSwitched;
			Util.BaseView.AddSubview (popUpView);
			popUpView.AnimateIn ();
			
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
		
		protected void TriggerRefresh (bool showStatus)
		{
			if (Util.IsIos6) {
				refreshRequested (this, EventArgs.Empty);
				return;
			}
			if (refreshRequested == null)
				return;

			if (reloading)
				return;
			
			reloading = true;
			if (refreshView != null)
				refreshView.SetActivity (true);
			refreshRequested (this, EventArgs.Empty);

			if (reloading && showStatus && refreshView != null && !DarkThemed) {
				UIView.BeginAnimations ("reloadingData");
				UIView.SetAnimationDuration (0.2);
				TableView.ContentInset = new UIEdgeInsets (60, 0, 0, 0);
				UIView.CommitAnimations ();
			}
		}
				
		public void ReloadComplete ()
		{
			if(Util.IsIos6)
			{
				RefreshControl.AttributedTitle = new NSAttributedString(String.Format ("Last Updated".Translate() + ":{0:g}", DateTime.Now));
				RefreshControl.EndRefreshing();
				return;
			}

			//this.TableView.ReloadData();
			if (refreshView != null)
				refreshView.LastUpdate = DateTime.Now;
			if (!reloading)
				return;

			reloading = false;
			if (refreshView == null)
				return;
			
			refreshView.SetActivity (false);
			refreshView.Flip (false);
			UIView.BeginAnimations ("doneReloading");
			UIView.SetAnimationDuration (0.3f);
			TableView.ContentInset = new UIEdgeInsets (0, 0, 0, 0);
			refreshView.SetStatus (RefreshViewStatus.PullToReload);
			UIView.CommitAnimations ();
		}
		
		public void HandleUtilSongsCollectionChanged ()
		{
			this.InvokeOnMainThread (delegate {
				try {
					this.TableView.ReloadData ();
				} catch(Exception ex) {
					Console.WriteLine(ex);
				}
				HasRefreshed = true;
				ReloadComplete ();
			});
		}
		public static UIButton CreateNowPlayingButton()
		{
			var npImage = UIImage.FromFile ("Images/forward.png").CreateResizableImage( new UIEdgeInsets(0,4,0,14));
			var nowPlayingBtn = new UIButton(new RectangleF(PointF.Empty,npImage.Size));
			nowPlayingBtn.SetTitle ("Now Playing".Translate(), UIControlState.Normal);
			nowPlayingBtn.TitleLabel.Font = UIFont.BoldSystemFontOfSize(9f);
			nowPlayingBtn.TitleLabel.AdjustsFontSizeToFitWidth = true;
			nowPlayingBtn.TitleLabel.Lines = 0;
			nowPlayingBtn.TitleLabel.TextAlignment = UITextAlignment.Center;
			nowPlayingBtn.TitleLabel.Layer.ShadowColor = UIColor.Black.CGColor;
			nowPlayingBtn.TitleLabel.Layer.ShadowOffset = new SizeF(0,-1);
			
			nowPlayingBtn.ContentEdgeInsets = new UIEdgeInsets(6,6,6,14);
			nowPlayingBtn.SetTitleColor (UIColor.White, UIControlState.Normal);
			nowPlayingBtn.SetBackgroundImage (npImage,UIControlState.Normal);
			nowPlayingBtn.TouchDown += delegate {
				Util.MainVC.ShowNowPlaying ();
			};
			return nowPlayingBtn;
		}

		protected static bool ShouldLoadSongs = true;
		
		public override void ViewWillAppear (bool animated)
		{
			checkIfLoaded ();
			//Console.WriteLine("View did appear base view controller");
			if (TableView.TableHeaderView != headerView)
				setupTable ();
			base.ViewWillAppear (animated);
			if(this.NavigationController != null)
			{
				this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
				try{
					if(DarkThemed)
					  {
						if(shuffleLabel != null)
						{
							var frame = headerView.Frame;
							frame.Y = 0;
							frame.Height = 85f;
							headerView.Frame = frame;
							shuffleView.Frame = frame;
							TableView.TableHeaderView = shuffleView;
							shuffleLabel.TextColor = UIColor.White;
							shuffleLabel.Font = UIFont.BoldSystemFontOfSize(30f);
							frame = shuffleLabel.Frame;
							frame.Height = 30f;
							frame.Y = (headerView.Frame.Height - frame.Height)/2;
							shuffleLabel.Frame = frame;
							frame = shuffleimage.Frame;
							frame.X = shuffleLabel.Frame.Width + 20;
							frame.Y =  shuffleLabel.Frame.Y + (shuffleLabel.Frame.Height - frame.Height)/2 ;
							shuffleimage.Frame = frame;

						}
						TableView.ContentInset = new UIEdgeInsets(0,0,0,0);
						//this.NavigationController.NavigationBar.SetBackgroundImage(UIImage.FromFile("Images/topBar.png"),UIBarMetrics.Default);
					 //var back =	this.NavigationController.NavigationBar.BackItem;
						//(this.NavigationController.NavigationBar as UIView).Alpha = .5f;
						float[] colorMask = new float[6]{222, 255, 0, 255, 222, 255};
						UIImage img = UIImage.FromFile("Images/menubar.png");
						UIImage maskedImage = UIImage.FromImage(img.CGImage.WithMaskingColors(colorMask));
						
						this.NavigationController.NavigationBar.SetBackgroundImage( maskedImage,UIBarMetrics.Default);
					}
				}
				catch(Exception ex){
					Console.WriteLine(ex);
				}
				NavigationItem.HidesBackButton = false;
				if (!DarkThemed && Util.CurrentSong != null && Util.IsIphone){

					var nowPlayingBtn = BaseViewController.CreateNowPlayingButton();
					this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(nowPlayingBtn);// new UIBarButtonItem (UIImage.FromFile ("Images/nowPlaying.png"), UIBarButtonItemStyle.Bordered, delegate {
					//	Util.ShowNowPlaying ();	
					//});
				}
				else
					this.NavigationItem.RightBarButtonItem = null;
				if(!DarkThemed  && (Util.IsIphone || (Util.MainVC.InterfaceOrientation == UIInterfaceOrientation.Portrait || Util.MainVC.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown))){
					if(!HasBackButton)
					this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIImage.FromFile("Images/menu.png"), UIBarButtonItemStyle.Bordered, delegate {
						//if(DarkThemed)
						//	Util.AppDelegate.MainVC.tvViewController.ToggleMenu();
						//else{
						searchBar.ResignFirstResponder();
						Util.MainVC.ToggleMenu();
						//}
						//this.PresentModalViewController (new SettingsViewController (), true);
					});
				}
				else 
				{
					this.NavigationItem.HidesBackButton = true;
					this.NavigationItem.LeftBarButtonItem = null;
					this.NavigationController.NavigationBar.Frame = this.NavigationController.NavigationBar.Frame.SetLocation(0,0);
					/*
					this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromFile("Images/refresh.png"),UIBarButtonItemStyle.Bordered,delegate
					{
						this.TriggerRefresh(true);	
					});
					*/
				}
			}
			else
				Console.WriteLine("navigation was null");

			if(DarkThemed){
				this.View.BackgroundColor = UIColor.Clear;
				TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
				TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
			//this.TableView.ReloadData();
		
		}
		public override void ViewDidLoad ()
		{
			if(Util.IsIpad)
				this.NavigationItem.HidesBackButton = true;

		}
		protected void checkIfLoaded ()
		{
			//Util.StartThirdParty();

//			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && !HasRefreshed && !Util.IsIos6) {
//				
//				refreshView.SetStatus (RefreshViewStatus.ReleaseToReload);
//				reloading = true;
//				if (refreshView != null)
//					refreshView.SetActivity (true);
//	
//				if (refreshView != null && !DarkThemed) {
//					//UIView.BeginAnimations ("reloadingData");
//					//UIView.SetAnimationDuration (0.2);
//					TableView.ContentInset = new UIEdgeInsets (60, 0, 0, 0);
//					//UIView.CommitAnimations ();
//				}	
//			}
			
			if (string.IsNullOrEmpty (Settings.UserName) || string.IsNullOrEmpty (Settings.Key)) {
				Util.MainVC.ShowLogin (this);
				DataLoaded = true;
				return;
			} else if (!DataLoaded) {
				Util.TryLogin();
				DataLoaded = true;
			}
//			if (!DataLoaded || (Util.Songs.Count == 0 && Settings.SongsCount > 0)) {
//				if (!DataLoaded) {
//					Util.MainVC.ShowLoadingScreen();
//
//				}
//				DataLoaded = true;
//				if(isLoading)
//					return;
//				isLoading = true;
//				Task.Factory.StartNew(loadMusic);	
//			}

			
			
			//Console.WriteLine(Settings.UserName);
		
		}

		public static bool DataLoaded;
		public static bool isLoading{get;private set;}
		protected bool HasRefreshed = false;
		UIControl sectionOverlay;
		SectionScrollView scrollViewPopUp;
		public override void ViewDidAppear (bool animated)
		{

			if(DarkThemed && sectionOverlay == null )
			//if(sectionOverlay == null )
			{
				var frame = TableView.Frame;
				frame.X = frame.Width - 50;
				frame.Width = 50;
				sectionOverlay = new UIControl(frame);
				scrollViewPopUp = new SectionScrollView();
				sectionOverlay.TouchDown += delegate {
					if(TableView.Source.SectionIndexTitles(TableView).Length <= 1)
						return;
					try{
						scrollViewPopUp.Sections = (TableView.Source).SectionIndexTitles(TableView);
						scrollViewPopUp.SelectedIndex = TableView.IndexPathsForVisibleRows.First().Section;
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
					}
					scrollViewPopUp.Show(TableView);
				};
				View.Superview.AddSubview(sectionOverlay);
				
			
				
				//this.View.Superview.AddGestureRecognizer(tapGest);
			}
			
			if(sectionOverlay != null)
			{
				var frame = TableView.Frame;
				frame.X = frame.Width - 50;
				frame.Width = 50;
				sectionOverlay.Frame = frame;
			}
			base.ViewDidAppear (animated);
				
			FlurryAnalytics.FlurryAnalytics.LogPageView();
			UIApplication.SharedApplication.BeginReceivingRemoteControlEvents ();
			this.BecomeFirstResponder ();
			if(DarkThemed)
				TableView.ContentInset = new UIEdgeInsets (0, 0, 0, 0);
			
		}


		private void loadMusic ()
		{
			Util.LoadData();
			Util.TryLogin();
			Util.EnsureInvokedOnMainThread (delegate{
				isLoading = false;
				Util.MainVC.HideLoadingScreen();
			
			});

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
		
		public abstract void StartSearch ();

		public abstract void FinishSearch ();

		public abstract void PerformFilter (string text);

		public abstract void SearchButtonClicked (string text);
		
		class SearchDelegate : UISearchBarDelegate
		{
			BaseViewController container;
			
			public SearchDelegate (BaseViewController container)
			{
				this.container = container;
			}
			
			public override void OnEditingStarted (UISearchBar searchBar)
			{
				searchBar.ShowsCancelButton = true;
				container.StartSearch ();
			}
			
			public override void OnEditingStopped (UISearchBar searchBar)
			{
				searchBar.ShowsCancelButton = false;
				container.FinishSearch ();
			}

			string SearchText;
			bool isSearching;

			public override void TextChanged (UISearchBar searchBar, string searchText)
			{
				SearchText = searchText ?? "";
				if (string.IsNullOrEmpty (searchText)) {
					isSearching = false;
					//container.PerformFilter (searchText ?? "");
					return;
				}
				lastKeyPressed = DateTime.Now;
				if (!isSearching) {
					ThreadPool.QueueUserWorkItem (delegate{
						runSearchLoop ();
					});
					
				}
			}

			private bool runSearchNow = false;
			DateTime lastKeyPressed;

			private void runSearchLoop ()
			{
				while (!runSearchNow && (DateTime.Now - lastKeyPressed).TotalSeconds < 1) {
					Console.WriteLine ("sleeping: " + (DateTime.Now - lastKeyPressed).TotalSeconds);
					Thread.Sleep (500);	
				}
				runSearchNow = false;
				Console.WriteLine ("searching now");
				
				container.PerformFilter (SearchText);
				isSearching = false;
			}
			
			public override void CancelButtonClicked (UISearchBar searchBar)
			{
				searchBar.ShowsCancelButton = false;
				searchBar.Text = "";
				container.FinishSearch ();
				searchBar.ResignFirstResponder ();
			}
			
			public override void SearchButtonClicked (UISearchBar searchBar)
			{
				if (string.IsNullOrEmpty (SearchText)) {
					container.PerformFilter (SearchText ?? "");
					return;
				}
				lastKeyPressed = DateTime.Now;
				if (!isSearching) {
					isSearching = true;
					ThreadPool.QueueUserWorkItem (delegate{
						runSearchLoop ();
					});
					
				}
				runSearchNow = true;
				searchBar.ResignFirstResponder();
			}
		}
		
		#endregion
		public override bool ShouldAutorotate ()
		{
			if (Util.IsIphone)
				return false;
			return Util.ShouldRotate;
		}
	}
}

