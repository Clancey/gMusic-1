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
using System.Drawing;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;


#if iOS
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.UIKit;
#endif

public static class Extensions
{
	public static string ToOrdinal(this int number)
	{
	    switch(number % 100)
	    {
	        case 11:
	        case 12:
	        case 13:
	            return number.ToString() + "th";
	    }
	
	    switch(number % 10)
	    {
	        case 1:
	            return number.ToString() + "st";
	        case 2:
	            return number.ToString() + "nd";
	        case 3:
	            return number.ToString() + "rd";
	        default:
	            return number.ToString() + "th";
	    }
	}
	public static List<Cookie> Cookies(this CookieContainer container)
	{
		try{
			FieldInfo[] props = container.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo prop = props.Select (p => p).Where (p => p.Name == "cookies").FirstOrDefault ();
			if (prop != null)
			{
				var cookies = (CookieCollection)prop.GetValue (container);
				props = cookies.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
				prop = props.Select (p => p).Where (p => p.Name == "list").FirstOrDefault ();
				if(prop != null)
					return (List<Cookie>)prop.GetValue (cookies);
			}
		}
		catch(Exception ex)
		{

		}
		return new List<Cookie>();
	}
#if iOS
	public static void RemoveAllSubviews(this UIView view)
	{
		foreach(var sView in view.Subviews)
			sView.RemoveFromSuperview();
	}

	public static PointF GetCenter( this RectangleF rect)
	{
		return new PointF(rect.GetMidX(),rect.GetMidY());
	}

	public static CALayer CreateShadowLayer(this UIViewController view, RectangleF frame)
	{
		CAGradientLayer gradientLayer = new CAGradientLayer();
		gradientLayer.Frame = frame;
		UIColor lightColor = UIColor.Black.ColorWithAlpha(0);
		UIColor darkColor = UIColor.Black.ColorWithAlpha(0.3f);
		gradientLayer.Colors = new CGColor[]{darkColor.CGColor,lightColor.CGColor};
		return gradientLayer;
	}
#endif
}

