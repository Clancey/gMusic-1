using System;
using SQLite;

namespace GoogleMusic
{
	public class PlaylistSongs 
	{
		public PlaylistSongs ()
		{
		}
		[Indexed]
		public string ServerPlaylistId { get;set; }
		[Indexed]
		public string SongId { get;set; }
		public bool IsOnServer {get;set;}
		[Indexed]
		public int SOrder {get;set;}
#if !mp3tunes
		[PrimaryKey]
#endif
		public string EntryId {get;set;}
		public bool Deleted {get;set;}
	}
}

