using System;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;
using System.IO;

namespace GoogleMusic
{
	public class SonglistViewController : BaseDialogViewController
	{
		Screens SourceScreen;
		AlbumHeaderView albumHeader;
		public bool canEdit;
		List<Song> songs;
		public SonglistViewController(IntPtr handle):base(handle){}
		/*
		public SonglistViewController (Screens sourceScreen, string title, int artistId, int albumId) : this(sourceScreen,title,artistId,albumId,false,true)
		{

		}
		*/
		public SonglistViewController (Screens sourceScreen, string title, int artistId, int albumId,bool isDarkThemed, bool enableSearch) : base(true,enableSearch)
		{
			DarkThemed = isDarkThemed;
			this.SearchBarTintColor = UIColor.FromPatternImage (UIImage.FromFile ("Images/texture.png"));
			SourceScreen = Screens.Songs;
			var section = new Section ();
			section.Add (new StyledStringElement ("Shuffle".Translate(), delegate{
				Settings.Random = true;
				Random rand = new Random();
				var song = songs[rand.Next(0, songs.Count)];
				Util.PlaySong(song,artistId, albumId, false);
			}){
				BackgroundColor = UIColor.Clear,
				TextColor = DarkThemed ? UIColor.White : UIColor.Gray,
				Font = UIFont.BoldSystemFontOfSize(DarkThemed ? 30f : 18f),
				ShouldDeselect = true,
			});

			if (Settings.ShowOfflineOnly)
				lock (Util.OfflineSongs)
					songs = Util.OfflineSongs.Where (x => ((albumId == -1 && x.ArtistId == artistId) || x.AlbumId == albumId)).ToList ();
			else {
				List<string> ids;
				lock(Database.Main.DatabaseLocker)
				{
					if (albumId == -1) {
						ids = Database.Main.Query<Song>("select id from song where ArtistId = ? order by TitleNorm ",artistId).Select(x=> x.Id).ToList();
					} else {
						ids = Database.Main.Query<Song>("select id from song where AlbumId = ? order by Disc,Track",albumId).Select(x=> x.Id).ToList();
					}
				}
				songs = ids.Select(x=> Database.Main.GetObject<Song>(x)).ToList();

				//songs = Util.Songs.Where (x => ((albumId == -1 && x.ArtistId == artistId) || x.AlbumId == albumId)).ToList ();
			}
			if (albumId == -1) {
				songs = songs.OrderBy (x => x.Title).ToList ();
				foreach (var song in songs) {
					var thesong = song;
					var songElement = new SongElement (thesong, false,DarkThemed, AccessoryTapped);
					songElement.Tapped = delegate {
						
						Util.PlaySong (thesong, artistId, albumId, false);
					};
					section.Add (songElement);
				}
			} else {
				songs = songs.OrderBy (x => x.Track).OrderBy (x => x.Disc).ToList ();
				albumHeader = new AlbumHeaderView (Util.AlbumsDict[albumId], songs.Count, songs.Sum (x => x.Duration) / 1000,DarkThemed);
				section.Add (new AlbumHeaderElement ("", albumHeader, false));
				foreach (var song in songs) {
					var thesong = song;
					var songElement = new SongAlbumElement (thesong, false, DarkThemed, AccessoryTapped);
					songElement.Tapped = delegate {
						Console.WriteLine ("Tapped" + songElement.Song);
						Console.WriteLine ("Playing " + thesong);
						//Settings.Random = false;
						Util.PlaySong (thesong, artistId, albumId, false);
					};
					section.Add (songElement);
				}
				TableView.SeparatorColor = UIColor.Clear;
			}
			
			
			Root = new RootElement (title){section};
			Root.UnevenRows = true;
		}
		/*
		public SonglistViewController (Screens sourceScreen, string title, int genre)
		{
			this.Style = UITableViewStyle.Plain;
			Title = title;
			this.Style = UITableViewStyle.Plain;
			var section = new Section ();
			List<Song> songs;
			if (Settings.ShowOfflineOnly)
				lock (Util.OfflineSongs)
					songs = Util.OfflineSongs.Where (x => x.GenreId == genre).OrderBy (x => x.Title).ToList ();
			else
				lock (Util.Songs)
					songs = Util.Songs.Where (x => x.GenreId == genre).OrderBy (x => x.Title).ToList ();

			foreach (var song in songs) {
				var thesong = song;
				var songElement = new SongElement (thesong, true, AccessoryTapped);
				songElement.Tapped = delegate {					
					Util.PlayGenre (thesong, genre);
				};
				section.Add (songElement);
				
			}
			Root = new RootElement (title){section};
		}
		 */
		public Playlist Playlist;

		public SonglistViewController (Screens sourceScreen, Playlist playlist,bool darkThemed,bool enableSearch) :  base (true,enableSearch)
		{
			DarkThemed = darkThemed;
			this.Style = UITableViewStyle.Plain;
			Playlist = playlist;
			canEdit = !playlist.AutoPlaylist;
			Title = playlist.Name;
			var section = new Section ();
			List<PlaylistSongs> songs;
			string query = "";
			if (Settings.ShowOfflineOnly)
				query = "select p.* from PlaylistSongs p inner join song s on s.id = p.songid inner join SongOfflineClass soc on soc.Id = s.id where p.ServerPlaylistId = '" + playlist.ServerId + "' and soc.Offline = 1";
			else
				query = "select p.* from PlaylistSongs p inner join song s on s.id = p.songid where p.ServerPlaylistId = '" + playlist.ServerId + "'";
			
			lock (Database.Locker) {
				songs = Database.Main.Query<PlaylistSongs> (query).ToList ();
			}
			
			section.Add (new StyledStringElement ("Shuffle".Translate(), delegate{
				Settings.Random = true;
				Util.PlayPlaylist (null, playlist);
			}){
				BackgroundColor = UIColor.Clear,
				TextColor = DarkThemed ? UIColor.White : UIColor.Gray,
				Font = UIFont.BoldSystemFontOfSize(darkThemed ? 30f : 18f),
			});
			foreach (var song in songs) {
				var thesong = song;
				var songElement = new PlaylistSongElement (thesong, true,DarkThemed, AccessoryTapped);
				songElement.Tapped = delegate {		
					Util.PlayPlaylist (songElement.Song, playlist);
				};
				section.Add (songElement);
				
			}
			Root = new RootElement (playlist.Name){section};
		}

		UIBarButtonItem nowPlayingBtn;
		UIBarButtonItem editBtn;
		UIBarButtonItem doneBtn;

		public override void ViewWillAppear (bool animated)
		{
			if (!canEdit || DarkThemed) {
				base.ViewWillAppear (animated);
				return;
			}

			var npBtn = BaseViewController.CreateNowPlayingButton();
			nowPlayingBtn = new UIBarButtonItem (npBtn);
			editBtn = new UIBarButtonItem (UIBarButtonSystemItem.Edit, delegate {
				this.SetEditing (true, true);
				SetRightButton ();
			});
			doneBtn = new UIBarButtonItem (UIBarButtonSystemItem.Done, delegate{
				this.SetEditing (false, true);
				SetRightButton ();
			});
			SetRightButton ();
		}

		private void SetRightButton ()
		{
			if (Util.CurrentSong != null && Util.IsIphone && !this.Editing) {
				SetEditNowPlaying ();
			} else
				this.NavigationItem.RightBarButtonItem = this.Editing ? doneBtn : editBtn;
		}

		private void SetEditNowPlaying ()
		{
			var toolbar = new TransparentToolbar (new RectangleF (0, 0, 130, this.NavigationController.NavigationBar.Frame.Height));
			toolbar.BarStyle = UIBarStyle.Black;
			toolbar.Translucent = true;
			toolbar.SetItems (new UIBarButtonItem[]{editBtn,new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),nowPlayingBtn}, false);
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (toolbar);

		}
		
		private void AccessoryTapped (Song song, UITableView tableView, NSIndexPath indexPath)
		{
			var rowFrame = tableView.RectForRowAtIndexPath (indexPath);
			var midY = (rowFrame.Height / 2) + rowFrame.Y;
			var x = rowFrame.Width - 50;
			var point = this.View.ConvertPointToView (new PointF (x, midY), Util.BaseView);
			
			
			this.ShowPopUp (point, Screens.Songs, song.IsLocal ? song.IsLocal : song.ShouldBeLocal, (index) =>
			{
				//PlayNext
				if (index == 1)
					Util.PlaySongNext (song);//(song,song.ArtistId,-1,true);
				else if (index == 2) {
//					var plistVc = new PlaylistViewController (true,false,this.EnableSearch);
//					plistVc.OnPlaylistSelected = (playlist) => {
//						Util.Api.AddToPlaylist (playlist, new []{song}, (success) =>
//						{
//							if (!success) {
//								var alert = new BlockAlertView ("Error".Translate(), "There was an error adding the song to your playlist.".Translate());
//								alert.AddButton("Ok".Translate(),null);
//								alert.Show ();
//							}
//						});
//					};
//					this.ActivateController (plistVc);
				} else if (index == 3) {
					Util.Api.CreateMagicPlayList (song, (onSuccess) => {
						if (!onSuccess) {
							var alert = new BlockAlertView ("Error".Translate(), "There was an error creating your magic playlist.".Translate());
							alert.AddButton("Ok".Translate(),null);
							alert.Show ();
						}
								
					});
				} else if (index == 4) {
					var alert = new BlockAlertView ("Delete?".Translate(), "Are you sure you want to delete this song?".Translate());
					alert.AddButton ("Cancel".Translate(), BlockAlertView.ButtonColor.Black,null);
					alert.AddButton("Delete".Translate(), BlockAlertView.ButtonColor.Red, delegate{
						Util.Api.DeleteSong (song, (success) => {
							if (!success) {
								var alert2 = new BlockAlertView ("Error".Translate(), "There was an error deleting the song.".Translate());
								alert2.AddButton("Ok".Translate(),null);
								alert2.Show ();
							} else {
								this.TableView.DeleteRows (new[]{indexPath}, UITableViewRowAnimation.Fade);
							}
						});
					});
					alert.Show ();
				} else if (index == 5) {
					CurrentSongEditor = new EditSongViewController(song);
					this.ActivateController(CurrentSongEditor);
				}
			}, (shouldBeLocal) => {
				try{
				if (shouldBeLocal) {
					song.ShouldBeLocal = true;
					//TODO: fix downloader
						//Downloader.AddFile (song);
					//TODO: update offline
					
					lock (Database.Locker)
						Database.Main.Update (song);
							
				} else {
					if (song.IsLocal && File.Exists (Util.MusicDir + song.FileName))
						try {
							File.Delete (Util.MusicDir + song.FileName);
						} catch {
						}
					Database.Main.UpdateDeleteOffline (song);
						
							
				}
					
					tableView.ReloadData();
					
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
				//TODO Either download or remove	
			});
	
		}
		
		public class EditingSource : DialogViewController.Source
		{
			SonglistViewController viewController;
			public EditingSource (SonglistViewController dvc) : base (dvc)
			{
				viewController = dvc;
			}
			
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				// Trivial implementation: we let all rows be editable, regardless of section or row
				if(indexPath.Section == 0 && indexPath.Row == 0)
					return false;
				return viewController.canEdit;
			}

			public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
			{
				return viewController.canEdit && indexPath.Row != 0;
			}

			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
				Console.WriteLine("move row");
				bool goingUp = sourceIndexPath.Row < destinationIndexPath.Row;
				var row = destinationIndexPath.Row;
				var section = Container.Root.Sections [0];
				string prevId = "";
				string nextId = "";
				if (goingUp) {
					prevId = (section [row] as PlaylistSongElement).PlistSong.EntryId;
					nextId = section.Count == row + 1 ? "" : (section [row + 1] as PlaylistSongElement).PlistSong.EntryId;
				} else {
					prevId = row <= 1 ? "" : (section [row - 1] as PlaylistSongElement).PlistSong.EntryId;
					nextId = (section [row] as PlaylistSongElement).PlistSong.EntryId;
				}
				//Console.WriteLine(prevId + " - " + nextId);
				Util.Api.MoveSong ((section [sourceIndexPath.Row]as PlaylistSongElement).PlistSong, prevId, nextId,destinationIndexPath.Row + 1);
				
				//
			}

			public override NSIndexPath CustomizeMoveTarget (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
			{
				Console.WriteLine("CustomizeMoveTarget" + proposedIndexPath.Row);
				if(proposedIndexPath.Row == 0)
					return NSIndexPath.FromRowSection(1,0);
				return proposedIndexPath;
			}
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.Delete;
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				Console.WriteLine("commit editing style");
				var section = Container.Root [indexPath.Section];
				var element = section [indexPath.Row];
				var theSong = (element as PlaylistSongElement).PlistSong;
				Util.Api.DeleteSongFromPlaylist (theSong, (success) => {
					if (success)
						section.Remove (element);
				});
			}
			
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = base.GetCell (tableView, indexPath);
				cell.ShowsReorderControl = true;
				return cell;
			}

			public override float GetHeightForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = Root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				var sizable = element as IElementSizing;
				if (sizable == null)
					return tableView.RowHeight;
				return sizable.GetHeight (tableView, indexPath);
			}
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource (this);
		}
	
	}
}

