using System;
using MonoTouch.UIKit;
using MonoTouch.MediaPlayer;
using System.Drawing;
using ClanceysLib;
using System.IO;
using System.Linq;

namespace GoogleMusic
{
	public class NowPlayingViewController : UIViewController, INowPlayingViewController
	{
		public OBSlider CurrentTimeSlider{ get { return detailView.slider; } }
		
		public UIProgressView DownloadProgressView { get { return detailView.DownloadProgressView; } }
		
		public bool Seeking { get { return CurrentTimeSlider.Tracking; } }
		public void SetSliderProgress(float value)
		{
			if (!Seeking)
				CurrentTimeSlider.Value = value;
		}
		
		AlbumImageView albumView;
		TitleView titleView;
		UIToolbar toolbar;
		
		//UIButton playBtn;
		//UIButton pauseBtn;
		//UIButton nextBtn;
		//UIButton previousBtn;
		UIBarButtonItem playBtn;
		UIBarButtonItem pauseBtn;
		UIBarButtonItem nextBtn;
		UIBarButtonItem previousBtn;
		UIBarButtonItem menuBtn;
		UIBarButtonItem thumbsUpBtn;
		UIButton thumbsUp;
		static UIImage listImage = UIImage.FromFile("Images/list.png");
		static UIImage thumbsUpOff = UIImage.FromFile ("Images/thumbsUp.png");
		static UIImage thumbsUpOn = UIImage.FromFile ("Images/thumbsUpPressed.png");
		public DetailView detailView;
		public VolumeView volumeView;
		UIView baseView;
		UIView mainContentView;
		CurrentQueueViewController currentVC;
		
		public NowPlayingViewController ()
		{
			this.View.BackgroundColor = UIColor.Black;
			currentVC = new CurrentQueueViewController();
			/*
			var btnY = this.View.Bounds.Height - 94 - 50;
			previousBtn = new UIButton(new RectangleF(50,btnY,40,40));
			previousBtn.SetImage(previousImage,UIControlState.Normal);
			previousBtn.TouchDown += delegate {
				Util.Previous();
			};
			playBtn = new UIButton(new RectangleF(140,btnY,40,40));
			playBtn.SetImage(playImage,UIControlState.Normal);
			playBtn.TouchDown += delegate {
				Util.PlayPause();
			};
			nextBtn = new UIButton(new RectangleF(230,btnY,40,40));
			nextBtn.SetImage(nextImage,UIControlState.Normal);
			nextBtn.TouchDown += delegate {
				Util.Next();
			};
			*/
			playBtn = new UIBarButtonItem (UIBarButtonSystemItem.Play, delegate{
				Util.PlayPause ();
			});
			pauseBtn = new UIBarButtonItem (UIBarButtonSystemItem.Pause, delegate{
				Util.PlayPause ();
			}); 
			previousBtn = new UIBarButtonItem (UIBarButtonSystemItem.Rewind, delegate{
				Util.Previous ();
			});
			nextBtn = new UIBarButtonItem (UIBarButtonSystemItem.FastForward, delegate{
				Util.Next ();
			});
			
			menuBtn = new UIBarButtonItem (new UIImage ("Images/upArrow.png"), UIBarButtonItemStyle.Plain, delegate{
				showPopUp ();
			});
			
			//thumbsDownBtn.TintColor = UIColor.Blue.ColorWithAlpha(.8f);
			
			thumbsUp = new UIButton (new RectangleF (0, 0, 40, 40));
			thumbsUp.TouchDown += delegate {
				if (Util.CurrentSong == null)
					return;
				if (Util.CurrentSong.Rating > 0) {
					Util.Api.ThumbsDown (Util.CurrentSong, delegate {
						//TODO: update thumbs down
						/*
						lock(Database.Main)
						{
							Util.CurrentSong.Rating = 0;
							Database.Main.Update(Util.CurrentSong);
						}
						*/
					});
					
					thumbsUp.SetImage (thumbsUpOff, UIControlState.Normal);
					Scrobbler.Main.UnLoveSong (Util.CurrentSong);
				} else {
					
					Util.Api.ThumbsUp (Util.CurrentSong, delegate {
						//TODO: update thumbs down
						/*
						lock(Database.Main)
						{
							Util.CurrentSong.Rating = 5;
							Database.Main.Update(Util.CurrentSong);
						}
						*/
					});
					thumbsUp.SetImage (thumbsUpOn, UIControlState.Normal);
					Scrobbler.Main.LoveSong (Util.CurrentSong);
				}
			};
			thumbsUpBtn = new UIBarButtonItem (thumbsUp);
			
			toolbar = new UIToolbar (new RectangleF (0, this.View.Bounds.Height - 140, this.View.Frame.Width, 50));
			//toolbar.BarStyle = UIBarStyle.BlackTranslucent;
			toolbar.BarStyle = UIBarStyle.Black;
			toolbar.BackgroundColor = UIColor.Clear;
			toolbar.Translucent = true;
			toolbar.SetItems (
				playArray
				, false);
			//toolbar.Alpha = .5f;
			
			
			volumeView = new VolumeView ();
			var vFrame = toolbar.Frame;
			vFrame.Y += 50;
			volumeView.Frame = vFrame;
			
			titleView = new TitleView ();
			albumView = new AlbumImageView ();
			albumView.SetImage (MusicSectionView.DefaultLeft);
			detailView = new DetailView ();
			CurrentTimeSlider.BackgroundColor = UIColor.Clear;
			
			CurrentTimeSlider.MaximumTrackTintColor = UIColor.Clear;
			
			albumView.TouchUpInside += delegate{
				UIView.BeginAnimations ("detailView");				
				UIView.SetAnimationDuration (.3);
				if (detailView.Alpha != 0f) {
					detailView.Alpha = 0f;
					UIView.SetAnimationCurve (UIViewAnimationCurve.EaseIn);
					//detailView.RemoveFromSuperview();
				} else {
					UIView.SetAnimationCurve (UIViewAnimationCurve.EaseOut);
					detailView.Alpha = 1f;
				}
				//albumView.AddSubview(detailView);
				
				UIView.CommitAnimations ();
			};
			albumView.AddSubview (detailView);
			baseView = new UIView(View.Bounds);
			this.View.AddSubview(baseView);
			mainContentView = new UIView(View.Bounds);
			baseView.AddSubview(mainContentView);
			
			this.mainContentView.AddSubview (albumView);
			this.View.AddSubview (volumeView);
			this.View.AddSubview (toolbar);
			currentVC.View.Frame = baseView.Bounds.SubtractSize(0,toolbar.Frame.Height + volumeView.Frame.Height + 40f);
			//this.View.AddSubview(Meter);
			///this.View.AddSubview(previousBtn);
			//this.View.AddSubview(playBtn);
			//this.View.AddSubview(nextBtn);
		}
		
		PopUpView popUpView;
		protected EditSongViewController CurrentSongEditor;
		
		public void ShowPopUp ( Screens screen, bool shouldBeLocal, Action<int> clickedIndex, Action<bool> offLineChanged)
		{
			//Console.WriteLine("Pop up: " + startPoint); 
			popUpView = new PopUpView (Util.BaseView.Frame, screen, shouldBeLocal);
			popUpView.Clicked = clickedIndex;
			popUpView.OfflineToggled = offLineChanged;
			Util.BaseView.Superview.AddSubview (popUpView);
			popUpView.AnimateIn ();
			
		}
		
		bool popupshown;
		
		public void PlaylistChanged ()
		{
			currentVC.TableView.ReloadData();
		}
		
		private void showPopUp ()
		{
			if ((popUpView != null && popUpView.Superview != null) || Util.CurrentSong == null)
				return;
			var loc = toolbar.Frame.Location;
			loc.X += 25;
			var point = this.View.ConvertPointToView (loc, Util.BaseView);
			var song = Util.CurrentSong;
			this.ShowPopUp (Screens.NowPlaying, song.IsLocal ? song.IsLocal : song.ShouldBeLocal, (index) =>
			                {
				//PlayNext
				if (index == 1)
					Util.PlaySongNext (song);//(song,song.ArtistId,-1,true);
				else if (index == 2) {
					var plistVc = new PlaylistViewController (true,false);
					plistVc.OnPlaylistSelected = (playlist) => {
						Util.Api.AddToPlaylist (playlist, new []{song}, (success) =>
						{
							if (!success) {
								var alert = new BlockAlertView ("Error".Translate(), "There was an error adding the song to your playlist.".Translate());
								alert.AddButton("Ok".Translate(),null);
								alert.Show ();
							}
						});
					};
					this.NavigationController.PushViewController (plistVc, true);
				} else if (index == 3) {
					Util.Api.CreateMagicPlayList (song, (onSuccess) => {
						if (!onSuccess) {
							var alert = new BlockAlertView ("Error".Translate(), "There was an error creating your magic playlist.".Translate());
							alert.AddButton("Ok".Translate(),null);
							alert.Show ();
						}
						
					});
				} else if (index == 4) {
					var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this playlist?".Translate());//
					alert.SetCancelButton("Cancel".Translate(),null);
					alert.AddButton("Delete".Translate(),delegate{
						
						Util.Api.DeleteSong (song, (success) => {
							if (!success) {
								var alert2 = new BlockAlertView ("Error".Translate(), "There was an error deleting the song.".Translate());
								alert2.AddButton("Ok".Translate(),null);
								alert2.Show ();
							} else {
								//	this.TableView.DeleteRows(new[]{indexPath},UITableViewRowAnimation.Fade);
							}
						});
						
					});
					alert.Show ();
				} else if (index == 5) {
					CurrentSongEditor = new EditSongViewController(song);
					this.NavigationController.PushViewController(CurrentSongEditor,true);
				}
			}, (shouldBeLocal) => {
				if (shouldBeLocal) {
					song.ShouldBeLocal = true;
					Downloader.AddFile (song);
					//TODO: update offline
					
					lock (Database.DatabaseLocker)
						Database.Main.Update (song);
					
				} else {
					if (song.IsLocal && File.Exists (Util.MusicDir + song.FileName))
					try {
						File.Delete (Util.MusicDir + song.FileName);
					} catch {
					}
					Database.Main.UpdateDeleteOffline (song);
					
					
				}
				popUpView = null;
				//tableView.ReloadRows(new []{indexPath},UITableViewRowAnimation.None);
				//TODO Either download or remove	
			});
		}
		public void Flip()
		{
			
			var playlistShowing = mainContentView.Superview == null;
			UIView.Animate (.5, delegate() {
				
				UIView.SetAnimationTransition(playlistShowing ?  UIViewAnimationTransition.FlipFromLeft : UIViewAnimationTransition.FlipFromRight,menuBackView,true);
				
				if(!playlistShowing)
				{
					playlistButton.RemoveFromSuperview();
					menuBackView.AddSubview(albumButton);
				}
				else 
				{
					albumButton.RemoveFromSuperview();
					menuBackView.AddSubview(playlistButton);
				}
			});
			
			UIView.Animate(.5,delegate{
				//UIView.BeginAnimations ("Flipper");
				//UIView.SetAnimationDuration (.5);
				UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);
				
				UIView.SetAnimationTransition(playlistShowing ?  UIViewAnimationTransition.FlipFromLeft : UIViewAnimationTransition.FlipFromRight,baseView,true);
				
				if(!playlistShowing)
				{
					mainContentView.RemoveFromSuperview();
					baseView.AddSubview(currentVC.View);
					this.AddChildViewController(currentVC);
					//this.NavigationItem.RightBarButtonItem.Image = Util.CurrentSong.AlbumImage;
				}
				else
				{
					currentVC.View.RemoveFromSuperview();
					baseView.AddSubview(mainContentView);
					//this.NavigationItem.RightBarButtonItem.Image = listImage;
				}
			},delegate{
				try{
					//currentVC.Reload(Util.CurrentSong);
				}
				catch(Exception ex)
				{
					
				}
			});
			//UIView.CommitAnimations ();
		}
		
		UIView menuBackView;
		UIButton albumButton;
		UIButton playlistButton;
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			try {
				
				var menuBtnRect = new RectangleF(0,0,35,30);
				menuBackView = new UIView(menuBtnRect);
				//menuBackView.AddSubview(new UIImageView(menuBtnRect){Image = UIImage.FromFile("Images/blackbutton.png").CreateResizableImage(new UIEdgeInsets(0,5,0,5))});
				albumButton = new UIButton(menuBtnRect);
				albumButton.SetBackgroundImage(AlbumImageView.defaultImage, UIControlState.Normal);
				albumButton.TouchUpInside += delegate {
					Flip();
				};
				playlistButton = new UIButton(menuBtnRect);
				playlistButton.SetBackgroundImage(UIImage.FromFile("Images/menubar-button.png").CreateResizableImage(new UIEdgeInsets(0,5,0,5)), UIControlState.Normal);
				playlistButton.SetImage(listImage, UIControlState.Normal);
				playlistButton.TouchUpInside += delegate {
					Flip();
				};
				menuBackView.AddSubview(playlistButton);
				this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem ("Back".Translate(), UIBarButtonItemStyle.Bordered, delegate {
					//Util.MainViewController.NavigationController.ShowMenu();
					Util.MainVC.NavigationController.SelectedIndex = Settings.CurrentTab;
					//Util.AppDelegate.MainVC.ToggleMenu();
				});
				this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (menuBackView);//, UIBarButtonItemStyle.Bordered, delegate {
				//	Flip();
				//});
				//NSNotificationCenter.DefaultCenter.AddObserver ("AnisotropicImageUpdate", (notification) => {
				AnisotropicImage.ImageUpdated = (image) => {
					Util.EnsureInvokedOnMainThread(()=>{
						CurrentTimeSlider.SetThumbImage(image, UIControlState.Normal);
						if(Util.IsIos6)
							this.volumeView.VolumeSlider.SetVolumeThumbImage(image,UIControlState.Normal);
						else
							this.volumeView.Slider.SetThumbImage(image, UIControlState.Normal);
					});
				};
				
			} catch (Exception ex) {
				Console.WriteLine ("Error on view will appear: " + ex);	
			}
			
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.NavigationController.NavigationBar.BackgroundColor = UIColor.Black;
			//Util.IphoneViewController.BaseViewController.NavigationBar.Hidden = false;
			this.NavigationItem.TitleView = titleView;
			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			
			
			titleView.SongTitle.Text = Util.CurrentSong == null ? "" : Util.CurrentSong.Title;
			titleView.ArtistTitle.Text = Util.CurrentSong == null ? "" : Util.CurrentSong.Artist;
			if(mainContentView.Superview == null)
			{
				this.NavigationItem.RightBarButtonItem.Image = Util.CurrentSong.AlbumImage;
			}
		}
		
		public override bool ShouldAutorotate ()
		{
			return Util.ShouldRotate;
		}
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
		}
		[Obsolete ("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown;
		}
		public override void ViewDidAppear (bool animated)
		{
			try {
				
				UIApplication.SharedApplication.BeginReceivingRemoteControlEvents ();
				this.BecomeFirstResponder ();
				base.ViewDidAppear (animated);
				
				AnisotropicImage.StartUpdating ();
				//this.NavigationController.NavigationBar.BackItem.Title = "Back";
			} catch (Exception  ex) {
				Console.WriteLine ("Error on view did appear: " + ex);					
			}
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			try {
				
				AnisotropicImage.StopUpdating ();
				//UIApplication.SharedApplication.EndReceivingRemoteControlEvents ();
				base.ViewWillDisappear (animated);
				
				//Util.IphoneViewController.BaseViewController.NavigationBar.Hidden = true;
			} catch (Exception ex) {
				Console.WriteLine ("Error in view will disapear	" + ex);
			}
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			try {
				base.ViewDidDisappear (animated);
				//Util.IphoneViewController.BaseViewController.NavigationBar.Hidden = true;
			} catch (Exception ex) {
				Console.WriteLine ("Error in view did disapear" + ex);
			}
		}
		
		UIBarButtonItem[] playArray {
			get {
				return new UIBarButtonItem[]{
					menuBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace), previousBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),playBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),nextBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace)
						#if !mp3tunes
						,thumbsUpBtn
							#else
							,new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
							#endif
							
				};
			}
		}
		
		UIBarButtonItem[] pauseArray {
			get {
				return new UIBarButtonItem[]{
					menuBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace), previousBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),pauseBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),nextBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace)
						#if !mp3tunes
						,thumbsUpBtn
							#else
							,new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
							#endif
				};
			}
		}
		
		bool IsPlaying;
		MPNowPlayingInfo NowPlaying;
		
		public void SetState (bool isPlaying)
		{
			try {
				//playBtn.SetImage(isPlaying? pauseImage:playImage,UIControlState.Normal);
				toolbar.SetItems (isPlaying ? pauseArray : playArray, true);
				
				titleView.SongTitle.Text = Util.CurrentSong == null ? "" : Util.CurrentSong.Title;
				titleView.ArtistTitle.Text = Util.CurrentSong == null ? "" : Util.CurrentSong.Artist;
				
				SetPlayCount ();
				//if (Util.IsIos5) {
				var song = Util.CurrentSong ?? new Song();
				Console.WriteLine("Setting MPNowPlaying: " + song.Title);
				//Util.UpdateMpNowPlaying();
				
				
				//Console.WriteLine(MPMediaItem.TitleProperty);
				//Console.WriteLine(MPNowPlayingInfoCenter.NowPlayingTitle);
				//}
				//MPNowPlayingInfoCenter.ElapsedPlaybackTime = isPlaying ? 1 : 0;
				
				
				if (Util.CurrentSong != null) {
					thumbsUp.SetImage (Util.CurrentSong.Rating > 1 ? thumbsUpOn : thumbsUpOff, UIControlState.Normal);
					albumView.SetImage (Util.CurrentSong.AlbumImage);
					albumButton.SetImage(Util.CurrentSong.AlbumImage, UIControlState.Normal);
				}
				detailView.SetShuffle();
				if (IsPlaying == isPlaying)
					return;
				IsPlaying = isPlaying;
				//setThumbsTint();
			} catch (Exception ex) {
				Console.WriteLine ("error in SetState :" + ex);	
			}
			
		}
		public void Update ()
		{
			titleView.SongTitle.Text = Util.CurrentSong.Title;
			titleView.ArtistTitle.Text = Util.CurrentSong.Artist;
			albumView.SetImage (Util.CurrentSong.AlbumImage);
			albumButton.SetImage(Util.CurrentSong.AlbumImage, UIControlState.Normal);
			try{
				currentVC.Reload(Util.CurrentSong);
			}
			catch(Exception ex)
			{
				
			}
		}
		
		public void SetPlayCount ()
		{
			try {
				var playedCount = Util.PlayListPlayed.Count;
				detailView.countLabel.Text = "" + Util.CurrentSongIndex + " of " + Util.TotalPlayListCount;
				
				
				//Util.UpdateMpNowPlaying();
				
			} catch (Exception ex) {
				Console.WriteLine ("error in setPlayCount	" + ex);
			}
		}
		
		public class TitleView: UIView
		{	
			
			public UILabel SongTitle;
			public UILabel ArtistTitle;
			
			public TitleView () : base(new RectangleF(0,0,200,44))
			{
				this.AutosizesSubviews = true;
				this.BackgroundColor = UIColor.Clear;
				SongTitle = new UILabel (new RectangleF (0, 20, 200, 20));	
				SongTitle.TextColor = UIColor.White;
				SongTitle.ShadowColor = UIColor.DarkGray;
				SongTitle.ShadowOffset = new SizeF (0, -1);
				SongTitle.TextAlignment = UITextAlignment.Center;
				SongTitle.Font = UIFont.BoldSystemFontOfSize (13);
				SongTitle.BackgroundColor = UIColor.Clear;
				//SongTitle.AdjustsFontSizeToFitWidth = true;
				ArtistTitle = new UILabel (new RectangleF (0, 2, 200, 20));
				ArtistTitle.Font = UIFont.BoldSystemFontOfSize (UIFont.SmallSystemFontSize);
				ArtistTitle.BackgroundColor = UIColor.Clear;	
				ArtistTitle.TextColor = UIColor.LightGray;
				ArtistTitle.ShadowColor = UIColor.DarkGray;
				ArtistTitle.ShadowOffset = new SizeF (0, -1);
				ArtistTitle.TextAlignment = UITextAlignment.Center;
				ArtistTitle.Font = UIFont.BoldSystemFontOfSize (13);
				//ArtistTitle.AdjustsFontSizeToFitWidth = true;
				this.AddSubview (SongTitle);
				this.AddSubview (ArtistTitle);
				this.AutoresizingMask = (UIViewAutoresizing.FlexibleLeftMargin |
				                         UIViewAutoresizing.FlexibleRightMargin |
				                         UIViewAutoresizing.FlexibleTopMargin |
				                         UIViewAutoresizing.FlexibleBottomMargin);
			}
		}
		
		public class DetailView : UIView
		{
			public UILabel countLabel;
			public UILabel currentTimeLabel;
			public UILabel remainingTimeLabel;
			UIButton repeatButton;
			UIButton shuffleButton;
			public OBSlider slider;
			public UIProgressView DownloadProgressView;
			static UIImage shuffleOff = UIImage.FromFile ("Images/shuffleOff.png");
			static UIImage shuffleOn = UIImage.FromFile ("Images/shuffleOn.png");
			static UIImage repeatOff = UIImage.FromFile ("Images/repeatOff.png");
			static UIImage repeatAll = UIImage.FromFile ("Images/repeatAll.png");
			static UIImage repeatOne = UIImage.FromFile ("Images/repeatOne.png");
			
			public DetailView () : base (new RectangleF(0,0,320,75))
			{
				this.Alpha = 1f;
				this.BackgroundColor = UIColor.Black.ColorWithAlpha (.5f);
				countLabel = new UILabel (new RectangleF (0, 5, 320, 12));
				countLabel.TextColor = UIColor.White;
				countLabel.TextAlignment = UITextAlignment.Center;
				countLabel.BackgroundColor = UIColor.Clear;
				countLabel.Font = UIFont.BoldSystemFontOfSize (10);
				
				currentTimeLabel = new UILabel (new RectangleF (2, 18, 50, 35));
				currentTimeLabel.TextColor = UIColor.White;
				currentTimeLabel.TextAlignment = UITextAlignment.Right;
				currentTimeLabel.BackgroundColor = UIColor.Clear;
				currentTimeLabel.AdjustsFontSizeToFitWidth = true;
				currentTimeLabel.Font = UIFont.BoldSystemFontOfSize (13);
				
				remainingTimeLabel = new UILabel (new RectangleF (272, 18, 50, 35));
				remainingTimeLabel.TextColor = UIColor.White;
				remainingTimeLabel.TextAlignment = UITextAlignment.Left;
				remainingTimeLabel.BackgroundColor = UIColor.Clear;
				remainingTimeLabel.AdjustsFontSizeToFitWidth = true;
				remainingTimeLabel.Font = UIFont.BoldSystemFontOfSize (13);
				
				slider = new OBSlider (new RectangleF (52, 18, 216, 10));
				slider.EditingDidEnd += delegate(object sender, EventArgs e) {
					Console.WriteLine ("Touched up ");
					Util.Seek (slider.Value);
					//Seeking = false;
				};
				slider.TouchDown += delegate(object sender, EventArgs e) {
					Console.WriteLine ("Touched down");
					//Seeking = true;
				};
				slider.EditingDidEndOnExit += delegate(object sender, EventArgs e) {
					Console.WriteLine ("test");
				};
				slider.ValueChanged += delegate(object sender, EventArgs e) {
					
					slider.BackgroundColor = UIColor.Clear;
					slider.MaximumTrackTintColor = UIColor.Clear;
					
				};
				slider.MinimumTrackTintColor = UIColor.Orange;
				DownloadProgressView = new UIProgressView (new RectangleF (54, 25, 212, 25));
				DownloadProgressView.Progress = .0f;//.SetProgress(.0f,false);
				
				DownloadProgressView.ProgressTintColor = UIColor.Orange;
				
				shuffleButton = new UIButton (new RectangleF (285, 40, 32, 32));
				shuffleButton.SetImage (Settings.Random ? shuffleOn : shuffleOff, UIControlState.Normal);
				shuffleButton.TouchDown += delegate {
					Util.ToggleRandom ();
					shuffleButton.SetImage (Settings.Random ? shuffleOn : shuffleOff, UIControlState.Normal);
				};
				
				repeatButton = new UIButton (new RectangleF (0, 40, 32, 32));
				repeatButton.SetImage ((Settings.RepeatMode == 0 ? repeatOff : (Settings.RepeatMode == 1 ? repeatAll : repeatOne)), UIControlState.Normal);
				repeatButton.TouchDown += delegate {
					Util.TogleRepeat ();					
					repeatButton.SetImage ((Settings.RepeatMode == 0 ? repeatOff : (Settings.RepeatMode == 1 ? repeatAll : repeatOne)), UIControlState.Normal);
				};
				
				
				this.AddSubview (countLabel);
				this.AddSubview (remainingTimeLabel);
				this.AddSubview (currentTimeLabel);
				this.AddSubview (DownloadProgressView);
				this.AddSubview (slider);
				
				this.AddSubview (shuffleButton);
				this.AddSubview (repeatButton);
			}
			public void SetShuffle()
			{
				shuffleButton.SetImage (Settings.Random ? shuffleOn : shuffleOff, UIControlState.Normal);
			}
			
			
		}
		
		public class VolumeView : UIView
		{
			public MPVolumeView VolumeSlider;
			public UISlider Slider;
			
			public VolumeView () : base (new RectangleF(0,0,320,50))
			{
				this.BackgroundColor = UIColor.Black;
				VolumeSlider = new MPVolumeView (new RectangleF (20, 10, 280, 20));
				Slider = VolumeSlider.Subviews.Where(x=> x is UISlider).FirstOrDefault() as UISlider;
				Slider.MinimumTrackTintColor = UIColor.Orange;
				this.AddSubview (VolumeSlider);
			}
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				VolumeSlider.Frame = new RectangleF(20,10,this.Bounds.Width - 40,20);
			}
			
		}
	}
}

