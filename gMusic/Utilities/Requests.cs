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
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;


#if iOS
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
#endif

namespace GoogleMusic
{
	public static class Requests
	{
		
		public static Dictionary<string, string> DefaultForm (string url)
		{
			
			
			//var cookies = Util.cookies.GetCookieHeader (new Uri (url));
			return new Dictionary<string,string> () {
				{"accountType","HOSTED_OR_GOOGLE"},
			//{"Email","james.clancey@gmail.com"},
				{"service","cl"},
			//{"Passwd","Tng4life!"},
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

		public static Dictionary<string, string> DefaultHeaders ()
		{
#if gmusic
			//Util.cookies.GetCookieHeader (new Uri (url));
			var dict = new Dictionary<string,string> () {
				{"Accept","*/*"},
				{"Accept-Language","en-US,en;q=0.8"},
				{"Origin","https://play.google.com"},
				{"Referer","https://play.google.com/music/listen?u=0"},
				{"Accept-Encoding","gzip,deflate,sdch"},
				//{"User-Agent","Mozilla/5.0 (iPhone Simulator; U; CPU iPhone OS 4_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8F192 Safari/6533.18.5"},
				{"Host","play.google.com"},
				{"Pragma","no-cache"},
				{"Connection","Keep-Alive"},
			};
			if(!string.IsNullOrEmpty(Settings.Auth))
				dict.Add("Authorization","GoogleLogin auth=" + Settings.Auth);
#elif mp3tunes

var dict = new Dictionary<string,string> () {
				
			};
			
#endif
			return dict;
		}
		
		public static string HttpsPost (string url, Dictionary<string,string> form, IDictionary<string, string> headers)
		{
			//var formEncoded2 = string.Join ("&", (from i in form 
			//	select i.Key + "=" + i.Value).ToArray ());
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
		
		static string ReadResponseText (HttpWebRequest req)
		{
			using (var resp = (HttpWebResponse)req.GetResponse ()) {
				Util.Api.cookies.Add(resp.Cookies);
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
			var content = Encoding.UTF8.GetBytes (postData);
			var req = CreateGoogleRequest (url);
			req.Method = httpMethod;
			req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			req.ContentLength = content.Length;
			if(httpMethod == "POST")
			using (var s = req.GetRequestStream ()) {
				s.Write (content, 0, content.Length);
			}
			var theReturn = ReadResponseText (req);
			return theReturn;

//#if iOS
//			//Console.WriteLine ("Sending request");
//			//Console.WriteLine(url);
//			//Instantiate a NSMutableURLRequest 
//			IntPtr nsMutableRequestPtr = Messaging.IntPtr_objc_msgSend (new Class ("NSMutableURLRequest").Handle, 
//        new Selector ("new").Handle); 
//
//			//Since NSMutableURLRequest subclasses NSUrlRequest, we can use NSURLRequest to work with 
//			NSUrlRequest req = (NSUrlRequest)Runtime.GetNSObject (nsMutableRequestPtr);
//			//Messaging.void_objc_msgSend_bool_IntPtr (new Class ("NSMutableURLRequest").Handle,
//			//                                  new Selector ("setAllowsAnyHTTPSCertificate:forHost:").Handle, true,new NSString ("https://play.google.com/").Handle); 
//			
//			//Set the url of the request 
//			Messaging.void_objc_msgSend_IntPtr (req.Handle, new Selector ("setURL:").Handle,
//        new NSUrl (url).Handle); 
//			
//			
//			
//			//Set the HTTP Method (POST) 
//			Messaging.void_objc_msgSend_IntPtr (req.Handle, 
//        new Selector ("setHTTPMethod:").Handle, 
//        new NSString (httpMethod).Handle); 
//
//			//Make a selector to be used twice 
//			Selector selSetValueForHttpHeaderField = new Selector ("setValue:forHTTPHeaderField:"); 
//
//			//Set the Content-Length HTTP Header 
//			Messaging.void_objc_msgSend_IntPtr_IntPtr (req.Handle, 
//        selSetValueForHttpHeaderField.Handle, 
//        new NSString (postData.Length.ToString ()).Handle, 
//        new NSString ("Content-Length").Handle); 
//			if (headers != null)
//				foreach (var h in headers) {
//					Messaging.void_objc_msgSend_IntPtr_IntPtr (req.Handle, 
//		        selSetValueForHttpHeaderField.Handle, 
//		        new NSString (h.Value).Handle, 
//		        new NSString (h.Key).Handle); 
//				}
//			
// 			
//			///
//			//Make our c# string into a NSString of our post data 
//			NSString sData = new NSString (postData); 
//
//			//Now get NSData from that string using ASCII Encoding 
//			NSData pData = new NSData (Messaging.IntPtr_objc_msgSend_int_int (sData.Handle, 
//        new Selector ("dataUsingEncoding:allowLossyConversion:").Handle, 1, 1)); 
//
//			//Set the HTTPBody, which is our POST data 
//			Messaging.void_objc_msgSend_IntPtr (req.Handle, 
//        new Selector ("setHTTPBody:").Handle, 
//        pData.Handle); 
//
//			//Need to pass in a reference to the urlResponse object for the next method 
//			IntPtr urlRespHandle = IntPtr.Zero; 
//
//			//Send our request Synchronously 
//			NSData dataResult = new NSData (Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr_IntPtr (
//        new Class ("NSURLConnection").Handle, 
//        new Selector ("sendSynchronousRequest:returningResponse:error:").Handle, 
//        req.Handle, urlRespHandle, IntPtr.Zero)); 
//
//			//Get the urlResponse object 
//			// Get this if you need it 
//			//NSUrlResponse urlResp = (NSUrlResponse)Runtime.GetNSObject(urlRespHandle); 
//
//			//Get ourselves a new NSString alloc'd, but not init'd 
//			IntPtr resultStrHandle = Messaging.IntPtr_objc_msgSend (new Class ("NSString").Handle, 
//        new Selector ("alloc").Handle); 
//
//			//Init the NSString with our response data, and UTF8 encoding 
//			resultStrHandle = Messaging.IntPtr_objc_msgSend_IntPtr_int (resultStrHandle, 
//        new Selector ("initWithData:encoding:").Handle, dataResult.Handle, 4); 
//
//			//Finally, get our string result 
//			return new NSString (resultStrHandle); 
//#endif
			return "";
		}

		static string PostToGoogle (string url, IDictionary<string, string> form)
		{
			/*
			var formEncoded = string.Join ("&", (from i in form 
				select Uri.EscapeDataString (i.Key) + "=" + Uri.EscapeDataString (i.Value)).ToArray ());
			 */
			var content = Encoding.UTF8.GetBytes (url);
			var req = CreateGoogleRequest (url);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			req.ContentLength = content.Length;
			using (var s = req.GetRequestStream ()) {
				s.Write (content, 0, content.Length);
			}
			var theReturn = ReadResponseText (req);
			return theReturn;
		}
		
		public static string Get (string url, string referer = "")
		{
			return ReadResponseText (CreateRequest (url, referer: referer));
		}
		
		public static HttpWebRequest CreateRequest (string url, string referer = "")
		{
			var req = (HttpWebRequest)WebRequest.Create (url);
			//req.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.6) Mono/2.10 (HTML5, like Gecko) GooglePlus/1.0";
			// G+ is picky about User-Agents. So anti-web.
			req.UserAgent = "Mozilla/5.0 (iPhone Simulator; U; CPU iPhone OS 4_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8F192 Safari/6533.18.5";
			req.Accept = "application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5;application/json;";
			req.Headers.Add ("Accept-Charset", "utf-8");
			req.Headers.Add (HttpRequestHeader.AcceptEncoding, "gzip,deflate");
			if(!string.IsNullOrEmpty(Settings.Auth))
				req.Headers.Add("Authorization","GoogleLogin auth=" + Settings.Auth);
			req.AllowAutoRedirect = true;
			req.ProtocolVersion = HttpVersion.Version11;
			req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			req.CookieContainer = Util.Api.cookies;
			if (!string.IsNullOrEmpty (referer)) {
				req.Referer = referer;
			}
			return req;
		}
		
		static HttpWebRequest CreateGoogleRequest (string url)
		{
			var req = (HttpWebRequest)WebRequest.Create (url);
			//req.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.6) Mono/2.10 (HTML5, like Gecko) GooglePlus/1.0";
			// G+ is picky about User-Agents. So anti-web.
			req.UserAgent = "Mozilla/5.0 (iPhone Simulator; U; CPU iPhone OS 4_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8F192 Safari/6533.18.5";
			req.Accept = "*/*";
			req.Headers.Add ("Accept-Charset", "utf-8");
			req.Headers.Add ("Accept-Language", "en-US,en;q=0.8");
			req.Headers.Add("Origin","https://play.google.com");
			//req.Host = "play.google.com";
		//	req.Headers.Add ("Host", "play.google.com");
			if(!string.IsNullOrEmpty(Settings.Auth))
				req.Headers.Add("Authorization","GoogleLogin auth=" + Settings.Auth);


			req.Headers.Add (HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
			req.AllowAutoRedirect = true;
			req.ProtocolVersion = HttpVersion.Version11;
			req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			req.CookieContainer = Util.Api.cookies;
			req.Referer = "http://music.google.com/music/listen";
			req.KeepAlive = true;
			return req;
		}
	}
		
	
}

