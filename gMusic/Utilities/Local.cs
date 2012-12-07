using System;
//using MonoTouch.Foundation;


namespace GoogleMusic
{
	public static class Local
	{
		public static string GetString(string name, string comment = null)
		{
			return name;
			//Console.WriteLine (NSBundle.MainBundle.Localizations);
			//return NSBundle.MainBundle.LocalizedString (name, null);
		}
		public static string Translate(this string name)
		{
			var s  = GetString(name);
			return s;
		}
	}
}

