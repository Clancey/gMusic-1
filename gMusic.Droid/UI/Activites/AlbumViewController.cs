using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
namespace GoogleMusic
{
	[Activity (Label = "Albums")]			
	public class AlbumViewController : BaseFragment , IBaseViewController, IViewController
	{
		public AlbumViewController()
		{
			Title = "Albums";
		}
		public AlbumViewController( Artist artist)
		{
			Artist = artist;
			Title = Artist.Name;

		}
		public Artist Artist{get;set;}
		AlbumViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			if (Artist == null)
				model = new AlbumViewModel (Activity, this.ListView, this);
			else
				model = new AlbumViewModel (Activity, this.ListView, this, string.Format ("ArtistId = {0}", Artist.Id), "NameNorm");

			ListView.Adapter = model;
//			model.AlbumSelected = (selectedAlbum) => {
//				NavigationController.PushViewController(new SongListFragment(selectedAlbum.Name,string.Format("AlbumId = {0}",selectedAlbum.Id),"Disc, Track"),true);
//			};
			//model.PrecachData ();

		}

		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}

	}
}

