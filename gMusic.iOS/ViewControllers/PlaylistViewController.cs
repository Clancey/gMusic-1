using System;
using MonoTouch.UIKit;

namespace GoogleMusic
{
	public class PlaylistViewController : BaseViewController
	{	
		private PlaylistViewModel DataSource;
		bool IsTvOut;

		public PlaylistViewController () : this(true)
		{
		
		}

		public PlaylistViewController (bool isTvOut) : base(UITableViewStyle.Plain,true,isTvOut)
		{
			IsTvOut = isTvOut;
			Screen = Screens.Songs;
			this.Title = "Playlists".Translate ();
			this.ShuffleClicked += delegate {
				Util.PlayRandom ();
			};
		}

		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new PlaylistViewModel (this);
			this.TableView.Source = DataSource;
			if (IsTvOut) {
				TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
				TableView.SeparatorColor = UIColor.White;
			}
			this.TableView.RowHeight = DarkThemed ? 80 : 56;
		}
	
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.Title = "Playlists".Translate ();
		}
	
	
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
//			text = text.ToLower ();
//		
//			if (string.IsNullOrEmpty (text)) {
//				DataSource.IsSearching = false;
//			
//			} else if (Settings.ShowOfflineOnly) {
//				lock (Util.OfflineSongs)
//					DataSource.SearchResults = Util.OfflineSongs.Where (a => a.Title.IndexOf (text, StringComparison.OrdinalIgnoreCase) != -1 || a.Artist.IndexOf (text, StringComparison.OrdinalIgnoreCase) != -1).ToList ();	
//				DataSource.IsSearching = true;
//			} else {
//				//TODO: Fix Search
//				//DataSource.SearchResults = Util.Songs.Where( a=> a.Title.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1 || a.Artist.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();	
//			
//				DataSource.IsSearching = true;
//			}
//			this.BeginInvokeOnMainThread (delegate {
//				TableView.ReloadData ();
//			});
		}
	
		public override void SearchButtonClicked (string text)
		{
		
		}
	#endregion
	}
}

