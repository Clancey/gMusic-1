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
using MonoTouch.UIKit;
using System.Drawing;


namespace GoogleMusic
{
	public class TVOutBackgroundView : UIView
	{
		UIImageView albumImage;
		UIImageView bg;
		UIView transparentView;
		public TVOutBackgroundView ()
		{
			bg = new UIImageView(new UIImage("Images/bg.png"));
			albumImage = new UIImageView(new RectangleF(0,0,600,600));
			//albumImage.Alpha = .35f;
			albumImage.Image = AlbumImageView.defaultImage;
			transparentView = new UIView (){BackgroundColor = UIColor.Black,Alpha = .8f};
			this.AddSubview(bg);
			this.AddSubview(albumImage);
			this.AddSubview(transparentView);
		}
		public void AlbumArtUpdates(UIImage image)
		{
			albumImage.Image = image;
		}
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var min = Math.Max(Bounds.Width,Bounds.Height);
			albumImage.Frame = new RectangleF(0,0,min,min);
			albumImage.Center = this.Center;
			transparentView.Frame = albumImage.Frame;
			bg.Frame = this.Bounds;
		}
	}
}

