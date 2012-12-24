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
	public class AlbumSongView : BaseViewController
	{
		private Source DataSource;
		public List<Song> Songs;
		public Album Album;
		AlbumHeaderView albumHeader;
		public AlbumSongView (int albumId, bool isDarkThemed): base (UITableViewStyle.Plain,false,false)
		{
			DarkThemed = isDarkThemed;
			//DarkThemed = true;
			Album = Util.AlbumsDict[albumId];
			if (Settings.ShowOfflineOnly)
				lock (Util.OfflineSongs)
					Songs = Util.OfflineSongs.Where (x => x.AlbumId == albumId).ToList ();
			else {
				List<string> ids;
				lock(Database.DatabaseLocker)
				{
					ids = Database.Main.Query<Song>("select id from song where AlbumId = ? order by Disc,Track",albumId).Select(x=> x.Id).ToList();
				}
				Songs = ids.Select(x=> Database.Main.GetObject<Song>(x)).ToList();

			}
			Songs = Songs.OrderBy (x => x.Track).OrderBy (x => x.Disc).ToList ();
			HandleUtilSongsCollectionChanged();
			Screen = Screens.Songs;
		}

		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ? 80 : 56;
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

		}
		
		public override void ViewWillAppear (bool animated)
		{
			if(albumHeader == null)	
				albumHeader = new AlbumHeaderView (Album, Songs.Count, Songs.Sum (x => x.Duration) / 1000,DarkThemed);
			checkIfLoaded ();
			//Console.WriteLine("View did appear base view controller");
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
				Console.WriteLine("navigation was null");
			//this.TableView.ReloadData();

			TableView.TableHeaderView.Frame = albumHeader.Frame;
			TableView.TableHeaderView = albumHeader;
		
			
			if(DarkThemed){
				this.View.BackgroundColor = UIColor.Clear;
				TableView.BackgroundColor = UIColor.Black.ColorWithAlpha (.1f);
				TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
		}



		public void Selected(Song song)
		{
			//FinishSearch();
			Util.PlaySong(song,song.ArtistId,song.AlbumId,false);
		}
		
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			protected AlbumSongView Container;
			public bool IsSearching {get;set;}
			public List<Song> SearchResults = new List<Song>();
			
			
			public Source (AlbumSongView container)
			{
				this.Container = container;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				if(IsSearching)
					return SearchResults.Count;
				else if(Settings.ShowOfflineOnly)
					return Container.Songs.Count();
				return Container.Songs.Count();
			}
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override string TitleForFooter (UITableView tableView, int section)
			{
				return "";
			}
			string key = "songAlbumElement";
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Song thesong = Container.Songs[indexPath.Row];
				SongAlbumCell cell = (tableView.DequeueReusableCell (key) as SongAlbumCell);
				if (cell == null) {
					cell = new SongAlbumCell (UITableViewCellStyle.Default, key, thesong,Container.DarkThemed);
					cell.Accessory = Container.DarkThemed ? UITableViewCellAccessory.None : UITableViewCellAccessory.DetailDisclosureButton;
					//cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				} else
				{
					cell.IsDarkThemed = Container.DarkThemed;
					cell.UpdateCell (thesong);
				}
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				
				Song song = Container.Songs[indexPath.Row];
				Container.Selected (song);
				Container.TableView.DeselectRow(indexPath,true);
				Container.TableView.ReloadRows(new NSIndexPath[]{indexPath},UITableViewRowAnimation.Automatic);
			}	
			
		
		
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				var rowFrame = tableView.RectForRowAtIndexPath(indexPath);
				var midY = (rowFrame.Height / 2) + rowFrame.Y;
				var x = rowFrame.Width - 50;
				var point = Container.View.ConvertPointToView(new PointF(x,midY),Util.BaseView);
				
				Song song = Container.Songs[indexPath.Row];
				//if(IsSearching)
				//	song = SearchResults[indexPath.Row];
				//else if(Settings.ShowOfflineOnly)
				//	song= Util.OfflineSongsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				//else
				//	song= Util.SongsGrouped[indexPath.Section].ElementAt(indexPath.Row);
				Container.ShowPopUp(song.IsLocal ? song.IsLocal : song.ShouldBeLocal,(index)=>
				{
					//PlayNext
					if(index == 1)
						Util.PlaySongNext(song);//(song,song.ArtistId,-1,true);
					else if(index == 2)
					{
//						var plistVc = new PlaylistViewController(true,false,Container.HasSearch);
//						plistVc.OnPlaylistSelected = (playlist) => {
//							Util.Api.AddToPlaylist(playlist,new []{song},(success)=>
//							{
//								if(!success)
//								{
//									var alert = new BlockAlertView("Error".Translate(),"There was an error adding the song to your playlist.".Translate());
//									alert.AddButton("Ok",null);
//									alert.Show();
//								}
//							});
//						};
//						Container.ActivateController(plistVc);
					}
					else if(index == 3)
					{
						Util.Api.CreateMagicPlayList(song,(onSuccess) => {
							if(!onSuccess)
							{
								var alert = new BlockAlertView("Error".Translate(),"There was an error creating your magic playlist.".Translate());
								alert.AddButton("Ok".Translate(),null);
								alert.Show();
							}
								
						});
					}
					else if(index == 4)
					{
						var alert  = new BlockAlertView("Delete?".Translate(), "Are you sure you want to delete this song?".Translate());
						alert.AddButton("Cancel".Translate(), BlockAlertView.ButtonColor.Black,null);
						alert.AddButton("Delete".Translate(), BlockAlertView.ButtonColor.Red, delegate{
							Util.Api.DeleteSong(song,(success) =>{
								if(!success)
								{
									var alert2 = new BlockAlertView("Error".Translate(),"There was an error deleting the song.".Translate());
									alert2.AddButton("Ok".Translate(),null);
									alert2.Show();
								}
								else
								{
									Container.TableView.DeleteRows(new[]{indexPath},UITableViewRowAnimation.Fade);
								}
							});
							
						});
						alert.Show();
					}
					else if(index == 5)
					{
						Container.CurrentSongEditor = new EditSongViewController(song);
						Container.ActivateController(Container.CurrentSongEditor);
					}
				},(shouldBeLocal)=>{
					if(shouldBeLocal)
					{
						song.ShouldBeLocal = true;
						//TODO: fix downloader
						//Downloader.AddFile(song);
						//TODO: update offline
						
						lock(Database.DatabaseLocker)
						Database.Main.Update(song);
						
					}
					else
					{
						if(song.IsLocal && File.Exists(Util.MusicDir + song.FileName))
							try{
							File.Delete(Util.MusicDir + song.FileName);
						}
						catch{}
						Database.Main.UpdateDeleteOffline(song);
							
					}
					
					tableView.ReloadRows(new []{indexPath},UITableViewRowAnimation.None);
					//TODO Either download or remove	
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
			throw new System.NotImplementedException ();
		}

		public override void FinishSearch ()
		{
			throw new System.NotImplementedException ();
		}

		public override void PerformFilter (string text)
		{
			throw new System.NotImplementedException ();
		}

		public override void SearchButtonClicked (string text)
		{
			throw new System.NotImplementedException ();
		}
		#endregion

	}
}

