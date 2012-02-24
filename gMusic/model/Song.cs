using System;
using SQLite;
using System.Linq;
using MonoTouch.UIKit;

namespace gMusic
{
	[Serializable]
	public partial class Song : gMusic.web.songs
	{
		public string NameNorm {get;set;}
		public string IndexChar {get;set;}
		public int Order {get;set;}
		
		[Ignore]
		public Artist Artist
		{
			get{return Util.Artists[this.artist_id];}
		}
	}
}

