using System;
using System.IO;
using GoogleMusic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading.Tasks;


#if iOS
using MonoTouch.UIKit;
using MonoTouch.MediaPlayer;
using MonoTouch.Foundation;
#endif

public static partial class Util
{
	#if gmusic && ADHOC	
	public static string appId = "472342018";
	static string mticksId = "a0c25e41d6cd380524afcbaa1";
	static string flurryId = "SKPMCZ6HR65RVSAE6BVR";	
	#elif gmusic
	public static string appId = "472342018";
	static string mticksId = "94a1ca5e82bda5f3ac61bb6a6";
	static string flurryId = "A1QIA2DJ6QUMAY6YIG12";	
	#elif mp3tunes
	public static string appId = "515131621";
	static string mticksId = "939d350866e3178608173258d";
	static string flurryId = "ECSR2XBWB5R8MZB9YGY3";
	#endif

	public static RectangleF WindowFrame {
		get;
		set;
	}
	public static readonly string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString ();

	public static StreamingPlayback Player = new StreamingPlayback ();
	public static object CurrentSongLocker = new object ();
	static Song _currentSong = new Song ();
	
	public static Song CurrentSong {
		get {
			lock (CurrentSongLocker)
				return _currentSong;
		}
		set {
			lock (CurrentSongLocker)
			{
				if(_currentSong == value)
					return;
				_currentSong = value;
			}
			Settings.CurrentSongId = _currentSong.Id;
			UpdateMpNowPlaying();
		}
	}

	internal static Song _nextSong;
	internal static object NextSongLocker = new object ();
	
	internal static Song NextSong {
		get {
			lock (NextSongLocker)
				return _nextSong;
		}
		set {
			lock (NextSongLocker)
				_nextSong = value;
		}
	}

	public static List<Artist> Artists = new List<Artist> ();
	public static Dictionary<int,Artist> ArtistsDict = new Dictionary<int, Artist> ();
	#if !mp3tunes
	public static Dictionary<string,int> ArtistIdsDict = new Dictionary<string, int> ();
	public static Dictionary<Tuple<int,string>,int> AlbumsIdsDict = new Dictionary<Tuple<int,string>, int> ();
	public static Dictionary<string,int> GenresIdsDict = new Dictionary<string, int> ();
	#endif
	public static List<IGrouping<string, Artist>> ArtistsGrouped = new List<IGrouping<string, Artist>> ();
	public static List<Album> Albums = new List<Album> ();
	public static Dictionary<int,Album> AlbumsDict = new Dictionary<int, Album> ();
	public static List<IGrouping<string, Album>> AlbumsGrouped = new List<IGrouping<string, Album>> ();
	public static List<Genre> Genres = new List<Genre> ();
	public static Dictionary<int,Genre> GenresDict = new Dictionary<int, Genre> ();
	public static List<IGrouping<string, Genre>> GenreGroupped = new List<IGrouping<string, Genre>> ();
	public static List<Playlist> PlaylistsList = new List<Playlist> ();
	public static List<Playlist> AutoPlaylists = new List<Playlist> ();
	public static Dictionary<string,bool> OfflineSongsList = new Dictionary<string,bool> ();
	public static Dictionary<int,int> OfflineAlbumsList = new Dictionary<int,int> ();
	public static Dictionary<int,int> OfflineArtistList = new Dictionary<int,int> ();
	public static Dictionary<int,int> OfflineGenreList = new Dictionary<int,int> ();
	public static List<Song> OfflineSongs = new List<Song> ();
	public static List<System.Linq.IGrouping<string,Song>> OfflineSongsGrouped = new List<System.Linq.IGrouping<string, Song>> ();
	public static List<Artist> OfflineArtists = new List<Artist> ();
	public static List<System.Linq.IGrouping<string,Artist>> OfflineArtistsGrouped = new List<System.Linq.IGrouping<string, Artist>> ();
	public static List<Album> OfflineAlbums = new List<Album> ();
	public static List<System.Linq.IGrouping<string,Album>> OfflineAlbumsGrouped = new List<System.Linq.IGrouping<string, Album>> ();
	public static List<Genre> OfflineGenres = new List<Genre> ();
	public static List<System.Linq.IGrouping<string,Genre>> OfflineGenreGroupped = new List<System.Linq.IGrouping<string, Genre>> ();
	public static List<Playlist> OfflinePlaylistsList = new List<Playlist> ();

	public static void SongIsOver ()
	{
		Player.Pause ();
		if (Settings.RepeatMode == 2) {
			Seek (0);
			Player.Play();
		}
		else
			Next ();
	}

	public static void Seek (float value)
	{
		try{
		
			Player.Seek(value);
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex);
		}
	}

	public static readonly string DocumentsFolder = BaseDir + "/Documents/";
	public static readonly string PicDir = Path.Combine (BaseDir, "Library/Caches/Pictures/");
	public static readonly string TempDir = Path.Combine (BaseDir, "tmp/Music/");
	public static readonly string MusicDir = Path.Combine (BaseDir, "Library/Music/");
	public static float Scale = 1f;
	public static Api Api;

	public static bool ShouldRotate {
		get;
		set;
	}

	public static Version CurrentVersion {

#if iOS
		get{return new Version(NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("CFBundleVersion")).ToString());}
#else
		get{return new Version("1.0");}
#endif
	}

	public static void OpenUrl (string httpswwwgooglecomaccountsDisplayUnlockCaptcha)
	{
		throw new NotImplementedException ();
	}

#if iOS
	public static void EnsureInvokedOnMainThread (Action action)
	{
		if (IsMainThread ())
		{
			action ();
			return;
		}
		Invoker.BeginInvokeOnMainThread (() => 
		                                 action()
		                                 );
	}
	
	public static bool IsMainThread() {
		return NSThread.Current.IsMainThread;
		//return Messaging.bool_objc_msgSend(GetClassHandle("NSThread"), new Selector("isMainThread").Handle);
	}
	
	public static NSObject Invoker = new NSObject ();
	public static int BeginBackgroundTask(Action outOfTime)
	{
		return 1;
	}
	public static void EndBackgroundTask(int task)
	{

	}
	public static void PushNetworkActive ()
	{
		lock (networkLock) {
			active++;
			EnsureInvokedOnMainThread (delegate{
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			});
		}
	}
	
	public static void PopNetworkActive ()
	{
		lock (networkLock) {
			active--;
			if (active <= 0) {
				active = 0;
				EnsureInvokedOnMainThread (delegate{
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				});
			}
		}
	}
#elif Droid
	public static void EnsureInvokedOnMainThread (Action action)
	{
		action ();
	}
	public static int BeginBackgroundTask(Action outOfTime)
	{
		return 1;
	}
	public static void EndBackgroundTask(int task)
	{
		
	}
	public static void PushNetworkActive ()
	{

	}
	
	public static void PopNetworkActive ()
	{

	}
#endif
	static object networkLock = new object ();
	static int active;
	public static bool IsStreamCompletedDownloaded;
	public static bool ShouldShowNetwork{
		get{lock(networkLock)
			{
				return active > 0 || !IsStreamCompletedDownloaded;
			}
		}
	}

#if iOS
	public static MainViewController MainVC { get; set; }
#else
	public static IMainViewController MainVC { get; set; }
#endif

	public static void SetCrashlyticsData ()
	{
		//throw new NotImplementedException ();
	}

	public static string GetIndexChar (string name)
	{
		if (name.Length == 0)
			return "#";
		var theChar = name[0];//name.Substring (0, 1).ToUpper ();
		return char.IsLetter(theChar) ? name.Substring(0,1).ToUpper() : "#";
		//return alpha.IndexOf (theChar, StringComparison.OrdinalIgnoreCase) != -1 ? theChar.ToString () : "#";
	}

	public static bool HasInternet {
		get;
		set;
	}
	public static void ShowMessage(string title, string message, string buttonText)
	{

	}
	public static void ShowBlockAlert (string title)
	{

	}
	public static void HideBlockAlert()
	{

	}

	public static void UpdateMpNowPlaying ()
	{
		//throw new NotImplementedException ();
	}

	public static void QueueNext (Action<bool> nextSong)
	{
		//throw new NotImplementedException ();
	}
#if iOS	
	
	public static LevelMeter SongLevelMeter = new LevelMeter (new RectangleF (0, 0, 40, 40)){MeterBars = 10,PaddingForColumns = 4,PaddingForBars = .2f, DrawEmpty = false};
	public static bool IsIpad { get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad; } }
	
	public static bool IsIphone { get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; } }

	public static UIView BaseView { get { return (MainVC as UIViewController).View; } }
	public static bool IsIos6 {
		get {
			var version = new System.Version (UIDevice.CurrentDevice.SystemVersion);
			return version.Major >= 6 ;
		}
	}
	public static UIInterfaceOrientation LastOrientation;
	static bool? isTall;
	public static bool IsTall
	{
		get { 
			if(!isTall.HasValue)
				isTall = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && UIScreen.MainScreen.Bounds.Height* Scale >= 1136;
			return isTall.Value;
		}     
	}
	private static string tallMagic = "-568h@2x";
	public static UIImage FromBundle16x9(string path)
	{
		//adopt the -568h@2x naming convention
		if(IsTall)
		{
			var imagePath = Path.GetDirectoryName(path.ToString());
			var imageFile = Path.GetFileNameWithoutExtension(path.ToString());
			var imageExt = Path.GetExtension(path.ToString());
			imageFile = imageFile + tallMagic + imageExt;
			return UIImage.FromFile(Path.Combine(imagePath,imageFile));
		}
		else 
		{
			return UIImage.FromBundle(path.ToString());
		}
	}
#endif

	
	public static bool ShouldAutoPlay;
	public static void PlayPause ()
	{
		if (Player != null) {
			if (Player.CurrentState == StreamingPlayback.State.Pause)
				Player.Play ();
			else if (Player.CurrentState == StreamingPlayback.State.Playing)
				Player.Pause ();
			else if (Player.CurrentState == StreamingPlayback.State.Stoped && CurrentSong != null)
				Player.PlaySong (CurrentSong);
			else if (CurrentSong != null)
				Player.PlaySong (CurrentSong);
			else
				PlayRandom();

		}
		
		QueueNext(null);

	}
	public static void PlayRandom()
	{
		var count = Database.Main.GetObjects<Song>().Count;
		if(count >0)
		{
			Settings.Random = true;
			var song = Database.Main.GetObjects<Song>()[Util.random.Next(0, count)];
			Util.PlaySong(song,song.ArtistId,song.AlbumId,true);
		}
	}
	
	
	public static void ToggleRandom ()
	{
		
		Settings.Random = !Settings.Random;
		lock (playlistLocker) {
			
			PlayListPlayed.Clear ();	
			NextSongs.Clear ();
			PlayList.Clear ();
			NextSong = null;
			
			if(Settings.ShowOfflineOnly)
				PlayList = PlayListSorted.Where(x=> Database.Main.GetObject<Song>(x).IsLocal).ToList ();
			else
				PlayList = PlayListSorted.Select (x => x).ToList ();
			if(Settings.Random)
				SaveCache(false);
		}
		QueueNext (null);
		
	}
	
	public static void TogleRepeat ()
	{
		//if (CurrentSong != NextSong)
		//	NextSongs.Add (NextSong);
		var nextInt = Settings.RepeatMode + 1;
		if (nextInt > 2)
			nextInt = 0;
		Settings.RepeatMode = nextInt;
		
		//NextSong = null;
		//QueueNext ();
		
		
	}
	
	static DateTime lastPreviousPressed = DateTime.Now;
	public static bool previousIdRunning = false;
	
	public static void Previous ()
	{
		if (previousIdRunning)
			return;
		previousIdRunning = true;
		Task.Factory.StartNew(delegate{
				previous ();

		});
		
	}
	
	public static void previous ()
	{
		try{
			if (CurrentSong == null) {
				previousIdRunning = false;
				return;
			}
			lock (playlistLocker) {
				var diff = (DateTime.Now - lastPreviousPressed).TotalSeconds;
				Console.WriteLine("previous diff: " + diff);
				if ((PlayListPlayed.Count == 0 && (Settings.Random || Settings.ShowOfflineOnly)) || diff >= 5) {
					Util.Seek (0);
					previousIdRunning = false;
					Console.WriteLine("previous rewind");
					lastPreviousPressed = DateTime.Now;
					return;
				}
				Player.Pause();
				Console.WriteLine("Play previous");
				lastPreviousPressed = DateTime.Now;
				if (NextSong != null && !NextSongs.Contains (NextSong.Id) && (Settings.Random || Settings.ShowOfflineOnly))
					NextSongs.Insert (0, NextSong.Id);
				
				if (CurrentSong != null && (Settings.Random || Settings.ShowOfflineOnly))
					NextSongs.Insert (0, CurrentSong.Id);
				/*
		var currentIndex = Songs.IndexOf (Util.CurrentSong);
		if (Songs.Count == currentIndex)
			NextSong = Songs.First ();
		else 
			NextSong = Songs [currentIndex + 1];
				 */
				if (Settings.Random || Settings.ShowOfflineOnly) {
					NextSong = Database.Main.GetObject<Song>(PlayListPlayed.Last ());
					PlayListPlayed.Remove (NextSong.Id);
					
					ThreadPool.QueueUserWorkItem (delegate {
						UpdateCache (CurrentSong.Id);
					});
				} else {
					var index = PlayListSorted.IndexOf (CurrentSong.Id);
					if (index == 0) {
						Util.Seek (0);
						previousIdRunning = false;
						return;
					}
					NextSong = Database.Main.GetObject<Song>(PlayListSorted [index - 1]);
				}
				
				CurrentSong = NextSong;
			}
			NextSong = null;
			Player.PlaySong (CurrentSong);
			//QueueNext (null);
			
			previousIdRunning = false;
		}
		catch(Exception ex)
		{
			previousIdRunning = false;
			Console.WriteLine(ex);
			
		}
		
	}
	

	static void ShowNowPlaying()
	{
		MainVC.ShowNowPlaying ();
	}
	static bool shouldPlayNextSong = false;
	
	public static void Next ()
	{
		Player.Pause();
		if (nextIsRunning)
			return;
		nextIsRunning = true;
		Task.Factory.StartNew(delegate {

				next ();
		});
		
	}
	
	static bool nextIsRunning = false;
	static bool isQueueRunning = false;
	
	static void next ()
	{
		
		Console.WriteLine ("Next");
		if (isQueueRunning && NextSong == null) {
			shouldPlayNextSong = true;
			nextIsRunning = false;
			return;
		}
		shouldPlayNextSong = false;
		if (!isQueueRunning && NextSong == null) {
			QueueNext (hasSong => {
				if (hasSong)
					continueNext ();
				else {
					Player.Pause ();
					nextIsRunning = false;
				}
			});
		} else
			continueNext ();
		
	}
	
	static void continueNext ()
	{
		if (NextSong == null) {
			Util.EnsureInvokedOnMainThread (delegate {
				Util.MainVC.SetState (false);
			});
			nextIsRunning = false;
			return;		
		}
		lock (playlistLocker) {
			if (Settings.Random) {
				PlayListPlayed.Add (CurrentSong.Id);
			}
			
			ThreadPool.QueueUserWorkItem (delegate {
				UpdateCache (CurrentSong.Id);
			});
			CurrentSong = NextSong;
			
			NextSong = null;
		}
		
		
		
		Console.WriteLine ("Playing Next");
		AppRater.DidSomethingSignificant ();
		//Player = new StreamingPlayback ();
		Player.PlaySong (CurrentSong);
		
		QueueNext (null);
		nextIsRunning = false;
	}

	public static void PlaySongNext (Song song)
	{
		if (NextSong != null)
			NextSongs.Insert (0, NextSong.Id);
		
		NextSongs.Insert (0, song.Id);
		lock(playlistLocker)
		{
			if(PlayList.Contains(song.Id))
				PlayList.Remove(song.Id);
			if(PlayListPlayed.Contains(song.Id))
				PlayListPlayed.Remove(song.Id);
		}
		
		//Player.AddSong(song);
		
		NextSong = null;		
		QueueNext (null);
	}
	
	internal static bool PlayAllSongs;
	public static Random random = new Random ();
	
	public static void PlayAlbum (int albumId, bool shuffle)
	{
		try {
			
			FlurryAnalytics.FlurryAnalytics.LogEvent ("Play Album");
			Settings.Random = shuffle;
			lock (playlistLocker) {
				PlayListPlayed.Clear ();
				NextSongs.Clear ();
				NextSong = null;
				
				string song = null;
				//Console.WriteLine ("Getting playlist");
				lock(Database.DatabaseLocker)
					PlayListSorted = Database.Main.Query<Song>("select id from song where AlbumId = ? order by Disc,Track",albumId).Select(x=> x.Id).ToList();
				//PlayListSorted = Util.Songs.Where (x => x.AlbumId == albumId).OrderBy (x => x.Track).OrderBy (x => x.Disc).Select (x => x.Id).ToList ();
				
				if (Settings.ShowOfflineOnly)
					PlayList = PlayListSorted.Where (x => Util.OfflineSongsList.ContainsKey (x) && Util.OfflineSongsList [x]).ToList ();
				else if (shuffle)
					PlayList = PlayListSorted.Select (x => x).ToList ();
				if (shuffle) {
					var songInt = random.Next (0, PlayList.Count - 1);
					song = PlayList [songInt];
				} else
					song = PlayListSorted.First ();
				
				if (Settings.Random || Settings.ShowOfflineOnly) 
					PlayList.Remove (song);
				
				Util.CurrentSong = Database.Main.GetObject<Song>(song);
				
				ThreadPool.QueueUserWorkItem (delegate {
					SaveCache (true);
				});
				
			}
			//playSong (Util.CurrentSong);
			//Downloader.DownloadFileNow(song);
			
			AppRater.DidSomethingSignificant ();
			Player.PlaySong (Util.CurrentSong);
			
			ShowNowPlaying ();
			QueueNext (null);
		} catch (Exception ex) {
			Console.WriteLine ("Error in PlaySong" + ex);	
		}
		
	}
	
	public static void PlaySongFromPlaylist (Song song)
	{
		if (song != null){
			if(song == CurrentSong) {
				if(Player.CurrentlyPlayingSong == song)
					Player.Play();
				else
					Player.PlaySong(song);
				ShowNowPlaying ();
				return;
			}
			else
				Util.CurrentSong = song;
		}
		Task.Factory.StartNew (delegate {
			try {
				lock(playlistLocker)
				{
					if(PlayListPlayed.Contains(song.Id))
						PlayListPlayed.Remove(song.Id);
					if(NextSong == song || !Settings.Random)
						NextSong = null;
					if(NextSongs.Contains(song.Id))
						NextSongs.Remove(song.Id);
					if(PlayList.Contains(song.Id))
						PlayList.Remove(song.Id);
				}
				ThreadPool.QueueUserWorkItem (delegate {
					SaveCache (false);
				});
				AppRater.DidSomethingSignificant ();
				Player.PlaySong (Util.CurrentSong);
				
				ShowNowPlaying ();
				QueueNext (null);
			}
			catch(Exception ex)
			{
				
			}
		});
	}
	
	private static void playSong (Song song)
	{
		
		AppRater.DidSomethingSignificant ();
		//Player = new StreamingPlayback ();
		Player.PlaySong (song);
		
	}
	#if mp3tunes
	//static ClanceysLib.MyMoviePlayer movieView;
	static MPMoviePlayerViewController moviePlayer;
	public static void PlayMovie(Movie movie)
	{
		moviePlayer = new MPMoviePlayerViewController(new NSUrl(movie.PlayUrl));
		AppDelegate.MainVC.ShowMovieController(moviePlayer);
		moviePlayer.MoviePlayer.Play();	
	}
	#endif
	
	public static void Shuffle<T> (this IList<T> list)
	{
		RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider ();
		int n = list.Count;
		while (n > 1) {
			byte[] box = new byte[1];
			do
				provider.GetBytes (box); while (!(box[0] < n * (Byte.MaxValue / n)));
			int k = (box [0] % n);
			n--;
			T value = list [k];
			list [k] = list [n];
			list [n] = value;
		}
	}
	
	public static void PlaySong (Song song, int artistId, int albumId, bool playAllSongs)
	{
		try {
			if (song != null){
				if(song == CurrentSong) {
					if(Player.CurrentlyPlayingSong == song)
						Player.Play();
					else
						Player.PlaySong(song);
					ShowNowPlaying ();
					return;
				}
				else
					Util.CurrentSong = song;
			}
			Task.Factory.StartNew (delegate {
					try {
						Console.WriteLine (song);
						lock (playlistLocker) {
							if (song == null) {
								Console.WriteLine ("Song is null!!!!!!!");
							} else
								Console.WriteLine ("Playing: " + song.Title);
							PlayAllSongs = playAllSongs;
							PlayListPlayed.Clear ();
							NextSongs.Clear ();
							NextSong = null;
							if (!playAllSongs) {
								if (albumId == -1) {
									lock(Database.DatabaseLocker)
									PlayListSorted = Database.Main.Query<Song>("select id from song where ArtistId = ? order by TitleNorm ",artistId).Select(x=> x.Id).ToList();
									FlurryAnalytics.FlurryAnalytics.LogEvent ("Play Artist");
								} else {
								lock(Database.DatabaseLocker)
									PlayListSorted = Database.Main.Query<Song>("select id from song where AlbumId = ? order by Disc,Track",albumId).Select(x=> x.Id).ToList();
									FlurryAnalytics.FlurryAnalytics.LogEvent ("Play Album");
								}
								
								if (Settings.ShowOfflineOnly) {
									PlayList = PlayListSorted.Where (x => Util.OfflineSongsList.ContainsKey (x) && Util.OfflineSongsList [x]).ToList ();
									int index = PlayList.IndexOf (song.Id);
									if(index != -1)
									{
										PlayListPlayed.AddRange (PlayList.GetRange (0, index));
										PlayList.RemoveRange (0, index + 1);
									}
								} else if (Settings.Random)
									PlayList = PlayListSorted.Select (x => x).ToList ();
								if (Settings.Random) {
									if (song == null) {
										var songInt = random.Next (0, PlayList.Count - 1);
										song = Database.Main.GetObject<Song>(PlayList [songInt]);
									}
								} else if (song == null)
									song =  Database.Main.GetObject<Song>(PlayListSorted.First ());
								if (Settings.Random) {
									if (song != null)
										PlayList.Remove (song.Id);
								} 
							} else {
								Util.EnsureInvokedOnMainThread (delegate {
									FlurryAnalytics.FlurryAnalytics.LogEvent ("Play All Songs");
								});
								Console.WriteLine ("shuffleing");
							lock(Database.DatabaseLocker)
								PlayListSorted = Database.Main.Query<Song>("select id from song  Order by TitleNorm ").Select(x => x.Id).ToList();
								
								if (Settings.ShowOfflineOnly)
									PlayList = PlayListSorted.Where (x => Util.OfflineSongsList.ContainsKey (x) && Util.OfflineSongsList [x]).ToList ();
								else if (Settings.Random)
									PlayList = PlayListSorted.Select (x => x).ToList ();
								//if (Settings.Random)
								//	Shuffle<string> (PlayList);
								Console.WriteLine ("shuffleing complete");
							}
							//Downloader.DownloadFileNow(song);
							
							ThreadPool.QueueUserWorkItem (delegate {
								SaveCache (true);
							});
							Util.CurrentSong = song;
						}
						
						AppRater.DidSomethingSignificant ();
						Player.PlaySong (song);
						
						//Util.CurrentSong = song;
						//playSong (song);
						ShowNowPlaying ();
						QueueNext (null);
						
					} catch (Exception ex) {
						Console.WriteLine (ex);
					}

			});
			
		} catch (Exception ex) {
			Console.WriteLine ("Error in PlaySong" + ex);	
		}
	}
	
	public static void PlayGenre (Song song, int genreId)
	{
		try {
			lock (playlistLocker) {
				//Util.CurrentSong = song;
				FlurryAnalytics.FlurryAnalytics.LogEvent ("Play Genre");
				PlayAllSongs = false;

				PlayListSorted = Database.Main.Query<Song>("select id from song where GenreId = ? order TitleNorm",genreId).Select(x=> x.Id).ToList();

				if (Settings.ShowOfflineOnly)
					PlayList = PlayListSorted.Where (x => Util.OfflineSongsList.ContainsKey (x) && Util.OfflineSongsList [x]).ToList ();
				else if (Settings.Random)
					PlayList = PlayListSorted.Select (x => x).ToList ();
				if (song == null) {	
					if (Settings.Random) {
						var songInt = random.Next (0, PlayList.Count - 1);
						song = Database.Main.GetObject<Song>(PlayList [songInt]);
					} else
						song = Database.Main.GetObject<Song>(PlayListSorted.FirstOrDefault ());
				}
				CurrentSong = song;
				//Console.WriteLine ("clear playlist");
				PlayListPlayed.Clear ();
				
				NextSong = null;
				//Console.WriteLine ("clear next songs");			
				NextSongs.Clear ();
				if (Settings.Random && song != null)
					PlayList.Remove (song.Id);
				
				ThreadPool.QueueUserWorkItem (delegate {
					SaveCache (true);
				});
			}

			Player.PlaySong (song);
			
			//Console.WriteLine ("Setting Play State");
			//Util.Invoker.BeginInvokeOnMainThread (delegate {
			//	Util.AppDelegate.MainVC.SetState (true);
			//});
			//Console.WriteLine ("Show now playing");
			ShowNowPlaying ();
			//Console.WriteLine ("Queueing next song");
			QueueNext (null);
		} catch (Exception ex) {
			Console.WriteLine ("Error in PlaySong" + ex);	
		}
		
	}
	
	public static void PlayPlaylist (Song song, GoogleMusic.Playlist playlist)
	{
		Task.Factory.StartNew(delegate {
			playPlaylist (song, playlist);
		});
		
	}
	
	private static void playPlaylist (Song song, GoogleMusic.Playlist playlist)
	{
			try {
				if (song != null)
					Util.CurrentSong = song;
				FlurryAnalytics.FlurryAnalytics.LogEvent ("Play Playlist");
				PlayAllSongs = false;
				lock (playlistLocker) {
					Console.WriteLine ("Playlist" + playlist.ServerId);
					lock (Database.DatabaseLocker) {
						PlayListSorted = Database.Main.Query<Song> ("select SongId as Id from PlaylistSongs where ServerPlaylistId = '" + playlist.ServerId + "'").Select (x => x.Id).ToList ();
					}
					// = Database.Main.Table<Song> ().Where (x => x.GenreId == genreId).OrderBy(x=> x.Title).ToList ();
					
					if (Settings.ShowOfflineOnly)
						PlayList = PlayListSorted.Where (x => Util.OfflineSongsList.ContainsKey (x) && Util.OfflineSongsList [x]).ToList ();
					else if (Settings.Random)
						PlayList = PlayListSorted.Select (x => x).ToList ();
					
					if (song == null) {	
						if (Settings.Random) {
							var songInt = random.Next (0, PlayList.Count - 1);
							song = Database.Main.GetObject<Song>(PlayList [songInt]);
						} else
							song = Database.Main.GetObject<Song>(PlayListSorted.FirstOrDefault ());
					}
					
					Util.CurrentSong = song;
					//Console.WriteLine ("clear playlist");
					PlayListPlayed.Clear ();
					NextSong = null;
					//Console.WriteLine ("clear next songs");			
					NextSongs.Clear ();
					
					
				}
				
				ThreadPool.QueueUserWorkItem (delegate {
					SaveCache (true);
				});
				Player.PlaySong (song);
				//Console.WriteLine ("Setting Play State");
				//Console.WriteLine ("Setting Play State");
				//Util.Invoker.BeginInvokeOnMainThread (delegate {
				//	Util.AppDelegate.MainVC.SetState (true);
				//});
				//Console.WriteLine ("Show now playing");
				ShowNowPlaying ();
				//Console.WriteLine ("Queueing next song");
				QueueNext (null);
				
			} catch (Exception ex) {
				Console.WriteLine ("Error in PlaySong" + ex);	
			}

	}
	public static void ShowLogin()
	{

	}
	public static string FormatTimeSpan (TimeSpan timeSpan)
	{
		//Console.WriteLine("Formating time span to string");
		if (timeSpan.Hours > 0)
			return string.Format ("{0:h\\:mm\\:ss}", timeSpan);
		return string.Format ("{0:m\\:ss}", timeSpan);	
	}
	private static void printListDebug ()
	{
		return;
		#if !DEBUG
		return;	
		#endif
	
	}
	

	static object playlistLocker = new object ();
	internal static List<string> PlayList = new List<string> ();
	static List<string> playListSorted = new List<string> ();
	internal static List<string> PlayListSorted {
		get{return playListSorted;}
		set{
			playListSorted = value;
			EnsureInvokedOnMainThread(delegate{
				MainVC.PlaylistChanged();
			});
		}
	}
	internal static List<string> NextSongs = new List<string> ();
	internal static List<string> PlayListPlayed = new List<string> ();

	public static void UpdatePlaylist (bool notify)
	{
		
		var start = DateTime.Now;
		lock (Database.DatabaseLocker) {			
			Util.PlaylistsList = Database.Main.Table<Playlist> ().Where (x => x.AutoPlaylist == false).OrderBy (x => x.Name).ToList ();
			Util.AutoPlaylists = Database.Main.Table<Playlist> ().Where (x => x.AutoPlaylist == true).OrderBy (x => x.Name).ToList ();
			
		}
		UpdateOfflinePlaylists (false);
		if (notify)
			MainVC.RefreshPlaylist ();
		Console.WriteLine ("Playlists took: " + (DateTime.Now - start).TotalSeconds);
		
	}

	
	internal static void TryLogin()
	{
		if (Util.Api == null && !string.IsNullOrEmpty (Settings.UserName)) {				
			Task.Factory.StartNew(delegate {
				//ThreadPool.QueueUserWorkItem (delegate {
				try {
					//	var cookeisPath = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), Uri.EscapeDataString ("cookies-" + Settings.UserName));
#if gmusic
					if (Util.Api == null)
						Util.Api = new GoogleMusicApi (Settings.UserName);
	#elif amusic
					if (Util.Api == null)
						Util.Api = new AmazonApi (Settings.UserName);
	#elif mp3tunes
					if (Util.Api == null)
						Util.Api = new mp3tunesApi (Settings.UserName);
	#endif
					Util.Api.SignIn (Settings.UserName, Settings.Key, (success) =>{
						if(!success && Util.HasInternet && !Settings.ShowOfflineOnly)
						{
							Util.EnsureInvokedOnMainThread (delegate{
								Util.ShowLogin();
								Util.ShowMessage ("Error".Translate(), "There was an error with your account. Please login again.".Translate(),"Ok".Translate());
							});
						}
					});
					
				} catch (Exception error) {
					if (error.Message.ToString().Contains("BadAuthentication") || error.Message.Contains("Invalid Username/Password")) {
						Util.EnsureInvokedOnMainThread (delegate{
							
							Util.ShowMessage ("Error".Translate(), "There was an error with your account. Please login again.".Translate(),"Ok".Translate());
						});
					}
					Console.WriteLine (error);
				} finally {
					if(Util.ShouldAutoPlay || Settings.AutoPlay)
					{
						if(Util.CurrentSong != null)
						{
							Util.PlayPause();
						}
						else 
							PlayRandom();
					}	
				}
			});
		}		
	}


	static bool hasLoaded;
	public static void LoadData ()
	{
		Console.WriteLine ("loading data");
		if (Database.DatabaseExists (Database.CurrentUser)) {			
			Database.Main.Precache<Song>();
			LoadCache ();
			var currentSongId = Settings.CurrentSongId;
			if (!string.IsNullOrEmpty (currentSongId)) {
				CurrentSong = Database.Main.GetObject<Song>( currentSongId);
				if(!hasLoaded)
				{
					Util.EnsureInvokedOnMainThread (delegate{
						MainVC.SetState (false);
						MainVC.ShowNowPlaying ();
					});
					hasLoaded = true;
				}
			}
		}

		Util.UpdateOfflineSongs (true, false, false);
		Util.MainVC.RefreshSongs ();
		Util.MainVC.RefreshAlbum ();
		Util.MainVC.RefreshArtists ();
		Util.MainVC.RefreshPlaylist ();
		Util.MainVC.RefreshGenre ();
		
		//Console.WriteLine ("Loading from database took :" + (DateTime.Now - startTime).TotalSeconds);
	}

	
	public static void UpdateOfflineSongs (bool includeAll, bool notify, bool resetCurrentPlaying = false)
	{

//		lock (Util.OfflineSongs) {
//			Util.OfflineSongs = Songs.Where (x => x.IsLocal).ToList ();
//			Util.OfflineSongsGrouped = OfflineSongs.OrderBy (x => x.TitleNorm).GroupBy (x => x.IndexCharacter).OrderBy (x => x.Key).ToList ();	
//		}
//
//		if (notify)
//			MainVC.RefreshSongs ();
//		if (!includeAll)
//			return;
//		UpdateOfflineArtist (notify);
//		UpdateOfflineAlbum (notify);
//		#if !mp3tunes
//		UpdateOfflineGenre (notify);
//		#endif
//		UpdateOfflinePlaylists (notify);
//
//		if (resetCurrentPlaying) {
//			PlayListPlayed.Clear ();
//			NextSongs.Clear ();
//			PlayList.Clear ();
//			if (Settings.ShowOfflineOnly) {
//				foreach (var song in PlayListSorted) {
//					if (OfflineSongsList.ContainsKey (song) && OfflineSongsList [song])
//						PlayList.Add (song);
//				}
//				if (!Settings.Random && Util.CurrentSong != null) {
//					var curIndex = PlayList.IndexOf (Util.CurrentSong.Id);
//					if(curIndex != -1)
//					{
//						PlayListPlayed.AddRange (PlayList.GetRange (0, curIndex));
//						PlayList.RemoveRange (0, curIndex + 1);
//					}
//				}
//				//PlayList = PlayListSorted.Where (x => OfflineSongsList.ContainsKey (x) && OfflineSongsList [x]).ToList ();
//			} else if (Settings.Random)
//				PlayList = PlayListSorted.ToList ();
//			if (CurrentSong != null && PlayList.Contains (CurrentSong.Id))
//				PlayList.Remove (CurrentSong.Id);
//			
//			SaveCache (true);
//			Console.WriteLine ("finished reseting");
//			MainVC.SetPlayCount ();
//		}
		
	}
	
	public static void UpdateOfflineArtist (bool notify)
	{
		lock (Util.Artists) {
			lock (Util.OfflineArtists) {
				Util.OfflineArtists = Artists.Where (x => x.OffineCount > 0).ToList ();
				Util.OfflineArtistsGrouped = OfflineArtists.OrderBy (x => x.NormName).GroupBy (x => x.IndexCharacter).OrderBy (x => x.Key).ToList ();	
			}
		}
		
		if (notify)
			MainVC.RefreshArtists ();
	}
	
	public static void UpdateOfflineAlbum (bool notify)
	{
		lock (Util.Albums) {
			lock (Util.OfflineAlbums) {
				Util.OfflineAlbums = Albums.Where (x => x.OffineCount > 0).ToList ();
				Util.OfflineAlbumsGrouped = OfflineAlbums.OrderBy (x => x.NameNorm).GroupBy (x => x.IndexCharacter).OrderBy (x => x.Key).ToList ();	
			}
		}
		
		if (notify)
			MainVC.RefreshAlbum ();
	}
	
	public static void UpdateOfflineGenre (bool notify)
	{
		lock (Util.Genres) {
			lock (Util.OfflineGenres) {
				Util.OfflineGenres = Genres.Where (x => x.OffineCount > 0).ToList ();
				Util.OfflineGenreGroupped = OfflineGenres.OrderBy (x => x.Name).GroupBy (x => x.IndexCharacter).OrderBy (x => x.Key).ToList ();	
			}
		}
		if (notify)
			MainVC.RefreshGenre ();
	}
	
	public static void UpdateOfflinePlaylists (bool notify)
	{
		if (Database.Main == null)
			return;
		lock (Util.PlaylistsList) {
			lock (Util.OfflinePlaylistsList) {
				Util.OfflinePlaylistsList = Database.Main.Table<Playlist> ().Where (x => x.OffineCount > 0 && x.AutoPlaylist != true).ToList ();
			}
		}
		if (notify)
			MainVC.RefreshPlaylist ();
	}
	
	#if mp3tunes
	public static void UpdateMovies(bool notify)
	{
		lock(Database.DatabaseLocker)	
			Util.Movies = Database.Main.Table<Movie> ().OrderBy (x => x.Title).ToList ();
		Util.MoviesGrouped = Movies.GroupBy (x => x.IndexCharacter).ToList ();
		if (notify)
			AppDelegate.MainVC.RefreshMovies();
		
	}
	#endif

	private static void LoadCache ()
	{
		
		
		try{
			lock (Database.DatabaseLocker) {
				NextSongs = Database.Main.Query<stringClass> ("select id from NextSongCache").Select (x => x.id).ToList ();
				PlayListSorted = Database.Main.Query<stringClass> ("select id from PlaylistSortedCache").Select (x => x.id).ToList ();
				if (Settings.Random) {
					PlayListPlayed = Database.Main.Query<stringClass> ("select id from PreviousPlayedCache").Select (x => x.id).ToList ();
					PlayList = PlayListSorted.Select (x => x).Where (x => !PlayListPlayed.Contains (x)).ToList ();
					if(Settings.ShowOfflineOnly)
					{
						//PlayListPlayed = PlayListPlayed.Where(x=> Util.SongsDict.ContainsKey(x) && Util.SongsDict[x].IsLocal).ToList ();
						//PlayList = PlayList.Where(x=> Util.SongsDict.ContainsKey(x) &&  Util.SongsDict[x].IsLocal).ToList ();
					}
				}
				Console.WriteLine (Settings.CurrentSongId);
			}
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex);
		}
	}
	
	private static void SaveCache (bool saveSorted)
	{
		
		lock (Database.DatabaseLocker) {
			Database.Main.Execute ("drop table PreviousPlayedCache");
			Database.Main.Execute ("drop table NextSongCache");
			if(saveSorted){
				Database.Main.Execute ("drop table PlaylistSortedCache");
				Database.Main.CreateTable<PlaylistSortedCache> ();
			}
			Database.Main.CreateTable<PreviousPlayedCache> ();
			Database.Main.CreateTable<NextSongCache> ();
			
			lock (playlistLocker) {
				if ((Settings.Random || Settings.ShowOfflineOnly) && PlayListPlayed.Count > 0) {
					Database.Main.InsertAll (PlayListPlayed.Select (x => new stringClass (){id =x}), typeof(PreviousPlayedCache));
				}
				if (NextSongs.Count > 0)
					Database.Main.InsertAll (NextSongs.Select (x => new stringClass (){id =x}), typeof(NextSongCache));
				if (PlayListSorted.Count > 0 && saveSorted)
					Database.Main.InsertAll (PlayListSorted.Select (x => new stringClass (){id =x}), typeof(PlaylistSortedCache));
			}
		}
	}
	
	private static void UpdateCache (string previous)
	{
		
		Console.WriteLine ("start update cache");
		lock (Database.DatabaseLocker) {
			Database.Main.Execute ("drop table NextSongCache");
			Database.Main.Execute ("drop table PreviousPlayedCache");
			Database.Main.CreateTable<NextSongCache> ();
			Database.Main.CreateTable<PreviousPlayedCache> ();
			lock (playlistLocker) {
				if (NextSongs.Count > 0)
					Database.Main.InsertAll (NextSongs.Select (x => new stringClass (){id =x}), typeof(NextSongCache));
				
				
				if ((Settings.Random || Settings.ShowOfflineOnly) && PlayListPlayed.Count > 0) {
					Database.Main.InsertAll (PlayListPlayed.Select (x => new stringClass (){id =x}), typeof(PreviousPlayedCache));
				}
			}
		}
		Console.WriteLine ("finished update cache");
	}
	
	public static int CurrentSongIndex {
		get{ return (Settings.Random) ? PlayListPlayed.Count + 1 : PlayListSorted.IndexOf (Util.CurrentSong.Id) + 1;}
	}
	
	public static int TotalPlayListCount {
		get {
			if (!Settings.Random && !Settings.ShowOfflineOnly) {
				return PlayListSorted.Count;
			}
			return PlayList.Count + NextSongs.Count + PlayListPlayed.Count + (NextSong == null ? 0 : 1) + (CurrentSong == null ? 0 : 1);// + (Settings.RepeatMode == 2 ? -1 : 0);
		}
	}


}

public struct Tuple<T1, T2>
{
	public readonly T1 Item1;
	public readonly T2 Item2;

	public Tuple (T1 item1, T2 item2)
	{
		Item1 = item1;
		Item2 = item2;
	} 
}


