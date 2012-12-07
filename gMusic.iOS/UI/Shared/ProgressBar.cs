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
using ClanceysLib;

namespace GoogleMusic
{
	public class ProgressBar : UIView
	{
		UIProgressView DownloadProgressView;
		UISlider progressView;
		UILabel currentTimeLabel;
		UILabel remainingTimeLabel;
		public bool Seeking;
		public ProgressBar () 
		{
			Init();
		}
		public ProgressBar(RectangleF rect) : base(rect)
		{
			Init();
		}
		private void Init()
		{
			
			currentTimeLabel = new UILabel();
			currentTimeLabel.TextColor = UIColor.White;
			currentTimeLabel.TextAlignment = UITextAlignment.Right;
			currentTimeLabel.BackgroundColor = UIColor.Clear;
			currentTimeLabel.AdjustsFontSizeToFitWidth = true;
			currentTimeLabel.Font = UIFont.BoldSystemFontOfSize(13);
			this.AddSubview(currentTimeLabel);
			
			remainingTimeLabel = new UILabel();
			remainingTimeLabel.TextColor = UIColor.White;
			remainingTimeLabel.TextAlignment = UITextAlignment.Left;
			remainingTimeLabel.BackgroundColor = UIColor.Clear;
			remainingTimeLabel.AdjustsFontSizeToFitWidth = true;
			remainingTimeLabel.Font = UIFont.BoldSystemFontOfSize(13);
			this.AddSubview(remainingTimeLabel);
			
			DownloadProgressView = new UIProgressView();
			DownloadProgressView.Progress = .0f;//.SetProgress(.0f,false);

#if gmusic
			DownloadProgressView.ProgressTintColor = UIColor.Orange;
#elif mp3tunes
			DownloadProgressView.ProgressTintColor = UIColor.FromRGB(174,198,67);
#endif
		
			progressView = new UISlider();
			progressView.BackgroundColor = UIColor.Clear;

			progressView.MaximumTrackTintColor = UIColor.Clear;

			progressView.TouchUpInside += delegate(object sender, EventArgs e) {
				Console.WriteLine("Touched up ");
				Util.Seek(progressView.Value);
				Seeking = false;
			};
			progressView.TouchDown += delegate(object sender, EventArgs e) {
				Console.WriteLine("Touched down");
				Seeking = true;
			};
			progressView.EditingDidEndOnExit += delegate(object sender, EventArgs e) {
				Console.WriteLine("test");
			};
			progressView.ValueChanged += delegate(object sender, EventArgs e) {
				
			};	
			this.AddSubview(DownloadProgressView);
			this.AddSubview(progressView);
		}
		public override void LayoutSubviews ()
		{
			remainingTimeLabel.Frame = (new RectangleF(this.Bounds.Width - 55,(this.Bounds.Height -35) /2 ,50,35));
			currentTimeLabel.Frame = new RectangleF(0,(this.Bounds.Height -35)/2,50,35);
			DownloadProgressView.Frame = new RectangleF(57,(this.Bounds.Height) /2 -4,this.Bounds.Width - 114,25);
			progressView.Frame = new RectangleF(55,(this.Bounds.Height) /2 - 5,this.Bounds.Width - 110,10);
		}
		public void UpdateStatus (string currentTime, string remainingTime, float percent)
		{
			currentTimeLabel.Text = currentTime;
			remainingTimeLabel.Text = remainingTime;
			//if(!Seeking)
			progressView.SetValue(percent,true);
		}
		public void UpdateDownloadStatus(float percent)
		{
			DownloadProgressView.Progress = percent;	
		}
	}
}

