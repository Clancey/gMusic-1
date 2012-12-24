using System;
using MonoTouch.UIKit;

namespace GoogleMusic
{
	public class AlbumViewController : BaseViewController
	{
		
		private AlbumViewModel DataSource;
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
			DataSource = new AlbumViewModel(this);
			this.TableView.Source = DataSource;
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

