using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.AudioToolbox;
using System.Net;
using System.Threading;
using MonoTouch.AVFoundation;
using MonoTouch.CoreMedia;
using ClanceysLib;
using MonoTouch.MediaPlayer;
using System.Xml;
using System.Diagnostics;
using FlurryAnalytics;
using Pioneer;
//using MTIKS;

namespace GoogleMusic
{
	public class Application
	{
		static void Main (string[] args)
		{
			#if !DEBUG
			try {
				AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
					
					Console.WriteLine("Caught unhandled");
					Console.WriteLine (e);
					//	FlurryAnalytics.FlurryAnalytics.LogError ("UnhandledException", e.ToString (), new NSException ("Unhandled Exception",e.ToString(),null));
					Exception ex = (Exception)e.ExceptionObject;
					throw ex;
					//#if !DEBUG
					//					MonoTouch.TestFlight.TestFlight.Log (e.ToString ());
					//#endif
				};
				#endif
				#if RELEASE
				UIApplication.CheckForIllegalCrossThreadCalls = false;
				#endif
				ServicePointManager.DefaultConnectionLimit = 50;
				NSString appClass = new NSString (@"MyUIApp");
				NSString delegateClass = new NSString (@"AppDelegate");
				UIApplication.Main (args, appClass, delegateClass);
				
				#if !DEBUG
			} catch (Exception ex) {
				Console.WriteLine("Caught main");
				Console.WriteLine (ex);
				//FlurryAnalytics.FlurryAnalytics.LogError (ex.Source, ex.ToString (), new NSException (ex.Message,ex.ToString(),null));
				throw ex;
				
			}
			#endif
		}
	}
	
	[Register("MyUIApp")]
	public class MyUIApp : UIApplication
	{
		public override void SendEvent (UIEvent theEvent)
		{
			if (theEvent.Type == UIEventType.RemoteControl) {
				
				Console.WriteLine (theEvent.Subtype);
				switch (theEvent.Subtype) {
					
				case UIEventSubtype.RemoteControlPause:
				case UIEventSubtype.RemoteControlPlay:
				case UIEventSubtype.RemoteControlTogglePlayPause:
					Util.PlayPause ();
					break;
				case UIEventSubtype.RemoteControlPreviousTrack:
					Util.Previous ();
					break;
					
				case UIEventSubtype.RemoteControlBeginSeekingForward:
				case UIEventSubtype.RemoteControlNextTrack:
					Util.Next ();
					break;
					
				default:
					break;
				}
			} else 
				base.SendEvent (theEvent);
		}	
		
	}
	
	
	// The name AppDelegate is referenced in the MainWindow.xib file.
	
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		
		NSTimer progressUpdateTimer;
		NSTimer refreshTimer;
		// class-level declarations
		public UIWindow window;
		public MainViewController MainVC;
		public PPioneerManager _pioneerManager;
		
		int bgTask = 0;
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//var 

			Console.WriteLine ("finished loading started");
			
			window = new UIWindow (UIScreen.MainScreen.Bounds);			
			window.MakeKeyAndVisible ();
			Util.WindowFrame = window.Frame;
			Util.Scale = UIScreen.MainScreen.Scale;
			
			if (!string.IsNullOrEmpty (Settings.UserName))
				Database.SetDatabase (Settings.UserName);
			//crashlyics apikey: 9803131fee8ab2370b975a3dacffc2d25e8464cd
			
			AppRater.DaysInstalledCountNeeded = 2;
			AppRater.RunCountNeeded = 10;
			AppRater.AppLaunched (Util.appId);
			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackTranslucent;
			
			
			if (Util.IsIphone) {
				MainVC = new iPhoneBaseViewController ();
			} else {
				MainVC = new iPhoneBaseViewController ();
				//MainVC = new iPadBaseViewController ();
			}
			Util.MainVC = MainVC;
			window.RootViewController = MainVC;
			
			bool runOnce = false;
			refreshTimer = NSTimer.CreateRepeatingScheduledTimer (.05, delegate {
				
				Util.EnsureInvokedOnMainThread (delegate{
					//Util.MainApp.NetworkActivityIndicatorVisible = Util.ShouldShowNetwork;
				});
				if (Util.Player == null || (Util.Player.CurrentState != StreamingPlayback.State.Playing)){ //} && !MainVC.SilentPlayer.Playing)) {
					if(bgTask > 0)
					{
						UIApplication.SharedApplication.EndBackgroundTask(bgTask);
						bgTask = 0;
						Pioneer.PNotificationManager.SharedInstance().SetSoundOutPut(false);
					}
					return;
				}
				else if(Util.Player.CurrentState != StreamingPlayback.State.Playing)// && MainVC.SilentPlayer.Playing)
				{
					Console.WriteLine("had to hit playpause!!!");
					if(Util.CurrentSong.IsReady())
						Util.Player.Play ();
				}
				if(bgTask == 0)
				{
					bgTask = UIApplication.SharedApplication.BeginBackgroundTask(delegate{
						UIApplication.SharedApplication.EndBackgroundTask(bgTask);
						bgTask = 0;
					});
					Pioneer.PNotificationManager.SharedInstance().SetSoundOutPut(true);
				}
				MainVC.UpdateMeter ();
				
			});
			progressUpdateTimer = NSTimer.CreateRepeatingScheduledTimer (.5, delegate {
				try {
					//TODO: fix tvout
//					if(MainVC.tvViewController != null)
//						MainVC.tvViewController.UpdateClock();
					var currentTime = Math.Round (Util.Player == null ? 0 : Util.Player.CurrentTime);
					var totalTime = Math.Round (Util.Player == null ? 0 : Util.Player.duration);
					if (Util.Player == null || Util.Player.CurrentState != StreamingPlayback.State.Playing) {
						if (!runOnce) {
							MainVC.UpdateStatus (Util.FormatTimeSpan (TimeSpan.FromSeconds (currentTime)), "-" + Util.FormatTimeSpan (TimeSpan.FromSeconds (totalTime - currentTime)), Util.Player == null ? 0 : (float)Util.Player.Progress);
							runOnce = true;
						}

						return;
					}
					
					runOnce = false;
					
					if ((Util.CurrentSong.Duration > 30000) && (Util.Player.Progress > .5 || currentTime > 240)) {
						Scrobbler.Main.Scrobble (Util.CurrentSong);
					}

					var downLoadPercent = Util.CurrentSong.GetDownloadPercent();
					MainVC.UpdateCurrentSongDownloadProgress(downLoadPercent);
					MainVC.UpdateStatus (Util.FormatTimeSpan (TimeSpan.FromSeconds (currentTime)), "-" + Util.FormatTimeSpan (TimeSpan.FromSeconds (totalTime - currentTime)), (float)Util.Player.Progress);
					//Console.WriteLine("Setting remaining time");
					//TODO: fix now playing
//					var nowPlaying = Util.NowPlaying;
//					nowPlaying.PlaybackDuration = totalTime;
//					nowPlaying.ElapsedPlaybackTime = currentTime;
//					MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlaying;
					
					
					if ((totalTime - currentTime) < 0) {
						Util.Player.TimeIsOver();
					}
					
					
				} catch (Exception ex) {
					Console.WriteLine ("Error in update timer" + ex);	
				}
				
				//Console.WriteLine("(" + percent  + ") " + currentTime + " : " + totalTime + " : " + currentItem.CurrentTime.TimeScale );
			});
			
			#region TV Out
			
			
			_pioneerManager = PPioneerManager.SharedPioneerManager;
			
			del =new pioneerDelegate();
			_pioneerManager.Delegate = del;
			_pioneerManager.ApplicationDidFinishLaunching();
			if(UIScreen.Screens.Length > 1)
				MainVC.screenDidConnectNotification();
			
			#endregion
			Console.WriteLine ("finished launching");
			ApplyTheme ();
			return true;
		}
		
		void ApplyTheme()
		{
			var barItemImage = UIImage.FromFile("Images/menubar-button.png").CreateResizableImage(new UIEdgeInsets(0,5,0,5));
			UIBarButtonItem.Appearance.SetBackgroundImage(barItemImage, UIControlState.Normal, UIBarMetrics.Default);
			var backBtnImage = UIImage.FromFile("Images/back.png").CreateResizableImage( new UIEdgeInsets(0,14,0,4));
			UIBarButtonItem.Appearance.SetBackButtonBackgroundImage(backBtnImage, UIControlState.Normal, UIBarMetrics.Default);
			
			//UIImage thumbImage = new UIImage ("Images/slider-handle.png");
			//UISlider.Appearance.SetThumbImage (thumbImage, UIControlState.Normal);
			//UISlider.Appearance.SetThumbImage (thumbImage, UIControlState.Highlighted);
			
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) {
				var navBarImage = UIImage.FromFile ("Images/menubar.png");
				UINavigationBar.Appearance.SetBackgroundImage (
					navBarImage,
					UIBarMetrics.Default
					);
				
				
			} else {
				var navBarImage = UIImage.FromFile ("Images/menubar-right.png");
				UINavigationBar.Appearance.SetBackgroundImage (
					navBarImage,
					UIBarMetrics.Default
					);
				
				UINavigationBar.Appearance.SetTitleTextAttributes (
					new UITextAttributes (){
					TextColor = UIColor.FromRGBA(255,255,255,255),
					TextShadowColor = UIColor.FromRGBA(0,0,0,.8f),
					TextShadowOffset = new UIOffset(0,-1),
				}
				);
			}
			
			
		}
		
		public override void DidEnterBackground (UIApplication application)
		{
			_pioneerManager.ApplicationDidEnterBackground();
		}
		public override void OnActivated (UIApplication application)
		{
			try{
				AudioSession.SetActive (true);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
			_pioneerManager.ApplicationDidBecomeActive();
			if(UIScreen.Screens.Length > 1)
				MainVC.screenDidConnectNotification();
		}
		public override void WillEnterForeground (UIApplication application)
		{
			_pioneerManager.ApplicationWillEnterForeground();
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
		}
		public override void WillTerminate (UIApplication application)
		{
			_pioneerManager.ApplicationWillTerminate();
			//mtiks.SharedSession.Stop();
		}
		
		
		public override void OnResignActivation (UIApplication application)
		{
			_pioneerManager.ApplicationWillResignActive();
			if(UIScreen.Screens.Length > 1)
				MainVC.screenDidDisconnectNotification();
		}
		
		pioneerDelegate del;
		public class pioneerDelegate : PPioneerManagerDelegate
		{
			#region implemented abstract members of Pioneer.PPioneerManagerDelegate
			public override void Success ()
			{
				UIApplication.SharedApplication.IdleTimerDisabled = true;
				Util.ShouldRotate = false;
			}
			public override void Failed ()
			{
				Console.WriteLine("Pioneer Failed");
				Util.ShouldRotate = true;
				// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			}
			
			public override void Done ()
			{
				UIApplication.SharedApplication.IdleTimerDisabled = false;
				Util.ShouldRotate = true;
				Console.WriteLine("Pioner Done");
				// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			}
			
			public override void PhomekeyPressed ()
			{
				Console.WriteLine("PhomekeyPressed");
				// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			}
			
			public override void PreceivedData (NSData data)
			{
				Console.WriteLine("PreceivedData");
				// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			}
			#endregion
			
		}
		
		bool shouldPlay;
		
		void HandleAudioSessionInterrupted (object sender, EventArgs e)
		{
			shouldPlay = (Util.Player.CurrentState == StreamingPlayback.State.Playing);
			
			if (Util.Player != null)
				Util.Player.Pause ();		
			
		}
		
		string lastRoute="";
		
		public void PropertyChanged (AudioSessionProperty prop, int size, IntPtr data)
		{
			try {
				if (!lastRoute.Contains ("Speaker") && AudioSession.AudioRoute.Contains ("Speaker"))
					Util.Player.Pause ();
				lastRoute = AudioSession.AudioRoute;
				Console.WriteLine (AudioSession.OutputRoutes + " - Audio property changed: " + prop);
				//return new AudioSession.PropertyListener(prop,size,data);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
		
		string appName = "gMusic.app";
		
		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			if(string.IsNullOrEmpty (url.Host))
				return true;
			if(url.Host.IndexOf("playlist",StringComparison.OrdinalIgnoreCase) >= 0)
			{
				Settings.CurrentTab = 1;
			}
			else if(url.Host.IndexOf("play",StringComparison.OrdinalIgnoreCase) >= 0)
			{
				Util.ShouldAutoPlay = true;
			}
			else if(url.Host.IndexOf("random",StringComparison.OrdinalIgnoreCase) >= 0)
			{
				Settings.CurrentSongId = "";
				Util.ShouldAutoPlay = true;
			}
			if(Util.ShouldAutoPlay && BaseViewController.DataLoaded)
			{
				if(Util.CurrentSong != null)
				{
					Util.PlayPause();
				}
				else 
				{
					Util.PlayRandom();
				}
			}	
			
			return true;
		}

	}
}

