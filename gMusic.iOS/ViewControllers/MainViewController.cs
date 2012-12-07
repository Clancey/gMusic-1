using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using FlyOutNavigation;
using MonoTouch.CoreAnimation;
using System.Drawing;
using ClanceysLib;
using Pioneer;

namespace GoogleMusic
{
	public abstract class MainViewController : PUIViewController, IMainViewController
	{
		public MainViewController (bool includeTvOut)
		{
			offlineSwitch = new BooleanElement ("Offline Only".Translate(), Settings.ShowOfflineOnly){TextColor = UIColor.White};
			offlineSwitch.ValueChanged += delegate {
				Settings.ShowOfflineOnly = offlineSwitch.Value;
				FlurryAnalytics.FlurryAnalytics.LogEvent ("Offline Switch: " + Settings.ShowOfflineOnly);
				Util.UpdateOfflineSongs (true, true,true);
			};
			
			if(includeTvOut)
			{
//				tvManager = new TVOutManager.TVOutManager();
//				if(tvViewController == null)
//					tvViewController = CreateTvVc();
//				tvManager.UseVC = true;
//				tvManager.VC = tvViewController;
				// add observers for each of the screen notifications
				
				NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIScreenDidConnectNotification"),
				                                               (notify) => { 
					Util.ShouldRotate = false;
					screenDidConnectNotification(); 
					notify.Dispose();	});
				
				NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIScreenDidDisconnectNotification"),
				                                               (notify) => { 
					Util.ShouldRotate = true;
					screenDidDisconnectNotification(); 
					notify.Dispose(); });
				
				NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIScreenModeDidChangeNotification"),
				                                               (notify) => { 
					screenModeDidChangeNotification(); 
					notify.Dispose(); });
				
			}
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		}
		
		public virtual void screenDidConnectNotification()
		{
			Console.WriteLine(this);
//			if(UIScreen.Screens.Length == 1 || tvManager.IsRunning)
				return;
//			if(tvViewController == null)
//				tvViewController = CreateTvVc();
//			tvManager.UseVC = true;
//			tvManager.VC = tvViewController;
//			tvManager.startTVOut();
		}
		public virtual void screenDidDisconnectNotification()
		{
//			tvManager.stopTVOut();
		}
		
		public void screenModeDidChangeNotification()
		{
			//infoLabel.Text = string.Format("Screen mode changed");
		}
		
		public FlyOutNavigationController NavigationController;
		protected BooleanElement offlineSwitch;
//		public TVOutManager.TVOutManager tvManager;
//		public TvOutViewController tvViewController;
		
//		public virtual TvOutViewController CreateTvVc()
//		{
//			return new TvOutViewController();
//		}
//		
		
		public abstract void UpdateSong (Song currentSong);
		
		public abstract void SetState (bool state);
		
		public abstract void PlaylistChanged ();
		
		public abstract void UpdateStatus (string currentTime, string remainingTime, float percent);
		
		public abstract void SetPlayCount ();
		
		public abstract void ShowNowPlaying ();
		
		public abstract void RefreshSongs ();
		
		public abstract void RefreshArtists ();
		
		public abstract void RefreshGenre ();
		
		public abstract void RefreshAlbum ();
		
		public abstract void RefreshPlaylist ();
		
		public abstract void UpdateSongProgress (float percent);
		
		public abstract void UpdatePlaylistProgress (float percent);
		
		public abstract void UpdateCurrentSongDownloadProgress (float percent);
		
		public abstract void UpdateMeter ();
		public abstract void GoToArtist(int artistId);
		public abstract void GoToAlbum(int albumId);
		public abstract void GoToGenre(int genreId);
		
		public abstract void ToggleMenu ();
		public abstract void DownloaderUpdated();
		public static bool LoginShowing = false;
		public void ShowLogin (BaseViewController dvc)
		{
			if(LoginShowing)
				return;
			if(dvc.DarkThemed)
			{
				//AppDelegate.MainVC.tvViewController.ShowLoadingScreen();
				var alert = new BlockAlertView ("Log in".Translate(), "Please log in on your device.".Translate());
				alert.AddButton("Ok",null);
				alert.Show ();
				return;
			}
			LoginShowing = true;
			PresentViewController (new AccountController (), false,null);
		}

		public void ShowLoadingScreen ()
		{
			//throw new NotImplementedException ();
		}

		public void HideLoadingScreen ()
		{
			//throw new NotImplementedException ();
		}

		public void UpdateLoadingScreen ()
		{
			//throw new NotImplementedException ();
		}

		MBProgressHUD statusHud;
		public void ShowStatus(string title)
		{
			Util.EnsureInvokedOnMainThread (delegate {
				if (statusHud != null)
					statusHud.Hide (true);
				statusHud = new MBProgressHUD (this.View.Frame){TitleText = "Loading Library".Translate(),Mode = MBProgressHUDMode.Determinate};
				statusHud.Progress = 0f;
				statusHud.Show (true);
			});
		}
		public void HideStatus()
		{
			Util.EnsureInvokedOnMainThread (delegate {
				if (statusHud != null)
					statusHud.Hide (true);
			});
		}
		public void UpdateStatus(float percent)
		{
			if (statusHud == null)
				return;
			Util.EnsureInvokedOnMainThread (delegate {
				statusHud.Progress = percent;
			});
		}
		
		public MusicSectionView NowPlayingView { get; set; }
		
		public RootElement CreateRoot ()
		{
			return CreateRoot (null);
		}
		
		public RootElement CreateRoot(MusicSectionView nowPlayingView) 
		{
			return this.CreateRoot(nowPlayingView,true);
		}
		
		public RootElement CreateRoot (MusicSectionView nowPlayingView, bool includeNowPlaing, bool isTvOut = false)
		{
			//if (Util.IsIpad)
			UIFont font = isTvOut ?  UIFont.BoldSystemFontOfSize (30) : UIFont.BoldSystemFontOfSize (17);
			
			NowPlayingView = nowPlayingView ?? new MusicSectionView (new RectangleF (0, 0, 250, 100), "", "", "", null, false,false, null);
			RootElement root = new RootElement ("");
			//if (Util.IsIpad) {
			if(includeNowPlaing)
				root.Add (new Section ("Now Playing".Translate())
				          {
					new UIViewElement ("", NowPlayingView, false)
				});
			//}
			root.Add (
				new Section ("Library".Translate()){
				new StyledMultilineElement ("Playlists".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font, Height = isTvOut ? 60f:0f},
				new StyledMultilineElement ("Artists".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font, Height = isTvOut ? 60f:0f},
				new StyledMultilineElement ("Songs".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font, Height = isTvOut ? 60f:0f},
				new StyledMultilineElement ("Albums".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font, Height = isTvOut ? 60f:0f},
				#if mp3tunes
				//new StyledStringElement ("Movies"){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font},
				
				#else
				new StyledMultilineElement ("Genres".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font, Height = isTvOut ? 60f:0f},
				#endif						
				new StyledMultilineElement ("Auto Playlists".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font, Height = isTvOut ? 60f:0f},
				//new StyledStringElement ("Current Queue"){TextColor = UIColor.White,BackgroundColor = UIColor.Clear},
				//new StyledStringElement ("Download Queue"){TextColor = UIColor.White,BackgroundColor = UIColor.Clear},
				
			});
			var settings = new Section ("Settings".Translate()){
				offlineSwitch,
				new StyledStringElement ("Download Queue".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font},
			};
			if (!Util.IsIpad) {
				settings.Insert(0, UITableViewRowAnimation.None, new StyledStringElement("Equalizer"){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font});
			}
			if(!isTvOut){
				settings.Add(new StyledStringElement ("Settings".Translate()){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,Font = font});
				root.Add (settings);
			}
			root.UnevenRows = true;
			return root;
			
			
		}
		
		protected CALayer MakeBackgroundLayer (System.Drawing.RectangleF frame)
		{
			var texture = UIImage.FromBundle ("Images/texture.png");
			//var textureColor = UIColor.FromPatternImage (texture);
			
			
			UIGraphics.BeginImageContext (frame.Size);
			//var c = UIGraphics.GetCurrentContext ();
			
			texture.DrawAsPatternInRect (frame);
			
			UIImage.FromFile ("Images/menu-shadow.png").Draw (frame);
			var result = UIGraphics.GetImageFromCurrentImageContext ();
			
			UIGraphics.EndImageContext ();
			
			var back = new CALayer (){
				Frame = frame
			};
			Graphics.ConfigLayerHighRes (back);
			back.Contents = result.CGImage;
			return back;
		}
		
		#if mp3tunes
		
		public abstract void RefreshMovies();
		public abstract void ShowMovieController(MPMoviePlayerViewController vc);
		//public abstract void PushViewController(UIViewController vc);
#endif
	}

}

