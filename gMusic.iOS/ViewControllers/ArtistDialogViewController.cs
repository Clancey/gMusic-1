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
	public class ArtistDialogViewController : BaseViewController
	{
		private Source DataSource;
		public ArtistDialogViewController() : this(true)
		{
			
		}
		public ArtistDialogViewController (bool hasSearch) : base (UITableViewStyle.Plain,false,hasSearch)
		{
		//	Root = new RootElement ("Artists");
			Screen = Screens.Artist;
			this.Title = "Artists".Translate();
			
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ?  80 : 40;
		}
		public override bool CanBecomeFirstResponder {
			get { return true; }
		}


		public void Selected(Artist artist)
		{
			Console.WriteLine("Selected");
			FinishSearch();
			this.NavigationItem.HidesBackButton = false;
			var albums = Database.Main.Query<Album>("select * from album where ArtistId = '" + artist.Id +"' order by NameNorm" ).ToList();	
			if(Settings.ShowOfflineOnly)
				albums = albums.Where(x=> x.OffineCount > 0).ToList();
			if(albums.Count == 0)
			{
				var CurrentSongListViewController = new SonglistViewController(Screens.Artist,"All Songs".Translate(),artist.Id,-1,DarkThemed,!DarkThemed){DarkThemed = DarkThemed};
				if(DarkThemed){
					CurrentSongListViewController.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					CurrentSongListViewController.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
				}
				this.ActivateController(CurrentSongListViewController);

			}
			else if(albums.Count == 1)
			{
				var album = albums[0];
				var AlbumSongView = new AlbumSongView(album.Id,DarkThemed);
				if(DarkThemed){
					AlbumSongView.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					AlbumSongView.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
				}
				this.ActivateController(AlbumSongView);
			}
			else
			{
				var albumScreen = new ArtistAlbumViewController(artist){DarkThemed = DarkThemed, albums = albums};
				if(DarkThemed){
					albumScreen.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					albumScreen.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
				}
				albumScreen.HandleUtilSongsCollectionChanged();
				this.ActivateController(albumScreen);
			}
		}
		
		public override void ViewWillAppear (bool animated)
		{
			this.Title = "Artists".Translate();
			base.ViewWillAppear (animated);
		}
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			public bool IsSearching{get;set;}
			public List<Artist> SearchResults = new List<Artist>();
			protected ArtistDialogViewController Container;
			
			
			public Source (ArtistDialogViewController container)
			{
				this.Container = container;
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				if(IsSearching)
					return 1;
				else if(Settings.ShowOfflineOnly)
					return Util.OfflineArtistsGrouped.Count;
				return Util.ArtistsGrouped.Count();
			}
			public override int RowsInSection (UITableView tableview, int section)
			{
				if(IsSearching)
					return SearchResults.Count;
				else if(Settings.ShowOfflineOnly)
					return Util.OfflineArtistsGrouped[section].Count();
				var count =  Util.ArtistsGrouped[section].Count();//Items.Where(x=> x.IndexCharacter == headers[section]).Count();
				return count;
			}
			
			string[] array;
			public override string[] SectionIndexTitles (UITableView tableView)
			{
				
				if(IsSearching)
					array = new string[]{};
				else if(Settings.ShowOfflineOnly)
					array = Util.OfflineArtistsGrouped.Select(x=> x.Key).ToArray();
				else					
					array = Util.ArtistsGrouped.Select(x=> x.Key).ToArray();
			
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
				else if(Settings.ShowOfflineOnly)
					return Util.OfflineArtistsGrouped[section].Key;
				return Util.ArtistsGrouped[section].Key;
			}
			string key ="artistCell";
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var artist = ArtistForIndexPath(indexPath);
				if(artist.Id == 0)
					tableView.ReloadData();
				var cell = artist.GetCell(tableView,Container.DarkThemed);
				return cell;
			}

			Artist ArtistForIndexPath(NSIndexPath indexPath)
			{
				Artist artist;
				if(IsSearching){
					if(SearchResults.Count > indexPath.Row)
						artist = SearchResults[indexPath.Row];
					else{
						artist = new Artist();
					}
				}
				else if(Settings.ShowOfflineOnly){
					if(Util.OfflineArtistsGrouped.Count > indexPath.Section && Util.OfflineArtistsGrouped[indexPath.Section].Count() > indexPath.Row)
						artist= Util.OfflineArtistsGrouped[indexPath.Section].ElementAt(indexPath.Row);
					else
					{
						artist = new Artist();
					}
				}
				else{
					if(Util.ArtistsGrouped.Count > indexPath.Section && Util.ArtistsGrouped[indexPath.Section].Count() > indexPath.Row)
						artist= Util.ArtistsGrouped[indexPath.Section].ElementAt(indexPath.Row);//.Where(x=> x.IndexCharacter == headers[indexPath.Section]).ToArray()[indexPath.Row];
					else
					{
						artist = new Artist();
					}
				}
				return artist;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Console.WriteLine ("selected");
				Artist artist = ArtistForIndexPath(indexPath);
				Container.Selected (artist);
			}
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				//var rowFrame = tableView.RectForRowAtIndexPath(indexPath);
				//var midY = (rowFrame.Height / 2) + rowFrame.Y;
				//var x = rowFrame.Width - 50;
				//var point = Container.View.ConvertPointToView(new PointF(x,midY),Util.BaseView);			
				
				Artist artist;
				if(IsSearching)
					artist = SearchResults[indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					artist= Util.OfflineArtistsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				else
					artist= Util.ArtistsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				Container.ShowPopUp(artist.ShouldBeOffline ,(index)=>
				{
					if(index == 0)
						Util.PlaySong(null,artist.Id,-1,false);					
					else if(index == 2)
					{
						var plistVc = new PlaylistViewController(true,false,Container.HasSearch);
						plistVc.OnPlaylistSelected = (playlist) => {
							Util.Api.AddToPlaylist(playlist,artist,(success)=>
							{
								if(!success)
								{
									var alert = new BlockAlertView("Error".Translate(),"There was an error adding the artist to your playlist.".Translate());
									alert.AddButton("Ok".Translate(),null);
									alert.Show();
								}
							});
						};
						Container.ActivateController(plistVc);
					}
					else if(index == 4)
					{
						var alert  = new BlockAlertView("Delete?".Translate(), "Are you sure you want to delete all songs from this artist?".Translate());
						alert.AddButton("Cancel".Translate(), BlockAlertView.ButtonColor.Black, null);
						alert.AddButton("Delete".Translate(), BlockAlertView.ButtonColor.Red,delegate
							{
								Util.Api.DeleteArtist(artist,(success) =>{
									if(!success)
									{
										var alert2 = new BlockAlertView("Error".Translate(),"There was an error deleting the artist.".Translate());
										alert2.AddButton("Ok".Translate(),null);
										alert2.Show();
									}
									else
										Container.TableView.DeleteRows(new[]{ indexPath},UITableViewRowAnimation.Fade);
								});
						});
						alert.Show();
					}
					
				},
				(shouldBeLocal)=> {
					//TODO Eithre download or delete!
						ThreadPool.QueueUserWorkItem(delegate{
						try{
						var artistId = artist.Id;
						artist.ShouldBeOffline = shouldBeLocal;
						if(shouldBeLocal)
						{
							//TODO: update offline
							
							lock(Database.Locker)
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
							
						}
						else
						{
							//TODO: update offline
							
							List<Song> SongsToDelete;
							lock(Database.Locker)
							{
								Database.Main.Update(artist);
								lock(Util.Songs)
								SongsToDelete = Util.Songs.Where(s=> s.ArtistId == artistId && s.IsLocal == true).ToList();
								//Database.Main.Query<Song>("update song set IsLocal = ? ,ShouldBeLocal = ? where ArtistId = " + artistId,false,false);
									
							}
							foreach(var artistSong in SongsToDelete)
							{
								if(File.Exists(Util.MusicDir + artistSong.FileName))
										File.Delete(Util.MusicDir + artistSong.FileName);
								Database.Main.UpdateOffline(artistSong,false);
							}
							
						}
						}
						catch(Exception ex)
						{
							Console.WriteLine(ex);
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
			if(DataSource != null){
				DataSource.IsSearching = false;
				DataSource.SearchResults.Clear();
				TableView.ReloadData();
			}
		}

		public override void PerformFilter (string text)
		{
			//text = text.Replace("'","''");
			if(Settings.ShowOfflineOnly)
				DataSource.SearchResults = Util.OfflineArtists.Where( a=> a.Name.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();
			else	
				DataSource.SearchResults = Util.Artists.Where( a=> a.Name.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();
		
			DataSource.IsSearching = true;	
			this.BeginInvokeOnMainThread(delegate{
				TableView.ReloadData();
			});
		}

		public override void SearchButtonClicked (string text)
		{
			
		}
		#endregion
	}
	

}

