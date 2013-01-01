using System;


#if iOS
using MonoTouch.UIKit;
#elif Droid
using Android.Graphics.Drawables;
using Android.Graphics;
#endif

namespace GoogleMusic
{
	public class Image 
#if iOS
		: UIImage
#elif Droid
		: BitmapDrawable
#endif
	{
		public Image(string path) : base(path)
		{

		}
#if Droid
		public Image (Bitmap bitmap) : base(bitmap)
		{

		}
		
		public Image (Android.Content.Res.Resources resource,int id) : base(BitmapFactory.DecodeResource(resource,id))
		{
			
		}
#endif

	}
	public static class Images
	{

		static Image defaultAlbumImage;
		static Image defaultSmallAlbumImage;
		public static Image DefaultAlbumImage
		{
			get{
				if (defaultAlbumImage == null)
					defaultAlbumImage = new Image("Images/default_album_large.png");
				return defaultAlbumImage;
			}
		}

		public static Image DefaultSmallAlbumImage {
			get{
				if (defaultSmallAlbumImage == null)
					defaultSmallAlbumImage = new Image("Images/default_album.png");
				return defaultSmallAlbumImage;
			}
		}

#if Droid
		public static void Init(Android.Content.Res.Resources resource)
		{
			if(defaultAlbumImage == null)				
				defaultAlbumImage = new Image(resource,Resource.Drawable.default_album_large);
			if (defaultSmallAlbumImage == null)
				defaultSmallAlbumImage = new Image(resource,Resource.Drawable.default_album);
		}
#endif



	}
}

