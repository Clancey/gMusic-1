using System;
using MonoTouch.UIKit;

namespace GoogleMusic
{
	public class AlbumViewController : BaseViewController
	{
		
		private AlbumViewModel model;
		public Artist Artist{get;set;}
		public AlbumViewController() : this(true)
		{
			
		}
		public AlbumViewController (bool hasSearch) : base (UITableViewStyle.Plain,false,hasSearch)
		{
			//	Root = new RootElement ("Artists");
			Screen = Screens.Artist;
			this.Title = "Albums".Translate();
			
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			if (Artist == null)
				model = new AlbumViewModel (this);
			else {
				model = new AlbumViewModel (this, string.Format ("ArtistId = {0}", Artist.Id), "NameNorm");
				this.Title = Artist.Name;
			}
//			model.AlbumSelected = (selectedAlbum) => {
//				this.NavigationController.PushViewController(new SongListViewController(string.Format("AlbumId = {0}",selectedAlbum.Id),"Disc, Track"),true);
//			};
			this.TableView.Source = model;
			this.TableView.RowHeight = DarkThemed ?  80 : 40;
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

