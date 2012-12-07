using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace GoogleMusic
{
	public class OBSlider : UISlider
	{
		public OBSlider ()
		{
			initialize ();
		}
		public OBSlider(RectangleF rect) : base(rect)
		{
			initialize ();
		}
		public float ScrubbingSpeed { get; private set; }
		public float[] ScrubbingSpeedChangePositions { get; set; }
		public float[] ScrubbingSpeeds {get;set;}
		PointF beganTrackingLocation;
		float realPositionValue;
		private void initialize()
		{
			ScrubbingSpeed = 1f;
			ScrubbingSpeeds = new float[]{
				1,
				.5f,
				.25f,
				.1f,
				.05f,
				.01f,
				.005f
			};
			ScrubbingSpeedChangePositions = new float[]{
				25f,
				50f,
				100f,
				150f,
				250f,
				350f,
				400f,
			};
		}
		public override bool BeginTracking (UITouch uitouch, UIEvent uievent)
		{
			var beginTracking =  base.BeginTracking (uitouch, uievent);
			if(!beginTracking)
				return false;
			var thumbRect = this.ThumbRectForBounds (this.Bounds, this.TrackRectForBounds (this.Bounds), this.Value);
			beganTrackingLocation = new PointF (thumbRect.X + thumbRect.Width / 2, thumbRect.Y + thumbRect.Height / 2);
			realPositionValue = this.Value;

			return true;
		}
		public override bool ContinueTracking (UITouch touch, UIEvent uievent)
		{
			if (!this.Tracking)
				return false;
			var previousLocation = touch.PreviousLocationInView (this);
			var currentLocation = touch.LocationInView (this);
			var trackingOffset = currentLocation.X - previousLocation.X;

			float verticalOffset = Math.Abs (currentLocation.Y - beganTrackingLocation.Y);
			var scrubbingSpeedChangePosIndex = indexOfLowerScrubbingSpeed (verticalOffset);
			ScrubbingSpeed = ScrubbingSpeeds [scrubbingSpeedChangePosIndex];

			var trackRect = this.TrackRectForBounds (this.Bounds);
			realPositionValue = realPositionValue + (MaxValue - MinValue) * (trackingOffset / trackRect.Width);

			var valueAdjustment = ScrubbingSpeed * (MaxValue - MinValue) * (trackingOffset / trackRect.Width);
			var thumbAdjustment = 0f;

			if ( ((beganTrackingLocation.Y < currentLocation.Y) && (currentLocation.Y < previousLocation.Y)) ||
			    ((beganTrackingLocation.Y > currentLocation.Y) && (currentLocation.Y > previousLocation.Y)) )
			{
				// We are getting closer to the slider, go closer to the real location
				thumbAdjustment = (realPositionValue - Value) / (1 + Math.Abs(currentLocation.Y - beganTrackingLocation.Y));
			}
			Value += valueAdjustment + thumbAdjustment;

			if (Continuous) {
				this.SendActionForControlEvents(UIControlEvent.ValueChanged);
			}

			return true;
		}

		public override void EndTracking (UITouch uitouch, UIEvent uievent)
		{
			if (!Tracking)
				return;
			ScrubbingSpeed = ScrubbingSpeeds [0];
			this.SendActionForControlEvents(UIControlEvent.ValueChanged);
			this.SendActionForControlEvents(UIControlEvent.EditingDidEnd);
		}

		int indexOfLowerScrubbingSpeed(float verticalOffset)
		{
			for (var i = 0; i < ScrubbingSpeedChangePositions.Length; i++) {
				if(verticalOffset < ScrubbingSpeedChangePositions[i])
					return i;
			}
			return ScrubbingSpeedChangePositions.Length - 1;
		}

	}
}

