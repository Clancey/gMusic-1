using System;
using SQLite;
using GoogleMusic;
using System.Linq;
using System.IO;
using Xamarin.Data;
namespace GoogleMusic
{
	[Serializable]
	public partial class Song
	{
		public Song ()
		{
		}
		
		[PrimaryKey]
		[Indexed]
		public string Id { get; set; }

		public string Title { get; set; }
		[OrderBy]
		public string TitleNorm { get; set; }

		[Indexed]
		public int ArtistId { get; set; }
		[Indexed]
		public int AlbumId { get; set; }

		[Indexed]
		public int GenreId { get; set; }

		public int Disc { get; set; }

		public int Track{ get; set; }

		public int Duration { get; set; }

		public int PlayCount { get; set; }

		public int OfflinePlayCount { get; set; }

		public int Rating{ get; set; }

		[Indexed]
		[GroupBy]
		public string IndexCharacter { get; set; }

		public string Album { get; set; }

		[Ignore]
		public Album TheAlbum {
			get {
				if (Util.AlbumsDict.ContainsKey (this.AlbumId)) 
					return Util.AlbumsDict [this.AlbumId];
				return new Album ();
			}
		}
		
		[Ignore]
		public Artist TheArtist {
			get { 
				if (Util.ArtistsDict.ContainsKey (ArtistId))
					return Util.ArtistsDict [this.ArtistId];
				return new GoogleMusic.Artist ();
			}
		}
		[Ignore]
		public int StreamId { get; set; }
		
		[Ignore]
		public string Genre {
			get {
				if (Util.GenresDict.ContainsKey (GenreId))
					return Util.GenresDict [GenreId].Name;
				return  "";
			}
		}
		
		public string Artist { get ; set; }

		public string AlbumArtist{ get; set; }

		public string AlbumUrl { get; set; }

		[Ignore]
		public bool IsLocal {
			get {
				if(string.IsNullOrEmpty(Id))
					return false;
				if (Util.OfflineSongsList.ContainsKey (Id))
					return Util.OfflineSongsList [Id];
				return false;
			}
		}

		[Ignore]
		public bool IsTemp { get; set; }

		public bool IsReady ()
		{
			if(IsTemp || IsLocal)
				return true;
			return HasNotified;
		}

		public bool Deleted{ get; set; }

		public bool ShouldBeLocal { get; set; }
		
		[Ignore]
		public virtual string FileName {
			get{ return Id + ".mp3";}
		}
		
		DateTime expiration;
		string playUrl;
		
		[Ignore]
		public virtual string PlayUrl {
			get {
				try {
#if mp3tunes
					if (string.IsNullOrEmpty (playUrl))
					{
						playUrl = Util.Api.GetSongUrl (Id);
					}
					return playUrl;
#endif
					if (string.IsNullOrEmpty (playUrl) || expiration < DateTime.Now) {
						playUrl = Util.Api.GetSongUrl (Id);
						var exp = getValue ("expire", playUrl);
						expiration = new System.DateTime (1970, 1, 1, 0, 0, 0, 0).AddSeconds (int.Parse (exp)).ToLocalTime ();
					}
					return playUrl;
					
				} catch (Exception ex) {
					Console.WriteLine ("Error getting PlayUrl : " + ex);
					return "";	
				}
			}
		}
		
		public static string getValue (string name, string html)
		{
			html = html.Split (new string[]{"?"}, StringSplitOptions.None).ToList () [1];
			var dict = html.Split (new string[]{"&","="}, StringSplitOptions.None).ToList ();
			return dict [dict.IndexOf (name) + 1];
		}
#if iOS		
		[Ignore]
		public MonoTouch.UIKit.UIImage AlbumImage {
			get {
				if(Util.AlbumsDict.ContainsKey(AlbumId))
					return Util.AlbumsDict [AlbumId].AlbumArt (320);
				return Images.DefaultAlbumImage;
			}
		}

#endif
		public override string ToString ()
		{
			return Artist + " - " + Title;
		}

		[Ignore]
		public TimeSpan DurationSpan {
			get{ return TimeSpan.FromSeconds (Duration / 1000);}
		}
		
		public override bool Equals (object obj)
		{
			if (obj is Song)
				return this.Id == ((Song)obj).Id;
			return false;
		}

		public static bool operator == (Song x, Song y)
		{
			if (object.ReferenceEquals (x, y)) {
				// handles if both are null as well as object identity
				return true;
			}

			if ((object)x == null || (object)y == null) {
				return false;
			}
			return x.Id == y.Id;
		}
		
		public static bool operator != (Song x, Song y)
		{
			if (object.ReferenceEquals (x, y)) {
				// handles if both are null as well as object identity
				return false;
			}

			if ((object)x == null || (object)y == null) {
				return true;
			}
			return x.Id != y.Id;
		}

		bool HasNotified = false;


		[Ignore]
		public float DownloadPercent { get; set; }
		public float GetDownloadPercent()
		{
			if(DownloadPercent == 0)
			{
				DownloadPercent = IsTemp ? 1f: IsLocal ? 1f : 0f;
			}
			return DownloadPercent;
		}

		public void PulseDownload (float percent)
		{
			DownloadPercent = percent;
			return;
			//if (DownloadProgress != null)
			//	DownloadProgress (this, new EventArgs<float> (percent));
		}

		public event EventHandler<EventArgs<float>> DownloadProgress;
		
		private FileStream fileStream;

		public FileStream FileSream {
			get {
				if (fileStream == null || !fileStream.CanRead)
					fileStream = new FileStream ((IsTemp && !IsLocal ? Util.TempDir + FileName : Path.Combine (Util.MusicDir, FileName)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096);
				return fileStream;
			}
		}
	
	}
}

