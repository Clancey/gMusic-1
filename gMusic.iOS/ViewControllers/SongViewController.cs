using System;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.IO;

namespace GoogleMusic
{
	public class SongViewController : BaseViewController, IBaseViewController
	{
		private SongViewModel DataSource;
		bool IsTvOut;
		public SongViewController() : this(true) 
		{

		}
		public SongViewController (bool isTvOut) : base(UITableViewStyle.Plain,true,isTvOut)
		{
			IsTvOut = isTvOut;
			Screen = Screens.Songs;
			this.Title = "Songs".Translate();
			this.ShuffleClicked += delegate {
				Util.PlayRandom();
			};
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new SongViewModel(this);
			DataSource.PrecachData ();
			this.TableView.Source = DataSource;
			if(IsTvOut){
				TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
				TableView.SeparatorColor = UIColor.White;
			}
			this.TableView.RowHeight = DarkThemed ? 80 : 56;
		}

		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.Title = "Songs".Translate();
		}


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

			if(string.IsNullOrEmpty(text))
			{
				DataSource.IsSearching = false;

			}
			else if(Settings.ShowOfflineOnly)
			{
				lock(Util.OfflineSongs)
					DataSource.SearchResults = Util.OfflineSongs.Where( a=> a.Title.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1 || a.Artist.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();	
				DataSource.IsSearching = true;
			}
			else
			{
				//TODO: Fix Search
				//DataSource.SearchResults = Util.Songs.Where( a=> a.Title.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1 || a.Artist.IndexOf(text,StringComparison.OrdinalIgnoreCase) != -1).ToList();	
			
				DataSource.IsSearching = true;
			}
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

