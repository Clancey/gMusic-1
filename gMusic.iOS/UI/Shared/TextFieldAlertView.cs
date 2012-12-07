using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace GoogleMusic
{
	public class TextFieldAlertView : UIAlertView
	{
		private UITextField _tf;
		private bool _secureTextEntry;
		
		public TextFieldAlertView() : this(false) {}
		bool hasInitilized;
		public string ConfirmText = "Ok".Translate();
		public TextFieldAlertView(bool secureTextEntry, string title, string message) : this(secureTextEntry)
		{
			this.Title = title;
			this.Message = message;
		}
		
		public TextFieldAlertView(bool secureTextEntry)
		{	
			_secureTextEntry = secureTextEntry;
		}
		
		private void InitializeControl()
		{
			hasInitilized = true;
			this.AddButton("Cancel".Translate());
			this.AddButton(ConfirmText);
			_tf = ComposeTextFieldControl(_secureTextEntry);
			
			// add the text field to the alert view
			this.AddSubview(_tf);
			// shift the control up so the keyboard won't hide the control when activated.
			//this.Transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeTranslation(0f, 50f);
		}
		
		public string EnteredText { get { return _tf.Text; } }
		
		public override void LayoutSubviews ()
		{
			// layout the stock UIAlertView control
			base.LayoutSubviews ();
			
			// build out the text field
			
			// shift the contents of the alert view around to accomodate the extra text field
			AdjustControlSize();
		}

		private UITextField ComposeTextFieldControl(bool secureTextEntry)
		{
			UITextField textField = new UITextField (new System.Drawing.RectangleF (12f, 45f, 260f, 25f));
			textField.BackgroundColor = UIColor.White;
			textField.UserInteractionEnabled = true;
			textField.AutocorrectionType = UITextAutocorrectionType.No;
			textField.AutocapitalizationType = UITextAutocapitalizationType.None;
			textField.ReturnKeyType = UIReturnKeyType.Done;
			textField.SecureTextEntry = secureTextEntry;
			return textField;
		}
		bool hasAdjusted;
		private void AdjustControlSize()
		{

			hasAdjusted = true;
			float tfExtH = _tf.Frame.Size.Height + 16.0f;
			
			RectangleF frame = new RectangleF(this.Frame.X, 
			                                  this.Frame.Y - tfExtH/2,
			                                  this.Frame.Size.Width,
			                                  this.Frame.Size.Height + tfExtH);
			this.Frame = frame;
			foreach(var view in this.Subviews)
			{
				if(view is UIButton)
				{
					view.Frame = new RectangleF(view.Frame.X, 
					                            _tf.Frame.Y + tfExtH ,
					                            view.Frame.Size.Width, 
					                            view.Frame.Size.Height);
				}
			}
			
		}
		public override void Show ()
		{
			if(!hasInitilized)
				InitializeControl();
			base.Show ();
			_tf.BecomeFirstResponder();
		}
	}
}