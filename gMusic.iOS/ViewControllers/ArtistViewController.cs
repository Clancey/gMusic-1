using System;
using MonoTouch.UIKit;
using Xamarin.Data;

namespace GoogleMusic
{
	public class ArtistViewController : BaseViewController
	{
		
		private ArtistViewModel DataSource;
		public GroupInfo GroupInfo;
		public ArtistViewController() : this(true)
		{
			
		}
		public ArtistViewController (bool hasSearch) : base (UITableViewStyle.Plain,false,hasSearch)
		{
			//	Root = new RootElement ("Artists");
			Screen = Screens.Artist;
			this.Title = "Artists".Translate();
			
		}
		protected override void setupTable ()
		{
			base.setupTable ();
			DataSource = new ArtistViewModel(this,GroupInfo);
			this.TableView.Source = DataSource;
			this.TableView.RowHeight = DarkThemed ?  80 : 40;
		}

		public void Selected (Artist artist)
		{
			DataSource.RowSelected (artist);
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

