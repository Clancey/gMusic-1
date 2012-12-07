using System;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace GoogleMusic
{
	public class GenreViewController : BaseViewController
	{
		private Source DataSource;
		public GenreViewController() : this(true)
		{
			
		}
		public GenreViewController (bool hasSearchBar) : base (UITableViewStyle.Plain,false,hasSearchBar)
		{
		//	Root = new RootElement ("Artists");
			
			this.Title = "Genre".Translate();
			Screen = Screens.Genre;
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

		
		public void Selected(Genre Genre)
		{
			FinishSearch();
			//Util.CurrentSongListViewController = new SonglistViewController(Screen,Genre.Name,Genre.Id);
			List<int> artists;
			lock(Database.Locker)
				artists = Database.Main.Query<Song>("select distinct ArtistId from song where genreid = ? order by Artist",Genre.Id).Select(x=> x.ArtistId).ToList();
			if(Settings.ShowOfflineOnly)
				artists = Util.OfflineArtists.Where(x=> artists.Contains(x.Id)).OrderBy(x=> x.NormName).Select(x=> x.Id).ToList(); 

			if(artists.Count == 1){
				var artist = Util.ArtistsDict[artists[0]];

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
					var CurrentSongListViewController = new SonglistViewController(Screen,album.Name,album.ArtistId,album.Id,DarkThemed,!DarkThemed){DarkThemed = DarkThemed};
					if(DarkThemed){
						CurrentSongListViewController.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
						CurrentSongListViewController.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					}
					this.ActivateController(CurrentSongListViewController);
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
			else{
				var genreArtistVC = new GenreArtistViewController(Genre.Id){DarkThemed = DarkThemed, artists = artists};
				if(DarkThemed){
					genreArtistVC.TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
					genreArtistVC.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
				}
				genreArtistVC.HandleUtilSongsCollectionChanged();
				this.ActivateController(genreArtistVC);
			}
		}
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			protected GenreViewController Container;
			public bool IsSearching{get;set;}
			public List<Genre> SearchResults = new List<Genre>();
			
			
			public Source (GenreViewController container)
			{
				this.Container = container;
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				if(IsSearching)
					return 1;
				else if(Settings.ShowOfflineOnly)
					return Util.OfflineGenreGroupped.Count();
				return Util.GenreGroupped.Count();
			}
			public override int RowsInSection (UITableView tableview, int section)
			{
				if(IsSearching)
					return SearchResults.Count();
				else if(Settings.ShowOfflineOnly)
					return  Util.OfflineGenreGroupped[section].Count();
				var count =  Util.GenreGroupped[section].Count();//Items.Where(x=> x.IndexCharacter == headers[section]).Count();
				return count;
			}
			
			string[] array;
			public override string[] SectionIndexTitles (UITableView tableView)
			{
				if(IsSearching)
					array = new string[]{};
				else if(Settings.ShowOfflineOnly)
					array = Util.OfflineGenreGroupped.Select(x=> (x.Key ?? "")).ToArray();
				else
					array = Util.GenreGroupped.Select(x=> (x.Key ?? "")).ToArray();
			
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
					return Util.OfflineGenreGroupped[section].Key;
				return Util.GenreGroupped[section].Key;
			}
			//string key ="artist";
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Genre genre;
				if(IsSearching)
					genre = SearchResults[indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					genre = Util.OfflineGenreGroupped[indexPath.Section].ElementAt(indexPath.Row);
				else
					genre = Util.GenreGroupped[indexPath.Section].ElementAt(indexPath.Row);
				
				var cell = genre.GetCell(tableView,Container.DarkThemed);
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Genre genre;
				if(IsSearching)
					genre = SearchResults[indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					genre = Util.OfflineGenreGroupped[indexPath.Section].ElementAt(indexPath.Row);
				else
					genre = Util.GenreGroupped[indexPath.Section].ElementAt(indexPath.Row);
				Container.Selected (genre);
			}
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				//var rowFrame = tableView.RectForRowAtIndexPath(indexPath);
				//var midY = (rowFrame.Height / 2) + rowFrame.Y;
				//var x = rowFrame.Width - 50;
				//var point = Container.View.ConvertPointToView(new PointF(x,midY),Util.BaseView);
				
				Genre genre;
				if(IsSearching)
					genre = SearchResults[indexPath.Row];
				else if(Settings.ShowOfflineOnly)
					genre = Util.OfflineGenreGroupped[indexPath.Section].ElementAt(indexPath.Row);
				else
					genre = Util.GenreGroupped[indexPath.Section].ElementAt(indexPath.Row);
				Container.ShowPopUp(genre.ShouldBeLocal,(index)=>
				{
					if(index == 0)
						Util.PlayGenre(null,genre.Id);
					else if(index == 2)
					{
						var plistVc = new PlaylistViewController(true,false,Container.HasSearch);
						plistVc.OnPlaylistSelected = (playlist) => {
							Util.Api.AddToPlaylist(playlist,genre,(success)=>
							{
								if(!success)
								{
									var alert = new BlockAlertView("Error".Translate(),"There was an error adding the genre to your playlist.".Translate());
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
						try{
						var genreId = genre.Id;
						genre.ShouldBeLocal = shouldBeLocal;
						if(shouldBeLocal)
						{
							//TODO: update offline
							
							lock(Database.Locker)
							{
								Database.Main.Update(genre);
								Database.Main.Query<Song>("update song set ShouldBeLocal = ? where GenreId = " + genreId,true);
	
								
								lock(Util.Songs)
								foreach( var genreSong in Util.Songs.Where(s=> s.GenreId == genreId).ToList())
								{
									genreSong.ShouldBeLocal = true;
									//Database.Main.Update(genreSong);
									if (!genreSong.IsLocal)
										Downloader.AddFile (genreSong);
								}
							}
							
						}
						else
						{
							List<Song> SongsToDelete;
							lock(Database.Locker)
							{
								Database.Main.Update(genre);
								SongsToDelete = Util.Songs.Where(s=> s.GenreId == genreId && s.IsLocal == true).ToList();
								Database.Main.Query<Song>("update song set IsLocal = ? ,ShouldBeLocal = ? where GenreId = " + genreId,false,false);
									
							}
							foreach(var genreSong in SongsToDelete)
							{
								if(File.Exists(Util.MusicDir + genreSong.FileName))
										File.Delete(Util.MusicDir + genreSong.FileName);
								Database.Main.UpdateDeleteOffline(genreSong);
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
			try{
			if(searchBar == null)
				return;
			searchBar.ResignFirstResponder();
			if(DataSource == null || !DataSource.IsSearching)
				return;
			DataSource.IsSearching = false;
			DataSource.SearchResults.Clear();
			TableView.ReloadData();
			}
			catch(Exception ex)
			{

				Console.WriteLine(ex);
			}
		}

		public override void PerformFilter (string text)
		{
			//text = text.Replace("'","''");
			
			if(Settings.ShowOfflineOnly)
				DataSource.SearchResults = Util.OfflineGenres.Where( a=> a.Name.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();
			else
				DataSource.SearchResults = Util.Genres.Where( a=> a.Name.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();
			DataSource.IsSearching = true;	
			this.BeginInvokeOnMainThread(delegate{
				TableView.ReloadData();
			});
		}

		public override void SearchButtonClicked (string text)
		{
			//throw new NotImplementedException ();
		}
		#endregion
	}
}

