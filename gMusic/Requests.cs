// 
//  Copyright 2012  Xamarin Inc  (http://www.xamarin.com)
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using MonoTouch.ObjCRuntime;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Net;
using System.Linq;
using System.Text;

namespace gMusic
{
	public static class Requests
	{
		static CookieContainer cookies = new CookieContainer ();

		public static Dictionary<string, string> DefaultForm (string url)
		{
			
			
			//var cookies = Util.cookies.GetCookieHeader (new Uri (url));
			return new Dictionary<string,string> () {
				{"accountType","HOSTED_OR_GOOGLE"},
				{"Email","james.clancey@gmail.com"},
				{"service","cl"},
				{"Passwd","Tng4life!"},
				{"source","iis-gMusic-1.5"},
			//{"Accept","*/*"},
			/*{"Accept-Language","en-us"},
					{"Referer","http://music.google.com/music/listen#start_pl"},
					{"Accept-Encoding","gzip, deflate"},
					{"User-Agent","Mozilla/5.0 (iPhone Simulator; U; CPU iPhone OS 4_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8F192 Safari/6533.18.5"},
					{"Host","music.google.com"},
					{"Pragma","no-cache"},
					{"Connection","Keep-Alive"},
					{"Cookie",cookies},
					*/
				};
			
		}

		public static Dictionary<string, string> DefaultHeaders (string url)
		{
			//var cookies = Util.cookies.GetCookieHeader (new Uri (url));
			return new Dictionary<string,string> () {
				{"Accept","*/*"},
				{"Accept-Language","en-us"},
				{"Referer","http://music.google.com/music/listen#start_pl"},
				{"Accept-Encoding","gzip, deflate"},
				{"User-Agent","Mozilla/5.0 (iPhone Simulator; U; CPU iPhone OS 4_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8F192 Safari/6533.18.5"},
				{"Host","music.google.com"},
				{"Pragma","no-cache"},
				{"Connection","Keep-Alive"},
				//{"Cookie",cookies},
			};
		}
		
		public static string HttpsPost (string url, Dictionary<string,string> form, IDictionary<string, string> headers)
		{
			var formEncoded = string.Join ("&", (from i in form 
				select Uri.EscapeDataString (i.Key) + "=" + Uri.EscapeDataString (i.Value)).ToArray ());
			return HttpsPost (url, formEncoded, headers);
			
		}

		public static string HttpsPost (string url, string postData, IDictionary<string, string> headers)
		{
			return sendRequest (url, "POST", postData, headers);
		}

		public static string HttpsGet (string url, string postData, IDictionary<string, string> headers)
		{
			return sendRequest (url, "GET", postData, headers);
		}
		public static string HttpsGet (string url, Dictionary<string,string> form, IDictionary<string, string> headers)
		{
			var formEncoded = string.Join ("&", (from i in form 
				select Uri.EscapeDataString (i.Key) + "=" + Uri.EscapeDataString (i.Value)).ToArray ());
			return HttpsGet (url, formEncoded, headers);
			
		}
		
		public static string HttpsGet1 (string url, string postData, string auth)
		{
			postData = "{\"max-results\": 500,\"start-token\": \"0\"}";
			var content = Encoding.UTF8.GetBytes (postData);
			
			
			var req = (HttpWebRequest)WebRequest.Create ("http://www.googleapis.com//sj/v1beta1/trackfeed");
			req.CookieContainer = cookies;
			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			req.Accept = "*/*";
			req.Headers.Add ("Accept-Encoding", "identity");
			req.Referer = "file:///Applications/Install/CBD04A62-7E65-4A72-86D5-0E5358BF7202/Install/";
			req.Headers.Add (HttpRequestHeader.AcceptEncoding, "identity");
			if (!string.IsNullOrEmpty (auth))
				req.Headers.Add (HttpRequestHeader.Authorization, "GoogleLogin auth=" + auth);
			
			req.ContentLength = content.Length;
			var resp = ReadResponseText (req);
			return resp;
		}
		
		public static string HttpPost (string url, Dictionary<string,string> form, string auth)
		{
			var formEncoded = string.Join ("&", (from i in form 
				select Uri.EscapeDataString (i.Key) + "=" + Uri.EscapeDataString (i.Value)).ToArray ());
			
			var content = Encoding.UTF8.GetBytes (formEncoded);
			
			
			var req = (HttpWebRequest)WebRequest.Create (url);
			req.CookieContainer = cookies;
			req.ContentType = "application/x-www-form-urlencoded";
			req.Accept = "*/*";
			req.Headers.Add ("Accept-Encoding", "identity");
			req.Referer = "file:///Applications/Install/CBD04A62-7E65-4A72-86D5-0E5358BF7202/Install/";
			req.Method = "POST";
			if (!string.IsNullOrEmpty (auth))
				req.Headers.Add ("Authorization", "GoogleLogin auth=" + auth);
			//req.Headers.Add(HttpRequestHeader.Host,"www.googleapis.com");
			req.ContentLength = content.Length;
			using (var s = req.GetRequestStream ()) {
				s.Write (content, 0, content.Length);
			}
			var resp = ReadResponseText (req);
			return resp;
		}

		static string ReadResponseText (HttpWebRequest req)
		{
			using (var resp = (HttpWebResponse)req.GetResponse ()) {
				
				using (var stream = resp.GetResponseStream()) {
					StringBuilder sb = new StringBuilder ();
					Byte[] buf = new byte[8192];
					string tmpString = null;
					int count = 0;
					do {
						count = stream.Read (buf, 0, buf.Length);
						if (count != 0) {
							tmpString = Encoding.ASCII.GetString (buf, 0, count);
							sb.Append (tmpString);
						}
					} while (count > 0);
					return sb.ToString ();
				}
			}
		}

		static string sendRequest (string url, string httpMethod, string postData, IDictionary<string, string> headers)
		{ 
			//Console.WriteLine ("Sending request");
			//Console.WriteLine(url);
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
        new NSString (httpMethod).Handle); 

			//Make a selector to be used twice 
			Selector selSetValueForHttpHeaderField = new Selector ("setValue:forHTTPHeaderField:"); 

			//Set the Content-Length HTTP Header 
			Messaging.void_objc_msgSend_IntPtr_IntPtr (req.Handle, 
        selSetValueForHttpHeaderField.Handle, 
        new NSString (postData.Length.ToString ()).Handle, 
        new NSString ("Content-Length").Handle); 
			if (headers != null)
				foreach (var h in headers) {
					Messaging.void_objc_msgSend_IntPtr_IntPtr (req.Handle, 
		        selSetValueForHttpHeaderField.Handle, 
		        new NSString (h.Value).Handle, 
		        new NSString (h.Key).Handle); 
				}
			
 			
			///
			//Make our c# string into a NSString of our post data 
			NSString sData = new NSString (postData); 

			//Now get NSData from that string using ASCII Encoding 
			NSData pData = new NSData (Messaging.IntPtr_objc_msgSend_int_int (sData.Handle, 
        new Selector ("dataUsingEncoding:allowLossyConversion:").Handle, 1, 1)); 

			//Set the HTTPBody, which is our POST data 
			Messaging.void_objc_msgSend_IntPtr (req.Handle, 
        new Selector ("setHTTPBody:").Handle, 
        pData.Handle); 

			//Need to pass in a reference to the urlResponse object for the next method 
			IntPtr urlRespHandle = IntPtr.Zero; 

			//Send our request Synchronously 
			NSData dataResult = new NSData (Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr_IntPtr (
        new Class ("NSURLConnection").Handle, 
        new Selector ("sendSynchronousRequest:returningResponse:error:").Handle, 
        req.Handle, urlRespHandle, IntPtr.Zero)); 

			//Get the urlResponse object 
			// Get this if you need it 
			//NSUrlResponse urlResp = (NSUrlResponse)Runtime.GetNSObject(urlRespHandle); 

			//Get ourselves a new NSString alloc'd, but not init'd 
			IntPtr resultStrHandle = Messaging.IntPtr_objc_msgSend (new Class ("NSString").Handle, 
        new Selector ("alloc").Handle); 

			//Init the NSString with our response data, and UTF8 encoding 
			resultStrHandle = Messaging.IntPtr_objc_msgSend_IntPtr_int (resultStrHandle, 
        new Selector ("initWithData:encoding:").Handle, dataResult.Handle, 4); 

			//Finally, get our string result 
			return new NSString (resultStrHandle); 
		}

	}
}

