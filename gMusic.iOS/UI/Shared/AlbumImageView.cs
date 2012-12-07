using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace GoogleMusic
{
	
	public class AlbumImageView : UIControl,IImageUpdated
	{
		
		public static UIImage defaultImage = UIImage.FromFile ("Images/default_album_large.png");
		UIImage Image;
		UIImageView ImageView;
		UIImageView ReflectedImageView;
		public bool ShouldReflect = true;
		public UISlider Progress;
		
		public AlbumImageView () : this(new RectangleF(0,0,320,480),true)
		{
			
		}
		
		public AlbumImageView (RectangleF rectangle, bool shouldReflect) : base(rectangle)
		{
			ShouldReflect = shouldReflect;
			this.BackgroundColor = UIColor.Clear;
			var rect = new RectangleF (0, 0, rectangle.Width, rectangle.Width);
			if (shouldReflect) {
				ReflectedImageView = new UIImageView (new RectangleF (0, 2, rectangle.Width, rectangle.Width));
				ReflectedImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
				ReflectedImageView.Alpha = .4f;
				ReflectedImageView.Transform = new CGAffineTransform (1f, 0f, 0f, -1f, 0f, rectangle.Width);
				
				this.AddSubview (ReflectedImageView);
			}
			
			ImageView = new UIImageView (rect);
			ImageView.Image = defaultImage;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			this.AddSubview (ImageView);
		}
		
		//MPMediaItemArtwork mediaArt;
		
		public void SetImage (UIImage image)
		{
			//if (Util.IsIos5) {
			if(Util.CurrentSong != null && Util.CurrentSong.AlbumImage == image){	
				//var mediaArt = new MPMediaItemArtwork (image);				
				//MPNowPlayingInfoCenter.AlbumArt = mediaArt;
				//var nowPlaying = Util.NowPlaying;
				//nowPlaying.Artwork = mediaArt;
				//Util.UpdateMPNowPlaying(nowPlaying);
				
				//}
			}
			Image = image;
			if (ShouldReflect)
				ReflectedImageView.Image = image;
			ImageView.Image = Image;
		}
		
		public void SetImage (string url)
		{
			try {
				if (string.IsNullOrEmpty (url))
					SetImage (defaultImage);
				else
					SetImage (ImageLoader.DefaultLoader.RequestImage (url, this) ?? defaultImage);
			} catch (Exception ex) {
				Console.WriteLine ("Error in set image" + ex);	
			}
		}
		
				#region IImageUpdated implementation
		void IImageUpdated.UpdatedImage (string uri)
		{
			SetImage (ImageLoader.DefaultLoader.RequestImage (uri, this) ?? defaultImage);
		}
		#endregion
	}
}

