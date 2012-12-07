// 
//  Copyright 2012  Xamarin Inc  (http://www.xamarin.com)
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
	public class GenreArtistViewController: BaseViewController
	{
		private Source DataSource;
		public List<int> artists = new List<int>();
		int GenreId;
		public GenreArtistViewController (int genreId) : base (UITableViewStyle.Plain,true)
		{
			GenreId = genreId;
		//	Root = new RootElement ("Artists");
			Screen = Screens.Artist;
			this.Title = "Artists".Translate();
			ShuffleClicked = shuffle;
			
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ? 80 : 56;
		}
		public override bool CanBecomeFirstResponder {
			get { return true; }
		}

		void shuffle()
		{
			Settings.Random = true;
			Util.PlayGenre(null,GenreId);
		}
		
		void Selected(Artist artist)
		{
			FinishSearch();
			this.NavigationItem.HidesBackButton = false;
			var albums = Database.Main.Query<Album>("select * from album where ArtistId = '" + artist.Id +"' order by NameNorm" ).ToList();	
			if(Settings.ShowOfflineOnly)
				albums = albums.Where(x=> x.OffineCount > 0).ToList();
			if(albums.Count == 0)
			{
				var CurrentSongListViewController = new SonglistViewController(Screens.Artist,"All Songs".Translate(),artist.Id,-1,DarkThemed,!(DarkThemed || Util.IsIpad)){DarkThemed = DarkThemed};
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
			this.Title = DarkThemed ? Util.GenresDict[GenreId].Name : "Artists".Translate();
			//NSLogWriter.Default.WriteLine("View did appear base view controller");
			if (TableView.TableHeaderView != headerView)
				setupTable ();
			if(this.NavigationController != null)
			{
				this.NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;


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
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			public bool IsSearching{get;set;}
			
			protected GenreArtistViewController Container;
			
			
			public Source (GenreArtistViewController container)
			{
				this.Container = container;
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			public override int RowsInSection (UITableView tableview, int section)
			{
				return Container.artists.Count;
			}
			
			string[] array;
			public override string[] SectionIndexTitles (UITableView tableView)
			{
				if(array == null)
				array = new string[]{};
			
			    return array;
			}
			public override string TitleForHeader (UITableView tableView, int section)
			{
				return " ";
			}

			public override int SectionFor (UITableView tableView, string title, int atIndex)
			{
				return atIndex;
			}
			
			string key ="artistCell";
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Artist artist = Util.ArtistsDict[Container.artists[indexPath.Row]];
				var cell = artist.GetCell (tableView,Container.DarkThemed);
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Artist artist = Util.ArtistsDict[Container.artists[indexPath.Row]];
				Container.Selected (artist);
			}
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				Artist artist;
					artist = Util.ArtistsDict[Container.artists[indexPath.Row]];
				Container.ShowPopUp(artist.ShouldBeOffline ,(index)=>
				{
					if(index == 0)
						Util.PlaySong(null,artist.Id,-1,false);					
					else if(index == 2)
					{
						var plistVc = new PlaylistViewController(true);
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
						var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this playlist?".Translate());//
						alert.SetCancelButton("Cancel".Translate(),null);
						alert.AddButton("Delete".Translate(),delegate{
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
								Database.Main.Query<Song>("update song set IsLocal = ? ,ShouldBeLocal = ? where ArtistId = " + artistId,false,false);
									
							}
							foreach(var artistSong in SongsToDelete)
							{
								if(File.Exists(Util.MusicDir + artistSong.FileName))
										File.Delete(Util.MusicDir + artistSong.FileName);
								Database.Main.UpdateDeleteOffline(artistSong);
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
			DataSource.IsSearching = false;
			TableView.ReloadData();
		}

		public override void PerformFilter (string text)
		{
			//text = text.Replace("'","''");
			//if(Settings.ShowOfflineOnly)
			//	DataSource.SearchResults = Util.OfflineArtists.Where(a=> a.NormName.Contains(text.ToLower())).ToList();
			//else	
			//	DataSource.SearchResults = Util.Artists.Where(a=> a.NormName.Contains(text.ToLower())).ToList();
		
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

