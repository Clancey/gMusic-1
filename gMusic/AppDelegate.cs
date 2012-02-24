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
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Text.RegularExpressions;
using SQLite;
using System.IO;
using System.Threading;

namespace gMusic
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
	public static string user = "james.clancey@gmail.com";	
	public static readonly string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString ();
		string serviceRoot = "http://music.google.com/music/services/";
	public static readonly string DatabaseFile = BaseDir + "/Documents/";
		// class-level declarations
		UIWindow window;
		UIWebView webview;
		string url;
		public static SongViewController SongVc;
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			webview = new UIWebView(UIScreen.MainScreen.Bounds);
			window.AddSubview(webview);
			if(Database.DatabaseExists(user))
			{
				var startLoad = DateTime.Now;
				Database.SetDatabase(user);
				Util.Songs = Database.Main.Table<Song>().OrderBy(x=> x.Order).ToList();
				Util.SongsDict = Util.Songs.ToDictionary(x=> x.id,x=> x);
				Util.SongGroups = Database.Main.Table<SongGroup>().OrderBy(x=> x.Index).ToList();
				
				
				Util.ArtistGroups = Database.Main.Table<ArtistGroup>().OrderBy(x=> x.Index).ToList();;
				Util.Artists=  Database.Main.Table<Artist>().OrderBy(x=> x.Order).ToList();
				Util.ArtistsDict = Util.Artists.ToDictionary(x=> x.id,x=> x);
				
				Console.WriteLine("Finished Loading From Database " + (DateTime.Now - startLoad).TotalSeconds);
			}
			SongVc = new SongViewController();
			window.RootViewController = SongVc;
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);
			Api api = new Api();
			
			var PreMyHTML = Requests.HttpsPost("https://accounts.google.com/ServiceLogin?service=sj&passive=1209600&continue=http://music.google.com/music/listen&followup=http://music.google.com/music/listen","",new Dictionary<string,string>());
			String Pattern = "name=\"GALX\"[\\s]*value=\"([a-zA-Z0-9-_\\.]*)\"";
			Match MyMatch = Regex.Match (PreMyHTML, Pattern);
			var GALX = MyMatch.Groups [1].ToString ();
			var cookies = NSHttpCookieStorage.SharedStorage.CookiesForUrl(new NSUrl("https://www.google.com"));
			
			var html = Requests.HttpsPost("https://accounts.google.com/ServiceLogin",new Dictionary<string,string> () {				
				{"ltmpl", "es2st"},
			    {"pstMsg", "1"},
			    {"dnConn", ""},
			    {"service", "sj"},
				{"continue","http://music.google.com/music/listen"},
				{"followup","http://music.google.com/music/listen"},
				//{"dsh", "-2527351217389807091"},
				{"passive", "1209600"},
			    {"hl", "en-US"},
				//{"ltmpl", "es2st"},
			    {"timeStmp", ""},
			    {"secTok", ""},
			    {"GALX", GALX},
			    {"Email", user},
			    {"Passwd", "Tng4life!"},
			    {"PersistentCookie", "yes"},
			    {"rmShown", "1"},
			    {"signIn", "Sign in"},
				{"asts", ""},
			},Requests.DefaultForm(""));
			var html2 = Requests.HttpsGet("http://music.google.com/music/listen","",Requests.DefaultHeaders(""));
			//var cookie = Requests.HttpPost("https://www.google.com/accounts/ClientLogin",Requests.DefaultForm(""),"");
			//cookie = getValue("Auth", cookie);
			//api.GetSong(cookie);
			// make the window visible
			
			start  = DateTime.Now;
			var cookie = NSHttpCookieStorage.SharedStorage.Cookies.Where(x=> x.Name == "xt").FirstOrDefault().Value;
			url = serviceRoot + "loadalltracks?u=0&xt=" + cookie;
			//ThreadPool.QueueUserWorkItem(delegate{loadJson();});
			webview.LoadStarted += delegate(object sender, EventArgs e) {
				
			ThreadPool.QueueUserWorkItem(delegate{loading();});
				//start = DateTime.Now;
			};
			webview.LoadFinished += delegate(object sender, EventArgs e) {
				Console.WriteLine("finished");
				//var div2 = webview.EvaluateJavascript(@"document.documentElement.outerHTML");
				var div = webview.EvaluateJavascript("document.getElementById(\"loadingContainer\").getAttribute('style');");
			Console.WriteLine(div);	
				/*
				WebDatabase.Setup();
			var db = WebDatabase.Main.Table<Databases>().Where(x=> x.name == "GoogleMusic").FirstOrDefault();
				
				File.Copy(WebDatabase.BaseDir + db.origin + "/" + db.path,DatabaseFile + user + "-webDatabases.db",true);
				File.Delete(WebDatabase.BaseDir + db.origin + "/" + db.path);
			Database.SetDatabase(user);
			Indexer.Index();
			
			*/
				//var alert = new UIAlertView("Done","Took" + (DateTime.Now - start).TotalSeconds,null,"Ok");
				//	alert.Show();
				/*
				var db = WebDatabase.Main.Table<Databases>().Where(x=> x.name == "GoogleMusic").FirstOrDefault();
				
				File.Copy(WebDatabase.BaseDir + db.origin + "/" + db.path,DatabaseFile,true);
				File.Delete(WebDatabase.BaseDir + db.origin + "/" + db.path);
				*/
			};
			
			//var alert1 = new UIAlertView("starting","",null,"Ok");
			//alert1.Show();
			webview.LoadRequest(new NSUrlRequest(new NSUrl("http://music.google.com")));
			//webview.
			window.MakeKeyAndVisible ();
			
			return true;
		}
		DateTime start;
		void loading()
		{
			Thread.Sleep(2000);
			var isLoading = true;
			
			var shown = "display: block;";
			while(isLoading)
			{
				this.InvokeOnMainThread(delegate{
					var div = webview.EvaluateJavascript("document.getElementById(\"loadingContainer\").getAttribute('style');").Trim();
					var totalWidth = webview.EvaluateJavascript("document.defaultView.getComputedStyle(loadingProgress,\"\").getPropertyValue(\"width\");").Replace("px","");
					var currentWidth =  webview.EvaluateJavascript("document.defaultView.getComputedStyle(loadingProgress1,\"\").getPropertyValue(\"width\");").Replace("px","");
					double totalWidthD = 0;
					double currentWidthD = 0;
					if(double.TryParse(totalWidth,out totalWidthD) && double.TryParse(currentWidth,out currentWidthD))
					{
					float progress = (float)(currentWidthD/totalWidthD);
						Console.WriteLine(progress);
					}
					isLoading = String.IsNullOrEmpty(div) ? true : div != shown;
				});
				Thread.Sleep(100);
			}
			isLoading = true;
			while(isLoading)
			{
				this.InvokeOnMainThread(delegate{
					var div = webview.EvaluateJavascript("document.getElementById(\"loadingContainer\").getAttribute('style');").Trim();
						var totalWidth = webview.EvaluateJavascript("document.defaultView.getComputedStyle(loadingProgress,\"\").getPropertyValue(\"width\");").Replace("px","");
					var currentWidth =  webview.EvaluateJavascript("document.defaultView.getComputedStyle(loadingProgress1,\"\").getPropertyValue(\"width\");").Replace("px","");
					double totalWidthD = 0;
					double currentWidthD = 0;
					if(double.TryParse(totalWidth,out totalWidthD) && double.TryParse(currentWidth,out currentWidthD))
					{
					float progress = (float)(currentWidthD/totalWidthD);
						Console.WriteLine(progress);
					}
					isLoading = div == shown;
				});
				Thread.Sleep(100);
			}
			
			this.BeginInvokeOnMainThread(delegate{
				var alert = new UIAlertView("Done","Took " + (DateTime.Now - start).TotalSeconds + " Seconds",null,"Ok");
				alert.Show();
				
					Console.WriteLine(webview.EvaluateJavascript("document.getElementById(\"loadingContainer\").getAttribute('style');").Trim());
					Console.WriteLine("Container Width:" + webview.EvaluateJavascript("document.defaultView.getComputedStyle(loadingProgress,\"\").getPropertyValue(\"width\");"));
					Console.WriteLine("Current:" + webview.EvaluateJavascript("document.defaultView.getComputedStyle(loadingProgress1,\"\").getPropertyValue(\"width\");"));
			});
			WebDatabase.Setup();
			var db = WebDatabase.Main.Table<Databases>().Where(x=> x.name == "GoogleMusic").FirstOrDefault();
				
				File.Copy(WebDatabase.BaseDir + db.origin + "/" + db.path,DatabaseFile + user + "-webDatabases.db",true);
				File.Delete(WebDatabase.BaseDir + db.origin + "/" + db.path);
			Database.SetDatabase(user);
			Indexer.Index();
			
		}
		public string getValue (string name, string html)
		{
			var dict = html.Split(new string[]{"\n","="},StringSplitOptions.None).ToList();
			return dict[dict.IndexOf(name) + 1];
		}
	}
}

