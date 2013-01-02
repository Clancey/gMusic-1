using System;
using MonoTouch.UIKit;
using Xamarin.Data;

namespace GoogleMusic
{
	public class AlbumArtistViewController : BaseViewController
	{
		
		private ArtistAlbumViewModel model;
		GroupInfo GroupInfo;
		public AlbumArtistViewController(string title, GroupInfo groupInfo) : base (UITableViewStyle.Plain,false,true)
		{
			Title = title;
			Screen = Screens.Songs;
			this.model = new ArtistAlbumViewModel (this, groupInfo);// = new ArtistAlbumViewModel (this, string.Format ("ArtistId = {0}", artist.Id), "Album, Disc, Track");
			//(this, string.Format ("AlbumId = {0}", album.Id), "Album, Disc, Track");
			
		}
		protected override void setupTable ()
		{
			base.setupTable ();
			this.TableView.Source = model;
			this.TableView.RowHeight = DarkThemed ?  80 : 50;
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
		}
		
		#region implemented abstract members of BaseViewController
		
		public override void StartSearch ()
		{
			//throw new NotImplementedException ();
		}
		
		public override void FinishSearch ()
		{
			//throw new NotImplementedException ();
		}
		
		public override void PerformFilter (string text)
		{
			//throw new NotImplementedException ();
		}
		
		public override void SearchButtonClicked (string text)
		{
			//throw new NotImplementedException ();
		}
		
		#endregion
	}
}

