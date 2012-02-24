using System;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.IO;

namespace gMusic
{
	public class SongViewController : BaseViewController
	{
		private Source DataSource;
		public SongViewController () : base(UITableViewStyle.Plain,true)
		{
			this.Title = "Songs";
		}
		protected override void setupTable ()
		{
			base.setupTable ();			
			DataSource = new Source(this);
			this.TableView.Source = DataSource;
		}
		
		public void Selected(Song song)
		{
			//FinishSearch();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.Title = "Songs";
		}
		
		#region data source
			public class Source : UITableViewSource {
			const float yboundary = 65;
			protected SongViewController Container;
			public bool IsSearching {get;set;}
			public List<Song> SearchResults = new List<Song>();
			
			
			public Source (SongViewController container)
			{
				this.Container = container;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				return Util.SongGroups[section].Count();
			}
			public override int NumberOfSections (UITableView tableView)
			{
				return Util.SongGroups.Count();
			}
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return "";
			}
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var songGroup = Util.SongGroups[indexPath.Section];
				Song thesong = songGroup.ElementAt(indexPath.Row).id; //Util.Songs[songGroup.Start + indexPath.Row];
				return thesong.GetCell (tableView);
			}
			
			
			
			NSArray array;
			[Export ("sectionIndexTitlesForTableView:")]
			public new NSArray SectionIndexTitles (UITableView tableView)
			{
				array = NSArray.FromStrings (Util.SongGroups.Select(x=> x.Index).ToArray());
				return array;
			}
			
			public override int SectionFor (UITableView tableView, string title, int atIndex)
			{
				return atIndex;
			}
			
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				return Util.SongGroups[section].Index;
			}
			
			
	
		
			
		}
		
		#endregion
		
		
	}
}

