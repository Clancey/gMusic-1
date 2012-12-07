using System;
using MonoTouch.UIKit;
using ClanceysLib;
using System.Drawing;
using System.Threading;
using MonoTouch.Foundation;
using System.IO;
//using MonoTouch.Facebook.Authorization;

namespace GoogleMusic
{
	public class SettingsViewController : UIViewController
	{
		UIScrollView scrollView;
		UITextField emailField;
		UITextField passwordField;
		//UIToolbar topBar;
		UILabel offlineLabel;
		UISwitch offlineSlider;
		UILabel offlineOnlyLabel;
		UISwitch wifiOnlySwitch;
		UIButton resyncBtn;
		UILabel lastFmLabel;
		UISwitch lastFmSwitch;
		UIButton facebook;
		UILabel TotalSongsLabel;
		UIButton clearData;
		UISwitch autoPlaySwitch;
		UILabel autoPlayLabel;
		
		public SettingsViewController ()
		{
			Title = "Settings".Translate();
			
			
			if(Util.IsIphone)
			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIImage.FromFile("Images/menu.png"), UIBarButtonItemStyle.Bordered, delegate {
					
					((iPhoneBaseViewController)Util.AppDelegate.MainVC).NavigationController.ToggleMenu();
				//this.PresentModalViewController (new SettingsViewController (), true);
			});
			else
				this.NavigationItem.HidesBackButton = true;
		}
		
		UIImageView bgImage;
		UIImage red,black,green;
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var image = UIImage.FromFile("Images/alert-red-button.png");
			red  = image.StretchableImage(10,0);
			image = UIImage.FromFile("Images/alert-black-button.png");
			black = image.StretchableImage(10,0);
			image = UIImage.FromFile("Images/alert-green-button.png");
			green = image.StretchableImage(10,0);

			scrollView = new UIScrollView(View.Bounds);
			//this.View.Frame = this.View.Frame.SetWidth(this.NavigationItem.TitleView.Frame.Width);
			var widthOffset = Util.IsIphone ? 20 : 60;
			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			bgImage = new UIImageView(UIImage.FromFile("Images/bg.png" ));
			bgImage.Frame = this.View.Bounds;
			View.AddSubview(bgImage);
			//topBar = new UIToolbar(new RectangleF(0,0,this.View.Frame.Width,40));
			//topBar.BarStyle = UIBarStyle.Black;
			//topBar.SetItems(new []{ new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace,null),NavigationItem.RightBarButtonItem},false);
			//this.View.AddSubview(topBar);
			var offset = Util.IsIphone ? 20 : 20; 
			emailField = new UITextField {
				//BackgroundColor = UIColor.White,
				BorderStyle = UITextBorderStyle.RoundedRect, 
				AutocapitalizationType = UITextAutocapitalizationType.None,
				AutocorrectionType = UITextAutocorrectionType.No,
				KeyboardType = UIKeyboardType.EmailAddress,
				Placeholder = "Email".Translate(),
				Frame = new RectangleF (10, offset, this.View.Frame.Width - widthOffset, 33),
				ReturnKeyType = UIReturnKeyType.Next,
				ShouldReturn = delegate {
					passwordField.BecomeFirstResponder ();
					return true;
				},
			};
			emailField.Text = Settings.UserName;
			scrollView.AddSubview (emailField);
			offset += 50;
			passwordField = new UITextField {
				//BackgroundColor = UIColor.White,
				BorderStyle = UITextBorderStyle.RoundedRect, 
				AutocapitalizationType = UITextAutocapitalizationType.None,
				AutocorrectionType = UITextAutocorrectionType.No,
				Placeholder = "Password".Translate(),
				SecureTextEntry = true,
				Frame = new RectangleF (10, offset,  this.View.Frame.Width - widthOffset, 33),
				ReturnKeyType = UIReturnKeyType.Done,
				ShouldReturn = delegate {
					ResignFirstResponder ();
					TrySignin ();
					return true;
				},
			};
			passwordField.Text = Settings.Key;
			scrollView.AddSubview (passwordField);
			offset += 50;
			offlineLabel = new UILabel(new RectangleF(10,offset,190,33)){BackgroundColor = UIColor.Clear,Text = "Save songs by default.".Translate(),TextColor = UIColor.White};
			scrollView.AddSubview(offlineLabel);
			
			offlineSlider = new UISwitch(new RectangleF(215,offset,100,33));
			offlineSlider.On = Settings.ShouldSaveFilesByDefault;
			offlineSlider.ValueChanged += delegate {
				Settings.ShouldSaveFilesByDefault = offlineSlider.On;
			};
			scrollView.AddSubview(offlineSlider);
			offset += 50;

			autoPlayLabel = new UILabel(new RectangleF(10,offset,190,33)){BackgroundColor = UIColor.Clear,Text = "Auto play on launch.".Translate(),TextColor = UIColor.White};
			scrollView.AddSubview(autoPlayLabel);
			
			autoPlaySwitch = new UISwitch(new RectangleF(215,offset,100,33));
			autoPlaySwitch.On = Settings.AutoPlay;
			autoPlaySwitch.ValueChanged += delegate {
				Settings.AutoPlay = autoPlaySwitch.On;
			};
			scrollView.AddSubview(autoPlaySwitch);
			offset += 50;
			
			/*
						
			offlineOnlyLabel = new UILabel(new RectangleF(10,270,190,33)){BackgroundColor = UIColor.Clear,Text = "Only show offline songs"};
			View.AddSubview(offlineLabel);
			
			offlineSlider = new UISwitch(new RectangleF(215,270,100,33));
			offlineSlider.On = Settings.ShouldSaveFilesByDefault;
			offlineSlider.ValueChanged += delegate {
				Settings.ShouldSaveFilesByDefault = offlineSlider.On;
			};
			
			View.AddSubview(offlineSlider);
			
			*/

			lastFmLabel = new UILabel(new RectangleF(10,offset,150, 33)){BackgroundColor = UIColor.Clear,Text = "Scrobble Last.fm".Translate(),TextColor = UIColor.White};
			
			lastFmSwitch = new UISwitch(new RectangleF(215,offset,100,33)){On = Settings.LastFmEnabled};
			lastFmSwitch.ValueChanged += delegate {
				if(!Settings.LastFmEnabled)
					this.NavigationController.PushViewController(new lastFmLogin(),true);
				Settings.LastFmEnabled = lastFmSwitch.On;
				
			};
			
			this.scrollView.AddSubview(lastFmLabel);
			this.scrollView.AddSubview(lastFmSwitch);			
			
			/*
			offset += 50;
			facebook = new UIGlassyButton(new RectangleF(10,offset, this.View.Frame.Width - widthOffset,33));
			facebook.Color = UIColor.Blue;
			facebook.Title = "Login to Facebook";
			facebook.TouchDown += delegate {
				loginFacebook();
			};
			this.View.AddSubview(facebook);
			*/
			
			offset += 50;
			offlineOnlyLabel = new UILabel(new RectangleF(10,offset,150, 33)){BackgroundColor = UIColor.Clear,Text = "Stream over Cellular".Translate(),TextColor = UIColor.White};
			
			wifiOnlySwitch = new UISwitch(new RectangleF(215,offset,100,33)){On= !Settings.DownloadWifiOnly};
			wifiOnlySwitch.ValueChanged += delegate(object sender, EventArgs e) {
				Settings.DownloadWifiOnly = !wifiOnlySwitch.On;
			};
			this.scrollView.AddSubview(offlineOnlyLabel);
			this.scrollView.AddSubview(wifiOnlySwitch);
			offset += 50;
			resyncBtn = UIButton.FromType(UIButtonType.Custom);
			resyncBtn.Frame = (new RectangleF(10,offset, this.View.Frame.Width - widthOffset,44));
			resyncBtn.BackgroundColor = UIColor.Clear;
			resyncBtn.SetBackgroundImage(green, UIControlState.Normal);
			//resyncBtn.SetTitleColor(UIColor.White, ;
			resyncBtn.SetTitle("Reload Database".Translate(), UIControlState.Normal);
			resyncBtn.TouchDown += delegate {
				try{
				Database.ResetDatabase();
				Util.LoadData();
				Settings.CurrentSongId = "";
				Settings.HasData = false;
				Settings.LastSongsUpdate = "";
				Settings.LastUpdateCompleted = DateTime.MinValue;
				Settings.LastUpdateRequest = "";
				Settings.LastPlaylistUpdate = "";
				Settings.ContinuationToken = "";
				Settings.OverrideSync = true;
				Settings.SongsCount = 0;
				Util.Api.GetSongsIfNeeded();	
					var alert = new BlockAlertView("","Your database has been reset".Translate());
				alert.AddButton("Ok",null);
				//alert.Show();
				FlurryAnalytics.FlurryAnalytics.LogEvent("Database Reset");
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
				
				/*
				
				///
				//Util.Api = null;
				Database.ResetDatabase();
				//Util.UpdateMusic(true,true);
				Settings.CurrentSongId = "";
				Settings.HasData = false;
				Settings.LastSongsUpdate = "";
				Settings.LastUpdateCompleted = DateTime.MinValue;
				Settings.LastUpdateRequest = "";
				Settings.ContinuationToken = "";
				Settings.OverrideSync = true;
				Settings.SongsCount = 0;
				Util.LoadData();
				//Util.Api = new GoogleMusicApi(Settings.UserName);
				Util.Api.GetSongsIfNeeded();
				*/
			};
			scrollView.AddSubview(resyncBtn);
			offset += 50;
			clearData = UIButton.FromType(UIButtonType.Custom);
			clearData.Frame  =(new RectangleF(10,offset, this.View.Frame.Width - widthOffset,44));
			clearData.SetTitle("Reset Offline".Translate(), UIControlState.Normal);
			clearData.BackgroundColor = UIColor.Clear;
			clearData.SetBackgroundImage (red, UIControlState.Normal);
			//clearData.TitleColor = UIColor.White;
			clearData.TouchDown += delegate {
				foreach (var file in Directory.GetFiles(Util.MusicDir))
				try{
					File.Delete (file);
				}
				catch(Exception ex)
				{

				}
				
				lock(Database.Main){
					Database.Main.ResetOffline();
				}
				//Directory.CreateDirectory (Util.MusicDir);
				Util.OfflineSongsList.Clear();
				Util.OfflineAlbums.Clear();
				Util.OfflineAlbumsGrouped.Clear();
				Util.OfflineAlbumsList.Clear();
				Util.OfflineArtistList.Clear();
				Util.OfflineGenreList.Clear();
				var alert = new BlockAlertView("","All Offline music has been deleted.".Translate());
				alert.AddButton("Ok",null);
				alert.Show();

			};
			scrollView.AddSubview(clearData);
			offset += 50;
			TotalSongsLabel = new UILabel(resyncBtn.Frame.SetLocation(10,offset)){TextColor = UIColor.White};
			TotalSongsLabel.Text = "Total Songs".Translate() +": " + Settings.SongsCount.ToString("#,###");

			TotalSongsLabel.BackgroundColor = UIColor.Clear;
			
			offset += 50;
			scrollView.AddSubview(TotalSongsLabel);

			var versionLabel = new UILabel(new RectangleF(10,offset, this.View.Frame.Width - widthOffset,44)){
				Text = "Version".Translate() +": " + Util.CurrentVersion.ToString(),
				BackgroundColor = UIColor.Clear,
				TextColor = UIColor.White,
			};
			scrollView.AddSubview(versionLabel);
			offset += 50;

			var frame = View.Bounds;
			frame.Height = offset + 50;
			scrollView.ContentSize = frame.Size;
			scrollView.BackgroundColor = UIColor.Clear;
			this.View.AddSubview (scrollView);
			
		}
		MBProgressHUD progress;
		void TrySignin ()
		{
			passwordField.ResignFirstResponder ();
			var email = (emailField.Text ?? "").Trim ();
			var password = (passwordField.Text ?? "");
			
			
			if (string.IsNullOrEmpty (email) || string.IsNullOrEmpty (password)) {
				var failureAlert = new BlockAlertView (
					"Incomplete Information".Translate(),
					"Please provide both an email address and password.".Translate());
				failureAlert.AddButton("Ok".Translate(),null);
				failureAlert.Show ();
				return;
			}
			if(email != Settings.UserName)
			{
				BlockAlertView alert = new BlockAlertView("Are you sure".Translate(),"This will log you out of the current account and delete all local data. Are you sure you want to continue?".Translate());
				alert.SetCancelButton("Cancel".Translate(),null);
				alert.AddButton("Yes".Translate(),delegate{
						Downloader.Reset();
						Settings.HasData = false;
						Settings.LastSongsUpdate = "";
						Settings.LastUpdateCompleted = DateTime.MinValue;
						Settings.LastUpdateRequest = "";
						Settings.ContinuationToken = "";
						Settings.OverrideSync = true;
						Settings.SongsCount = 0;
						Database.SetDatabase(email);
						Settings.CurrentSongId = "";
						ClearCookies();
						Util.Api = null;
						if(Util.Player != null)
							Util.Player.Pause();
						//Database.ResetDatabase();
						//Util.UpdateMusic(true,true);
						Settings.HasData = false;
						startLogin(true);
				});
				alert.Show();
			}
			else
				startLogin(false);
			
	
		}
		private void loginFacebook()
		{
			if ((string.IsNullOrEmpty (Settings.FbAuth) || Settings.FbAuthExpire <= DateTime.Now)) {
				var fvc = new FacebookAuthorizationViewController ("256249271115171", new string[] { "publish_actions" }, FbDisplayType.Touch);
				fvc.AccessToken += delegate(string accessToken, DateTime expires) {
					Settings.FbAuth = accessToken;
					Settings.FbAuthExpire = expires;
					fvc.View.RemoveFromSuperview ();
					Settings.FbEnabled = true;
					
					//DataAccess.GetFriends (false);
					//BackgroundUpdater.AddToFacebook();				
					//this.DismissModalViewControllerAnimated(true);
				};
				//this.DismissModalViewControllerAnimated(true);
				fvc.AuthorizationFailed += delegate(string message) { Settings.FbEnabled = false; fvc.View.RemoveFromSuperview (); };
				fvc.Canceled += delegate {
					NSLogWriter.Default.WriteLine ("Canceled clicked");
					fvc.View.RemoveFromSuperview ();
					 Settings.FbEnabled = false;
					//this.DismissModalViewControllerAnimated(true);
					//this.NavigationController.PopViewControllerAnimated(false);
				};
				fvc.View.Frame = this.View.Frame;// = new System.Drawing.RectangleF (System.Drawing.PointF.Empty, new System.Drawing.SizeF (fvc.View.Frame.Height, fvc.View.Frame.Width));
				this.View.AddSubview(fvc.View);
			} else
			{
					//DataAccess.GetFriends (false);
			}	
		}
		
		public override void ViewWillAppear (bool animated)
		{
			TotalSongsLabel.Text = "Total Songs".Translate() + ": " + Util.Songs.Count.ToString("#,###");
			this.bgImage.Frame = this.View.Bounds;
		}
		string password;
		string email;
		void startLogin(bool resetApi)
		{
			email = (emailField.Text ?? "").Trim ();
			password = (passwordField.Text ?? "");
				//var cookeisPath = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
				//Uri.EscapeDataString ("cookies-" + email));
			
			if (Util.Api == null || resetApi)
#if gmusic
				Util.Api = new GoogleMusicApi (email);
#elif amusic
				Util.Api = new AmazonApi (email);
#elif mp3tunes
				Util.Api = new mp3tunesApi(email);
#endif
			
			
			if(NavigationItem.LeftBarButtonItem != null)NavigationItem.LeftBarButtonItem.Enabled = false;
			if(NavigationItem.RightBarButtonItem != null)NavigationItem.RightBarButtonItem.Enabled = false;
			progress = new MBProgressHUD(Util.WindowFrame);
			progress.TitleText = "Logging in".Translate();
			progress.Show(true);
			Thread thread = new Thread(login);
			thread.Start();
		}
		
		public static void ClearCookies()
		{
			foreach(var cookie in NSHttpCookieStorage.SharedStorage.Cookies)
				NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
		}
		
		void login()
		{
			using(new NSAutoreleasePool())
			{
				try {
				
			
					Util.Api.SignIn (email, password, (success) =>{
						BeginInvokeOnMainThread (delegate {
						progress.Hide(true);
							if(success)
							{
								OnSigninSuccess(Util.Api);
								Settings.UserName = email;
								Settings.Key = password;
							}
							else								
								OnSigninFailure (new Exception("Error logging in. Please try again."));
							if(NavigationItem.LeftBarButtonItem != null)
								NavigationItem.LeftBarButtonItem.Enabled = true;
							if(NavigationItem.RightBarButtonItem != null)NavigationItem.RightBarButtonItem.Enabled = true;
						});
					});
					
				} catch (Exception error) {
					BeginInvokeOnMainThread (delegate {					
						progress.Hide(true);
						OnSigninFailure (error);
					});
				} 
			}
		}
		
		void OnSigninFailure (Exception error)
		{
			var errMEssage = error.ToString();
			errMEssage = errMEssage.Contains("Error logging in") ? (@"Invalid Username/Password.".Translate()) : errMEssage.ToString();

			var failureAlert = new BlockAlertView (
				"Sign in Failure".Translate(),
					errMEssage);
			failureAlert.AddButton("Help".Translate(), BlockAlertView.ButtonColor.Green, delegate{
				UIApplication.SharedApplication.OpenUrl(new NSUrl("http://www.gmusicapp.com/mfaq.html"));
			});
			failureAlert.AddButton("Try Again".Translate(), BlockAlertView.ButtonColor.Black,null);
			failureAlert.Show ();
		}
		
		void OnSigninSuccess (Api api)
		{
			DismissViewController (true,null);
				
			var c = AccountChanged;
			if (c != null) {
				c (this, new ApiEventArgs {
					Api = api,
				});
			}
		}
		
		public static event EventHandler<ApiEventArgs> AccountChanged;		
	}
	
}

