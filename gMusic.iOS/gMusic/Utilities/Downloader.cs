using System;
using System.Collections.Generic;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.ObjCRuntime;
using System.Net;
using MonoTouch.AVFoundation;
using MonoTouch.CoreMedia;
using MonoTouch.AudioToolbox;
using Un4seen.Bass;
using System.Runtime.InteropServices;

namespace GoogleMusic
{
	public static class Downloader
	{
		private static bool _isBusy, _isPolling;
		public static List<Song> currentlyDownloading = new List<Song> ();
		public static List<Song> remainingFiles = new List<Song> ();
		private static int bgTask = 0;
		private static bool _cancel = false;
		private static bool _ShowProgress = false;
		static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim ();
		private static NSString invoker = new NSString ("");
		private static UIApplication app = UIApplication.SharedApplication;
		
		static int currentIndex = 0;
		public static Dictionary<int,string> songsInt = new Dictionary<int, string> ();
		public static Dictionary<string,int> songIndex = new Dictionary<string, int>();
		public static void StartDownload ()
		{
			
			Util.MainVC.DownloaderUpdated ();
			// If already downloading do nothing...
			if (isBusy)
				return;
			//If the list is empty do nothing...
			if (Remaining == 0) {
				isBusy = false;
				return;
			}
			cancel = false;
			isBusy = true;
			
			Thread thread = new Thread (new ThreadStart (startDownloading));
			thread.Start ();
			Poller ();
			
		}
		
		public static void Poller ()
		{
			if (isPolling)
				return;
			new Thread (new ThreadStart (poller)).Start ();		
		}
		static Dictionary<int,Stream> fileStreams = new Dictionary<int, Stream> ();
		[MonoTouch.MonoPInvokeCallback (typeof(DOWNLOADPROC))]
		public static void callback(IntPtr buffer, int length, IntPtr user)
		{
			using (new NSAutoreleasePool()) {
				var id = user.ToInt32 ();
				if (!songsInt.ContainsKey (id))
					return;
				var song = Database.Main.GetObject<Song>(songsInt [id]);
				if(song == null || string.IsNullOrEmpty(song.Id))
					return;
				var soFar = Bass.BASS_StreamGetFilePosition (song.StreamId, BASSStreamFilePosition.BASS_FILEPOS_DOWNLOAD);
				var total = Bass.BASS_StreamGetFilePosition (song.StreamId, BASSStreamFilePosition.BASS_FILEPOS_END);
				var percent = (float)soFar / (float)total;
				song.PulseDownload (percent);

				
				Console.WriteLine("song progress {0} : {1}",song,percent);
				
				Stream writeStream = null;
				if(fileStreams.ContainsKey(id))
					writeStream = fileStreams[id];
				var storage = Path.Combine(Util.TempDir , song.FileName);
				if(writeStream == null || !writeStream.CanWrite){
					if(File.Exists(storage))
						File.Delete(storage);
					writeStream = new FileStream (storage, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 4096);
					fileStreams.Add(id,writeStream);
				}
				if(length > 0)
				{
					try{
						var data = new byte[length];
						Marshal.Copy(buffer,data,0,length);
						writeStream.Write(data,0,length);
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
				if((percent == 1 && soFar > -1) || length == 0) {
					if(Util.CurrentSong == song)
					{
						Util.QueueNext ((nextSong) => {
							Downloader.DownloadFileNow (Util.NextSong);
						});
					}
					lock(streamLocker)
					{
						if(currentlyDownloading.Contains(song))							
							currentlyDownloading.Remove(song);
					}
					
					removeFile(song);
					if (Remaining == 0 || cancel)
						downloadComplete ();
					else
						downloadAllFiles ();

					try{
						writeStream.Close();
						writeStream.Dispose();
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
					}
					fileStreams.Remove(id);
					writeStream = null;

					if (song.ShouldBeLocal || Settings.ShouldSaveFilesByDefault) {
						File.Copy (storage,Path.Combine ( Util.MusicDir, song.FileName));
						Database.Main.UpdateOffline (song, true);
						song.IsTemp = false;
						
						ThreadPool.QueueUserWorkItem (delegate{
							Util.UpdateOfflineSongs (true, true);
						});
						try {
							if(File.Exists(storage))
								File.Delete (storage);
						} catch {
							
						}
					}
					
					bool shouldKeep = song == Util.CurrentSong || song == Util.NextSong;
					if(shouldKeep)
					{
						song.IsTemp = true;
					}
					else
						StreamingPlayback.FlushSong(song);

				}
				
			}
		}

		private static void poller ()
		{
			using (new NSAutoreleasePool()) {
				while (true) {
					try{
					var connection = Reachability.InternetConnectionStatus ();
					bool reachable = NetworkStatus.NotReachable != connection;// ("http://music.google.com");
					if (reachable && Settings.DownloadWifiOnly)
						reachable = connection == NetworkStatus.ReachableViaWiFiNetwork;
					if(reachable)
							reachable = !Settings.ShowOfflineOnly;
					//Console.WriteLine("Polling:"+ connection);
					if (!isBusy && Remaining > 0 && reachable) {
						Console.WriteLine ("Poller started downloads");
						startDownloading ();
					}
					Thread.Sleep (new TimeSpan (0, 0, 10));
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
						Thread.Sleep (new TimeSpan (0, 0, 10));
					}
				}
			}
		}
		
		private static void startDownloading ()
		{
			//Thread gc...
			using (NSAutoreleasePool pool = new NSAutoreleasePool ()) {
				Console.WriteLine ("Starting the downloading process");
				downloadAllFiles ();
			}
		}
		
		public static int Remaining {
			get {
				_locker.EnterReadLock ();
				try {
					return remainingFiles.Count;
				} finally {
					_locker.ExitReadLock ();
				}
			}
		}
		
		public static bool isBusy {
			get {
				_locker.EnterReadLock ();
				try {
					return _isBusy;
				} finally {
					_locker.ExitReadLock ();
				}
			}
			private set {
				_locker.EnterWriteLock ();
				try {
					_isBusy = value;
				} finally {
					_locker.ExitWriteLock ();
				}
				
			}
		}
		
		public static bool isPolling {
			get {
				_locker.EnterReadLock ();
				try {
					return _isPolling;
				} finally {
					_locker.ExitReadLock ();
				}
			}
			private set {
				_locker.EnterWriteLock ();
				try {
					_isPolling = value;
				} finally {
					_locker.ExitWriteLock ();
				}
				
			}
		}
		
		public static bool ShowProgress {
			get {
				_locker.EnterReadLock ();
				try {
					return _ShowProgress;
				} finally {
					_locker.ExitReadLock ();
				}
			}
			set {
				_locker.EnterWriteLock ();
				try {
					_ShowProgress = value;
				} finally {
					_locker.ExitWriteLock ();
				}
				
			}
		}
		
		private static bool cancel {
			get {
				_locker.EnterReadLock ();
				try {
					return _cancel;
				} finally {
					_locker.ExitReadLock ();
				}
			}
			set {
				_locker.EnterWriteLock ();
				try {
					_cancel = value;
				} finally {
					_locker.ExitWriteLock ();
				}
				
			}
		}
		
		private static void downloadAllFiles ()
		{
			//if(Util.CurrentSong != null)
			//	Util.QueueNext(null);
			Util.MainVC.DownloaderUpdated ();
			if (Remaining == 0 || cancel) {
				downloadComplete ();
				return;
			}
			if (bgTask == 0)
				bgTask = app.BeginBackgroundTask (delegate {
					outOfTime ();
					Console.WriteLine ("Didnt update on time...");
				});
			lock (streamLocker) {
				if(currentlyDownloading.Count > 0)
					return;
			}
			var song = getSong (0);
			if (song == null) {
				removeFile (song);
				downloadAllFiles ();
				return;
			} else if (song.IsLocal || song.IsTemp) {
				removeFile(song);
				downloadAllFiles();
				return;
			}
			setStream (song);
		}
		
		private static void downloadComplete ()
		{
			Console.WriteLine ("Downloads Complete");
			isBusy = false;
			if(bgTask != 0)
				app.EndBackgroundTask (bgTask);
			bgTask = 0;
			Util.MainVC.DownloaderUpdated ();
		}
		
		static bool needsNotified = false;
		static bool shouldContinue = true;
		
		public static Song CurrentSong { get; private set; }
		
		static object streamLocker = new object();
		static bool hasNotified;
		static object notifyLocker = new object ();
		static Song setStream(Song song)
		{
			StreamingPlayback.InitBass ();
			if (song == null)
				return song;
			lock (streamLocker) {
				if (song.StreamId != 0  && songIndex.ContainsKey(song.Id))
				{
					remainingFiles.Remove(song);
					return song;
				}
				else if(songIndex.ContainsKey(song.Id))
				{
					song.StreamId = songIndex[song.Id];
					remainingFiles.Remove(song);
					return song;
				}
				int stream = 0;
				currentIndex ++;
				if ((song.IsLocal && File.Exists (Path.Combine (Util.MusicDir, song.FileName))) || (song.IsTemp)) {
					
					song.PulseDownload (1f);
					Util.MainVC.UpdateCurrentSongDownloadProgress (1f);
					stream = Bass.BASS_StreamCreateFile (Path.Combine (Util.MusicDir, song.FileName), 0, 0, BASSFlag.BASS_STREAM_PRESCAN);
					song.StreamId = stream;
					songIndex.Add (song.Id, currentIndex);
					songsInt.Add (currentIndex, song.Id);
					
				} else {
					
//					var status = Reachability.RemoteHostStatus();
//					if(Settings.DownloadWifiOnly && status ==  NetworkStatus.ReachableViaCarrierDataNetwork)
//					{
//						lock(notifyLocker)
//						{
//							if(hasNotified)
//								return song;
//							hasNotified = true;
//							(new UIAlertView("Unable to Play","WiFi only is enabled.",null,"Ok")).Show();
//						}
//						return song;
//					}

					try {
						var url = song.PlayUrl;
						if(string.IsNullOrEmpty(url))
							return null;
						songIndex.Add (song.Id, currentIndex);
						songsInt.Add (currentIndex, song.Id);
						stream = Bass.BASS_StreamCreateURL (url, 0, BASSFlag.BASS_STREAM_PRESCAN , callback, new IntPtr (currentIndex));
						song.StreamId = stream;
						if(stream != 0)
							currentlyDownloading.Add(song);
						else
							setStream(song);
						//bassStream = Bass.BASS_StreamCreateURL ("http://ccmixter.org/content/bradstanfield/bradstanfield_-_People_Let_s_Stop_The_War.mp3", 0, BASSFlag.BASS_STREAM_PRESCAN, callback, IntPtr.Zero);;
						var info = Bass.BASS_ChannelGetInfo (stream);
					} catch (Exception e) {
						Console.WriteLine (e);
						//status.Text = "Error: " + e.ToString ();
					}
				}
				if (stream != 0) {
					Bass.BASS_ChannelUpdate (stream, 0);
					Bass.BASS_ChannelGetInfo (stream);
				}
				currentIndex ++;
				return song;
			}
		}

		private enum statusType
		{
			Update,
			Error,
			Completed
		}
		
		public static void StopDownloading ()
		{
			isBusy = false;
			cancel = true;
		}
		
		public static void Reset ()
		{
			cancel = true;
			_locker.EnterWriteLock ();
			try {
				remainingFiles.Clear ();
			} finally {
				_locker.ExitWriteLock ();
			}
		}
		
		public static void AddFile (Song song, int index = -1)
		{
			_locker.EnterUpgradeableReadLock ();
			try {
				if (!remainingFiles.Contains (song)) {
					_locker.EnterWriteLock ();
					try {
						if (index == -1)
							remainingFiles.Add (song);
						else
							remainingFiles.Insert (index, song);
					} finally {
						_locker.ExitWriteLock ();
					}
				} else if (index != -1) {
					_locker.EnterWriteLock ();
					try {
						remainingFiles.Insert (index, song);
						remainingFiles.Remove (song);
						
					} finally {
						_locker.ExitWriteLock ();
					}
				}
				StartDownload ();
			} finally {
				_locker.ExitUpgradeableReadLock ();
			}
			
		}

		
		public static Song DownloadFileNow (Song song)
		{
			_locker.EnterUpgradeableReadLock ();
			try {
				if (!remainingFiles.Contains (song)) {
					_locker.EnterWriteLock ();
					try {
						
						remainingFiles.Insert (0, song);
					} finally {
						_locker.ExitWriteLock ();
					}
				} else {
					_locker.EnterWriteLock ();
					try {
						remainingFiles.Remove (song);
						remainingFiles.Insert (0, song);
					} finally {
						_locker.ExitWriteLock ();
					}
				}
				needsNotified = true;
				setStream(song);
				return song;
				StartDownload ();
			} finally {
				_locker.ExitUpgradeableReadLock ();
			}
		}
		
		public static void removeFile (Song song)
		{
			_locker.EnterWriteLock ();
			try {
				remainingFiles.Remove (song);
			} finally {
				_locker.ExitWriteLock ();
			}
		}
		
		private static Song getSong (int index)
		{
			_locker.EnterReadLock ();
			try {
				return remainingFiles [index];
			} finally {
				_locker.ExitReadLock ();
			}
			
		}
		/// <summary>
		/// This will send a local push notification warning the user the download 
		/// is not complete and will be canceled if they dont reopen the app, If
		/// opened on time the download will continue perfectly.
		/// </summary>
		private static void outOfTime ()
		{
			return;
			var notify = new UILocalNotification ();
			notify.AlertAction = "Ok";
			notify.AlertBody = "Your Download is about to be canceled!";
			notify.HasAction = true;
			notify.SoundName = UILocalNotification.DefaultSoundName;
			notify.FireDate = NSDate.Now;
			notify.TimeZone = NSTimeZone.DefaultTimeZone;
			
			//NSDictionary param = NSDictionary.FromObjectsAndKeys(objs,keys);
			//notify.UserInfo = param;
			app.ScheduleLocalNotification (notify);
			
			Console.WriteLine ("out of time:" + app.BackgroundTimeRemaining);
		}
		
	}
}

