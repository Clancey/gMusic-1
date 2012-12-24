using System;
using SQLite;
using Xamarin.Data;

namespace GoogleMusic
{
	public partial class Genre
	{
		public Genre ()
		{
		}
		
		[PrimaryKeyAttribute,AutoIncrement]
		public int Id { get; set; }

		[Indexed]
		[OrderBy]
		public string Name { get; set; }

		[Indexed]
		[GroupBy]
		public string IndexCharacter { get; set; }

		public bool ShouldBeLocal { get; set; }

		public int OffineCount {
			get {
				if (Util.OfflineGenreList.ContainsKey (Id))
					return Util.OfflineGenreList [Id];
				return 0;
				
			}
		}
	}
}

