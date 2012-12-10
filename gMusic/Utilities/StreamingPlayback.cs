using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


#if iOS
using MonoTouch.AudioToolbox;
using MonoTouch.Foundation;
#endif

namespace GoogleMusic
{
	/// <summary>
	/// Wrapper around OutputQueue and AudioFileStream to allow streaming of various filetypes
	/// </summary>
	public class StreamingPlayback : IDisposable
	{
		public double duration {
			get {
				try
				{
					return CurrentlyPlayingSong.Duration / 1000;
				}
				catch
				{
					return 0;	
				}
			}
		}

		public double CurrentTime {
			get { 
				
				double timeInterval = 0;

				if(bassStream == 0)
					return 0;
				var pos = Bass.BASS_ChannelGetPosition(bassStream,BASSMode.BASS_POS_BYTES);

				timeInterval = Bass.BASS_ChannelBytes2Seconds(bassStream,pos);				
				return timeInterval;
			}
		}

		public int bassStream {
			get{ if(CurrentlyPlayingSong == null) return 0;
				return CurrentlyPlayingSong.StreamId; }
			set{ CurrentlyPlayingSong.StreamId = value;}
		}
		
		// level metering


		
		public float[] AudioLevelState {
			get {
				var level = Bass.BASS_ChannelGetLevel(bassStream);
				float left = BassUtil.LowWord32(level)/32768f; // the left level
				float right = BassUtil.HighWord32(level)/32768f;
				//Console.WriteLine(left);
				return new float[]{left,right};
			}
		}
		
		public long Offset { get; set; }

		public float Progress {
			get {
				try{
					if (duration == 0)
						return 0;
					return (float)CurrentTime / (float)duration;
				}
				catch
				{
					return 0;	
				}
			}
		}


		public event EventHandler Finished;
		
		public StreamingPlayback () 
		{
		}

		public void ResetOutputQueue ()
		{
			try
			{


			}
			catch(Exception ex)
			{
				
			};
		}
		
		public void Seek (float percent)
		{
			double seconds = duration * percent;
			double secondsSoFar = CurrentlyPlayingSong.GetDownloadPercent() * duration;
			//Console.WriteLine (length);
			//Console.WriteLine (soFar);

			Bass.BASS_ChannelSetPosition (bassStream, Math.Min(seconds,secondsSoFar));
		}
		
		bool saveCopy;
		Song songForRequest;
		public Stream inputStream;
		Thread parsingThread;
		public Song CurrentlyPlayingSong;
		bool shouldCancel;
		bool hasCanceled;
		static bool isInitialized;

		public void PlaySong(Song song)
		{
			PlaySong(song,Settings.ShouldSaveFilesByDefault);
		}
		static int currentIndex = 0;
		static Dictionary<int,string> songsInt = new Dictionary<int, string> ();
		static Dictionary<int,bool> songShoudSave = new Dictionary<int, bool> ();
		static Dictionary<int,int> songStream = new Dictionary<int, int> ();
		static Dictionary<string,int> songIndex = new Dictionary<string, int>();
		bool isParsing;
		bool isAudioInitialized;
		public void PlaySong (Song song, bool shouldSave)
		{
			//if (isParsing)
			//	return;
			//isParsing = true;
			if (bassStream != 0)
				FlushAndClose ();
			CurrentlyPlayingSong = song;

			
			Util.EnsureInvokedOnMainThread (delegate{
				
				Util.MainVC.UpdateCurrentSongDownloadProgress(0f);
				Util.MainVC.SetState (true);
			});		
			CurrentState = State.Playing;
			saveCopy = shouldSave ? true : (song.ShouldBeLocal && !song.IsLocal);
			//if (OutputQueue != null)
			//	OutputQueue.Stop (true);
			//ResetOutputQueue();
			if (!isInitialized) {
				initBass();

			}
			if (!isAudioInitialized) {
				try{
				//	AudioSession.SetActive (true);
					isAudioInitialized = true;
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			}


			Task.Factory.StartNew (delegate {
				bassStream = setStream (song);
			}).ContinueWith(t=>{
				if (bassStream != 0) {
					_mySync = new SYNCPROC(EndSync);
					Bass.BASS_ChannelSetSync(bassStream, BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_END,0,_mySync,new IntPtr(songIndex [song.Id]));
					Bass.BASS_ChannelPlay (bassStream, false);
#if iOS
					AudioSession.Category = AudioSessionCategory.MediaPlayback;
#endif
				}
				Scrobbler.Main.NowPlaying(song);
				//isParsing = false;
				if(song.DownloadPercent == 1f){
					Util.QueueNext ((nextSong) => {
						setStream (Util.NextSong);
					});
				}
				return t;
			});

		}
		SYNCPROC _mySync;
		[MonoTouch.MonoPInvokeCallback (typeof(SYNCPROC))]
		private static void EndSync(int handle, int channel, int data, IntPtr user)	
		{
			finishedPlaying (Database.Main.GetObject<Song>(songsInt[user.ToInt32()]));
		}
		static object streamLocker = new object();
		public static int setStream(Song song)
		{
			lock (streamLocker) {
				if (songIndex.ContainsKey (song.Id))
					return songStream [songIndex [song.Id]];
				int stream = 0;
				if ((song.IsLocal && File.Exists (Path.Combine (Util.MusicDir, song.FileName))) || (song.IsTemp)) {

					song.PulseDownload (1f);
					Util.MainVC.UpdateCurrentSongDownloadProgress (1f);
					stream = Bass.BASS_StreamCreateFile (Path.Combine (Util.MusicDir, song.FileName), 0, 0, BASSFlag.BASS_STREAM_PRESCAN);
					song.StreamId = stream;
					songStream.Add (currentIndex, stream);
					songIndex.Add (song.Id, currentIndex);
					songsInt.Add (currentIndex, song.Id);
					songShoudSave.Add (currentIndex, false);

				} else {
					try {
						var url = song.PlayUrl;
						songIndex.Add (song.Id, currentIndex);
						songsInt.Add (currentIndex, song.Id);
						stream = Bass.BASS_StreamCreateURL (url, 0, BASSFlag.BASS_STREAM_PRESCAN, callback, new IntPtr (currentIndex));
						//bassStream = Bass.BASS_StreamCreateURL ("http://ccmixter.org/content/bradstanfield/bradstanfield_-_People_Let_s_Stop_The_War.mp3", 0, BASSFlag.BASS_STREAM_PRESCAN, callback, IntPtr.Zero);;
						var info = Bass.BASS_ChannelGetInfo (stream);
						songShoudSave.Add (currentIndex, Settings.ShouldSaveFilesByDefault ? true : song.ShouldBeLocal);
						songStream.Add (currentIndex, stream);
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
				return stream;
			}
		}

		static Dictionary<int,Stream> fileStreams = new Dictionary<int, Stream> ();
#if iOS
		[MonoTouch.MonoPInvokeCallback (typeof(DOWNLOADPROC))]
#endif
		public static void callback(IntPtr buffer, int length, IntPtr user)
		{
#if iOS
			using (new NSAutoreleasePool()) {
#endif
				var id = user.ToInt32 ();
				if (!songStream.ContainsKey (id))
					return;
				var stream = songStream [id];
				var soFar = Bass.BASS_StreamGetFilePosition (stream, BASSStreamFilePosition.BASS_FILEPOS_DOWNLOAD);
				var total = Bass.BASS_StreamGetFilePosition (stream, BASSStreamFilePosition.BASS_FILEPOS_END);
				var percent = (float)soFar / (float)total;
				var song = Database.Main.GetObject<Song>(songsInt [id]);
				song.PulseDownload (percent);
				if (length == 0)
					Util.QueueNext ((nextSong) => {
						setStream (Util.NextSong);
					});
				
				Console.WriteLine("song progress {0} : {1}",song,percent);
				if((!songShoudSave.ContainsKey(id) || !songShoudSave[id]) && !Settings.ShouldSaveFilesByDefault)
					return;

				Stream writeStream = null;
				if(fileStreams.ContainsKey(id))
					writeStream = fileStreams[id];
				var storage = Path.Combine(Util.TempDir , song.FileName);
				if(writeStream == null){
					writeStream = new FileStream (storage, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 4096);
					fileStreams.Add(id,writeStream);
				}
				if(length > 0)
				{
					var data = new byte[length];
					Marshal.Copy(buffer,data,0,length);
					writeStream.Write(data,0,length);
				}
				if(percent == 1 || length == 0) {
					writeStream.Close();
					fileStreams.Remove(id);
					writeStream.Dispose();
					fileStreams.Remove(id);

					if (song.ShouldBeLocal || Settings.ShouldSaveFilesByDefault) {
						File.Copy (Path.Combine(Util.TempDir , song.FileName),Path.Combine ( Util.MusicDir, song.FileName));
						Database.Main.UpdateOffline (song, true);
						song.IsTemp = false;

						ThreadPool.QueueUserWorkItem (delegate{
							Util.UpdateOfflineSongs (true, true);
						});
						try {
							File.Delete (Util.TempDir + song.FileName);
						} catch {
							
						}
					}
				}
#if iOS
			}
#endif
		}
		
		bool doneParsing;
		DateTime lastKeyPressed;
		bool isSearching;

		public State CurrentState{ get; set; }
		
		public enum State
		{
			Stoped,
			Pause,
			Playing
		}
		public void Pause ()
		{
			CurrentState = State.Pause;
			Bass.BASS_ChannelPause (bassStream);
			Util.EnsureInvokedOnMainThread (delegate{
				Util.MainVC.SetState (false);
			});
		}
		
		/// <summary>
		/// Starts the OutputQueue
		/// </summary>
		public void Play ()
		{
			Console.WriteLine (Bass.BASS_ChannelIsActive (bassStream));
			Util.EnsureInvokedOnMainThread (delegate{
				Util.MainVC.SetState (true);
			});
			CurrentState = State.Playing;
			var played = Bass.BASS_ChannelPlay (bassStream,false);
			Console.WriteLine (Bass.BASS_ChannelIsActive (bassStream));
			if(!played)
			{
				initBass();
				
				Bass.BASS_ChannelPlay (bassStream,false);
			}
		}
		private void initBass()
		{
			Bass.BASS_SetConfig(BASSConfig.IOS_MIXAUDIO, 0);
			isInitialized = Bass.BASS_Init (-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
			//Bass.BASS_SetConfig(BASSConfig.
			//Bass.BASS_SetConfig (BASSConfig.BASS_CONFIG_IOS_SPEAKER, 1);
			Bass.BASS_SetConfig(BASSConfig.IOS_MIXAUDIO, 0);
#if iOS
			Bass.SetInturuption (Inturupt);
			try{
				AudioSession.AddListener (AudioSessionProperty.AudioRouteChange, PropertyChanged);
				AudioSession.SetActive (true);
				IntPtr lib = MonoTouch.ObjCRuntime.Dlfcn.dlopen (MonoTouch.Constants.AudioToolboxLibrary, 0);
				
				//AudioRouteChangeReasonKey = new NSString (MonoTouch.ObjCRuntime.Dlfcn.GetIntPtr (lib, "kAudioSession_AudioRouteChangeKey_Reason"));
				//AudioRouteOldRouteKey = new NSString (MonoTouch.ObjCRuntime.Dlfcn.GetIntPtr (lib, "kAudioSession_AudioRouteChangeKey_OldRoute"));
				//AudioRouteOldNotAvailable = new NSString (MonoTouch.ObjCRuntime.Dlfcn.GetIntPtr (lib, "kAudioSessionRouteChangeReason_OldDeviceUnavailable"));

				//lastRoute = AudioSession.InputRoute;
				isAudioInitialized = true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
			
#endif
		}
		enum AudioSessionRouteChangeReason {
			Unknown = 0,
			NewDeviceAvailable = 1,
			OldDeviceUnavailable = 2,
			CategoryChange = 3,
			Override = 4,
			WakeFromSleep = 6,
			NoSuitableRouteForCategory = 7
		}
#if iOS
		[MonoTouch.MonoPInvokeCallback (typeof(NotifyProc))]
		public static void Inturupt(BassIosNotify status)
		{
			var reason = AudioSession.InterruptionType;
			Console.WriteLine (status);
		}
		AudioSessionInputRouteKind lastRoute;
		NSString AudioRouteChangeReasonKey = new NSString("OutputDeviceDidChange_Reason");
		NSString AudioRouteOldRouteKey = new NSString("OutputDeviceDidChange_OldRoute");
		public void PropertyChanged (AudioSessionProperty prop, int size, IntPtr data)
		{
			try {
				var dict = (NSDictionary) MonoTouch.ObjCRuntime.Runtime.GetNSObject (data);
				var reason = (AudioSessionRouteChangeReason)((NSNumber)dict[AudioRouteChangeReasonKey]).Int32Value;
				var oldRoute = dict[AudioRouteOldRouteKey];
				if(reason == AudioSessionRouteChangeReason.OldDeviceUnavailable)
					Pause();
				Console.WriteLine("Changed: {0} , {1} ",oldRoute,reason);

			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
#endif

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose (true);
		}

		#endregion
		
		/// <summary>
		/// Cleaning up all the native Resource
		/// </summary>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				FlushAndClose();
			}
		}
		
		/// <summary>
		/// Saving the decoded Packets to our active Buffer, if the Buffer is full queue it into the OutputQueue
		/// and wait until another buffer gets freed up
		/// </summary>
	
		/// <summary>
		/// Flush the current buffer and close the whole thing up
		/// </summary>
		public void FlushAndClose ()
		{
			Bass.BASS_ChannelStop(bassStream);
			Bass.BASS_StreamFree(bassStream);
			var index = songIndex [CurrentlyPlayingSong.Id];
			CurrentlyPlayingSong.DownloadPercent = 0f;
			CurrentlyPlayingSong.StreamId = 0;
			songsInt.Remove (index);
			songShoudSave.Remove (index);
			songStream.Remove (index);
			songIndex.Remove(CurrentlyPlayingSong.Id);
			bassStream = 0;
		}
		
		private static void finishedPlaying (Song song)
		{
			if(song.Duration > 30000)
			{
				Scrobbler.Main.Scrobble(song);
			}
			
			Util.SongIsOver();// ();
			
		}

		float lastVolume = 1f;
		DateTime lastZeroVolume;
		public void TimeIsOver ()
		{
			try{
				var chan = this.AudioLevelState;
				var volumeL = chan[1];
				var volumeR = chan[0];
				var vol = Math.Max(volumeL,volumeR);
				if(vol == 0 && lastVolume == 0)
				{
					if((DateTime.Now - lastZeroVolume ).TotalSeconds > 1)
					{
						Console.WriteLine("Should have switched songs but didn't!!!");
						finishedPlaying(Util.CurrentSong);
					}
				}
				else{
					lastZeroVolume = DateTime.Now;
					lastVolume = vol;
				}
			}
			catch(Exception ex)
			{
				Console.Write(ex);
			}
		}
	}
}

