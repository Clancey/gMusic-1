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
using MonoTouch.MediaPlayer;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Dialog;
using FlyOutNavigation;
using MonoTouch.Foundation;
using ClanceysLib;
using System.Threading;
using Pioneer;
using MonoTouch.CoreGraphics;

namespace GoogleMusic
{
	public class TvOutViewController : MainViewController
	{
		TvOutView view;
		
		protected SonglistViewController CurrentSongListViewController;
		public TvOutViewController () : base(false)
		{
		}
		public override bool ShouldAutorotate ()
		{
			return Util.ShouldRotate;
		}


		public override void screenDidConnectNotification ()
		{
			Console.WriteLine ("Should not be called");
		}

		public override void screenDidDisconnectNotification ()
		{
			Console.WriteLine ("Should not be called");
		}
		public override void PlaylistChanged ()
		{

		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			view = new TvOutView (this);
			//View = view;

			
			NavigationController = new FlyOutNavigationController ();
			NavigationController.NavigationRoot = CreateRoot (null, false,true);
			NavigationController.NavigationTableView.BackgroundColor = UIColor.Clear;
			NavigationController.NavigationTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			//navigationController.AlwaysShowLandscapeMenu = true;
			//navigationController.ForceMenuOpen = true;
			//NavigationController.HideShadow = true;
			
			NavigationController.NavigationTableView.BackgroundView = new UIView (NavigationController.NavigationTableView.Frame);
			NavigationController.NavigationTableView.BackgroundView.Layer.AddSublayer (MakeBackgroundLayer (NavigationController.NavigationTableView.Frame));
			
			NavigationController.ViewControllers = new UIViewController[]{ new AutoRotateUIViewController (){View = view}};
		
			//navigationController.ViewControllers = navigationRoots;
			NavigationController.SelectedIndex = Settings.CurrentTab - 1;
			NavigationController.SelectedIndexChanged = delegate() {
				view.detailView.SetIndex (NavigationController.SelectedIndex);
				NavigationController.HideMenu ();
			};
			this.View.AddSubview (NavigationController.View);
			if(BaseViewController.isLoading)
				ShowLoadingScreen();
			//ThreadPool.QueueUserWorkItem(delegate{Thread.Sleep(1000);NavigationController.ShowMenu();});
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
		}

		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			NavigationController.View.Frame = this.View.Bounds;
			view.Frame = this.View.Bounds;
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			Util.LastOrientation = this.InterfaceOrientation;
			base.DidRotate (fromInterfaceOrientation);
			
			NavigationController.View.Frame = this.view.Bounds;
		}

		LoadingScreen loadingScreen;
		int LoadingScreenVisisble = 0;
		public void ShowLoadingScreen()
		{
			LoadingScreenVisisble ++;
			if( LoadingScreenVisisble > 1 || view == null)
				return;
			loadingScreen = new LoadingScreen (true);
			loadingScreen.Frame = view.Bounds;
			View.AddSubview(loadingScreen);
		}
		public void HideLoadingScreen()
		{
			LoadingScreenVisisble --;
			if(LoadingScreenVisisble > 0)
				return;
			if (loadingScreen != null)
			{
				loadingScreen.RemoveFromSuperview ();
				loadingScreen = null;
			}
		}

		public void UpdateClock ()
		{
			if (view != null)
				view.Updateclock ();
		}
		
		#region implemented abstract members of GoogleMusic.MainViewController
		public override void SetState (bool state)
		{
			/*
			NowPlayingView.MainLabel.Text = Util.CurrentSong.Artist;
			NowPlayingView.MiddleLabel.Text = Util.CurrentSong.Title;
			NowPlayingView.BottomLabel.Text = Util.CurrentSong.Album;
			NowPlayingView.UpdateAlbum (Util.CurrentSong.TheAlbum);*/
			if (view != null)
				view.SetState (state);
		}
		public override void UpdateSong (Song currentSong)
		{
			if(view != null)
			{
				view.UpdateAlbum (currentSong.TheAlbum);
			}
		}

		public override void UpdateStatus (string currentTime, string remainingTime, float percent)
		{
			if (view != null)
				view.UpdateStatus (currentTime, remainingTime, percent);
		}

		public override void SetPlayCount ()
		{
			
		}

		public override void ShowNowPlaying ()
		{
		
		}

		public override void RefreshSongs ()
		{
			if (view != null)
				view.RefreshSongs ();
		}

		public override void RefreshArtists ()
		{
			if (view != null)
				view.RefreshArtists ();
		}

		public override void RefreshGenre ()
		{
#if !mp3tunes
			if (view != null)
				view.detailView.genreVc.HandleUtilSongsCollectionChanged ();
#endif
		}

		public override void RefreshAlbum ()
		{
			if (view != null)
				view.detailView.albumVc.HandleUtilSongsCollectionChanged ();
		}

		public override void RefreshPlaylist ()
		{
			if (view != null) {
				view.detailView.playlistVc.HandleUtilSongsCollectionChanged ();
				view.detailView.autoPlaylistVc.HandleUtilSongsCollectionChanged ();
			}
		}
		public void RefreshRows()
		{
			if(view != null)
				view.RefreshRows();
		}

		public override void UpdateSongProgress (float percent)
		{
			//if(view != null)
		}

		public override void UpdatePlaylistProgress (float percent)
		{
			;
		}

		public override void UpdateCurrentSongDownloadProgress (float percent)
		{
			//throw new System.NotImplementedException ();
			if (view != null)
				view.UpdateCurrentSongDownloadProgress (percent);
		}

		public override void UpdateMeter ()
		{
			if (view == null)
				return;
			view.levelMeter.AudioLevelState = Util.Player.AudioLevelState;
			view.levelMeter.SetNeedsDisplay ();
		}

		public override void GoToArtist (int artistId)
		{
			if (view == null)
				return;
			if (!Util.ArtistsDict.ContainsKey (artistId))
				return;
			
			NavigationController.SelectedIndex = 1; 
			view.detailView.artistDvc.NavigationController.PopToRootViewController (false);
			view.detailView.artistDvc.Selected (Util.ArtistsDict [artistId]);
		}

		public override void GoToAlbum (int albumId)
		{			
			if (!Util.AlbumsDict.ContainsKey (albumId))
				return;
			NavigationController.SelectedIndex = 3;
			view.detailView.albumVc.NavigationController.PopToRootViewController (false);
			var album = Util.AlbumsDict [albumId];
			view.detailView.albumVc.Selected(album);
		}

		public override void GoToGenre (int genreId)
		{
#if !mp3tunes
			if (!Util.GenresDict.ContainsKey (genreId))
				return;
			NavigationController.SelectedIndex = 4;
			view.detailView.genreVc.NavigationController.PopToRootViewController (false);
			view.detailView.genreVc.Selected(Util.GenresDict[genreId]);
#endif
		}

		public override void ToggleMenu ()
		{
			NavigationController.ToggleMenu ();
		}

		public override void DownloaderUpdated ()
		{
			//throw new System.NotImplementedException ();
		}
		#endregion
		
#if mp3tunes
		public override void RefreshMovies ()
		{
			//TODO: enable videos
			//movieVc.HandleUtilSongsCollectionChanged();
		}
		
		public override void ShowMovieController(MPMoviePlayerViewController vc)
		{
			//NavigationController.View.AddSubview(vc.View);
			//	vc.SetFullscreen(true,true);
			//vc.MoviePlayer.SetFullscreen
			//NavigationController.PresentModalViewController(vc,true);
		}
			

#endif
		class TouchEvent
		{
			public NSSet Touches;
			public UIEvent Event;
			
			public TouchEvent (NSSet touches, UIEvent evt)
			{
				Touches = touches;
				Event = evt;
			}
		}
		
		TouchEvent capturedEnded;
		bool ignoreUntilLift;
		bool swipeDetectionDisabled;
		PointF? touchStart;
			
		public override void PtouchesBeganwithEvent (NSSet touches, NSObject evt)
		{
			var touch = touches.AnyObject as PTouch;
			if(touch.View is UISlider || touch.View is MPVolumeView)
				Console.WriteLine("slider");
			touchStart = touch.LocationInView (this.View);
			swipeDetectionDisabled = false;
			//base.PtouchesBeganwithEvent(touches,evt);

		}
			
		public override void PtouchesEndedwithEvent (NSSet touches, UIEvent evt)
		{
			ignoreUntilLift = false;

				
			touchStart = null;
	//		base.PtouchesEndedwithEvent(touches,evt);
		}

		public override void PtouchesMovedwithEvent (NSSet touches, UIEvent evt)
		{
			if (!swipeDetectionDisabled) {
				if (touchStart != null) {
					var touch = touches.AnyObject as PTouch;
					var currentPos = touch.LocationInView (this.View);
					var progressPoint = touch.LocationInView(view.progessBar);
					Console.WriteLine(view.progessBar.Bounds.Contains(progressPoint) + " - " + progressPoint + " : " + view.progessBar.Bounds);
					if(view.progessBar.Bounds.Contains(progressPoint))
						return;
					var deltaX = Math.Abs (touchStart.Value.X - currentPos.X);
					var deltaY = Math.Abs (touchStart.Value.Y - currentPos.Y);
							
					if (deltaY < 10 && deltaX > 30) {
						
						this.NavigationController.ShowMenu ();
						ignoreUntilLift = true;
						swipeDetectionDisabled = true;
						touchStart = null;
						return;
								
					} 
						
				}
			}
			if (ignoreUntilLift)
				return;
		//	base.PtouchesMovedwithEvent(touches,evt);
		}
		
		public class TvOutView : UIView
		{
			TVOutBackgroundView bg;
			public ProgressBar progessBar;
			//iPadPlayBackControls controls;
			MusicSectionView musicView;
			TvOutViewController Parent;
			UILabel clock;
			UIImageView albumImage;
			public LevelMeter levelMeter;
			public UIButton artistBtn;
			public UIButton albumBtn;
			public UIButton genreBtn;
			public UILabel songLabel;
			public DetailViewController detailView;
			public UIView test;
			public UIButton playPause;
			public UIButton previous;
			public UIButton next;
			UIButton menuButton;
			
			UIImage playImage;
			UIImage pauseImage;
			static UIImage shuffleOff = UIImage.FromFile("Images/shuffleOff.png");
			static UIImage shuffleOn = UIImage.FromFile("Images/shuffleOn.png");
			static UIImage repeatOff = UIImage.FromFile("Images/repeatOff.png");
			static UIImage repeatAll = UIImage.FromFile("Images/repeatAll.png");
			static UIImage repeatOne = UIImage.FromFile("Images/repeatOne.png");
			static UIImage thumbsUpOff = UIImage.FromFile("Images/thumbsUp.png");
			static UIImage thumbsUpOn = UIImage.FromFile("Images/thumbsUpPressed.png");
			public TvOutView (TvOutViewController parent)
			{
				test = new UIView (new RectangleF (0, 0, 40, 40));
				test.BackgroundColor = UIColor.Red;
				Parent = parent;
				bg = new TVOutBackgroundView ();
				bg.AlbumArtUpdates (null);
				this.AddSubview (bg);
				
				progessBar = new ProgressBar ();
				this.AddSubview (progessBar);
				
				//controls = new iPadPlayBackControls ();
				//controls.BackgroundColor = UIColor.Black.ColorWithAlpha(.5f);
				//this.Add (controls);
				
				clock = new UILabel (){TextColor = UIColor.White,BackgroundColor = UIColor.Clear,AdjustsFontSizeToFitWidth = true};
				clock.Font = UIFont.BoldSystemFontOfSize (30f);
				clock.ShadowColor = UIColor.DarkGray;
				clock.ShadowOffset = new SizeF (1, 1);
				clock.TextAlignment = UITextAlignment.Center;
				this.AddSubview (clock);
				
				detailView = new DetailViewController ();
				this.AddSubview (detailView.View);
				//songVc = new SongViewController(){DarkThemed = true};
				//songVc.TableView.BackgroundColor = UIColor.Clear;// UIColor.White.ColorWithAlpha(.15f);
				//songVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.DoubleLineEtched;
				//songVc.ViewWillAppear(true);
				//songNav = new UINavigationController(songVc);
				//this.AddSubview(songNav.View);
				
				albumImage = new UIImageView ();
				this.AddSubview (albumImage);
				
				levelMeter = new LevelMeter (new RectangleF (0, 0, 700, 320));
				//levelMeter.PaddingForColumns = 340;
				artistBtn = new UIButton ();
				artistBtn.TouchDown += delegate(object sender, EventArgs e) {
					if (Util.CurrentSong == null || Util.CurrentSong.ArtistId <= 0)
						return;
					Parent.GoToArtist (Util.CurrentSong.ArtistId);
				};
				artistBtn.Font = UIFont.BoldSystemFontOfSize (20f);
				artistBtn.SetTitleColor (UIColor.LightTextColor, UIControlState.Normal);
				artistBtn.SetTitleShadowColor (UIColor.DarkGray, UIControlState.Normal);
				artistBtn.TitleShadowOffset = new SizeF (1, 1);
				
				albumBtn = new UIButton ();
				albumBtn.SetTitleColor (UIColor.LightTextColor, UIControlState.Normal);
				albumBtn.TouchDown += delegate(object sender, EventArgs e) {
					if (Util.CurrentSong == null || Util.CurrentSong.AlbumId <= 0)
						return;
					Parent.GoToAlbum (Util.CurrentSong.AlbumId);
				};
				albumBtn.Font = UIFont.BoldSystemFontOfSize (20f);
				genreBtn = new UIButton ();				
				genreBtn.SetTitleColor (UIColor.LightTextColor, UIControlState.Normal);
				genreBtn.TouchDown += delegate(object sender, EventArgs e) {
					//if (Util.CurrentSong == null || Util.CurrentSong.GenreId <= 0)
					//	return;
					//Parent.GoToGenre (Util.CurrentSong.GenreId);
				};
				songLabel = new UILabel ();
				songLabel.Font = UIFont.BoldSystemFontOfSize (25f);
				songLabel.TextColor = UIColor.White;
				songLabel.BackgroundColor = UIColor.Clear;
				songLabel.TextAlignment = UITextAlignment.Center;
				//this.AddSubview(levelMeter);

				playPause = new UIButton();
				playImage = UIImage.FromFile("Images/playTv.png");
				pauseImage = UIImage.FromFile("Images/pauseTv.png");
				playPause.SetImage(playImage,UIControlState.Normal);
				playPause.TouchDown += delegate {
					Util.PlayPause();
				};
				this.AddSubview(playPause);

				previous = new UIButton();
				previous.SetImage(UIImage.FromFile("Images/previousTv.png"),UIControlState.Normal);
				previous.TouchDown += delegate {
					Util.Previous();
				};
				this.AddSubview(previous);

				next = new UIButton();
				next.SetImage(UIImage.FromFile("Images/nextTv.png"),UIControlState.Normal);
				next.TouchDown += delegate {
					Util.Next();
				};
				this.AddSubview(next);

				menuButton = new UIButton(UIButtonType.InfoDark);
				menuButton.Frame = new RectangleF(10,progessBar.Frame.Y,100,100);
				menuButton.SetImage(UIImage.FromFile("Images/menu.png"), UIControlState.Normal);
				try
				{
					menuButton.SetBackgroundImage( UIImage.FromFile("Images/menubar-button.png"), UIControlState.Normal);
				}
				catch(Exception)
				{

				}
				menuButton.TouchDown += delegate {
					parent.NavigationController.ShowMenu();
				};
				this.AddSubview (menuButton);

				this.AddSubview (artistBtn);
				this.AddSubview (albumBtn);
				this.AddSubview (genreBtn);
				this.AddSubview (songLabel);
				
				//navigationController.ShowMenu ();

			}

			void NavigationSelected (MonoTouch.Foundation.NSIndexPath obj)
			{

			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				bg.Frame = this.Bounds;
				
				RectangleF frame = RectangleF.Empty;
				frame.Width = (this.Bounds.Width / 3) * 2;
				frame.Height = 30;
				frame.Y = 10f;
				frame.X = (this.Bounds.Width - frame.Width) / 2;
				progessBar.Frame = frame;
				
				clock.Frame = new RectangleF (this.Bounds.Right - 160, 10, 150, clock.Font.PointSize + 5);
				
				frame.Height = progessBar.Frame.Bottom - clock.Frame.Bottom;
				frame.Width = frame.Height * 2;
				frame.Y = clock.Frame.Bottom + 10;
				frame.X = (progessBar.Frame.X - levelMeter.Frame.Width) / 2;
				levelMeter.Frame = frame;
				
				
				frame.Y = progessBar.Frame.Bottom;
				frame.Width = 400;
				frame.X = Bounds.Right - frame.Width - 10;//FlyOutNavigationController.menuWidth);
				frame.Height = this.Bounds.Height - frame.Y;
				detailView.View.Frame = frame;
				
				var left = detailView.View.Frame.Left;
				var mid = left / 2;
				var albumWidth = (this.Bounds.Right - 400) / 3;
				frame = new RectangleF ((mid - albumWidth / 2), progessBar.Frame.Bottom + 43, albumWidth, albumWidth);
				albumImage.Frame = frame;
				
				
				frame.Y = frame.Bottom + 10;
				frame.X = 10;
				frame.Width = left - 20;
				frame.Height = 25;
				var padding = 10;
				artistBtn.Frame = frame;
				//artistBtn.SizeToFit ();
				
				frame.Y += artistBtn.Frame.Height + padding;
				songLabel.Frame = frame;
				//songLabel.SizeToFit ();
				frame.Y += songLabel.Frame.Height + padding;
				albumBtn.Frame = frame;
				//albumBtn.SizeToFit ();
				frame.Y += albumBtn.Frame.Height + padding;
				genreBtn.Frame = frame;
				//genreBtn.SizeToFit ();
				frame.Y += genreBtn.Frame.Height + padding;

				
				var playSize = 80;
			 	playPause.Frame = new RectangleF(frame.GetMidX() - (playSize /2),Bounds.Height  - padding - playSize,playSize,playSize);
				var prevSize = 70;
				previous.Frame = new RectangleF(playPause.Frame.X - (prevSize + 10),playPause.Frame.Y + 3,prevSize,prevSize);
				next.Frame = new RectangleF(playPause.Frame.Right + 10 ,previous.Frame.Y,prevSize,prevSize);
				/*
				frame = songNav.View.Frame;
				frame.X = navigation.TableView.Frame.Right + 10;
				frame.Y = controls.Frame.Bottom;
				frame.Width = Bounds.Width - frame.X - 10;
				frame.Height = this.Bounds.Height - frame.Y - 10;
				songNav.View.Frame = frame;
				*/
			}

			public void RefreshSongs ()
			{
				detailView.songVc.HandleUtilSongsCollectionChanged ();
			}
			public void RefreshRows()
			{
				var paths = detailView.songVc.TableView.IndexPathsForVisibleRows;
				if(paths != null && paths.Length > 0)
					detailView.songVc.TableView.ReloadRows(paths, UITableViewRowAnimation.None);
			}
			
			public void RefreshArtists ()
			{
				detailView.artistDvc.HandleUtilSongsCollectionChanged ();
			}
			
			public void UpdateCurrentSongDownloadProgress (float percent)
			{
				progessBar.UpdateDownloadStatus (percent);
				//controls.DownloadProgressView.Progress = percent;
			}

			public  void SetState (bool state)
			{
				playPause.SetImage(state? pauseImage : playImage , UIControlState.Normal);
				//controls.SetState (state);
				UpdateAlbum (Util.CurrentSong.TheAlbum);
				if (Util.CurrentSong != null) {	
					albumBtn.SetTitle (Util.CurrentSong.Album, UIControlState.Normal);
					//albumBtn.SizeToFit ();
					artistBtn.SetTitle (Util.CurrentSong.Artist, UIControlState.Normal);
					//artistBtn.SizeToFit ();
					genreBtn.SetTitle (Util.CurrentSong.Genre, UIControlState.Normal);
					//genreBtn.SizeToFit ();
					songLabel.Text = Util.CurrentSong.Title;
					//songLabel.SizeToFit ();
				}
			}
	
			public  void UpdateStatus (string currentTime, string remainingTime, float percent)
			{
				progessBar.UpdateStatus (currentTime, remainingTime, percent);
				
			}

			public void Updateclock ()
			{
				clock.Text = DateTime.Now.ToShortTimeString ();
			}
			
			Album currentAlbum;

			public void UpdateAlbum (Album album)
			{
				if (album == currentAlbum)
					return;
				try {
					if (currentAlbum != null)
						currentAlbum.ALbumArtUpdated -= HandleCurrentAlbumALbumArtUpdated;
				} catch (Exception ex) {
					
				}
				
				currentAlbum = album;
				if (currentAlbum == null) {
					albumImage.Image = AlbumImageView.defaultImage;
					bg.AlbumArtUpdates (AlbumImageView.defaultImage);
					return;
				}
				
				currentAlbum.ALbumArtUpdated += HandleCurrentAlbumALbumArtUpdated;
				albumImage.Image = currentAlbum.AlbumArt ((int)Math.Max (this.Bounds.Width, this.Bounds.Height));
				bg.AlbumArtUpdates (albumImage.Image);
			}
			
			void HandleCurrentAlbumALbumArtUpdated (object sender, EventArgs e)
			{
				albumImage.Image = currentAlbum.AlbumArt ((int)Math.Max (this.Bounds.Width, this.Bounds.Height));
				bg.AlbumArtUpdates (albumImage.Image);
			}
			
			public class DetailViewController : UIViewController
			{
				public ArtistDialogViewController artistDvc;
				public AlbumViewController albumVc;
				public SongViewController songVc;
#if !mp3tunes
				public GenreViewController genreVc;
#endif
				public PlaylistViewController playlistVc;
				public PlaylistViewController autoPlaylistVc;
				public SettingsViewController settingVc;
				public DownloadQueue downloadVc;
				//CoverflowView CoverflowView;
				//public CurrentQueueViewController queueVc;
				//public DownloadQueue downloadVc;
				//public CurrentQueueViewController currentQueueVc;
#if mp3tunes
		//public MovieViewController movieVc;
#endif
				
				public UIViewController[] navigationRoots;

				public DetailViewController ()
				{
						
					#if mp3tunes
					//movieVc = new MovieViewController(){DarkThemed = true};
					//movieVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					//movieVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					#else
					genreVc = new GenreViewController (false){DarkThemed = true};
					genreVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					genreVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					#endif
					artistDvc = new ArtistDialogViewController (false){DarkThemed = true};
					artistDvc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					artistDvc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					albumVc = new AlbumViewController (false){DarkThemed = true};
					albumVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					;
					albumVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					songVc = new SongViewController (false){DarkThemed = true};
					songVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					songVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

					playlistVc = new PlaylistViewController (false){DarkThemed = true};
					playlistVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					playlistVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

					autoPlaylistVc = new PlaylistViewController (false, true, false){DarkThemed = true};
					autoPlaylistVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					autoPlaylistVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

					settingVc = new SettingsViewController ();
					downloadVc = new DownloadQueue (){DarkThemed = true};
					downloadVc.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					;
					downloadVc.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					//queueVc = new CurrentQueueViewController ();
					//downloadVc = new DownloadQueue();
					//currentQueueVc = new CurrentQueueViewController();
					#if mp3tunes
					int settingsIndex = 5;
					#else
					int settingsIndex = 6;
					#endif
					navigationRoots = new UIViewController[]{
							new UINavigationController (playlistVc),
							new UINavigationController (artistDvc),
							new UINavigationController (songVc),
							new UINavigationController (albumVc),
					#if mp3tunes
							//new UINavigationController(movieVc),
					#else
							new UINavigationController (genreVc),
					#endif
						
							new UINavigationController (autoPlaylistVc),
		
							new UINavigationController (downloadVc),
							null,
							new UINavigationController (settingVc),
					};
					SetIndex (0);
					playlistVc.HandleUtilSongsCollectionChanged ();
					//currentView.ViewWillAppear(true);
				}

				UIViewController currentView;

				public void SetIndex (int index)
				{
					RectangleF frame = RectangleF.Empty;
					if (navigationRoots [index] == null)
						return;
					if (currentView != null) {
						frame = currentView.View.Frame;
						currentView.View.RemoveFromSuperview ();
					}
					currentView = navigationRoots [index];
					if (frame != RectangleF.Empty)
						currentView.View.Frame = frame;
					this.View.AddSubview (currentView.View);
					currentView.ViewWillAppear (true);
				
				}
			}
		}
	
	}
}

