using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ClanceysLib
{
	public class StackPanel : UIView
	{
		public StackPanel ()
		{
			init ();
		}
		public StackPanel (RectangleF rect) : base(rect) 
		{
			init ();
		}
		void init()
		{
			this.BackgroundColor = UIColor.Clear;
		}
		float padding;
		public float Padding
		{
			get{ return padding;}
			set{ 
				if (padding == value)
					return;
				padding = value;
				LayoutSubviews ();
			}
		}
		public override void LayoutSubviews ()
		{
			float h = padding;
			var width = this.Bounds.Width - (padding * 2);
			foreach (var view in Subviews) {
				var frame = view.Frame;
				frame.Y = h;
				// You can also set the width to fill!
				//frame.Width = width;
				view.Frame = frame;
				h = frame.Bottom + padding;
			}
		}
	}
}

