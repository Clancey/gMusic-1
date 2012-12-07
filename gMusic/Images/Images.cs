using System;
#if iOS
using MonoTouch.UIKit;
#endif

namespace GoogleMusic
{
	public class Image 
#if iOS
		: UIImage
#endif
	{
		public static Image Load(string path)
		{
#if iOS
			return (Image)UIImage.FromFile (path);
#endif 
			return new Image ();
		}
	}
	public static class Images
	{
		static Image defaultAlbumImage;
		public static Image DefaultAlbumImage
		{
			get{
				if (defaultAlbumImage == null)
					defaultAlbumImage = Image.Load("Images/default_album_large.png");
				return defaultAlbumImage;
			}
		}
	}
}

