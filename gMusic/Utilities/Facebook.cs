// 
//  Copyright 2012  James Clancey, Xamarin Inc  (http://www.xamarin.com)
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
using System.Globalization;
using System.Net;
using System.Web;
using System.IO;
using System.Threading;

namespace GoogleMusic
{
	
	public static class Facebook
	{
		
		private const string postUrl = "https://graph.facebook.com/me/iosgmusicapp:listen?song={0}&access_token={1}";
		private const string songUrl = "http://www.gmusicapp.com/fb/?title={0}&artist={1}&duration={2}";
		public static void NowPlaying(Song song)
		{
			string formatedSongUrl = string.Format(CultureInfo.InvariantCulture, songUrl, song.Title, song.Artist,song.Duration.ToString());
			string formattedUri = String.Format (CultureInfo.InvariantCulture, postUrl,HttpUtility.UrlEncode(formatedSongUrl), Settings.FbAuth);
			ThreadPool.QueueUserWorkItem(delegate{
				try
				{
				PostToWall(formattedUri);
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			});
		}
		
		public static bool PostToWall (string formattedUri)
		{
			string PostId = "";
			string ErrorMessage = "";
			var webRequest = WebRequest.Create (formattedUri);
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Method = "POST";
			
			
			//formattedUri = formattedUri.AppendQueryString("song", HttpUtility.UrlEncode(songUrl));
			
			
			// Send the request to Facebook, and query the result to get the confirmation code
			try {
				var webResponse = webRequest.GetResponse ();
				StreamReader sr = null;
				try {
					sr = new StreamReader (webResponse.GetResponseStream ());
					PostId = sr.ReadToEnd ();
				} finally {
					if (sr != null)
						sr.Close ();
				}
			} catch (WebException ex) {
				// To help with debugging, we grab the exception stream to get full error details
				StreamReader errorStream = null;
				try {
					errorStream = new StreamReader (ex.Response.GetResponseStream ());
					ErrorMessage = errorStream.ReadToEnd ();
				} finally {
					if (errorStream != null)
						errorStream.Close ();
				}
			}
			return string.IsNullOrEmpty(ErrorMessage);
		}
		
		public static string AppendQueryString (this string url, string key, string value)
		{
			if (url.IndexOf ('?') != -1) {
				url += "&";
			} else {
				url += "?";
			}
			url += key + "=" + value;
			return url;
		}
	}
}

