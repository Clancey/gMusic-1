// 
//  Copyright 2012  Clancey
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
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Linq;
using System.Drawing;
using System.IO;
using MonoTouch.Dialog;


namespace GoogleMusic
{
	public class DownloadQueue : BaseViewController
	{
		private Source DataSource;
		public DownloadQueue () : base (UITableViewStyle.Plain,false)
		{
			Screen = Screens.Songs;
			this.Title = "Download Queue".Translate();
			HandleUtilSongsCollectionChanged();
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			TableView.ReloadData();
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ?  80 : 56;
		}
		
		public void Selected(Song song)
		{
			//FinishSearch();
			//Util.PlaySong(song,song.ArtistId,song.AlbumId,true);
		}
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			protected DownloadQueue Container;
			public bool IsSearching{get;set;}
			public List<Song> SearchResults = new List<Song>();
			
			
			public Source (DownloadQueue container)
			{
				this.Container = container;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				return Downloader.remainingFiles.Count;
			}
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return "";
			}
			string key ="artistCell";
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				//NSLogWriter.Default.WriteLine(indexPath.Row);
				try{
					Song thesong = Downloader.remainingFiles[indexPath.Row];//.Where(x=> x.IndexCharacter == headers[indexPath.Section]).ToArray()[indexPath.Row];

					return thesong.GetCell(tableView);
				}
				catch(Exception ex)
				{
					return new UITableViewCell();
				}
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				try{	
				Song song;
				if(IsSearching)
					song = SearchResults[indexPath.Row];
				else
					song= Downloader.remainingFiles[indexPath.Row];
				Container.Selected (song);
				}
				catch(Exception ex)
				{

				}
			}	
			
			
			public override int SectionFor (UITableView tableView, string title, int atIndex)
			{
				return atIndex;
			}


			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
//				var rowFrame = tableView.RectForRowAtIndexPath(indexPath);
//				var midY = (rowFrame.Height / 2) + rowFrame.Y;
//				var x = rowFrame.Width - 50;
//				var point = Container.View.ConvertPointToView(new PointF(x,midY),Util.BaseView);
				
				Song song;
				if(IsSearching)
					song = SearchResults[indexPath.Row];
				else
					song= Downloader.remainingFiles[indexPath.Row];
				Container.ShowPopUp(song.IsLocal ? song.IsLocal : song.ShouldBeLocal,(index)=>
				{
					//PlayNext
					if(index == 1)
						Util.PlaySongNext(song);//(song,song.ArtistId,-1,true);
					else if(index == 2)
					{
						var plistVc = new PlaylistViewController(true);
						plistVc.OnPlaylistSelected = (playlist) => {
							Util.Api.AddToPlaylist(playlist,new []{song},(success)=>
							{
								if(!success)
								{
									var alert = new BlockAlertView("Error".Translate(),"There was an error adding the song to your playlist.".Translate());
									alert.AddButton("Ok".Translate(),null);
									alert.Show();
								}
							});
						};
						Container.ActivateController(plistVc);
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
						var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this playlist?".Translate());//
						alert.SetCancelButton("Cancel".Translate(),null);
						alert.AddButton("Delete".Translate(),delegate{
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
						//Container.ActivateController(
					}
				},(shouldBeLocal)=>{
					if(shouldBeLocal)
					{
						song.ShouldBeLocal = true;
						Downloader.AddFile(song);
					}
					else
					{
						if(song.IsLocal && File.Exists(Util.MusicDir + song.FileName))
							try{
							File.Delete(Util.MusicDir + song.FileName);
						}
						catch{}
						Database.Main.UpdateOffline(song,false);
						song.ShouldBeLocal = false;
						
							
					}
					//TODO: update offline
					/*
					lock(Database.Main)
						Database.Main.Update(song);
						*/
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
			//lock(Database.Main)
			//	DataSource.SearchResults = Database.Main.Query<Song>("select * from song where Title like ('%" + text + "%')");	
			text = text.ToLower();
			lock(Util.Songs)
				DataSource.SearchResults = Util.Songs.Where( x=> x.Title.ToLower().Contains(text) || x.Artist.ToLower().Contains(text)).ToList();	
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

