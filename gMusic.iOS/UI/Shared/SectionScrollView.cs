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
using System.Threading;
using MonoTouch.Foundation;
using Pioneer;

namespace GoogleMusic
{
	public class SectionScrollView : UIView
	{
		const float width = 190f;
		const float rightPadding = 50f;
		string[] sections;
		int selectedIndex;
		public int SelectedIndex{
			get{return selectedIndex;}
			set{if(selectedIndex == value) return;
				selectedIndex = value;
			}
		}
		UIButton upBtn;
		UIButton downBtn;
		UILabel selectedLabel;
		public string[] Sections{
			get{return sections;}
			set{if(sections == value)
				return;
				sections = value;
				UpdateHeader();
			}
		}
		public SectionScrollView ()
		{
			var frame = new RectangleF(0,0,100,100);
			upBtn = new UIButton(UIButtonType.RoundedRect);
			upBtn.Frame = frame;
			upBtn.SetTitle("^", UIControlState.Normal);
			upBtn.Font = UIFont.BoldSystemFontOfSize(30);
			upBtn.TouchDown += delegate(object sender, EventArgs e) {
				ResetTimer();
				if(selectedIndex <= 0)
					return;
				selectedIndex --;
				UpdateHeader();
				UpdateTableView();
			};
			downBtn = new UIButton(UIButtonType.RoundedRect);
			downBtn.Frame = frame;
			downBtn.SetTitle("v", UIControlState.Normal);
			downBtn.TouchDown += delegate(object sender, EventArgs e) {
				ResetTimer();
				if(selectedIndex >= sections.Length - 1)
					return;
				selectedIndex ++;
				UpdateHeader();
				UpdateTableView();
			};
			downBtn.Font = UIFont.BoldSystemFontOfSize(30);
			selectedLabel = new UILabel(frame);
			selectedLabel.Font = UIFont.BoldSystemFontOfSize(50);
			selectedLabel.TextAlignment = UITextAlignment.Center;
			AddSubview(upBtn);
			AddSubview(downBtn);
			AddSubview(selectedLabel);
			this.BackgroundColor = UIColor.Black.ColorWithAlpha(.5f);


		}
		const float padding = 10f;
		public override void LayoutSubviews ()
		{
			var x = (width - rightPadding)/2;
			var y = Bounds.Height /2;
			selectedLabel.Center = new PointF(x,y);
			upBtn.Center = new PointF(x,y - ((upBtn.Frame.Height ) + padding));
			downBtn.Center = new PointF(x,y + (downBtn.Bounds.Height + padding));
		}
		public override void TouchesMoved (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			return;
			var touch = (UITouch)touches.AnyObject;
			var tapLocation = touch.LocationInView(this);
			Console.WriteLine(tapLocation);
			ResetTimer();
			if(tapLocation.X > Bounds.Width - rightPadding)
				CalculateHeader (tapLocation.Y);
		}
		UITableView TableView;
		public void Show(UITableView tableView)
		{
			UpdateHeader();
			TableView = tableView;
			var parentView = TableView.Superview;
			var frame = TableView.Frame;
			frame.X = frame.Width;
			frame.Width = width;
			Frame = frame;
			parentView.AddSubview (this);
			UIView.Animate(0.5,delegate {
				frame.X -= width;
				Frame = frame;
			},delegate{
				ResetTimer();
			});
		}
		object locker = new object();
		NSObject invoker = new NSObject();
		bool threadRunning = false;
		DateTime LastTouch;
		void ResetTimer()
		{
			lock(locker)
			{
				LastTouch = DateTime.Now;
				if(threadRunning)
					return;
				threadRunning = true;
			}
			ThreadPool.QueueUserWorkItem (delegate{
				runResetLoop ();
			});
			Console.WriteLine("Passed loop start");
		}
		void runResetLoop()
		{
			try{
				bool keepRunning = true;
				while (keepRunning) {

					lock(locker)
					{
						keepRunning = (DateTime.Now - LastTouch).TotalSeconds < 3;
						threadRunning = keepRunning;
					}
					Thread.Sleep (500);
				}
				
				invoker.BeginInvokeOnMainThread(delegate{
					Hide();
				});
			}
			catch(Exception ex)
			{

			}
		}
		public void Hide()
		{
			Console.WriteLine("Hide");
			if(Superview == null)
				return;
			UIView.Animate(0.5,delegate{
				var frame = Frame;
				frame.X += width;
				Frame = frame;
			},delegate{
				this.RemoveFromSuperview();
			});
		}
		public void UpdateHeader()
		{
			string header = "";
			if(sections != null && sections.Length > selectedIndex)
				header = sections[selectedIndex];
			selectedLabel.Text = header;
		}
		public void CalculateHeader(float height)
		{
			var sectionH = Bounds.Height / sections.Length;
			selectedIndex = (int)(height / sectionH);
			UpdateHeader();
			UpdateTableView();
		}

		public void UpdateTableView()
		{
			if(TableView == null)
				return;
			TableView.ScrollToRow(NSIndexPath.FromRowSection(0,selectedIndex), UITableViewScrollPosition.Top,false);
			Console.WriteLine("Tableview updated");
		}
	}
}

