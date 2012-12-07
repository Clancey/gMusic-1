using System;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Threading;
using System.IO;
using ClanceysLib;

namespace GoogleMusic
{
	public class PlaylistViewController: BaseViewController
	{
		private Source DataSource;
		public bool IsPicker = false;
		public Action<Playlist> OnPlaylistSelected;
		bool AutoPlaylist;
		

		public PlaylistViewController (bool hasShuffle) : base (UITableViewStyle.Plain,false,hasShuffle)
		{
			//	Root = new RootElement ("Artists");
			Screen = Screens.Playlist;
			this.Title = "Playlists".Translate();
		}
		public PlaylistViewController(bool isPicker,bool autoPlaylist) : this(isPicker,autoPlaylist,true)
		{
			
		}
		public PlaylistViewController (bool isPicker,bool autoPlaylist,bool hasShuffle) : this(hasShuffle)
		{
			AutoPlaylist = autoPlaylist;
			IsPicker = isPicker;
			HasRefreshed = true;
			if(autoPlaylist)
				this.Title = "Auto Playlists".Translate();
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ? 80 : 40;
		}
		
		public override bool CanBecomeFirstResponder {
			get { return true; }
		}
		
		TextFieldAlertView textInput;
		public override void ViewWillAppear (bool animated)
		{
			if(TableView.TableHeaderView != headerView)
				setupTable();
			//base.ViewWillAppear (animated);
			
			checkIfLoaded();
			if(this.NavigationController != null)
			{
				this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
				try
				{
					if(DarkThemed)
					{
						float[] colorMask = new float[6]{222, 255, 0, 255, 222, 255};
						UIImage img = UIImage.FromFile("Images/menubar.png");
						UIImage maskedImage = UIImage.FromImage(img.CGImage.WithMaskingColors(colorMask));
						
						this.NavigationController.NavigationBar.SetBackgroundImage( maskedImage,UIBarMetrics.Default);
					}
				}
				catch{
				}
				NavigationItem.HidesBackButton = false;
				if (!DarkThemed && Util.CurrentSong != null && !IsPicker && Util.IsIphone)
				{
					var nowPlayingBtn = BaseViewController.CreateNowPlayingButton();
					this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(nowPlayingBtn);// new UIBarButtonItem (UIImage.FromFile ("Images/nowPlaying.png"), UIBarButtonItemStyle.Bordered, delegate {

				}
	//#if !gmusic
				else if (IsPicker)
					this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, delegate {
						this.BeginInvokeOnMainThread (delegate{
							textInput = new TextFieldAlertView (false, "New Playlist Name".Translate(), "");
							textInput.ConfirmText = "Add".Translate();
							textInput.Clicked += delegate(object sender, UIButtonEventArgs e) {
								if (e.ButtonIndex > 0) {
									FinishSearch ();
									this.DeactivateController (true);
									if (OnPlaylistSelected != null)
										OnPlaylistSelected (new Playlist (){Name = textInput.EnteredText});
								}
							};
							textInput.Show ();
							
							
						});
					});
	//#endif
				else
					this.NavigationItem.RightBarButtonItem = null;
				if(!IsPicker && !DarkThemed && (Util.IsIphone || (Util.MainVC.InterfaceOrientation == UIInterfaceOrientation.Portrait || Util.MainVC.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)))
					this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIImage.FromFile("Images/menu.png"), UIBarButtonItemStyle.Bordered, delegate {
						//TODO fix TVOUT
						//if(DarkThemed)
						//	Util.MainVC.tvViewController.ToggleMenu();
						//else
						{
							searchBar.ResignFirstResponder();
							Util.MainVC.ToggleMenu();
						}
					});				
				else if(!IsPicker) 
				{
					this.NavigationItem.LeftBarButtonItem = null;
					this.NavigationItem.HidesBackButton = true;
				}
				if(Util.IsIpad)					
					this.NavigationController.NavigationBar.Frame = this.NavigationController.NavigationBar.Frame.SetLocation(0,0);
			}
		
			
			
		}

		void Selected (Playlist playlist)
		{
			FinishSearch ();
			this.NavigationItem.HidesBackButton = false;
			if (IsPicker) {
				this.DeactivateController (true);
				if (OnPlaylistSelected != null)
					OnPlaylistSelected (playlist);
				
			} else
			{
				CurrentSongListViewController = new SonglistViewController (Screen, playlist,DarkThemed, !(DarkThemed || Util.IsIpad)){DarkThemed = DarkThemed};
				if(DarkThemed){
					CurrentSongListViewController.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					CurrentSongListViewController.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
				}
				this.ActivateController (CurrentSongListViewController);
			}
			//this.ActivateController(new ArtistAlbumViewController(artist));
		}
		#region data source
		public class Source : UITableViewSource
		{
			const float yboundary = 65;
			protected PlaylistViewController Container;

			public bool IsSearching{ get; set; }

			public List<Playlist> SearchResults = new List<Playlist> ();
			
			public Source (PlaylistViewController container)
			{
				this.Container = container;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				if (IsSearching)
					return SearchResults.Count;
				else if(Settings.ShowOfflineOnly)
					return (Container.AutoPlaylist) ? Database.Main.AutoPlaylists.Count() : Database.Main.OfflinePlaylistsList.Count ();
				var count = (Container.AutoPlaylist) ? Database.Main.AutoPlaylists.Count() : Database.Main.PlaylistsList.Count ();//Items.Where(x=> x.IndexCharacter == headers[section]).Count();
				return count;
			}
			
			
			string[] array;
			public override string[] SectionIndexTitles (UITableView tableView)
			{
				if(array == null)
					array = new string[]{};
				//array = NSArray.FromStrings (Util.Genre.Select(x=> (x.Key ?? "")).ToArray());
			
			    return array;
			}
			
			public override int SectionFor (UITableView tableView, string title, int atIndex)
			{
				return atIndex;
			}
			
			
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				return""; //Util.PlaylistsList[section].Name;
			}

			string key = "playlistCell";

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Playlist playlist;
				if (IsSearching)
					playlist = SearchResults [indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					playlist = Container.AutoPlaylist ? Database.Main.AutoPlaylists[indexPath.Row] : Database.Main.OfflinePlaylistsList [indexPath.Row];	
				else
					playlist = Container.AutoPlaylist ? Database.Main.AutoPlaylists[indexPath.Row] : Database.Main.PlaylistsList [indexPath.Row];//.Where(x=> x.IndexCharacter == headers[indexPath.Section]).ToArray()[indexPath.Row];
				var cell  = playlist.GetCell(tableView,Container.IsPicker,Container.DarkThemed);
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Playlist playlist;
				if (IsSearching)
					playlist = SearchResults [indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					playlist = Container.AutoPlaylist ? Database.Main.AutoPlaylists[indexPath.Row] : Database.Main.OfflinePlaylistsList [indexPath.Row];					
				else
					playlist = Container.AutoPlaylist ? Database.Main.AutoPlaylists[indexPath.Row] : Database.Main.PlaylistsList [indexPath.Row];
				Container.Selected (playlist);
			}

			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				var rowFrame = tableView.RectForRowAtIndexPath (indexPath);
				var midY = (rowFrame.Height / 2) + rowFrame.Y;
				var x = rowFrame.Width - 50;
				var point = Container.View.ConvertPointToView (new PointF (x, midY), Util.BaseView);
				Playlist playlist;
				if (IsSearching)
					playlist = SearchResults [indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					playlist = Container.AutoPlaylist ? Database.Main.AutoPlaylists[indexPath.Row] : Database.Main.OfflinePlaylistsList [indexPath.Row];
				else
					playlist = Container.AutoPlaylist ? Database.Main.AutoPlaylists[indexPath.Row] : Database.Main.PlaylistsList [indexPath.Row];
				Container.ShowPopUp (playlist.ShouldBeLocal, (index) =>
				{
					if (index == 0)
						Util.PlayPlaylist (null, playlist);
					else if (index == 4) {

						var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this playlist?".Translate());//
						alert.SetCancelButton("Cancel".Translate(),null);
						alert.AddButton("Delete".Translate(),delegate{
								Util.Api.DeletePlaylist (playlist, (success) => {
									if (!success) {
									var alert2 = new BlockAlertView ("Error".Translate(), "There was an error deleting the playlist.".Translate());
									alert2.AddButton("Ok".Translate(),null);
										alert2.Show ();
									} else {
										Container.TableView.DeleteRows (new[]{indexPath}, UITableViewRowAnimation.Fade);
									}
								});
							}
						);
						alert.Show ();	
						
					}
				}, (shouldBeLocal) => {
					//TODO Eithre download or delete!
					ThreadPool.QueueUserWorkItem (delegate{
						//var genreId = playlist.Id;
						playlist.ShouldBeLocal = shouldBeLocal;
						if (shouldBeLocal) {
							//TODO: update offline
							
							lock (Database.Locker) {
								Database.Main.Update (playlist);
								List<Song> songs;
								
								Database.Main.Query<Song> ("update song  set ShouldBeLocal = ? where id in (select s.id from song s inner join PlaylistSongs p on s.Id = p.SongId where p.ServerPlaylistId = '" + playlist.ServerId + "')", true);
								string query = "select s.Id from song s inner join PlaylistSongs p on s.Id = p.SongId where p.ServerPlaylistId = '" + playlist.ServerId + "' order by p.SOrder";
								
								songs = Database.Main.Query<Song> (query).ToList ();
	
								foreach (var song in songs) {
									Song realSong  = Database.Main.SongsDict[song.Id];
									realSong.ShouldBeLocal = true;
									//Database.Main.Update(realSong);
									//TODO: Fix downloader
									//if (!realSong.IsLocal)
									//	Downloader.AddFile (realSong);
								}
							}
								
						} else {
							//TODO: update offline
							
							List<Song> SongsToDelete;
							lock (Database.Locker) {
								Database.Main.Update (playlist);
								
								Database.Main.Query<Song> ("update song set IsLocal = ?, ShouldBeLocal = ? where id in (select s.id from song s inner join PlaylistSongs p on s.Id = p.SongId where p.ServerPlaylistId = '" + playlist.ServerId + "')", false, false);
								string query = "select distinct s.* from song s inner join PlaylistSongs p on s.Id = p.SongId where p.ServerPlaylistId = '" + playlist.ServerId + "' order by p.SOrder";
								
								SongsToDelete = Database.Main.Query<Song> (query).ToList ();
	
							}
							foreach (var song in SongsToDelete) {
								if (File.Exists (Util.MusicDir + song.FileName))
									File.Delete (Util.MusicDir + song.FileName);
								Database.Main.UpdateDeleteOffline(song);
							}
							
						}
					});
					//done shouldbeOffline	
				});
			}
		
						
			#region Pull to Refresh support
			
			bool checkForRefresh;

			public override void Scrolled (UIScrollView scrollView)
			{
				if (!checkForRefresh)
					return;
				if (Container.reloading)
					return;
				var view = Container.refreshView;
				if (view == null)
					return;
				
				var point = Container.TableView.ContentOffset;
				
				if (view.IsFlipped && point.Y > -yboundary && point.Y < 0) {
					view.Flip (true);
					view.SetStatus (RefreshViewStatus.PullToReload);
				} else if (!view.IsFlipped && point.Y < -yboundary) {
					view.Flip (true);
					view.SetStatus (RefreshViewStatus.ReleaseToReload);
				}
			}
			
			public override void DraggingStarted (UIScrollView scrollView)
			{
				checkForRefresh = true;
			}
			
			public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
			{
				if (Container.refreshView == null)
					return;
				
				checkForRefresh = false;
				if (Container.TableView.ContentOffset.Y > -yboundary)
					return;
				Container.TriggerRefresh (true);
			}
			#endregion
		
		
			
		}
		
			
		
		#endregion
		
		
		
		#region implemented abstract members of GoogleMusic.BaseViewController
		public override void StartSearch ()
		{
			searchBar.BecomeFirstResponder ();
		}

		public override void FinishSearch ()
		{
			searchBar.ResignFirstResponder ();
			DataSource.IsSearching = false;
			DataSource.SearchResults.Clear ();
			TableView.ReloadData ();
		}

		public override void PerformFilter (string text)
		{
			//text = text.Replace("'","''");
			DataSource.SearchResults = Database.Main.PlaylistsList.Where (a => a.Name.ToLower ().Contains (text.ToLower ())).ToList ();
			DataSource.IsSearching = true;	
			this.BeginInvokeOnMainThread (delegate{
				TableView.ReloadData ();
			});
		}

		public override void SearchButtonClicked (string text)
		{
			//throw new NotImplementedException ();
		}
		#endregion
	}
}

