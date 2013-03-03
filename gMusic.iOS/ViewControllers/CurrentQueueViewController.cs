using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GoogleMusic
{
	public class CurrentQueueViewController: BaseViewController
	{
		private Source DataSource;
		
		public CurrentQueueViewController () : base (UITableViewStyle.Plain,false,false)
		{
			Screen = Screens.Songs;
			this.Title = "Current".Translate();
			HandleUtilSongsCollectionChanged ();
			//this.View.BackgroundColor = UIColor.Black;
		}
		
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source (this);
			this.TableView.Source = DataSource;
		}
		
		public void Selected (Song song)
		{
			FinishSearch ();
			Util.PlaySongFromPlaylist (song);
		}
		
		string lastSong;
		
		public void Reload (Song song)
		{
			if (song.Id == lastSong)
				return;
			lastSong = song.Id;
			TableView.ReloadData ();
			ScrollToCurrentSong();
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			ScrollToCurrentSong();
			
		}
		public void ScrollToCurrentSong()
		{
			if(TableView.Source == null)
				return;
			try {
				NSIndexPath path;
				if (Settings.Random)
					path = NSIndexPath.FromRowSection (Util.PlayListPlayed.Count - (Util.CurrentSong != null ? 0 : 1), 0);
				else
					path = NSIndexPath.FromRowSection (Util.PlayListSorted.IndexOf (Util.CurrentSong.Id), 0);
				var section = TableView.Source.NumberOfSections (TableView);
				var rows = TableView.Source.RowsInSection (TableView,path.Section);
				if (path.Section < section && path.Row <rows && path.Row >= 0 && path.Section >= 0 )
					TableView.ScrollToRow (path, UITableViewScrollPosition.Top, true);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
			
		}
		#region data source
		public class Source : UITableViewSource
		{
			const float yboundary = 65;
			protected CurrentQueueViewController Container;
			
			public bool IsSearching{ get; set; }
			
			public List<Song> SearchResults = new List<Song> ();
			
			public Source (CurrentQueueViewController container)
			{
				this.Container = container;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				if (!Settings.Random)
					return Util.PlayListSorted.Count;
				switch (section) {
				case 0:
					return Util.PlayListPlayed.Count + (Util.CurrentSong == null ? 0 : 1);
				case 1:
					return (Util.NextSong == null ? 0 : 1) + Util.NextSongs.Count;
				case 2:
					return (Util.PlayList.Count);
				}
				return 0;
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				return Settings.Random ? 3 : 1;
			}
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				return "";//section == 2 && Settings.Random ? "Future Songs" : "";
			}
			
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return  "";
			}
			
			string key = "songDarkCell";
			
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				Song thesong = SongForIndexPath(indexPath);
				var cell = thesong.GetCell (tableView, false);
				//cell.BackgroundColor = UIColor.Black;
				return cell;
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				
				Song song = SongForIndexPath(indexPath);
				
				Container.Selected (song);
			}
			
			private Song SongForIndexPath(NSIndexPath indexPath)
			{
				if (!Settings.Random)
					return Database.Main.GetObject<Song>(Util.PlayListSorted [indexPath.Row]);
				else
				try {
					switch (indexPath.Section) {
					case 0:
						return indexPath.Row == Util.PlayListPlayed.Count ? Util.CurrentSong : Database.Main.GetObject<Song>(Util.PlayListPlayed [indexPath.Row]); //Util.SongsDict[Util.PlayListSorted[indexPath.Row]];//.Where(x=> x.IndexCharacter == headers[indexPath.Section]).ToArray()[indexPath.Row];
					case 1:
						return indexPath.Row == 0 && Util.NextSong != null ? Util.NextSong :Database.Main.GetObject<Song>(Util.NextSongs [indexPath.Row - (Util.NextSong == null ? 0 : 1)]);
					case 2:
						return Database.Main.GetObject<Song>(Util.PlayList [indexPath.Row]);
					}
				} catch (Exception ex) {
					return new Song ();
				}
				return new Song();
			}
			
			public override int SectionFor (UITableView tableView, string title, int atIndex)
			{
				return atIndex;
			}
			
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				var rowFrame = tableView.RectForRowAtIndexPath (indexPath);
				var midY = (rowFrame.Height / 2) + rowFrame.Y;
				var x = rowFrame.Width - 50;
				var point = Container.View.ConvertPointToView (new PointF (x, midY), Util.BaseView);
				
				Song song = SongForIndexPath(indexPath);
				
				Container.ShowPopUp (song.IsLocal ? song.IsLocal : song.ShouldBeLocal, (index) =>
				                     {
					//PlayNext
					if (index == 1)
						Util.PlaySongNext (song);//(song,song.ArtistId,-1,true);
					else if (index == 2) {
						var plistVc = new PlaylistViewController (true, false, Container.HasSearch);
						plistVc.OnPlaylistSelected = (playlist) => {
							Util.Api.AddToPlaylist (playlist, new []{song}, (success) =>
							{
								if (!success) {
									var alert = new BlockAlertView ("Error".Translate(), "There was an error adding the song to your playlist.".Translate());
									alert.AddButton ("Ok".Translate(), null);
									alert.Show ();
								}
							});
						};
						Container.ActivateController (plistVc);
					} else if (index == 3) {
						Util.Api.CreateMagicPlayList (song, (onSuccess) => {
							if (!onSuccess) {
								var alert = new BlockAlertView ("Error".Translate(), "There was an error creating your magic playlist.".Translate());
								alert.AddButton ("Ok".Translate(), null);
								alert.Show ();
							}
							
						});
					} else if (index == 4) {
						var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this playlist?".Translate());//
						alert.SetCancelButton ("Cancel".Translate(), null);
						alert.AddButton ("Delete".Translate(), delegate {
							
							Util.Api.DeleteSong (song, (success) => {
								if (!success) {
									var alert2 = new BlockAlertView ("Error".Translate(), "There was an error deleting the song.".Translate());
									alert2.AddButton ("Ok".Translate(), null);
									alert2.Show ();
								} else {
									Container.TableView.DeleteRows (new[]{indexPath}, UITableViewRowAnimation.Fade);
								}
							});
							
						});
						alert.Show ();
					} else if (index == 5) {
						//Container.ActivateController(
					}
				}, (shouldBeLocal) => {
					try {
						if (shouldBeLocal) {
							song.ShouldBeLocal = true;
							Downloader.AddFile (song);
						} else {
							if (song.IsLocal && File.Exists (Util.MusicDir + song.FileName))
							try {
								File.Delete (Util.MusicDir + song.FileName);
							} catch {
							}
							Database.Main.UpdateOffline (song, false);
							song.ShouldBeLocal = false;
							
							
						}
						//TODO: update offline
						/*
					lock(Database.Main)
						Database.Main.Update(song);
						*/
						tableView.ReloadData ();
						//TODO Either download or remove
						
					} catch (Exception ex) {
						Console.WriteLine (ex);
					}
				});
			}
			
			
			
			
			
			
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
			//lock(Database.Main)
			//	DataSource.SearchResults = Database.Main.Query<Song>("select * from song where Title like ('%" + text + "%')");	
			text = text.ToLower ();
//			lock (Util.Songs)
//				DataSource.SearchResults = Util.Songs.Where (x => x.Title.ToLower ().Contains (text) || x.Artist.ToLower ().Contains (text)).ToList ();	
			DataSource.IsSearching = true;
			this.BeginInvokeOnMainThread (delegate {
				TableView.ReloadData ();
			});
		}
		
		public override void SearchButtonClicked (string text)
		{
			
		}
		#endregion
	}
}

