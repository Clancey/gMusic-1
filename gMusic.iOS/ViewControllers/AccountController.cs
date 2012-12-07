using System;
using MonoTouch.UIKit;
using System.Threading;
using System.Drawing;
using ClanceysLib;
using MonoTouch.Foundation;
using System.Threading.Tasks;

namespace GoogleMusic
{
	public class AccountController : UIViewController
	{
		UILabel insructions;
		UITextField emailField;
		UITextField passwordField;
		NSObject invoker = new NSObject();
		public AccountController ()
		{
			Title = "Log in".Translate();
			
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
				UIBarButtonSystemItem.Cancel,
				delegate {
				
				DismissViewController (true,null);
				
			});
			
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (
				"Sign in".Translate(),
				UIBarButtonItemStyle.Done,
				delegate {
				
				TrySignin ();		
			});
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var x = ((Util.IsIpad ? 1024 : this.View.Frame.Width) - 300)/2;
			var y = Util.IsIpad ? 50 : 0;
			this.View.AddSubview(new UIImageView(UIImage.FromFile(Util.IsIphone ? "Images/bg.png" : "Images/bgipad.png")));
			insructions = new UILabel(new RectangleF(x,y + 30,300,66)){
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextColor = UIColor.White,
				BackgroundColor = UIColor.Clear,
				TextAlignment  = UITextAlignment.Center,
				Font = UIFont.SystemFontOfSize(18),
				ShadowColor = UIColor.DarkGray,
				ShadowOffset = new SizeF(-1,-1),
			};
#if gmusic
			insructions.Text = "Please log in using your\r\n Google Play Account".Translate();
#elif mmusic
			insructions.Text = "Please log in using your\r\n MP3tunes.com Account";
#endif
			View.AddSubview (insructions);

			emailField = new UITextField {
				//BackgroundColor = UIColor.White,
				BorderStyle = UITextBorderStyle.RoundedRect, 
				AutocapitalizationType = UITextAutocapitalizationType.None,
				AutocorrectionType = UITextAutocorrectionType.No,
				KeyboardType = UIKeyboardType.EmailAddress,
				Placeholder = "Email".Translate(),
				Frame = new RectangleF (x, y + 120, 300, 33),
				ReturnKeyType = UIReturnKeyType.Next,
				ShouldReturn = delegate {
					passwordField.BecomeFirstResponder ();
					return true;
				},
			};
			View.AddSubview (emailField);
			
			passwordField = new UITextField {
				//BackgroundColor = UIColor.White,
				BorderStyle = UITextBorderStyle.RoundedRect, 
				AutocapitalizationType = UITextAutocapitalizationType.None,
				AutocorrectionType = UITextAutocorrectionType.No,
				Placeholder = "Password".Translate(),
				SecureTextEntry = true,
				Frame = new RectangleF (x, y + 170, 300, 33),
				ReturnKeyType = UIReturnKeyType.Go,
				ShouldReturn = delegate {
					Console.WriteLine("Done hit");
					ResignFirstResponder ();
					TrySignin ();
					return true;
				},
			};
			View.AddSubview (passwordField);
		}
		MBProgressHUD progress;
		void TrySignin ()
		{
			passwordField.ResignFirstResponder ();
			email = (emailField.Text ?? "").Trim ();
			password = (passwordField.Text ?? "");
			
			if (string.IsNullOrEmpty (email) || string.IsNullOrEmpty (password)) {
				var failureAlert = new BlockAlertView (
					"Incomplete Information".Translate(),
					"Please provide both an email address and password.".Translate());
				failureAlert.AddButton("Ok".Translate(),null);
				failureAlert.Show (UIApplication.SharedApplication.Windows[0]);
				return;
			}
			
			//var cookeisPath = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
			//	Uri.EscapeDataString ("cookies-" + email));
#if gmusic
			if (Util.Api == null)
				Util.Api = new GoogleMusicApi (email);
#elif amusic
			if (Util.Api == null)
				Util.Api = new AmazonApi (email);
#elif mp3tunes
			if (Util.Api == null)
				Util.Api = new mp3tunesApi (email);
#endif
			
			
			NavigationItem.LeftBarButtonItem.Enabled = false;
			NavigationItem.RightBarButtonItem.Enabled = false;
			progress = new MBProgressHUD(Util.WindowFrame);
			progress.TitleText = "Logging in".Translate();
			progress.Show(true);
			var task = Task.Factory.StartNew (delegate{
				loagin();
			});
		}
		
		string email;
		string password;
		void loagin()
		{
				try {
				
					Util.Api.SignIn (email, password, (success) =>{
						invoker.BeginInvokeOnMainThread (delegate {
						progress.Hide(true);
							if(success)
							{
								OnSigninSuccess(Util.Api);
								Settings.UserName = email;
								Settings.Key = password;
								//Util.MainVC.HideLogin();
							}
							else								
								OnSigninFailure (new Exception("Error logging in. Please try again."));
							
							NavigationItem.LeftBarButtonItem.Enabled = true;
							NavigationItem.RightBarButtonItem.Enabled = true;
							UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
						});
					});
					
				} catch (Exception error) {
					invoker.BeginInvokeOnMainThread (delegate {					
						progress.Hide(true);
						OnSigninFailure (error);
					});
				} 

		}
		
		void OnSigninFailure (Exception error)
		{
			var errMEssage = error.ToString();
			errMEssage = errMEssage.Contains("Error logging in") || errMEssage.Contains("BadAuthentication") ? (@"Invalid Username/Password.".Translate()) : errMEssage.ToString();
			if(errMEssage.Length > 300)
				errMEssage.Substring(0,300);
			var failureAlert = new BlockAlertView (
				"Sign in Failure".Translate(),
					errMEssage);
			failureAlert.AddButton("Help".Translate(), BlockAlertView.ButtonColor.Green, delegate{
				UIApplication.SharedApplication.OpenUrl(new NSUrl("http://www.gmusicapp.com/mfaq.html"));
			});
			failureAlert.AddButton("Try Again".Translate(), BlockAlertView.ButtonColor.Black,delegate {
			
			});
			failureAlert.Show (UIApplication.SharedApplication.Windows[0]);
		}
		
		void OnSigninSuccess (Api api)
		{
			this.DismissViewController(true,null);
				
			var c = AccountChanged;
			if (c != null) {
				c (this, new ApiEventArgs {
					Api = api,
				});
			}
		}
		public override bool ShouldAutorotate ()
		{
			if (Util.IsIphone)
				return false;
			return Util.ShouldRotate;
		}

		
		public static event EventHandler<ApiEventArgs> AccountChanged;		
	}
	
	public class ApiEventArgs : EventArgs
	{
		public Api Api { get; set; }
	}
}

