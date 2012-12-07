using System;
using MonoTouch.UIKit;
using Un4seen.Bass;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using ClanceysLib;
using System.Linq;
using Un4seen.Bass.AddOn.Fx;

namespace GoogleMusic
{
	public class EqualizerViewController : UIViewController
	{
		EqualizerView equalizerView;
		bool DarkThemed = false;
		public EqualizerViewController () 
		{
			if (Bands.Length == 0)
				Bands = TenBandPreset();
			fxEq = new int[Bands.Length];
		}
	
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Title = "Equalizer".Translate();
			equalizerView = new EqualizerView (this);
			this.View = equalizerView;
		}
		//static BASS_DX8_PARAMEQ pEQ = new BASS_DX8_PARAMEQ () {fBandwidth = 12f,fGain = 0};
		static Band[] Bands = new Band[0];
		static int[] fxEq = new int[0];
		static int fxStream = 0;
		private static int _fxEQ;
		//private static BASS_BFX_COMPRESSOR2 _cmp1 = new BASS_BFX_COMPRESSOR2();

		private static BASS_BFX_PEAKEQ eq = new BASS_BFX_PEAKEQ();

		public static bool Enabled { get; set; }

		[Obsolete ("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.All;
		}
		public static void ApplyEqualizer()
		{
			if (!Settings.EqualizerEnabled)
				return;

			if (fxStream == Util.CurrentSong.StreamId) {
				for (int i = 0; i < Bands.Count(); i++) {
					UpdateFX(i,Bands[i].Gain);
				}
				return;
			}
			fxStream = Util.CurrentSong.StreamId;

			//var comp = Bass.BASS_ChannelSetFX(fxStream, BASSFXType.BASS_FX_BFX_COMPRESSOR2, 0);
			//_cmp1.fThreshold = 0.3f;
			//_cmp1.fAttack = 1.0f;
			//_cmp1.fRelease = 10.0f;
			//Bass.BASS_FXSetParameters (comp, _cmp1);

			_fxEQ = Bass.BASS_ChannelSetFX(fxStream, BASSFXType.BASS_FX_BFX_PEAKEQ, 1);
			if(_fxEQ == 0)
				Console.WriteLine(Bass.BASS_ErrorGetCode());
			//BASS_BFX_PEAKEQ eq = new BASS_BFX_PEAKEQ(); 
			eq.fQ = 0f;
			eq.fBandwidth = .6f;
			eq.lChannel = BASSFXChan.BASS_BFX_CHANALL;
			for (int i = 0; i < Bands.Count(); i++) {
				eq.lBand = i;
				eq.fCenter = Bands[i].Center;
				eq.fGain = Bands[i].Gain;
				Console.WriteLine(eq.fCenter);
				Bass.BASS_FXSetParameters (_fxEQ, eq);
				//UpdateFX(i,Bands[i].Gain);
			}

			


			return;


			for (int i = 0; i <fxEq.Length; i++) {
				fxEq [i] = Bass.BASS_ChannelSetFX (Util.CurrentSong.StreamId, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
			}

			for (int i = 0; i < Bands.Length; i++) {
				UpdateGain(i);
			}
		}
		private static void UpdateFX(int band, float gain)
		{
			//BASS_BFX_PEAKEQ eq = new BASS_BFX_PEAKEQ();
			// get values of the selected band
			eq.lBand = band;
			Bass.BASS_FXGetParameters(_fxEQ, eq);
			//eq.fGain = gain+( _cmp1.fThreshold * (1 / _cmp1.fRatio - 1));
			eq.fGain = gain;
			Bass.BASS_FXSetParameters(_fxEQ, eq);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if(!DarkThemed  && (Util.IsIphone || (Util.AppDelegate.MainVC.InterfaceOrientation == UIInterfaceOrientation.Portrait || Util.AppDelegate.MainVC.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown))){
				this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIImage.FromFile("Images/menu.png"), UIBarButtonItemStyle.Bordered, delegate {
					Util.AppDelegate.MainVC.ToggleMenu();
				});
			}
			else 
			{
				this.NavigationItem.HidesBackButton = true;
				this.NavigationItem.LeftBarButtonItem = null;
				this.NavigationController.NavigationBar.Frame = this.NavigationController.NavigationBar.Frame.SetLocation(0,0);
			}
		}
		public static void UpdateGain (int index)
		{
			if (index >= Bands.Length || Util.CurrentSong == null || Util.CurrentSong.StreamId == 0)
				return;
			var gain = Bands [index].Gain;;
			Bass.BASS_FXGetParameters(_fxEQ, eq);
			//eq.fGain = gain+( _cmp1.fThreshold * (1 / _cmp1.fRatio - 1));
			eq.fGain = gain;
			Console.WriteLine (gain);
			///pEQ.fGain = Bands [index].Gain;
			Bass.BASS_FXSetParameters (fxEq [index], eq);
		}
		public static void Clear()
		{
			if (Util.CurrentSong == null || Util.CurrentSong.StreamId == 0)
				return;
			//Bass.BASS_ChannelRemoveFX (fxStream, _fxEQ);
			for (int i = 0; i <Bands.Length; i++) {
				UpdateFX(i,0);
			}
		}
		private class EqualizerView : UIView {
			
			List<UISlider> Sliders = new List<UISlider>();
			List<UILabel> labels = new List<UILabel>();
			//RCSwitch onSwitch;
			UIButton onSwitch;
			float sliderH = 0f;
			UILabel onLabel;
			UIComboBox combobox;
			public override void LayoutSubviews ()
			{
				var xOffset = 30;
				var width = (this.Bounds.Width - xOffset) / (Sliders.Count + 1);
				var padding = width / 2;

				onSwitch.Frame = onSwitch.Frame.SetLocation (xOffset + padding, 10);
				onLabel.Frame = onLabel.Frame.SetLocation (onSwitch.Frame.Right + 5, 10);
				
				sliderH = onSwitch.Frame.Bottom + 5;

				float height = 0;
				if (Util.IsIphone)
					height = Math.Min (this.Bounds.Height - 100 - sliderH, 250);
				else
					height = this.Bounds.Height - 60;

				for (int i = 0; i < Sliders.Count; i++) {
					var x = width * i + padding + xOffset;
					var slider = Sliders[i]; 
					var label = labels[i];
					slider.Frame = new RectangleF(x,sliderH,width,height);
					label.Frame = new RectangleF(x,slider.Frame.Bottom,width,25);
				}

				combobox.Frame = combobox.Frame.SetLocation (xOffset + padding, height + 75);
				SetNeedsDisplay ();
			}
			EqualizerViewController  Parent;
			public EqualizerView(EqualizerViewController eqvc)
			{
				Parent = eqvc;
				onSwitch = UIButton.FromType (UIButtonType.Custom);
				onSwitch.Frame = new RectangleF(0,0,22,22);
				onSwitch.TouchDown += delegate {
					Settings.EqualizerEnabled = !Settings.EqualizerEnabled;
					if(Settings.EqualizerEnabled) 
						ApplyEqualizer();

					else
						Clear();

					updateImages();
				};
				onLabel = new UILabel(new RectangleF(0,0,100,22)){Text = "On".Translate(), BackgroundColor = UIColor.Clear};
				this.AddSubview(onSwitch);
				this.AddSubview(onLabel);

				combobox = new UIComboBox ();
				combobox.DisplayMember = "Name";
				combobox.Items = Presets.ToArray();
				combobox.ValueChanged += delegate {
					SetPreset((EqualizerPreset)combobox.SelectedItem);
				};
				combobox.ViewForPicker = this;
				this.AddSubview (combobox);

				init();

			}
			
			public void SetPreset(EqualizerPreset preset)
			{
				for (int i = 0; i < preset.Values.Length; i ++) {
					Sliders[i].SetValue((float)preset.Values[i],true);
					HandleValueChanged(Sliders[i],EventArgs.Empty);
				}
			}
			private void updateImages()
			{
				var image = Settings.EqualizerEnabled ? thumbImageOn : thumbImageOff;
				foreach (var slider in Sliders) {
					
					slider.SetThumbImage(image, UIControlState.Normal);
					slider.SetThumbImage(image, UIControlState.Highlighted);
				}
				
				onSwitch.SetBackgroundImage(Settings.EqualizerEnabled ? switchOn : switchOff, UIControlState.Normal);
			}
			private void init()
			{
				clearSliders ();
				for (int i = 0; i < Bands.Length; i++) {
					createSlider(i);
					//UpdateGain(i);
				}
				updateImages();
			}
			private void clearSliders()
			{
				foreach (var slider in Sliders) {
					slider.RemoveFromSuperview ();
					slider.ValueChanged-= HandleValueChanged;
				}
				foreach (var label in labels) {
					label.RemoveFromSuperview ();
				}
				labels = new List<UILabel> ();
				Sliders = new List<UISlider> ();
			}
			UIImage thumbImageOn, thumbImageOff, switchOn, switchOff;
			private void createSlider(int index)
			{
				if (thumbImageOn == null) {
					thumbImageOn = UIImage.FromFile ("Images/slider-handle-rotated.png"); 
					thumbImageOff = UIImage.FromFile("Images/slider-handle-off-rotated.png");
					switchOn = UIImage.FromFile("Images/cb_glossy_on.png");
					switchOff = UIImage.FromFile("Images/cb_glossy_off.png");
					
				}
				var band = Bands [index];
				var slider = new UISlider (){MinValue = -1 * range,MaxValue = range,Value = band.Gain};
				slider.Transform = CGAffineTransform.MakeRotation ((float)Math.PI * -.5f);
				slider.ValueChanged += HandleValueChanged;
				slider.Tag = index;
				slider.MaximumTrackTintColor = UIColor.DarkGray;
				slider.MinimumTrackTintColor = UIColor.DarkGray;
				Sliders.Add (slider);
				this.AddSubview (slider);
				
				var label = new UILabel (){Text = band.ToString(),TextColor = UIColor.Black,BackgroundColor = UIColor.Clear, Font = UIFont.SystemFontOfSize(10), TextAlignment = UITextAlignment.Center};
				labels.Add (label);
				this.AddSubview (label);
			}
			static float range = 12f;
			void HandleValueChanged (object sender, EventArgs e)
			{

				var slider = sender as UISlider;
				//UpdateGain (slider.Tag);
				//UpdateEQ (slider.Tag, slider.Value);
				//return;
				
				BASS_DX8_PARAMEQ eq = new BASS_DX8_PARAMEQ();
				var gain = slider.Value;
				Bands[slider.Tag].Gain = gain; 
				UpdateFX (slider.Tag, gain);
				return;
				Bands[slider.Tag].Gain = gain; 
				if (Bass.BASS_FXGetParameters(fxEq[slider.Tag], eq))
				{
					//UpdateGain(slider.Tag);
					//eq.fGain = gain+( _cmp1.fThreshold * (1 / _cmp1.fRatio - 1));
					eq.fGain = slider.Value; 
					Bass.BASS_FXSetParameters(fxEq[slider.Tag], eq);
				}
			}

			public override void Draw (RectangleF rect)
			{
				//// General Declarations
				var colorSpace = CGColorSpace.CreateDeviceRGB();
				var context = UIGraphics.GetCurrentContext();
				
				//// Color Declarations
				UIColor gradient2Color = UIColor.FromRGBA(0.906f, 0.910f, 0.910f, 1.000f);
				UIColor gradient2Color2 = UIColor.FromRGBA(0.588f, 0.600f, 0.616f, 1.000f);
				
				//// Gradient Declarations
				var gradient2Colors = new CGColor [] {gradient2Color.CGColor, gradient2Color2.CGColor};
				var gradient2Locations = new float [] {0, 1};
				var gradient2 = new CGGradient(colorSpace, gradient2Colors, gradient2Locations);
				
				//// Abstracted Attributes
				var textContent = "+ " + range+ " dB";
				var text2Content = "0 dB";
				var text3Content = "- " + range + " dB";

				//// Rectangle Drawing
				var rectanglePath = UIBezierPath.FromRect(rect);
				context.SaveState();
				rectanglePath.AddClip();
				context.DrawLinearGradient(gradient2, new PointF(rect.Height, 0), new PointF(rect.Height, rect.Height), 0);
				context.RestoreState();


				if (Sliders.Count == 0)
					return;

				var sliderFrame = Sliders [0].Frame;
				var thumbH = Sliders [0].CurrentThumbImage.Size.Height /2;
				var h = (sliderFrame.Height - (thumbH * 2) ) / 8;
				var offset = sliderFrame.Y;
				var x = sliderFrame.X;
				var width = Sliders.Last ().Frame.Right;
				UIColor.Black.ColorWithAlpha(.5f).SetStroke ();
				for (int i = 0; i < 9; i++) {
					
					UIColor.Black.ColorWithAlpha(.5f).SetStroke ();
					var currH = (i * h) + thumbH;
					context.MoveTo (x, currH + offset);
					context.AddLineToPoint (width, currH +offset);
					context.StrokePath ();
					if(i ==0)
					{
						//// Text Drawing
						var textRect = new RectangleF(0, currH + offset - 7.5f, 37, 13);
						UIColor.Black.SetFill();
						new MonoTouch.Foundation.NSString(textContent).DrawString(textRect, UIFont.FromName("Helvetica", 10), UILineBreakMode.WordWrap, UITextAlignment.Right);
					}
					else if(i == 4)
					{
						//// Text Drawing
						var textRect = new RectangleF(0, currH + offset - 7.5f, 37, 13);
						UIColor.Black.SetFill();
						new MonoTouch.Foundation.NSString(text2Content).DrawString(textRect, UIFont.FromName("Helvetica", 10), UILineBreakMode.WordWrap, UITextAlignment.Right);
					}
					else if(i == 8)
					{
						//// Text Drawing
						var textRect = new RectangleF(0, currH + offset- 7.5f, 37, 13);
						UIColor.Black.SetFill();
						new MonoTouch.Foundation.NSString(text3Content).DrawString(textRect, UIFont.FromName("Helvetica", 10), UILineBreakMode.WordWrap, UITextAlignment.Right);
					}
				}
			}
			public List<EqualizerPreset> Presets = new List<EqualizerPreset> ()
			{
				new EqualizerPreset(){
					Name = "None",
					Values = new double[10]{
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						0,
						0,
					}
					
				},
				new EqualizerPreset(){
					Name = "Rock",
					Values = new double[10]{
						4.5,
						4,
						3,
						1,
						-1,
						-1.5,
						0,
						2.5,
						3,
						3.5
					}
				},
				new EqualizerPreset(){
					Name = "R&B",
					Values = new double[10]{
						2.5,
						7,
						5.5,
						1,
						-3,
						-2,
						2,
						2.5,
						2.5,
						3,
					}
				},
			};
		}

	
		public Band[] TenBandPreset(){
			return new Band[]{
				new Band{ Center = 32},
				new Band{ Center = 64},
				new Band{ Center = 125},
				new Band{ Center = 250},
				new Band{ Center = 500},
				new Band{ Center = 1000},
				new Band{ Center = 2000},
				new Band{ Center = 4000},
				new Band{ Center = 8000},
				new Band{ Center = 16000},
			};
		}

		
		public Band[] ThreeBandPreset(){
			return new Band[]{
				new Band{ Center = 100},
				new Band{ Center = 1000},
				new Band{ Center = 8000},
			};
		}
	}
	public class Band
	{
		public float Center { get; set; }
		
		public float Gain { get; set; }
		
		public override string ToString ()
		{
			if (Center < 1000)
				return string.Format ("{0}", Center);
			return string.Format ("{0}K", Center / 1000);
		}
	}
	public class EqualizerPreset
	{
		public string Name {get;set;}
		public double[] Values { get; set; }
	}
}

