using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;
using Xamarin.Data;

namespace GoogleMusic
{
	public class PlaylistSongViewModel : BaseViewModel<PlaylistSongs>
	{
		Playlist Playlist;
		#if iOS

		public PlaylistSongViewModel (IBaseViewController parent, Playlist playlist) : base(parent)
		{
			init (playlist);
		}
		#elif Droid
		public PlaylistSongViewModel (Android.Content.Context context,Android.Widget.ListView listView,IBaseViewController parent, Playlist playlist ) : base (context,listView, parent)
		{
			init (playlist);
		}
		#endif
		void init(Playlist playlist)
		{
			Playlist = playlist;
			GroupInfo = new GroupInfo{Filter = string.Format("ServerPlaylistId = \"{0}\" and SongId in (select Id from song where Id = SongId)",Playlist.ServerId), OrderBy = "SOrder", Ignore = true};
		}
		#region implemented abstract members of SectionedAdapter
		public override void RowSelected (PlaylistSongs ps)
		{
			var song = Database.Main.GetObject<Song> (ps.SongId);
			Util.PlayPlaylist (song, Playlist);
		}
		public override void LongPressOnItem (PlaylistSongs item)
		{

		}
		public override ICell GetICell (int section, int position)
		{
			var ps = Database.Main.ObjectForRow<PlaylistSongs> (GroupInfo, section, position);
			return Database.Main.GetObject<Song> (ps.SongId);
		}

		public override string[] SectionIndexTitles ()
		{
			return null;
		}
		#endregion
	}
}

