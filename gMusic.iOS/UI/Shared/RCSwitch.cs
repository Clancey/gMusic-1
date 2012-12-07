// 
//  Copyright 2012  James Clancey, Xamarin Inc  (http://www.xamarin.com)
//
//	based on code by Robert Chin
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
using MonoTouch.CoreGraphics;


public class RCSwitchOnOff : RCSwitch
{
	UILabel onText;
	UILabel offText;
	public RCSwitchOnOff () : this(new RectangleF(0, 0, 80, 36))
	{
		
	}
	public RCSwitchOnOff( RectangleF rect) : base(rect)
	{

	}

	protected override void Initialize ()
	{
		base.Initialize ();
		onText = new UILabel(){
			Text = "ON",
			TextColor = UIColor.White,
			Font = UIFont.BoldSystemFontOfSize(15),
			ShadowColor = UIColor.FromWhiteAlpha(.175f,1f)};
		offText = new UILabel(){
			Text = "OFF",
			TextColor = UIColor.White,
			Font = UIFont.BoldSystemFontOfSize(15)
		};

	}
	protected override void DrawUnderLayers (RectangleF rect, float offset, float trackWidth)
	{
		var textRect = this.Bounds;
		textRect.X += 14 + (offset - trackWidth);

		onText.DrawText(textRect);

		textRect = this.Bounds;
		textRect.X += (offset + trackWidth) - 8;
		offText.DrawText(textRect);
	}
}

public class RCSwitch : UIControl
{
	public float DrawHeight,Percent,OldPercent, KnobWidth,EndcapWidth,Scale;
	public double AnimationDuration;
	public UIImage KnobImage;
	public UIImage KnobImagePressed;
	public UIImage SliderOff;
	public UIImage SliderOn;
	public SizeF LastBoundsSize;
	public PointF StarTPoint;
	DateTime? endDate;
	bool MustFlip;

	public RCSwitch () : this(new RectangleF(0, 0, 80, 36))
	{

	}
	public RCSwitch(RectangleF rect) : base(rect)
	{
		Initialize();
	}
	protected virtual void Initialize()
	{
		Scale = UIScreen.MainScreen.Scale;
		DrawHeight = 28;
		AnimationDuration = .25;
		ContentMode = UIViewContentMode.Redraw;
		SetKnobWidth(35f);
		RegenerateImages();
		SliderOff = UIImage.FromFile("Images/switch-off.png").CreateResizableImage(new UIEdgeInsets(0,12,0,12));

		this.Opaque = false;
	}

	void SetKnobWidth (float s)
	{
		if(KnobWidth == s)
			return;
		KnobWidth = (float)Math.Round(s);
		EndcapWidth = (float)Math.Round(KnobWidth/2);

		KnobImage = CreateKnobImage("Images/switch-thumb-off.png");
		KnobImagePressed = CreateKnobImage("Images/switch-thumb-on.png");
	}

	UIImage CreateKnobImage(string image)
	{
		var knobTmp = UIImage.FromFile(image).CreateResizableImage(new UIEdgeInsets(0,0,0,0));
		var knobRect = new RectangleF(0,3.7f,KnobWidth, knobTmp.Size.Height);

		UIGraphics.BeginImageContextWithOptions(knobRect.Size,false,Scale);
		knobTmp.Draw(knobRect);
		var theImage = UIGraphics.GetImageFromCurrentImageContext();
		UIGraphics.EndImageContext();
		return theImage;
	}

	void RegenerateImages ()
	{
		UIImage onSwitchImage = UIImage.FromFile("Images/switch-on.png").CreateResizableImage(new UIEdgeInsets(0,12,0,12));

		var sliderOnRect = this.Bounds;
		sliderOnRect.Height = onSwitchImage.Size.Height;
		
		UIGraphics.BeginImageContextWithOptions(sliderOnRect.Size,false,Scale);
		onSwitchImage.Draw(sliderOnRect);
		SliderOn =  UIGraphics.GetImageFromCurrentImageContext();
		UIGraphics.EndImageContext();

	}
	protected virtual void DrawUnderLayers(RectangleF rect,float offset,float trackWidth)
	{

	}
	public override void Draw (RectangleF rect)
	{	
		var boundsRect = this.Bounds;
		boundsRect.Height = DrawHeight;

		if(boundsRect.Size != LastBoundsSize)
		{
			RegenerateImages();
			LastBoundsSize = boundsRect.Size;
		}

		var width = boundsRect.Width;
		var drawPercent = Percent;
		if(((width - KnobWidth) * drawPercent) < 3)
			drawPercent = 0;
		else if(((width - KnobWidth) * drawPercent) > (width - KnobWidth - 3))
			drawPercent = 1;

		if(endDate.HasValue)
		{
			var interval = (endDate.Value - DateTime.Now).TotalSeconds;
			if(interval< 0)
				endDate = null;
			else{
				if(Percent == 1)
					drawPercent =  (float)Math.Cos((interval/AnimationDuration) * (float)(Math.PI/2));
				else
					drawPercent = 1f -  (float)Math.Cos((interval/AnimationDuration) *  (float)(Math.PI / 2));
			
				this.PerformSelector(new MonoTouch.ObjCRuntime.Selector("setNeedsDisplay"),null,0);
			}
		}
		var context = UIGraphics.GetCurrentContext();
		context.SaveState();
		UIGraphics.PushContext(context);
		var sliderOffRect = boundsRect;
		sliderOffRect.Height = SliderOff.Size.Height;
		SliderOff.Draw(sliderOffRect);

		if(drawPercent > 0)
		{

			float onWidth = KnobWidth / 2 + ((width - KnobWidth /2) - KnobWidth / 2) * drawPercent;
			var sourceRect = new RectangleF(0,0,onWidth * Scale,SliderOn.Size.Height * Scale);
			var drawRect = new RectangleF(0,0,onWidth,SliderOn.Size.Height);
			var sliderOnSubImage = SliderOn.CGImage.WithImageInRect(sourceRect);
			context.SaveState();
			context.ScaleCTM(1,-1);
			context.TranslateCTM(0,-drawRect.Height);
			context.DrawImage(drawRect,sliderOnSubImage);
			context.RestoreState();
			sliderOnSubImage.Dispose();
		}

		{

			context.SaveState();
			UIGraphics.PushContext(context);

			var insetClipRect = boundsRect.Inset(4,4);
			UIGraphics.RectClip(insetClipRect);
			DrawUnderLayers(rect,drawPercent * (boundsRect.Width - KnobWidth),boundsRect.Width - KnobWidth);
			UIGraphics.PopContext();
			context.RestoreState();
		}
		{
			context.ScaleCTM(1,-1);
			context.TranslateCTM(0,-boundsRect.Height);
			PointF location = boundsRect.Location;
			UIImage imageToDraw = this.On ? KnobImagePressed : KnobImage;
			float xLocation;
			if(drawPercent == 0)
				xLocation = location.X + (float)Math.Round((drawPercent * (boundsRect.Width - KnobWidth + 2)));
			else
			{
				xLocation = location.X - 2 + (float)Math.Round((drawPercent * (boundsRect.Width - KnobWidth + 2)));
				xLocation = xLocation < 0 ? 0 : xLocation;
			}

			var drawOnRect = new RectangleF(xLocation,location.Y - 7,KnobWidth,KnobImage.Size.Height);
			context.DrawImage(drawOnRect,imageToDraw.CGImage);
		}
		UIGraphics.PopContext();
		context.RestoreState();


	}

	public override bool BeginTracking (UITouch uitouch, UIEvent uievent)
	{
		this.Highlighted = true;
		OldPercent = Percent;
		endDate = null;
		MustFlip = true;
		this.SetNeedsDisplay();
		this.SendActionForControlEvents(UIControlEvent.TouchDown);
		return true;
	}
	public override bool ContinueTracking (UITouch uitouch, UIEvent uievent)
	{
		var point = uitouch.LocationInView(this);
		Percent  = (point.X - KnobWidth/2) / (this.Bounds.Width - KnobWidth);
		if(Percent <0)
			Percent = 0;
		else if(Percent > 1)
			Percent = 1;
		if((OldPercent < 0.25f && Percent > 0.5f) || (OldPercent > 0.75f && Percent < 0.5f))
			MustFlip = false;
		this.SetNeedsDisplay();
		this.SendActionForControlEvents( UIControlEvent.TouchDragInside);
		return true;
	}
	public override void EndTracking (UITouch uitouch, UIEvent uievent)
	{
		finishEvent();
	}

	public override void CancelTracking (UIEvent uievent)
	{
		finishEvent();
	}

	private void finishEvent()
	{
		Highlighted = false;
		endDate = null;
		float toPercent = (1.0f - OldPercent);
		if(!MustFlip){
			if(OldPercent < 0.25f){
				if(Percent > 0.5f)
					toPercent = 1.0f;
				else
					toPercent = 0.0f;
			}
			if(OldPercent > 0.75f){
				if(Percent < 0.5f)
					toPercent = 0.0f;
				else
					toPercent = 1.0f;
			}
		}
		performSwitch(toPercent);
	}
	public bool On{
		get{return Percent > .5f;}
		set{
			SetOn(value);
		}
	}
	public void SetOn(bool on)
	{
		SetOn(on,false);
	}
	public void SetOn(bool on, bool animated)
	{
		float toPercent = on ? 1 : 0;
		if(animated)
		{
			if((Percent <  .5f && on) || (Percent > .5f && !on))
			   performSwitch(toPercent);
		}
		else
		{
			Percent = toPercent;
			SetNeedsDisplay();
			SendActionForControlEvents(UIControlEvent.ValueChanged);
		}
	}
	private void performSwitch(float percent)
	{
		var seconds = Math.Abs((Percent - percent)) * AnimationDuration;
		endDate = DateTime.Now.AddSeconds(seconds);
		Percent = percent;
		this.SetNeedsDisplay();
		SendActionForControlEvents(UIControlEvent.ValueChanged);
		SendActionForControlEvents(UIControlEvent.TouchUpInside);
	}
}


