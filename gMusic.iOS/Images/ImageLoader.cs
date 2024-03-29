// Copyright 2010-2011 Miguel de Icaza
//
// Based on the TweetStation specific ImageStore
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog.Utilities;
using System.Security.Cryptography;
using MonoTouch.ObjCRuntime;

namespace GoogleMusic
{
	/// <summary>
	///    This interface needs to be implemented to be notified when an image
	///    has been downloaded.   The notification will happen on the UI thread.
	///    Upon notification, the code should call RequestImage again, this time
	///    the image will be loaded from the on-disk cache or the in-memory cache.
	/// </summary>
	public interface IImageUpdated
	{
		void UpdatedImage (string uri);
	}
	
	/// <summary>
	///   Network image loader, with local file system cache and in-memory cache
	/// </summary>
	/// <remarks>
	///   By default, using the static public methods will use an in-memory cache
	///   for 50 images and 4 megs total.   The behavior of the static methods 
	///   can be modified by setting the public DefaultLoader property to a value
	///   that the user configured.
	/// 
	///   The instance methods can be used to create different imageloader with 
	///   different properties.
	///  
	///   Keep in mind that the phone does not have a lot of memory, and using
	///   the cache with the unlimited value (0) even with a number of items in
	///   the cache can consume memory very quickly.
	/// 
	///   Use the Purge method to release all the memory kept in the caches on
	///   low memory conditions, or when the application is sent to the background.
	/// </remarks>

	public class ImageLoader
	{
		public readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");
		const int MaxRequests = 6;
		static string PicDir; 
		
		// Cache of recently used images
		LRUCache<string,UIImage> cache;
		
		// A list of requests that have been issues, with a list of objects to notify.
		static Dictionary<string, List<IImageUpdated>> pendingRequests;
		
		// A list of updates that have completed, we must notify the main thread about them.
		static HashSet<string> queuedUpdates;
		
		// A queue used to avoid flooding the network stack with HTTP requests
		static Stack<string> requestQueue;
		static NSString nsDispatcher = new NSString ("x");
		static MD5CryptoServiceProvider checksum = new MD5CryptoServiceProvider ();
		
		/// <summary>
		///    This contains the default loader which is configured to be 50 images
		///    up to 4 megs of memory.   Assigning to this property a new value will
		///    change the behavior.   This property is lazyly computed, the first time
		///    an image is requested.
		/// </summary>
		public static ImageLoader DefaultLoader;
		
		static ImageLoader ()
		{
			PicDir = Path.Combine (BaseDir, "Library/Caches/Pictures.MonoTouch.Dialog/");
			
			if (!Directory.Exists (PicDir))
				Directory.CreateDirectory (PicDir);
			
			pendingRequests = new Dictionary<string,List<IImageUpdated>> ();
			queuedUpdates = new HashSet<string> ();
			requestQueue = new Stack<string> ();
		}
		
		/// <summary>
		///   Creates a new instance of the image loader
		/// </summary>
		/// <param name="cacheSize">
		/// The maximum number of entries in the LRU cache
		/// </param>
		/// <param name="memoryLimit">
		/// The maximum number of bytes to consume by the image loader cache.
		/// </param>
		public ImageLoader (int cacheSize, int memoryLimit)
		{
			cache = new LRUCache<string, UIImage> (cacheSize, memoryLimit, sizer);
		}
		
		static int sizer (UIImage img)
		{
			var cg = img.CGImage;
			return cg.BytesPerRow * cg.Height;
		}
		
		/// <summary>
		///    Purges the contents of the DefaultLoader
		/// </summary>
		public static void Purge ()
		{
			if (DefaultLoader != null)
				DefaultLoader.PurgeCache ();
		}
		
		/// <summary>
		///    Purges the cache of this instance of the ImageLoader, releasing 
		///    all the memory used by the images in the caches.
		/// </summary>
		public void PurgeCache ()
		{
			cache.Purge ();
		}
		
		static int hex (int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v - 10;
		}

		static string md5 (string input)
		{
			var bytes = checksum.ComputeHash (Encoding.UTF8.GetBytes (input));
#if mp3tunes
			return ByteArrayToString(bytes); 
#endif
			var ret = new char [32];
			for (int i = 0; i < 16; i++) {
				ret [i * 2] = (char)hex (bytes [i] >> 4);
				ret [i * 2 + 1] = (char)hex (bytes [i] & 0xf);
			}
			return new string (ret);
		}
		static string ByteArrayToString(byte[] arrInput)
		{
			int i;
			StringBuilder sOutput = new StringBuilder(arrInput.Length);
			for (i=0;i < arrInput.Length; i++) 
			{
				sOutput.Append(arrInput[i].ToString("X2"));
			}
			return sOutput.ToString();
		}
		
		/// <summary>
		///   Requests an image to be loaded using the default image loader
		/// </summary>
		/// <param name="uri">
		/// The URI for the image to load
		/// </param>
		/// <param name="notify">
		/// A class implementing the IImageUpdated interface that will be invoked when the image has been loaded
		/// </param>
		/// <returns>
		/// If the image has already been downloaded, or is in the cache, this will return the image as a UIImage.
		/// </returns>
		public static UIImage DefaultRequestImage (string uri, IImageUpdated notify)
		{
			if (DefaultLoader == null)
				DefaultLoader = new ImageLoader (20, 4 * 1024 * 1024);
			return DefaultLoader.RequestImage (uri, notify);
		}
		
		/// <summary>
		///   Requests an image to be loaded from the network
		/// </summary>
		/// <param name="uri">
		/// The URI for the image to load
		/// </param>
		/// <param name="notify">
		/// A class implementing the IImageUpdated interface that will be invoked when the image has been loaded
		/// </param>
		/// <returns>
		/// If the image has already been downloaded, or is in the cache, this will return the image as a UIImage.
		/// </returns>
		public UIImage RequestImage (string uri, IImageUpdated notify)
		{
			UIImage ret;
			
			lock (cache) {
				ret = cache [uri];
				if (ret != null)
					return ret;
			}

			lock (requestQueue) {
				if (pendingRequests.ContainsKey (uri))
					return null;
			}
			var newUri = new Uri(uri);
			string picfile = newUri.IsFile ? newUri.LocalPath : PicDir + md5 (uri);
			if (File.Exists (picfile)) {
				ret = UIImage.FromFile (picfile);
				if (ret != null) {
					lock (cache)
						cache [uri] = ret;
					return ret;
				}
			} 
			if (newUri.IsFile)
				return null;
			QueueRequest (uri, picfile, notify);
			return null;
		}
		
		static void QueueRequest (string uri, string target, IImageUpdated notify)
		{
			if (notify == null)
				throw new ArgumentNullException ("notify");
			
			lock (requestQueue) {
				if (pendingRequests.ContainsKey (uri)) {
					//Util.Log ("pendingRequest: added new listener for {0}", id);
					pendingRequests [uri].Add (notify);
					return;
				}
				var slot = new List<IImageUpdated> (4);
				slot.Add (notify);
				pendingRequests [uri] = slot;
				
				if (picDownloaders >= MaxRequests)
					requestQueue.Push (uri);
				else {
					ThreadPool.QueueUserWorkItem (delegate { 
						try {
							StartPicDownload (uri, target); 
						} catch (Exception e) {
							Console.WriteLine (e);
						}
					});
				}
			}
		}
		
		public static void CopyStream (Stream input, Stream output)
		{
			byte[] buffer = new byte[8 * 1024];
			int len;
			while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
				output.Write (buffer, 0, len);
			}    
		}
		
		static bool Download (string uri, string target)
		{
			var buffer = new byte [4 * 1024];
			
			try {
				var tmpfile = target + ".tmp";
				var data = HttpsGet (uri);
				using (Stream file = File.OpenWrite(tmpfile)) {
					CopyStream (data.AsStream (), file);
				}
				/*
				using (var file = new FileStream (tmpfile, FileMode.Create, FileAccess.Write, FileShare.Read)) {
					
	                	var req = WebRequest.Create (uri) as HttpWebRequest;
					
	                using (var resp = req.GetResponse()) {
						using (var s = resp.GetResponseStream()) {
							int n;
							while ((n = s.Read (buffer, 0, buffer.Length)) > 0){
								file.Write (buffer, 0, n);
	                        }
						}
	                }
	                
					file.w
				}
				*/
				//if(File.Exists(target))
				//	File.Delete(target);
				File.Move (tmpfile, target);
				return true;
			} catch (Exception e) {
				Console.WriteLine ("Problem with {0} {1}", uri, e);
				return false;
			}
		}

		
		
		static NSData HttpsGet (string url)
		{ 
			
			Util.PushNetworkActive();
#if mp3tunes
			url = string.Format(url,Settings.sid);
#endif
			//Instantiate a NSMutableURLRequest 
			IntPtr nsMutableRequestPtr = Messaging.IntPtr_objc_msgSend (new Class ("NSMutableURLRequest").Handle, 
        new Selector ("new").Handle); 

			//Since NSMutableURLRequest subclasses NSUrlRequest, we can use NSURLRequest to work with 
			NSUrlRequest req = (NSUrlRequest)Runtime.GetNSObject (nsMutableRequestPtr); 

			//Set the url of the request 
			Messaging.void_objc_msgSend_IntPtr (req.Handle, new Selector ("setURL:").Handle,
        new NSUrl (url).Handle); 
			
			
			
			//Set the HTTP Method (POST) 
			Messaging.void_objc_msgSend_IntPtr (req.Handle, 
        new Selector ("setHTTPMethod:").Handle, 
        new NSString ("GET").Handle); 

			//Make a selector to be used twice 
			Selector selSetValueForHttpHeaderField = new Selector ("setValue:forHTTPHeaderField:"); 

			//Need to pass in a reference to the urlResponse object for the next method 
			IntPtr urlRespHandle = IntPtr.Zero; 

			//Send our request Synchronously 
			NSData dataResult = new NSData (Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr_IntPtr (
        new Class ("NSURLConnection").Handle, 
        new Selector ("sendSynchronousRequest:returningResponse:error:").Handle, 
        req.Handle, urlRespHandle, IntPtr.Zero)); 

			//Get the urlResponse object 
			// Get this if you need it 
			NSUrlResponse urlResp = (NSUrlResponse)Runtime.GetNSObject (urlRespHandle); 
			
			Util.PopNetworkActive();
			return dataResult;
				
				
			//Get ourselves a new NSString alloc'd, but not init'd 
			IntPtr resultStrHandle = Messaging.IntPtr_objc_msgSend (new Class ("NSString").Handle, 
        new Selector ("alloc").Handle); 

			/*//Init the NSString with our response data, and UTF8 encoding 
			resultStrHandle = Messaging.IntPtr_objc_msgSend_IntPtr_int (resultStrHandle, 
        new Selector ("initWithData:encoding:").Handle, dataResult.Handle, 4); 

			//Finally, get our string result 

			return new NSString (resultStrHandle); 
			*/
		}
		
		static long picDownloaders;
		
		static void StartPicDownload (string uri, string target)
		{
			Interlocked.Increment (ref picDownloaders);
			try {
				_StartPicDownload (uri, target);
			} catch (Exception e) {
				Console.Error.WriteLine ("CRITICAL: should have never happened {0}", e);
			}
			//Util.Log ("Leaving StartPicDownload {0}", picDownloaders);
			Interlocked.Decrement (ref picDownloaders);
		}
		
		static void _StartPicDownload (string uri, string target)
		{
			do {
				bool downloaded = false;
				
				//System.Threading.Thread.Sleep (5000);
				downloaded = Download (uri, target);
				if (!downloaded)
					Console.WriteLine ("Error fetching picture for {0} to {1}", uri, target);
				
				// Cluster all updates together
				bool doInvoke = false;
				
				lock (requestQueue) {
					if (downloaded) {
						queuedUpdates.Add (uri);
					
						// If this is the first queued update, must notify
						if (queuedUpdates.Count == 1)
							doInvoke = true;
					} else
						pendingRequests.Remove (uri);

					// Try to get more jobs.
					if (requestQueue.Count > 0) {
						uri = requestQueue.Pop ();
						if (uri == null) {
							Console.Error.WriteLine ("Dropping request {0} because url is null", uri);
							pendingRequests.Remove (uri);
							uri = null;
						}
					} else {
						//Util.Log ("Leaving because requestQueue.Count = {0} NOTE: {1}", requestQueue.Count, pendingRequests.Count);
						uri = null;
					}
				}	
				if (doInvoke)
					nsDispatcher.BeginInvokeOnMainThread (NotifyImageListeners);
				
			} while (uri != null);
		}
		
		// Runs on the main thread
		static void NotifyImageListeners ()
		{
			lock (requestQueue) {
				foreach (var quri in queuedUpdates) {
					var list = pendingRequests [quri];
					pendingRequests.Remove (quri);
					foreach (var pr in list) {
						try {
							pr.UpdatedImage (quri);
						} catch (Exception e) {
							Console.WriteLine (e);
						}
					}
				}
				queuedUpdates.Clear ();
			}
		}
	}
}