using System;
using System.Linq;
using MonoTouch.Dialog;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace GoogleMusic
{
	public class AlbumViewController : BaseViewController
	{

		private Source DataSource;
		public AlbumViewController() : this(true)
		{
			
		}
		public AlbumViewController (bool hasSearch) : base (UITableViewStyle.Plain,false,hasSearch)
		{
		//	Root = new RootElement ("Artists");
			this.Title = "Albums".Translate();
			Screen = Screens.Albums;
		}
		
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ? 90 : 55;
		}
		
		
		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}
		
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
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

		public void GoToAlbum(Album album)
		{
			NavigationController.PopToViewController(this,false);
			AlbumSongView = new AlbumSongView(album.Id,DarkThemed);
			this.ActivateController(AlbumSongView);

		}
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			protected AlbumViewController Container;
			public bool IsSearching{get;set;}
			public List<Album> SearchResults = new List<Album>();


			
			public Source (AlbumViewController container)
			{
				this.Container = container;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				if(IsSearching)
					return SearchResults.Count;
				else if(Settings.ShowOfflineOnly)
					return Util.OfflineAlbumsGrouped[section].Count();
				return Util.AlbumsGrouped[section].Count();
			}
			public override int NumberOfSections (UITableView tableView)
			{
				if(IsSearching)
					return 1;
				else if(Settings.ShowOfflineOnly)
					return Util.OfflineAlbumsGrouped.Count();
				return Util.AlbumsGrouped.Count;
			}
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return "";
			}
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Album album;
				
				if(IsSearching){
					if(SearchResults.Count > indexPath.Row)
						album = SearchResults[indexPath.Row];
					else{
						album = new Album();
						tableView.ReloadData();
					}
				}
				else if(Settings.ShowOfflineOnly)
				{
					if(Util.OfflineAlbumsGrouped.Count > indexPath.Section && Util.OfflineAlbumsGrouped[indexPath.Section].Count() > indexPath.Row)
						album = Util.OfflineAlbumsGrouped[indexPath.Section].ElementAt(indexPath.Row);
					else{
						album = new Album();
						tableView.ReloadData();
					}
				}
				else{
					if(Util.AlbumsGrouped.Count > indexPath.Section && Util.AlbumsGrouped[indexPath.Section].Count() > indexPath.Row)
						album = Util.AlbumsGrouped[indexPath.Section].ElementAt(indexPath.Row);
					else{
						album = new Album();
						tableView.ReloadData();
					}
				}
				var cell = album.GetCell(tableView,Container.DarkThemed);
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Album album;
				if(IsSearching)
					album = SearchResults[indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					lock(Util.OfflineAlbumsGrouped)
						album = Util.OfflineAlbumsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				else
					lock(Util.AlbumsGrouped)
						album = Util.AlbumsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				Container.Selected (album);
			}	

			
			string[] array;
			public override string[] SectionIndexTitles (UITableView tableView)
			{
				if(IsSearching)
					array = new string[]{};
				else if(Settings.ShowOfflineOnly)
					array = Util.OfflineAlbumsGrouped.Select(x=> x.Key).ToArray();
				else
					array = Util.AlbumsGrouped.Select(x=> x.Key).ToArray();
			
			    return array;
			}
			
			public override int SectionFor (UITableView tableView, string title, int atIndex)
			{
				return atIndex;
			}
			
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				if(IsSearching)
					return "";
				return Util.AlbumsGrouped[section].Key;
			}
			
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				Album  album;
				
				if(IsSearching)
					album = SearchResults[indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					album = Util.OfflineAlbumsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				else
					album = Util.AlbumsGrouped[indexPath.Section].ElementAt(indexPath.Row);
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
						var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this playlist?".Translate());//
						alert.SetCancelButton("Cancel".Translate(),null);
						alert.AddButton("Delete".Translate(),delegate{

								Util.Api.DeleteAlbum(album,(success) =>{
									if(!success)
									{
										var alert2 = new BlockAlertView("Error".Translate(),"There was an error deleting the playlist.".Translate());
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
			DataSource.SearchResults.Clear();
			TableView.ReloadData();
		}

		public override void PerformFilter (string text)
		{
			//text = text.Replace("'","''");
			if(string.IsNullOrEmpty(text))
				DataSource.SearchResults = Util.AlbumsGrouped.SelectMany( x=> x).ToList();
			else if(Settings.ShowOfflineOnly)
				DataSource.SearchResults = Util.OfflineAlbums.Where( a=> a.Name.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();		
			else
				DataSource.SearchResults = Util.Albums.Where( a=> a.Name.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();
			
			DataSource.IsSearching = true;
			this.BeginInvokeOnMainThread(delegate{
			TableView.ReloadData();
			});
		}
		public override void SearchButtonClicked (string text)
		{
			throw new System.NotImplementedException ();
		}

		#endregion
	}
}

