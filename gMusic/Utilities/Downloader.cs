using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;

#if iOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace GoogleMusic
{
	public static class Downloader
	{
		private static bool _isBusy, _isPolling;
		public static List<Song> remainingFiles = new List<Song> ();
		private static int bgTask = 0;
		private static bool _cancel = false;
		private static bool _ShowProgress = false;
		static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim ();
		//private static NSString invoker = new NSString ("");
		//private static UIApplication app = UIApplication.SharedApplication;

#if iOS
		static UIApplication app = UIApplication.SharedApplication;
#endif

		public static void StartDownload ()
		{
			
			Util.MainViewController.DownloaderUpdated ();
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
		
		private static void poller ()
		{
#if iOS
			using (new NSAutoreleasePool()) {
#endif
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
				
#if iOS
			}
#endif
		}
		
		private static void startDownloading ()
		{
			
#if iOS
			using (NSAutoreleasePool pool = new NSAutoreleasePool ()) {
#endif
				Console.WriteLine ("Starting the downloading process");
				downloadAllFiles ();
				
#if iOS
			}
			
#endif
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
			Util.MainViewController.DownloaderUpdated ();
			if (Remaining == 0 || cancel) {
				downloadComplete ();
				return;
			}
#if iOS
			if (bgTask == 0)
				bgTask = app.BeginBackgroundTask (delegate {
					outOfTime ();
					Console.WriteLine ("Didnt update on time...");
				});
#endif
			var song = getFilePath (0);
			if (CurrentSong != null && !song.Equals (CurrentSong))
				song = CurrentSong;
			downloadFile (song, delegate {
				if (Remaining == 0 || cancel) {
					downloadComplete ();
				} else
					downloadAllFiles ();
				
			});
		}
		
		private static void downloadComplete ()
		{
			Console.WriteLine ("Downloads Complete");
			isBusy = false;
			
			#if iOS
			if(bgTask != 0)
				app.EndBackgroundTask (bgTask);
			#endif
			bgTask = 0;
			Util.MainViewController.DownloaderUpdated ();
		}
		
		static bool needsNotified = false;
		static bool shouldContinue = true;
		
		public static Song CurrentSong { get; private set; }
		
		private static void downloadFile (Song song, Action completed)
		{
			CurrentSong = song;
			Console.WriteLine ("Starting: " + song);
			shouldContinue = true;
			try {
				
				if (File.Exists (Util.MusicDir + song.FileName)) {
					
					Database.Main.UpdateOffline (song, true);
					song.IsTemp = false;
					// TODO: update song using new offline
					//lock (Database.Main)
					//	Database.Main.Update (song);
					
					removeFile (song);
					Console.WriteLine ("Already exists!: " + song);
					Util.readyToPlay (song);
					CurrentSong = null;
					if (completed != null)
						completed ();
					return;
				}
				var reachability = Reachability.InternetConnectionStatus ();
				Console.WriteLine (reachability + " : " + Settings.DownloadWifiOnly);
				if ((Settings.DownloadWifiOnly) && (reachability == NetworkStatus.ReachableViaCarrierDataNetwork && !Settings.ShowOfflineOnly)) {
					var alert = new BlockAlertView ("Error".Translate(), "WIFI only is enabled. Please disable wifi only mode if you want to continue".Translate());
					alert.AddButton("Ok".Translate(),null);
					alert.Show ();
					StopDownloading ();
					return;
				}
				//var fileName = song.Id;
				
				//var dump = File.Create ("/tmp/" + song.Id, 8192);
				
				byte [] buffer = new byte [8192];
				int count;
				var qs = song.QueueStream;
				int tryCount = 0;
				bool success = false;
				shouldContinue = true;
				while (!success && tryCount < 5) {
					Console.WriteLine ("Should continue" + shouldContinue);
					if (!shouldContinue) {
						//StopDownloading ();
						CurrentSong = null;
						if (completed != null)
							completed ();
						Util.failedToDownload (song);
						return;	
						
					}
					if ((SongShouldBeDownloading != null && SongShouldBeDownloading != song)) {
						removeFile (song);					
						if (getFilePath (0) != SongShouldBeDownloading)
							AddFile (SongShouldBeDownloading, 0);
						CurrentSong = null;
						if (completed != null)
							completed ();
						
						return;	
					}
					try {
						success = downloadWithResume (qs, song, qs.ReaderLength, completed);
						Console.WriteLine (success);
						if (tryCount > 5)
							throw new Exception ("Error downloading");
						if (!success)
							tryCount ++;
						
					} catch (Exception ex) {
						tryCount ++;
						if (tryCount > 5)
							throw ex;
						if (Reachability.InternetConnectionStatus () == NetworkStatus.NotReachable)							
							Thread.Sleep (2000);
					}
				}
				if (!success) {
					//removeFile(song);
					Util.failedToDownload (song);
					if (Util.CurrentSong != song)
						qs.Dispose ();
					CurrentSong = null;
					if (completed != null)
						completed ();
					//StopDownloading ();
					return;
				}
				
				song.IsTemp = true;
				if (SongShouldBeDownloading == song)
					SongShouldBeDownloading = null;
				
				if (song.ShouldBeLocal || Settings.ShouldSaveFilesByDefault) {
					File.Copy (Util.TempDir + song.FileName, Util.MusicDir + song.FileName);
					Database.Main.UpdateOffline (song, true);
					song.IsTemp = false;
					//TODO: update new offline
					/*
					lock (Database.Main)
					{
						Database.Main.Update (song);
					}
					*/
					//Database.Main.UpdateOffline(song);
					ThreadPool.QueueUserWorkItem (delegate{
						Util.UpdateOfflineSongs (true, true);
					});
					try {
						File.Delete (Util.TempDir + song.FileName);
					} catch {
						
					}
				}
				
				Console.WriteLine ("Done reading from the web");
				//Remove after download is complete
				removeFile (song);
				
			} catch (Exception ex) {
				//StopDownloading ();
			}
			CurrentSong = null;
			//Tell the other thread your done....
			Console.WriteLine ("Completed: " + song);
			if (completed != null)
				completed ();
			
		}
		static bool alertIsShown;
		private static bool downloadWithResume (QueueStream qs, Song song, long offset, Action completed)
		{
			
			try {
				Util.PushNetworkActive ();
				long totalBytes = offset;
				bool hasCalledPlayed = false;
				var url = song.PlayUrl;
				Console.WriteLine (url);
				if (string.IsNullOrEmpty(url) || url == "ERROR") {
					downloadComplete ();
					shouldContinue = false;
					removeFile(song);
					return false;
				}
				if (offset > 0) {
					float percent = (float)((double)totalBytes / (double)qs.TotalLength);
					song.PulseDownload (percent);
					if (Util.CurrentSong == song) {
						if (percent > .05f && (!hasCalledPlayed || needsNotified)) {
							Console.WriteLine ("Buffer is ready to go!");
							needsNotified = false;
							Util.readyToPlay (song, qs);
							hasCalledPlayed = true;
						}
						//Console.WriteLine (percent);
						Util.AppDelegate.MainVC.UpdateCurrentSongDownloadProgress (percent);
					}	
				}
				var request = Util.Api.GetSongWebRequest (url);
				request.ReadWriteTimeout = (int)(new TimeSpan (0, 0, 50).TotalMilliseconds);
				request.AddRange ((int)offset);
				using (var response = request.GetResponse ()) {
					var webStream = response.GetResponseStream ();
					qs.TotalLength = offset + response.ContentLength;
					var tbuf = new byte [10240];
					int count1;
					//writerInput.AppendSampleBuffer(new CMSampleBuffer(
					
					while ((count1 = webStream.Read (tbuf, 0, tbuf.Length)) != 0) {
						
						if (SongShouldBeDownloading != null && SongShouldBeDownloading != song) {
							Util.PopNetworkActive ();	
							if (qs != null)
								qs.Dispose ();
							removeFile (song);
							return false;	
						}
						if (qs != null) {
							qs.TotalLength = qs.ReaderLength + response.ContentLength;
							qs.Push (tbuf, 0, count1);
						}
						totalBytes += count1;
						float percent = (float)((double)totalBytes / (double)qs.TotalLength);
						if (Util.CurrentSong == song) {
							if (percent > .1f && (!hasCalledPlayed || needsNotified)) {
								Console.WriteLine ("Buffer is ready to go!");
								needsNotified = false;
								Util.readyToPlay (song, qs);
								hasCalledPlayed = true;
							}
							//Console.WriteLine (percent);
							//Util.AppDelegate.MainVC.UpdateCurrentSongDownloadProgress (percent);
						}
						
						song.PulseDownload (percent);
						//Util.AppDelegate.PlayingViewController.CurrentProgress.Progress = (float)(totalBytes / (float) response.ContentLength);
						
					}
					
					song.PulseDownload (1f);
					if (Util.CurrentSong == song) {
						Util.AppDelegate.MainVC.UpdateCurrentSongDownloadProgress (1f);
					}
					if (!hasCalledPlayed || needsNotified) {
						needsNotified = false;
						Util.readyToPlay (song, qs);
					}
				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
				
				Util.PopNetworkActive ();
				if (ex.Message.Contains ("(404) File not found")) {
					Console.WriteLine("Mising song :" + song.ToString()); 
					if(!alertIsShown){
						alertIsShown = true;
						Util.EnsureInvokedOnMainThread (delegate {						
							var alert = new BlockAlertView ("Error".Translate(), "\"" + song.ToString () + "\" " + "was not found on the server.".Translate());
							alert.AddButton("Ok".Translate(),delegate {
								alertIsShown = false;
							});
							alert.Show ();
						});
					}
					shouldContinue = false;
					removeFile (song);
				} else if (Reachability.InternetConnectionStatus () == NetworkStatus.NotReachable)
					shouldContinue = false;
				
				return false;
			} finally {
				
				qs.Done ();
				if (song != Util.CurrentSong) {
					qs.Dispose ();
					qs = null;
				}
				Util.PopNetworkActive ();		
			}
			return true;
			
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
			return;
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
		
		private static Song SongShouldBeDownloading;
		
		public static void DownloadFileNow (Song song)
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
				SongShouldBeDownloading = song;
				StartDownload ();
			} finally {
				_locker.ExitUpgradeableReadLock ();
			}
		}
		
		private static void removeFile (Song song)
		{
			_locker.EnterWriteLock ();
			try {
				remainingFiles.Remove (song);
			} finally {
				_locker.ExitWriteLock ();
			}
		}
		
		private static Song getFilePath (int index)
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
//			var notify = new UILocalNotification ();
//			notify.AlertAction = "Ok";
//			notify.AlertBody = "Your Download is about to be canceled!";
//			notify.HasAction = true;
//			notify.SoundName = UILocalNotification.DefaultSoundName;
//			notify.FireDate = NSDate.Now;
//			notify.TimeZone = NSTimeZone.DefaultTimeZone;
//			
//			//NSDictionary param = NSDictionary.FromObjectsAndKeys(objs,keys);
//			//notify.UserInfo = param;
//			app.ScheduleLocalNotification (notify);
//			
//			Console.WriteLine ("out of time:" + app.BackgroundTimeRemaining);
		}
		
	}
}

