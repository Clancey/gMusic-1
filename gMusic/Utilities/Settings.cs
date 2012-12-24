using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GoogleMusic;


#if iOS
using MonoTouch.Foundation;
using MonoTouch.Security;
#endif

public static class Settings
{
#if iOS
	private static NSUserDefaults prefs = NSUserDefaults.StandardUserDefaults ;
#elif Droid
	private static class prefs
	{
		private static Dictionary<string,object> currentState;
		private static object locker = new object();
		private static string stateFile = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "state");
		
		public static object GetObject(string key)
		{
			lock(locker)
			{
				loadState();
				if(!currentState.ContainsKey(key))
					return null;
				return currentState[key];
			}
		}

		public static void SetObject(string key,object value)
		{
			lock(locker)
			{
				loadState();
				
				if(currentState.ContainsKey(key))
					currentState[key] = value;
				else
					currentState.Add(key,value);
			}
		}
		
		private static void loadState()
		{
			if(currentState != null)
				return;
			if(!File.Exists(stateFile))
			{
				currentState = new Dictionary<string, object>();
				return;
			}
			
			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(stateFile,FileMode.Open, FileAccess.Read, FileShare.Read)){
				currentState = (Dictionary<string,object>) formatter.Deserialize(stream);
				stream.Close();
			}
			
		}
		private static void saveState()
		{
			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(stateFile,FileMode.Create,FileAccess.Write, FileShare.None))
			{
				formatter.Serialize(stream, currentState);
				stream.Close();
			}
		}
		public static void Synchronize()
		{
			lock(locker)
			{
				saveState();
			}
		}

		public static bool BoolForKey(string key)
		{
			return (bool)(GetObject(key) ?? false);
		}
		public static void SetBool( bool value,string key)
		{
			SetObject (key, value);
		}
		public static string StringForKey(string key)
		{
			return (string)GetObject (key) ?? "";//.ToString();
		}
		public static void SetString(string value, string key)
		{
			SetObject (key, value);
		}
		public static int IntForKey(string key)
		{
			return (int)GetObject (key);
		}
		public static void SetInt(int value, string key)
		{
			SetObject (key, value);
		}
	}
	
	#endif
	public static bool EqualizerEnabled {
		get{ return 

			prefs.BoolForKey ("EqualizerEnabled");
		}
		set {
			prefs.SetBool (value, "EqualizerEnabled");
			prefs.Synchronize ();
		}			
	}
	public static string UserName {
		get { return prefs.StringForKey ("UserName"); }
		set {
			prefs.SetString (value, "UserName");
			prefs.Synchronize ();
		}
	}
	
	public static string Key {
		
#if iOS
		get { 
			var rec = new SecRecord (SecKind.GenericPassword){
				Generic = NSData.FromString ("key"),
				Service = "gMusic",
			};
			
			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord (rec, out res);
			if (res == SecStatusCode.Success)
				return match.ValueData.ToString ();
			else {
				var key = prefs.StringForKey ("key");
				if (!string.IsNullOrEmpty (key)) {
					Key = key;
					prefs.SetString ("", "key");
					prefs.Synchronize ();
				}
				return key;
			}
			
		}
		set {
			var s = new SecRecord (SecKind.GenericPassword) {
				Service = "gMusic",
				Generic = NSData.FromString ("key"),
				ValueData = NSData.FromString (value),
			};
			
			var rec = new SecRecord (SecKind.GenericPassword){
				Service = "gMusic",
				Generic = NSData.FromString ("key"),
				ValueData = NSData.FromString (value),
			};
			
			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord (rec, out res);
			if (res == SecStatusCode.Success) {
				SecKeyChain.Remove(rec);
				//rec.ValueData = NSData.FromString(value);
				//SecKeyChain.Update (rec, s);
			} 

			var err = SecKeyChain.Add (s);
			
		}
#else
		get{return prefs.StringForKey("key");}
		set{prefs.SetString(value,"key");}
#endif
	}
	
	public static string FbAuth {
		get { return prefs.StringForKey ("FbAuth"); }
		set {
			prefs.SetString (value, "FbAuth");
			prefs.Synchronize ();
		}
	}
		
	public static DateTime FbAuthExpire {
		get { return DateTime.Parse (prefs.StringForKey ("fbauthexp")); }
		set {
			prefs.SetString (value.ToString (), "fbauthexp");
			prefs.Synchronize ();
		}
	}
	
	public static bool FbEnabled {
		get{ return prefs.BoolForKey ("FbEnabled");}
		set {
			prefs.SetBool (value, "FbEnabled");
			prefs.Synchronize ();
		}			
	}
	
	public static string CurrentSongId {
		get { return prefs.StringForKey ("CurrentSongId"); }
		set { prefs.SetString (value, "CurrentSongId");}	
	}

	public static string FbId {
		get { return prefs.StringForKey ("FbId"); }
		set { prefs.SetString (value, "FbId");}
	}
	
	public static string LastFmUserName {
		get { return prefs.StringForKey ("LastFmUserName"); }
		set {
			prefs.SetString (value, "LastFmUserName");
			prefs.Synchronize ();
		}
	}
	
	public static string LastFmKey {
		get { return prefs.StringForKey ("LastFmkey"); }
		set {
			prefs.SetString (value, "LastFmkey");
			prefs.Synchronize ();
		}
	}
	
	public static string Auth {
		get { return prefs.StringForKey ("Auth"); }
		set {
			prefs.SetString (value, "Auth");
			prefs.Synchronize ();
		}
	}

	public static string LastFmSession {
		get { return prefs.StringForKey ("LastFmSession"); }
		set {
			prefs.SetString (value, "LastFmSession");
			prefs.Synchronize ();
		}
	}
	
	public static bool LastFmEnabled {
		get{ return prefs.BoolForKey ("LastFmEnabled");}
		set {
			prefs.SetBool (value, "LastFmEnabled");
			prefs.Synchronize ();
		}			
	}
	
	public static bool LastFmLoggedIn {
		get{ return prefs.BoolForKey ("LastFmLoggedIn");}
		set {
			prefs.SetBool (value, "LastFmLoggedIn");
			prefs.Synchronize ();
		}			
	}

	public static bool AutoPlay {
		get{ return prefs.BoolForKey ("AutoPlay");}
		set {
			prefs.SetBool (value, "AutoPlay");
			prefs.Synchronize ();
		}			
	}
	
	public static bool groupingDone {
		get{ return prefs.BoolForKey ("groupingDone");}
		set {
			prefs.SetBool (value, "groupingDone");
			prefs.Synchronize ();
		}			
	}
	
	static object locker = new object ();
	static string continuation = prefs.StringForKey ("ContinuationToken");

	public static string ContinuationToken {
		get {
			lock (locker)
				return continuation;
		}
		set {
			if (continuation == value)
				return;
			lock (locker) {
				continuation = value;
				prefs.SetString (value, "ContinuationToken");
			}
			prefs.Synchronize ();
		}
	}
		
	public static int DatabaseVersion {
		get { return prefs.IntForKey ("dbVersion"); }
		set {
			prefs.SetInt (value, "dbVersion");
			prefs.Synchronize ();
		}
	}
	
	public static string LastUpdateRequest {
		get { return prefs.StringForKey ("LastUpdateRequest"); }
		set {
			prefs.SetString (value, "LastUpdateRequest");
			prefs.Synchronize ();
		}
	}

	public static DateTime LastUpdateCompleted {
		get {
			var date = prefs.StringForKey ("LastUpdateCompleted");
			if (string.IsNullOrEmpty (date))
				return DateTime.MinValue;
			return DateTime.Parse (date);
		}
		set {
			prefs.SetString (value.ToString (), "LastUpdateCompleted");
			prefs.Synchronize ();
		}
			
	}
	
	public static string LastSongsUpdate {
		get { return prefs.StringForKey ("LastSongsUpdate"); }
		set {
			prefs.SetString (value, "LastSongsUpdate");
			prefs.Synchronize ();
		}
	}
	
	public static string LastPlaylistUpdate {
		get { return prefs.StringForKey ("LastPlaylistUpdate"); }
		set {
			prefs.SetString (value, "LastPlaylistUpdate");
			prefs.Synchronize ();
		}
	}
	
	public static int SongsCount {
		get{ 
			lock(Database.DatabaseLocker)
				return Database.Main.ExecuteScalar<int>("select count(*) from Song");
		}	
	}
	
	public static int CurrentSyncSong { get; set; }
	/*{
		get{ return prefs.IntForKey ("CurrentSyncSong");}
		set {
			prefs.SetInt (value, "CurrentSyncSong");
			//prefs.Synchronize ();
		}			
	}
	*/
	public static int CurrentTab {
		get{ return prefs.IntForKey ("CurrentTab");}
		set {
			prefs.SetInt (value, "CurrentTab");
			prefs.Synchronize ();
		}			
	}
	
	public static int AvailableSongs {
		get{ return prefs.IntForKey ("AvailableSongs");}
		set {
			prefs.SetInt (value, "AvailableSongs");
			prefs.Synchronize ();
		}			
	}
	
	public static bool Random {
		get{ return prefs.BoolForKey ("random");}
		set {
			prefs.SetBool (value, "random");
			prefs.Synchronize ();
		}			
	}
	
	public static bool OverrideSync {
		get{ return prefs.BoolForKey ("OverrideSync");}
		set {
			prefs.SetBool (value, "OverrideSync");
			prefs.Synchronize ();
		}			
	}
	
	public static bool HasData {
		get{ return prefs.BoolForKey ("HasData");}
		set {
			prefs.SetBool (value, "HasData");
			prefs.Synchronize ();
		}			
	}
	
	public static int RepeatMode {
		get{ return prefs.IntForKey ("repeatMode");}
		set {
			prefs.SetInt (value, "repeatMode");
			prefs.Synchronize ();
		}			
	}
	
	public static bool ShouldSaveFilesByDefault {
		get{ return prefs.BoolForKey ("ShouldSaveFilesByDefault");}
		set {
			prefs.SetBool (value, "ShouldSaveFilesByDefault");
			prefs.Synchronize ();
		}
	}
	
	public static bool ShowOfflineOnly {
		get{ return prefs.BoolForKey ("ShowOfflineOnly");}
		set {
			prefs.SetBool (value, "ShowOfflineOnly");
			prefs.Synchronize ();
		}
	}
	
	public static bool ProcessedOffline {
		get{ return prefs.BoolForKey ("ProcessedOffline");}
		set {
			prefs.SetBool (value, "ProcessedOffline");
			prefs.Synchronize ();
		}
	}

	static bool? downloadWiFiOnly;

	public static bool DownloadWifiOnly {
		get {
			if (!downloadWiFiOnly.HasValue)
				downloadWiFiOnly = prefs.BoolForKey ("DownloadWifiOnly");
			return downloadWiFiOnly.Value;
		}
		set {
			downloadWiFiOnly = value;
			prefs.SetBool (value, "DownloadWifiOnly");
			prefs.Synchronize ();
		}
	}
	
	public static Version lastResyncVersion {
		get {
			var version = prefs.StringForKey ("lastResyncVersion");
			if (string.IsNullOrEmpty (version))
				return new Version (0, 0);
			return new Version (version);
		}
		set { 
			prefs.SetString (value.ToString (), "lastResyncVersion");
			prefs.Synchronize ();
		}
	}

	public static Version LastVersionUpdateRequired {
		get {
#if mp3tunes
			return new Version(1,0);
#else
			return new Version (2, 3);
#endif
		}
	}
	
#if mp3tunes
	public static string sid {
		get { return prefs.StringForKey ("sid"); }
		set { prefs.SetString (value, "sid");
			prefs.Synchronize ();}
	}
	public static bool HighQuality
	{
		get{return true;}
		//get{return prefs.BoolForKey("HighQuality");}
		set{prefs.SetBool(value,"HighQuality");
			prefs.Synchronize ();}
		
	}
#endif
	
#if amusic
	public static string customer {
		get { return prefs.StringForKey ("customer"); }
		set { prefs.SetString (value, "customer");
			prefs.Synchronize ();}
	}
	public static string device {
		get { return prefs.StringForKey ("device"); }
		set { prefs.SetString (value, "device");
			prefs.Synchronize ();}
	}
	public static string deviceType {
		get { return prefs.StringForKey ("deviceType"); }
		set { prefs.SetString (value, "deviceType");
			prefs.Synchronize ();}
	}
#endif
		
	
}
