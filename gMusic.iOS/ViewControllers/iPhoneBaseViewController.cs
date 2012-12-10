using System;
using MonoTouch.UIKit;
using FlyOutNavigation;
using ClanceysLib;
using MonoTouch.Dialog;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.MediaPlayer;
using MonoTouch.CoreAnimation;
using System.Drawing;
using MonoTouch.CoreGraphics;
using Pioneer;
//using Tapku;
using MonoTouch.AVFoundation;

namespace GoogleMusic
{
	public class iPhoneBaseViewController : MainViewController
	{
		AVAudioPlayer SilentPlayer;
		SongViewController songVc;

		string currentPlayId {
			get;
			set;
		}

		public iPhoneBaseViewController () : base(true)
		{	
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var outError = new NSError ();
			//SilentPlayer = AVAudioPlayer.FromUrl(new NSUrl("empty.mp3",true),out outError);
			//SilentPlayer.NumberOfLoops = -1;
			if (Settings.CurrentTab == 0)
				Settings.CurrentTab = 1;
			NavigationController = new FlyOutNavigationController ();
			songVc = new SongViewController ();
			NavigationController.ViewControllers = new UIViewController[]{
				new UINavigationController (songVc),
			};
			NavigationController.NavigationRoot = new RootElement (""){
				new Section(){
					new StringElement("songs")
				}
			};
			this.View.AddSubview(NavigationController.View);
			this.AddChildViewController (NavigationController);
		}

		public override void UpdateSong (Song currentSong)
		{
			NowPlayingView.Update (currentSong);
//			if(playingViewController != null)
//			{
//				playingViewController.UpdateSong(currentSong);
//			}
		}
		public override void PlaylistChanged ()
		{
//			playingViewController.PlaylistChanged();
		}
		public override void SetState (bool state)
		{

//			if(state && !SilentPlayer.Playing)
//				SilentPlayer.Play();
//			else if(!state && SilentPlayer.Playing)
//				SilentPlayer.Pause();
				
			if (Util.CurrentSong == null)
				return;
//			if(tvViewController != null)
//				tvViewController.SetState(state);
//			NowPlayingView.MainLabel.Text = Util.CurrentSong.Artist;
//			NowPlayingView.MiddleLabel.Text = Util.CurrentSong.Title;
//			NowPlayingView.BottomLabel.Text = Util.CurrentSong.Album;
//			NowPlayingView.UpdateAlbum (Util.CurrentSong.TheAlbum);
				
//			if (playingViewController != null)
//				playingViewController.SetState (state);
			if (currentPlayId != Util.CurrentSong.Id) {
				try{
				var paths = songVc.TableView.IndexPathsForVisibleRows;
					if(paths != null && paths.Length > 0)
					songVc.TableView.ReloadRows(paths, UITableViewRowAnimation.Automatic);
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
//				if(tvViewController != null)
//				{
//				//	tvViewController.RefreshRows();
//				}
				currentPlayId = Util.CurrentSong.Id;
			}
			
		}

		public override void UpdateStatus (string currentTime, string remainingTime, float percent)
		{
//			if(tvViewController != null)
//				tvViewController.UpdateStatus(currentTime,remainingTime,percent);
//			if (playingViewController != null && playingViewController.detailView != null) {
//				if(playingViewController.Seeking)
//				{
//					var seekPercent =  playingViewController.CurrentTimeSlider.Value;
//					var totalTime = Util.CurrentSong.Duration/1000;
//					var newTime = totalTime * seekPercent;
//					playingViewController.detailView.currentTimeLabel.Text = Util.FormatTimeSpan (TimeSpan.FromSeconds (newTime));
//					playingViewController.detailView.remainingTimeLabel.Text =	"-" + Util.FormatTimeSpan (TimeSpan.FromSeconds (totalTime - newTime));
//
//				}
//				else{
//					playingViewController.detailView.currentTimeLabel.Text = currentTime;
//					playingViewController.detailView.remainingTimeLabel.Text = remainingTime;
//					playingViewController.SetSliderProgress(percent);
//				}
//			} else {
//				//	Console.WriteLine("Playing view contorller is null");	
//			}
		}
		
		public override void UpdateCurrentSongDownloadProgress (float percent)
		{
			Util.EnsureInvokedOnMainThread (delegate {
//				if(playingViewController != null)
//					playingViewController.DownloadProgressView.Progress = percent;
//				if(tvViewController != null)
//					tvViewController.UpdateCurrentSongDownloadProgress(percent);
			});
		}
		public override bool ShouldAutorotate ()
		{
			return false;
		}
			
		public override void UpdateMeter ()
		{
//			Util.SongLevelMeter.AudioLevelState = Util.Player.AudioLevelState;
//			Util.SongLevelMeter.SetNeedsDisplay ();
//			if(tvViewController != null)
//					tvViewController.UpdateMeter();
		}
		
		public override void ToggleMenu ()
		{
			NavigationController.ToggleMenu ();
		}

		public override void SetPlayCount ()
		{
//			if(tvViewController != null)
//				tvViewController.SetPlayCount();
//			if (playingViewController != null)
//				playingViewController.SetPlayCount ();
		}

		public override void ShowNowPlaying ()
		{			
			NavigationController.SelectedIndex = 0;
			// We can not push a nav controller into a nav controller
			//(NavigationController.CurrentViewController as UINavigationController).PushViewController (, true);
			//queueVc.TableView.ReloadData ();
		}

		public override void RefreshAlbum ()
		{
//			if(tvViewController != null)
//				tvViewController.RefreshAlbum();
//			albumVc.HandleUtilSongsCollectionChanged ();
			//if(Util.IsTall)
//			if(false)
//				CoverflowView.UpdatedAlbums ();
		}

		public override void RefreshArtists ()
		{
//			if(tvViewController != null)
//				tvViewController.RefreshArtists();
//			artistDvc.HandleUtilSongsCollectionChanged ();
		}

		public override void RefreshGenre ()
		{
#if !mp3tunes
//			if(tvViewController != null)
//				tvViewController.RefreshGenre();
//			genreVc.HandleUtilSongsCollectionChanged ();
#endif
		}
		public override void DownloaderUpdated ()
		{
			this.BeginInvokeOnMainThread(delegate{
//			if(tvViewController != null)
//				tvViewController.DownloaderUpdated();
//			downloadVc.TableView.ReloadData();
			});
		}

		public override void RefreshSongs ()
		{
			songVc.HandleUtilSongsCollectionChanged ();
//			if(tvViewController != null)
//				tvViewController.RefreshSongs();
		}

		public override void RefreshPlaylist ()
		{
//			if(tvViewController != null)
//				tvViewController.RefreshPlaylist();
//			playlistVc.HandleUtilSongsCollectionChanged ();
//			autoPlaylistVc.HandleUtilSongsCollectionChanged ();
		}
		public override void GoToArtist (int artistID)
		{
			if (!Util.ArtistsDict.ContainsKey (artistID))
				return;
			
//			NavigationController.SelectedIndex = 2; 
//			artistDvc.NavigationController.PopToRootViewController (false);
//			artistDvc.Selected(Util.ArtistsDict [artistID]);
		}
		public override void GoToAlbum (int albumId)
		{
			if (!Util.ArtistsDict.ContainsKey (albumId))
				return;
//			NavigationController.SelectedIndex = 4;
//			albumVc.NavigationController.PopToRootViewController (false);
//			var album = Util.AlbumsDict[albumId];
//			albumVc.GoToAlbum(album);
		}
		public override void GoToGenre (int genreId)
		{
#if !mp3tunes
			if (!Util.GenresDict.ContainsKey (genreId))
				return;
//			NavigationController.SelectedIndex = 5;
//			genreVc.NavigationController.PopToRootViewController (false);
//			genreVc.Selected(Util.GenresDict[genreId]);
#endif
		}
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

		#region implemented abstract members of GoogleMusic.MainViewController
		public override void UpdateSongProgress (float percent)
		{
			try {
				
//				if(tvViewController != null)
//					tvViewController.UpdateSongProgress(percent);
				songVc.UpdateProgress (percent);
#if !mp3tunes				
				//genreVc.UpdateProgress (percent);
#endif
			} catch (Exception ex) {
				Console.WriteLine (ex);	
			}
		}

		public override void UpdatePlaylistProgress (float percent)
		{
			
//			if(tvViewController != null)
//				tvViewController.UpdatePlaylistProgress(percent);
//			playlistVc.UpdateProgress (percent);
		}
		#endregion
			
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

		}
	}
}
