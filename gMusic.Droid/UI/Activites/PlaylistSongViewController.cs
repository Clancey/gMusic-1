using System;
using Android.OS;

namespace GoogleMusic
{
	public class PlaylistSongViewController : BaseViewController , IBaseViewController, IViewController
	{
		Playlist Playlist;
		public PlaylistSongViewController()
		{
			
		}
		public PlaylistSongViewController(Playlist playlist)
		{
			Playlist = playlist;
			Title = Playlist.Name;
		}
		PlaylistSongViewModel model;
		public override void OnActivityCreated (Bundle savedInstanceState)
		{			
			base.OnActivityCreated (savedInstanceState);
			//ListView.FastScrollEnabled = true;
			model = new PlaylistSongViewModel (Activity, this.ListView, this,Playlist);
			this.ListView.Adapter = model;
			//model.PrecachData ();
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}
	
