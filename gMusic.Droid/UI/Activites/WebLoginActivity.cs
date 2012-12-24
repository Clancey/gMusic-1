using System;
using Android.App;
using Android.Webkit;
using Android.Content;

namespace GoogleMusic
{
	[Activity (Label = "WebLoginActivity")]	
	public class WebLoginActivity : Activity
	{
		public WebLoginActivity ()
		{
		}
		protected override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RequestWindowFeature(Android.Views.WindowFeatures.Progress);
			
			WebView webview = new WebView(this);
			this.SetContentView(webview);
			webview.Settings.JavaScriptEnabled = true;
			webview.SetWebChromeClient (new Chrome (this));
			webview.SetWebViewClient (new Client (this));
			webview.LoadUrl ("http://play.google.com/music");

		}
		class Chrome : WebChromeClient
		{
			WebLoginActivity Parent;
			public Chrome(WebLoginActivity parent)
			{
				Parent = parent;
			}
			public override void OnProgressChanged (WebView view, int newProgress)
			{
				Parent.SetProgress (newProgress * 100);
				base.OnProgressChanged (view, newProgress);
			}
		}
		class Client : WebViewClient
		{
			WebLoginActivity Parent;
			public Client(WebLoginActivity parent)
			{
				Parent = parent;

			}
			public override void OnPageStarted (WebView view, string url, Android.Graphics.Bitmap favicon)
			{
				
				Console.WriteLine ("********************");
				Console.WriteLine ("Page Started {0}", url);
				Console.WriteLine ("********************");
				//Parent.OnPageStarted (url, favicon);
				if (url.IndexOf ("play.google.com/music/listen", StringComparison.InvariantCultureIgnoreCase) > -1) {
					string sid = "";
					string xt = "";
					CookieSyncManager.Instance.Sync ();
					string cookie = CookieManager.Instance.GetCookie (url);
					if(string.IsNullOrEmpty(cookie))
						return;
					string[] pairs = cookie.Split(";"[0]);
					for (int i = 0; i < pairs.Length; ++i) {
						String[] parts = pairs[i].Split(new char[]{"="[0]},2);
						//Console.WriteLine(pairs[i]);
						// If token is found, return it to the calling activity.
						if(parts.Length == 2)
						{
							if(parts[0].IndexOf("SID", StringComparison.InvariantCultureIgnoreCase) > -1)
								sid = parts[1];
							else if(parts[0].IndexOf("xt", StringComparison.InvariantCultureIgnoreCase) > -1)
								xt = parts[1];
						}
						//					if (parts.Length == 2 &&
						//					    parts[0].equalsIgnoreCase("oauth_token")) {
						//						Intent result = new Intent();
						//						result.putExtra("token", parts[1]);
						//						setResult(RESULT_OK, result);
						//						finish();
						//					}
					}
					if(!string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(xt))
					{
						Intent result = new Intent();
						result.PutExtra("sid", sid);
						result.PutExtra("xt", xt);
						Parent.SetResult(Result.Ok,result);
						Parent.Finish();
					}
				}

				base.OnPageStarted (view, url, favicon);
			}

			public override void OnPageFinished (WebView view, string url)
			{
				Console.WriteLine ("********************");
				Console.WriteLine ("********************");
				Console.WriteLine ("********************");
				Console.WriteLine ("Url :{0}", url);
				Console.WriteLine ("********************");
				Console.WriteLine ("********************");
				Console.WriteLine ("********************");
				base.OnPageFinished (view, url);

			}
		}
	}
}

