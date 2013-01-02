using System;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using ClanceysLib;

namespace GoogleMusic
{
	public class PlaylistSongElement : SongElement
	{
		public PlaylistSongs PlistSong;

		public PlaylistSongElement (PlaylistSongs pSong, bool showArtist, bool isDarkThemed, Action<Song,UITableView,NSIndexPath> accessoryTapped) : base(new Song(),showArtist,isDarkThemed,accessoryTapped)
		{
			PlistSong = pSong;
			Song = Database.Main.GetObject<Song> (PlistSong.SongId);
		}
	}

	public class SongElement : Element , IElementSizing
	{
		public Song Song;
		bool ShowArtist;
		Action<Song,UITableView,NSIndexPath> AccessoryTapped;
		public Action Tapped;
		public bool DarkThemed;

		public SongElement (Song song, bool showArtist,bool darkThemed, Action<Song,UITableView,NSIndexPath> accessoryTapped) : base(song.Title)
		{
			DarkThemed = darkThemed;
			Song = song;
			ShowArtist = showArtist;
			AccessoryTapped = accessoryTapped;
		}

		string key = "songElement";

		public override MonoTouch.UIKit.UITableViewCell GetCell (MonoTouch.UIKit.UITableView tv)
		{
			return Song.GetCell (tv,DarkThemed);
		}

		public override void AccessoryButtonTapped (DialogViewController dvc, MonoTouch.UIKit.UITableView tableview, MonoTouch.Foundation.NSIndexPath path)
		{
			if (AccessoryTapped != null)
				AccessoryTapped (Song, tableview, path);
		}

		public override void Selected (DialogViewController dvc, MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath path)
		{
			if (Tapped != null)
				Tapped ();
		}

		public override bool Matches (string text)
		{
			var lower = text.ToLower ();
			return Song.Title.ToLower ().Contains (lower) || Song.Artist.ToLower ().Contains (lower);
		}	

		#region IElementSizing implementation
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return DarkThemed ? 80 : 56;
		}
		#endregion


	}


}


	