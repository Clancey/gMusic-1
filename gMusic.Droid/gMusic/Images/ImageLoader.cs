using System;
using Android.Graphics.Drawables;
using MonoDroid.UrlImageStore;
using System.Security.Cryptography;
using System.Text;

namespace GoogleMusic
{
	public class ImageLoader
	{
		static UrlImageStore imageStore;
		public static Drawable DefaultRequestImage(string url,IImageUpdated imageUpdated)
		{
			if (imageStore == null)
				init ();
			return imageStore.RequestImage (md5(url), url, imageUpdated);

		}
		static void init()
		{
			imageStore = new UrlImageStore(100, "albumArt");
		}
		static MD5CryptoServiceProvider checksum = new MD5CryptoServiceProvider ();
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
		static int hex (int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v - 10;
		}
	}
}

