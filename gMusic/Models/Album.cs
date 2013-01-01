using System;
using SQLite;
using Xamarin.Data;


#if Droid
using MonoDroid.UrlImageStore;
#endif

namespace GoogleMusic
{
	public partial class Album : IImageUpdated
	{
		public Album ()
		{
		}

		[PrimaryKeyAttribute]
		[Indexed]
		public int Id { get; set; }

		[Indexed]
		public int ArtistId { get; set; }

		public string Url { get; set; }

		[Ignore]
		public Artist TheArtist {
			get{ return Database.Main.GetObject<Artist>(ArtistId) ?? new Artist ();}
		}

		public string Artist{ get { return TheArtist.Name; } }

		public bool ShouldBeLocal{ get; set; }
		
		string albumArt (int size)
		{	
			if (string.IsNullOrEmpty (Url))
				return "";
			if (!Url.Contains ("=s"))
				return Url;
			
			int index = Url.LastIndexOf ("=s");
			var newString = Url.Substring (0, index + 2);
			
			if (!newString.Contains ("http:"))
				newString = "http:" + newString;
			return newString + (size * Util.Scale) + "-c";
		}

		[Indexed]
		[GroupBy]
		public string IndexCharacter { get; set; }

		[Ignore]
		public int OffineCount {
			get {
				if (Util.OfflineAlbumsList.ContainsKey (Id))
					return Util.OfflineAlbumsList [Id];
				return 0;
				
			}
		}

		public string Name { get; set; }
		[OrderBy]
		[Indexed]
		public string NameNorm { get; set; }
#if iOS	
		public MonoTouch.UIKit.UIImage AlbumArt (int size)			
#elif Droid
			
		public Android.Graphics.Drawables.Drawable AlbumArt (int size)
				
#endif
		{
			var url = albumArt (size);
			if (string.IsNullOrEmpty (url))
				return Images.DefaultAlbumImage;
			return ImageLoader.DefaultRequestImage (albumArt (size), this);
		}

		public event EventHandler ALbumArtUpdated;
		void IImageUpdated.UpdatedImage (string uri)
		{
			if (ALbumArtUpdated != null)
				ALbumArtUpdated (this, new EventArgs ());
		}
	}
}

