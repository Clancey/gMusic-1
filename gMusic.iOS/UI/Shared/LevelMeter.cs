using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreGraphics;

namespace GoogleMusic
{
	public class LevelMeter : UIView
	{
		public float[] AudioLevelState { get; set; }

		public LevelMeter (RectangleF rect) :base (rect)
		{
			PaddingForColumns = 10;
			MeterBars = 20;
			PaddingForBars = 3;
			DrawEmpty = true;
			this.BackgroundColor = UIColor.Clear;
		}

		public int MeterBars { get; set; }

		public float PaddingForColumns { get; set; }

		public float PaddingForBars { get; set; }

		public bool DrawEmpty { get; set; }

		public override void Draw (RectangleF rect)
		{
			if (AudioLevelState == null || AudioLevelState.Length == 0) {
				base.Draw (rect);
				return;
			}
			
			var width = (this.Frame.Width - PaddingForColumns) / 2;
			var height = (this.Frame.Height);
			var curX = 0f;
			var curY = height;
			
			var context = UIGraphics.GetCurrentContext ();
			context.ClearRect (this.Bounds);
			
			
			int index = 0;
			foreach (var level in AudioLevelState) {
				if (index > 1)
					break;
				var power = level;
				if (power > 1)
					power = 1;
				else if (power < 0)
					power = 0;
				
				
				drawMeter (context, new RectangleF (curX, curY, (width - 2), height), power);
				
				curX += (width + PaddingForColumns);
				index ++;
			}
			context.Flush ();
		}
		
		private void drawMeter (CGContext context, RectangleF rect, float power)
		{
			//Console.WriteLine(power);
			var rectH = (rect.Height) / MeterBars;
			var currentRect = new RectangleF (rect.X, rect.Height, rect.Width, rectH - PaddingForBars);
			float powerEfficient = 1f / MeterBars;
			for (int i = 0; i < MeterBars; i++) {
				var curPower = (float)i * powerEfficient;
				UIColor color;
				if (curPower <= .5f)
					color = UIColor.Green;
				else if (curPower <= .75)
					color = UIColor.Yellow;
				else
					color = UIColor.Red;
				if (curPower < power || DrawEmpty)
					drawRect (context, currentRect, 1f, (curPower < power ? color.CGColor : color.ColorWithAlpha (.5f).CGColor));
				else 
					break;
				currentRect.Y -= rectH;
			}
		}
		
		private void drawRect (CGContext context, RectangleF rect, float radius, CGColor color)
		{
			context.BeginPath ();
			
			context.MoveTo (rect.GetMinX () + radius, rect.GetMinY ());
			context.AddArc (rect.GetMaxX () - radius, rect.GetMinY () + radius, radius, (float)(3 * Math.PI / 2), 0f, false);
			context.AddArc (rect.GetMaxX () - radius, rect.GetMaxY () - radius, radius, 0, (float)(Math.PI / 2), false);
			context.AddArc (rect.GetMinX () + radius, rect.GetMaxY () - radius, radius, (float)(Math.PI / 2), (float)Math.PI, false);
			context.AddArc (rect.GetMinX () + radius, rect.GetMinY () + radius, radius, (float)Math.PI, (float)(3 * Math.PI / 2), false);
			context.ClosePath ();
			context.SetFillColor (color);		
			context.FillPath ();
			
			context.SetStrokeColor (UIColor.DarkGray.CGColor);	
			context.SetLineWidth (.2f);
			context.BeginPath ();
			
			context.MoveTo (rect.GetMinX () + radius, rect.GetMinY ());
			context.AddArc (rect.GetMaxX () - radius, rect.GetMinY () + radius, radius, (float)(3 * Math.PI / 2), 0f, false);
			context.AddArc (rect.GetMaxX () - radius, rect.GetMaxY () - radius, radius, 0, (float)(Math.PI / 2), false);
			context.AddArc (rect.GetMinX () + radius, rect.GetMaxY () - radius, radius, (float)(Math.PI / 2), (float)Math.PI, false);
			context.AddArc (rect.GetMinX () + radius, rect.GetMinY () + radius, radius, (float)Math.PI, (float)(3 * Math.PI / 2), false);
			context.ClosePath ();
		
			context.StrokePath ();
		}
		
	}
}

