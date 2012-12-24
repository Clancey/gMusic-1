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
		public Image(string path)
#if iOS
			: base(path)
#endif
		{

		}
	
	}
	public static class Images
	{

		static Image defaultAlbumImage;
		public static Image DefaultAlbumImage
		{
			get{
				if (defaultAlbumImage == null)
					defaultAlbumImage = new Image("Images/default_album_large.png");
				return defaultAlbumImage;
			}
		}
	}
}

