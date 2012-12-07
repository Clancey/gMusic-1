using System;
using SQLite;

namespace GoogleMusic
{
	public partial class Artist
	{
		[PrimaryKey]
		public int Id { get;set; }
		[Indexed]
		public string Name{ get;set; }
		[Indexed]
		public string NormName { get;set; }
		[Indexed]
		public string IndexCharacter { get;set; }
		public bool ShouldBeOffline{get;set;}
		public int OffineCount{get
			{
				if(Util.OfflineArtistList.ContainsKey(Id))
					return Util.OfflineArtistList[Id];
				return 0;
				
			}
		}
	}
}

