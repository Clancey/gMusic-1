using System;
using MonoTouch.Dialog;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Threading;
using System.IO;
using MonoTouch.Foundation;

namespace GoogleMusic
{
	public class ArtistAlbumViewController : BaseViewController
	{
		public List<Album> albums = new List<Album>();
		Artist artist;
		public ArtistAlbumViewController (Artist artist) : base( UITableViewStyle.Plain,false,false)
		{
			this.artist = artist;
			this.Title = artist.Name;
			this.HasSearch = false;
		}
	
		private Source DataSource;
		
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ? 90 :  55;
		}
		
		
		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}
		public override void ViewWillAppear (bool animated)
		{
			checkIfLoaded ();
			//NSLogWriter.Default.WriteLine("View did appear base view controller");
			if (TableView.TableHeaderView != headerView)
				setupTable ();
			if(this.NavigationController != null)
			{
				this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
				try{
					if(DarkThemed)
					  {
						if(shuffleLabel != null)
							shuffleLabel.TextColor = UIColor.White;
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

			}
			else
				NSLogWriter.Default.WriteLine("navigation was null");
			//this.TableView.ReloadData();
		
		}
		AlbumSongView AlbumSongView;
		public void Selected(Album album)
		{
			FinishSearch();
			this.NavigationItem.HidesBackButton = false;
			AlbumSongView = new AlbumSongView(album.Id,DarkThemed);
			if(DarkThemed){
					AlbumSongView.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					AlbumSongView.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
			this.ActivateController(AlbumSongView);
		}
		public void AllSongsSelected(){
			CurrentSongListViewController = new SonglistViewController(Screens.Artist,"All Songs".Translate(),artist.Id,-1,DarkThemed,!(DarkThemed || Util.IsIpad)){DarkThemed = DarkThemed};
			if(DarkThemed){
				CurrentSongListViewController.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
				CurrentSongListViewController.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
			this.ActivateController(CurrentSongListViewController);
		}

		public void GoToAlbum(Album album)
		{
			NavigationController.PopToViewController(this,false);
			CurrentSongListViewController = new SonglistViewController(Screen,album.Name,album.ArtistId,album.Id,DarkThemed,!DarkThemed);
			if(DarkThemed){
				CurrentSongListViewController.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
				CurrentSongListViewController.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
			this.ActivateController(CurrentSongListViewController);

		}
		public void ShowPopUp ( Screens screen, bool shouldBeLocal, Action<int> clickedIndex, Action<bool> offLineChanged)
		{
			//NSLogWriter.Default.WriteLine("Pop up: " + startPoint); 
			popUpView = new PopUpView (Util.BaseView.Frame, screen, shouldBeLocal);
			popUpView.Clicked = clickedIndex;
			popUpView.OfflineToggled = offLineChanged;
			Util.BaseView.Superview.AddSubview (popUpView);
			popUpView.AnimateIn ();
			
		}
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			protected ArtistAlbumViewController Container;
			public bool IsSearching{get;set;}
			public Source (ArtistAlbumViewController container)
			{
				Container = container;
			}
			public override int RowsInSection (UITableView tableview, int section)
			{
			
				return Container.albums.Count + 1;
			}
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return "";
			}
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if(indexPath.Row == 0)
				{

					var mcell = tableView.DequeueReusableCell("allsongs") ?? new UITableViewCell(UITableViewCellStyle.Default,"allSongs");

					mcell.BackgroundColor = Container.DarkThemed ? UIColor.Clear : UIColor.White;
					mcell.TextLabel.BackgroundColor = UIColor.Clear;
					mcell.Accessory =  Container.DarkThemed ? UITableViewCellAccessory.None : UITableViewCellAccessory.DetailDisclosureButton;
					mcell.TextLabel.Text = "All Songs".Translate();	
					mcell.TextLabel.TextColor = Container.DarkThemed ? UIColor.White : UIColor.Black;
					mcell.TextLabel.Font =  UIFont.BoldSystemFontOfSize(Container.DarkThemed ? 30f : 18f);
					//mcell.SelectionStyle = UITableViewCellSelectionStyle.None;
					mcell.TextLabel.TextAlignment = UITextAlignment.Center;
					return mcell;
				}
				Album album;

				if(Container.albums.Count >= indexPath.Row)
					album = Container.albums[indexPath.Row -1];
				else{
					album = new Album();
					tableView.ReloadData();
				}

				var cell = album.GetCell(tableView,Container.DarkThemed);
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if(indexPath.Row == 0)
				{
					Container.AllSongsSelected();
					tableView.DeselectRow(indexPath,true);
					return;
				}
				if (indexPath.Row > Container.albums.Count)
					return;
				Album album = Container.albums[indexPath.Row -1];
				Container.Selected (album);
			}	
						
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				if(indexPath.Row == 0)
				{
					allSongsAccessory();
					return;
				}
				
				if (indexPath.Row > Container.albums.Count)
					return;
				Album album = Container.albums[indexPath.Row -1];

				Container.ShowPopUp(album.ShouldBeLocal,(index)=>
				{
					//Play
					if(index == 0)
						Util.PlaySong(null,album.ArtistId,album.Id,false);
					//Add To Playlist
					else if(index == 2)
					{
						var plistVc = new PlaylistViewController(true,false,Container.HasSearch);
						plistVc.OnPlaylistSelected = (playlist) => {
							Util.Api.AddToPlaylist(playlist,album,(success)=>
							{
								if(!success)
								{
									var alert = new BlockAlertView("Error".Translate(),"There was an error adding the album to your playlist.".Translate());
									alert.AddButton("Ok".Translate(),null);
									alert.Show();
								}
							});
						};
						Container.ActivateController(plistVc);
					}
					else if(index == 4)
					{
						var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this album?".Translate());//
						alert.SetCancelButton("Cancel".Translate(),null);
						alert.AddButton("Delete".Translate(),delegate{

								Util.Api.DeleteAlbum(album,(success) =>{
									if(!success)
									{
										var alert2 = new BlockAlertView("Error".Translate(),"There was an error deleting the album.".Translate());
										alert2.AddButton("Ok".Translate(),null);
										alert2.Show();
									}
									else
										Container.TableView.DeleteRows(new[]{indexPath},UITableViewRowAnimation.Fade);
								});

						});
						alert.Show();
					}
				},(shouldBeLocal)=>{
					ThreadPool.QueueUserWorkItem(delegate{
						try{
						var albumId = album.Id;
						album.ShouldBeLocal = shouldBeLocal;
						if(shouldBeLocal)
						{
							//TODO: update offline
							
							lock(Database.Locker)
							{
								Database.Main.Update(album);
								Database.Main.Query<Song>("update song set ShouldBeLocal = ? where AlbumId = " + albumId,true);
							
								
								lock(Util.Songs){
									var songs = Util.Songs.Where(s=> s.AlbumId == albumId).ToList();
									foreach (var song in songs) {
										song.ShouldBeLocal = true;
										//Database.Main.Update(song);
										if (!song.IsLocal)
											Downloader.AddFile (song);
									}
								}
							}
							
						}
						else
						{
							//TODO: update offline
							
							List<Song> SongsToDelete;
							lock(Database.Main)
							{
								Database.Main.Update(album);
								
								lock(Util.Songs)
									SongsToDelete = Util.Songs.Where(s=> s.AlbumId == albumId && s.IsLocal == true).ToList();
									
							}
							foreach(var albumSong in SongsToDelete)
							{
								if(File.Exists(Util.MusicDir + albumSong.FileName))
										File.Delete(Util.MusicDir + albumSong.FileName);
								Database.Main.UpdateOffline(albumSong,false);
							}
							
						}
						}
						catch(Exception ex)
						{
							Console.WriteLine(ex);
						}
					});
				});
			}

			private void allSongsAccessory()
			{
				Container.ShowPopUp(Screens.Albums,Container.artist.ShouldBeOffline,(index)=> {
						if(index == 0)
							Util.PlaySong(null,Container.artist.Id,-1,false);

						else if(index == 2)
						{
							var plistVc = new PlaylistViewController(true);
							plistVc.OnPlaylistSelected = (playlist) => {
								Util.Api.AddToPlaylist(playlist,Container.artist,(success)=>
								{
									if(!success)
									{
										var alert = new BlockAlertView("Error","There was an error adding the album to your playlist.".Translate());
										alert.AddButton("Ok".Translate(),null);
										alert.Show();
									}
								});
							};
							Container.ActivateController(plistVc);
						}
					},(shouldBeLocal)=>{
					//TODO Eithre download or delete!
						ThreadPool.QueueUserWorkItem(delegate{
						//var artistId = Container.artist.Id;
						Container.artist.ShouldBeOffline = shouldBeLocal;
						if(shouldBeLocal)
						{
								//TODO: update offline
								/*
							lock(Database.Main)
							{
								Database.Main.Update(artist);
								Database.Main.Query<Song>("update song set ShouldBeLocal = ? where ArtistId = " + artistId,true);
								lock(Util.Songs)
								foreach( var artistSong in Util.Songs.Where(s=> s.ArtistId == artistId).ToList())
								{
									artistSong.ShouldBeLocal = true;
									if(!artistSong.IsLocal)
										Downloader.AddFile(artistSong);
								}
							}
							*/
						}
						else
						{
								//TODO: Update offline
								/*
							List<Song> SongsToDelete;
							lock(Database.Main)
							{
								Database.Main.Update(artist);
								lock(Util.Songs)
								SongsToDelete = Util.Songs.Where(s=> s.ArtistId == artistId && s.IsLocal == true).ToList();
								Database.Main.Query<Song>("update song set IsLocal = ? ,ShouldBeLocal = ? where ArtistId = " + artistId,false,false);
									
							}
							foreach(var artistSong in SongsToDelete)
							{
								if(File.Exists(Util.MusicDir + artistSong.FileName))
										File.Delete(Util.MusicDir + artistSong.FileName);
								artistSong.ShouldBeLocal = false;
								artistSong.IsLocal = false;
							}
							*/
						}
					});
					//done shouldbeOffline
				});
			}
						
			#region Pull to Refresh support
			
			bool checkForRefresh;
			public override void Scrolled (UIScrollView scrollView)
			{
				if(Util.IsIos6)
					return;
				if (!checkForRefresh)
					return;
				if (Container.reloading)
					return;
				var view  = Container.refreshView;
				if (view == null)
					return;
				
				var point = Container.TableView.ContentOffset;
				
				if (view.IsFlipped && point.Y > -yboundary && point.Y < 0){
					view.Flip (true);
					view.SetStatus (RefreshViewStatus.PullToReload);
				} else if (!view.IsFlipped && point.Y < -yboundary){
					view.Flip (true);
					view.SetStatus (RefreshViewStatus.ReleaseToReload);
				}
			}
			
			public override void DraggingStarted (UIScrollView scrollView)
			{
				if(Util.IsIos6)
					return;
				checkForRefresh = true;
			}
			
			public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
			{
				if(Util.IsIos6)
					return;
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
			searchBar.ResignFirstResponder();
			DataSource.IsSearching = false;
			TableView.ReloadData();
		}

		public override void PerformFilter (string text)
		{

		}

		public override void SearchButtonClicked (string text)
		{
			//throw new NotImplementedException ();
		}
		#endregion
	}
}

