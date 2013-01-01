
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace GoogleMusic
{
	public class PlaylistFragment : BaseViewController , IBaseViewController
	{
		bool IsAutoPlaylist;
		PlaylistViewModel model;
		public PlaylistFragment() : this(false)
		{

		}
		public PlaylistFragment(bool isAutoPlaylist)
		{
			IsAutoPlaylist = isAutoPlaylist;
			
			Title = IsAutoPlaylist ? "Auto-Playlists" : "Playlists";
		}
		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
			ListView.FastScrollEnabled = true;
			model = new PlaylistViewModel (Activity,this.ListView, this,IsAutoPlaylist);
			model.PlaylistSelected = (selectedPlaylist) => {

			};
			this.ListView.Adapter = model;
			
		}
		
		public void ReloadData ()
		{
			model.NotifyDataSetChanged ();
		}
	}
}

