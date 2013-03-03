using System;
using MonoTouch.CoreMotion;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;


namespace GoogleMusic
{
	public class AnisotropicImage 
	{
		static object locker = new object();
		static CMMotionManager motionManager;
		static NSOperationQueue accelerometerQueue;
		static UIImage baseImage,darkImage,leftImage, rightImage;
		
		static float darkImageRotation = 0;
		static float leftImageRotation = 0;
		static float rightImageRotation = 0;
		public static Action<UIImage> ImageUpdated;
		public static UIImage CurrentImage;
		static AnisotropicImage()
		{
			motionManager = new CMMotionManager ();
			motionManager.AccelerometerUpdateInterval = .025;
			accelerometerQueue = new NSOperationQueue ();
			baseImage = UIImage.FromBundle("base");
			darkImage = UIImage.FromBundle("dark");
			leftImage = UIImage.FromBundle("left");
			rightImage = UIImage.FromBundle("right");
		}
		public AnisotropicImage ()
		{

		}
		static int currentCount = 0;
		public static void StartUpdating()
		{
			lock (locker) {
				currentCount ++;
				if (currentCount > 1)
					return;
			}
			motionManager.StartAccelerometerUpdates (accelerometerQueue, (data,error) => {
				if(error != null)
					return;
				CurrentImage = imageFromAccelerometerData(data);
				if(CurrentImage != null){
					//NSNotificationCenter.DefaultCenter.PostNotificationName("AnisotropicImageUpdate",CurrentImage);
					if(ImageUpdated != null)
						ImageUpdated(CurrentImage);
				}
			});
		}
		public static void StopUpdating()
		{
			lock (locker) {
				currentCount --;
				if (currentCount == 0)
					return;
			}
			motionManager.StopAccelerometerUpdates ();
		}
		static UIImage imageFromAccelerometerData (CMAccelerometerData data)
		{
			var acceleration = data.Acceleration;
			
			var imageSize = baseImage.Size;
			var drawPoint = new PointF (-imageSize.Width/2.0f,
			                                -imageSize.Height/2.0f);

			try {
				UIGraphics.BeginImageContextWithOptions (imageSize, false, 0);
			}
			catch(Exception ex) {
				UIGraphics.BeginImageContext(imageSize);
			}
			float mult1 = .7f;
			float mult2 = .4f;
			var context = UIGraphics.GetCurrentContext();
			context.TranslateCTM(imageSize.Width/2.0f,
			                      imageSize.Height/2.0f);
			baseImage.Draw (drawPoint);

			darkImageRotation = (darkImageRotation * mult1) + ((float)acceleration.X) * mult2;
			context.RotateCTM(darkImageRotation);
			darkImage.Draw (drawPoint);

			leftImageRotation = (leftImageRotation * mult1) + ((float)acceleration.Y - darkImageRotation) * mult2;
			context.RotateCTM(leftImageRotation);
			leftImage.Draw (drawPoint);
			
			rightImageRotation = (rightImageRotation * mult1) + ((float)acceleration.Z - leftImageRotation) * mult2;
			context.RotateCTM(rightImageRotation);
			rightImage.Draw (drawPoint);
			
			var result = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext();
			
			return result;
		}

	}
}

