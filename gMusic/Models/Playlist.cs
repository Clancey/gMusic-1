using System;
using SQLite;


namespace GoogleMusic
{
	public partial class Playlist 
	{
		public Playlist ()
		{
		}		
		[Indexed]
		[PrimaryKey]
		public string ServerId { get;set; }
		public string Name { get;set; }
		public bool CanEdit {get;set;}
		public bool AutoPlaylist{get;set;}
		public bool ShouldBeLocal{get;set;}
		public bool Deleted {get;set;}
		public int OffineCount{get;set;}
	}
}

