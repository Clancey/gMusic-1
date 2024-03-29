//   Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
using System;
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.ObjCRuntime;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace ClanceysLib
{
	
	public class UIComboBox : UITextField
	{
		PickerView pickerView;
		UIControl closeView;
		UIButton closeBtn;
		public UIView ViewForPicker; 
		public event EventHandler ValueChanged;
		public event EventHandler PickerClosed;
		public event EventHandler PickerShown;
		public UIComboBox() : this(new RectangleF(0,0,100,25))
		{

		}
		public UIComboBox(RectangleF rect) : base (rect)
		{
			this.BorderStyle = UITextBorderStyle.RoundedRect;
			pickerView = new PickerView();
			this.TouchDown += delegate {	
				ShowPicker();
			};
			this.ShouldChangeCharacters += delegate {
				return false;	
			};
			pickerView.IndexChanged += delegate {
				var oldValue = this.Text;
				this.Text = pickerView.StringValue;	
				if(ValueChanged!= null && oldValue != Text)
					ValueChanged(this,null);
					
			};
			closeBtn = new UIButton(new RectangleF(0,0,31,32));
			closeBtn.SetImage(UIImage.FromFile("Images/closebox.png"),UIControlState.Normal);
			closeBtn.TouchDown += delegate {
				HidePicker();
			};
			pickerView.AddSubview(closeBtn);
		}
		public override bool CanBecomeFirstResponder {
			get {
				return false;
			}
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var parentView = ViewForPicker?? this.Superview;
			var parentH = parentView.Frame.Height;
			pickerView.Frame = new RectangleF(0,parentH - pickerView.Frame.Height,parentView.Frame.Size.Width,pickerView.Frame.Height);
			closeBtn.Frame = closeBtn.Frame.SetLocation(new PointF(pickerView.Bounds.Width - 32,pickerView.Bounds.Y));
			pickerView.BringSubviewToFront(closeBtn);
		}
		
		public object [] Items {
			get{return pickerView.Items;}
			set{pickerView.Items = value;}
		}
		
		public string DisplayMember {
			get{return pickerView.DisplayMember;}
			set {pickerView.DisplayMember = value;}
		}
		bool pickerVisible;
		
		public void ShowPicker()
		{
			if(PickerShown != null)
				PickerShown(this,null);
			LayoutSubviews ();
			pickerView.BringSubviewToFront(closeBtn);
			pickerVisible = true;
			var parentView = ViewForPicker ?? this.Superview;
			var parentFrame = parentView.Frame;
			//closeView = new TapableView(parentView.Bounds);
			//closeView.Tapped += delegate{
			//	HidePicker();	
			//};
	
			pickerView.Frame = pickerView.Frame.SetLocation(new PointF(0,parentFrame.Height));
			
			UIView.BeginAnimations("slidePickerIn");			
			UIView.SetAnimationDuration(0.3);
			UIView.SetAnimationDelegate(this);
			UIView.SetAnimationDidStopSelector (new Selector ("fadeInDidFinish"));
			//parentView.AddSubview(closeView);			
			parentView.AddSubview(pickerView);
			var tb = new UITextField(new RectangleF(0,-100,15,25));
			pickerView.AddSubview(tb);
			tb.BecomeFirstResponder();
			tb.ResignFirstResponder();
			tb.RemoveFromSuperview();
			
			
			pickerView.Frame = pickerView.Frame.SetLocation(new PointF(0,parentFrame.Height - pickerView.Frame.Height));
			UIView.CommitAnimations();	
			
		}
		bool isHiding;
		public void HidePicker()
		{
			if(PickerClosed!=null)
				PickerClosed(this,null);
			if(isHiding)
				return;
			isHiding = true;
			var parentView = ViewForPicker ?? Superview;
			var parentH = parentView.Frame.Height;
			
			UIView.BeginAnimations("slidePickerOut");			
			UIView.SetAnimationDuration(0.3);
			UIView.SetAnimationDelegate(this);
			UIView.SetAnimationDidStopSelector (new Selector ("fadeOutDidFinish"));
			pickerView.Frame = pickerView.Frame.SetLocation(new PointF(0,parentH));
			UIView.CommitAnimations();
		}
		
		public object SelectedItem {
			get {return pickerView.SelectedItem;}
		}
		
		[Export("fadeOutDidFinish")]
		public void FadeOutDidFinish ()
		{
			pickerView.RemoveFromSuperview();
			//closeView.RemoveFromSuperview();
			pickerVisible = false;
			isHiding = false;
		}
		[Export("fadeInDidFinish")]
		public void FadeInDidFinish ()
		{
			pickerView.BecomeFirstResponder();
			pickerView.BringSubviewToFront(closeBtn);
		}
		
	}
	
	public class PickerView : UIPickerView
	{
		public PickerView () : base ()
		{
			this.ShowSelectionIndicator = true;
		}	
		
		object[] items;
		public object[] Items
		{
			get{return items;}
			set{
				items = value;
				this.Model = new PickerData(this);
				if(IndexChanged != null)
					IndexChanged(this,null);
				}
		}		
		public string DisplayMember{get;set;}
		
		int selectedIndex;
		public int SelectedIndex{
			get {return selectedIndex;}
			set{
				if(selectedIndex == value)
					return;
				selectedIndex = value;
				if(IndexChanged != null)
					IndexChanged(this,null);
			}
		}
		
		public object SelectedItem {get{return items[SelectedIndex];}}
		
		public string StringValue {
			get{
				if(string.IsNullOrEmpty(DisplayMember))
					return SelectedItem.ToString();
				return GetPropertyValue(SelectedItem,DisplayMember);
			}
		}
		
		public event EventHandler IndexChanged;
		public static string GetPropertyValue (object inObject, string propertyName)
		{
			PropertyInfo[] props = inObject.GetType ().GetProperties ();
			PropertyInfo prop = props.Select (p => p).Where (p => p.Name == propertyName).FirstOrDefault ();
			if (prop != null)
				return prop.GetValue (inObject, null).ToString ();
			return "";
		}
		
		public static object[] GetPropertyArray (object inObject, string propertyName)
		{
			PropertyInfo[] props = inObject.GetType ().GetProperties ();
			PropertyInfo prop = props.Select (p => p).Where (p => p.Name == propertyName).FirstOrDefault ();
			if (prop != null)
			{
				var currentObject = prop.GetValue (inObject, null);
				if (currentObject.GetType ().GetGenericTypeDefinition () == typeof(List<>))
				{
					return (new ArrayList ((IList)currentObject)).ToArray ();
				}
				
				else if (currentObject is Array)
				{
					return (object[])currentObject;
				}
				else
				{
					return new object[1];
				}
			}
			return new object[1];
		}

	}
	
	public class PickerData : UIPickerViewModel
	{	
		PickerView Picker;
		public PickerData (PickerView picker)
		{		
			Picker = picker;			
		}

		public override int GetComponentCount (UIPickerView uipv)
		{	
			return (1);	
		}
		
		public override int GetRowsInComponent (UIPickerView uipv, int comp)
		{
			//each component has its own count.
			int rows = Picker.Items.Length;
			return (rows);
		}
		
		public override string GetTitle (UIPickerView uipv, int row, int comp)
		{
			
			//each component would get its own title.
			
			var theObject = Picker.Items[row];
			if(string.IsNullOrEmpty(Picker.DisplayMember))
				return theObject.ToString();
			return PickerView.GetPropertyValue(theObject,Picker.DisplayMember);
		}

		
		public override void Selected (UIPickerView uipv, int row, int comp)
		{
			Picker.SelectedIndex = row;
			//Picker.Select(row,comp,false);
		}

		public override float GetComponentWidth (UIPickerView uipv, int comp)
		{
			return (300f);	
		}

		public override float GetRowHeight (UIPickerView uipv, int comp)
		{
			return (40f);	
		}
	}
}




