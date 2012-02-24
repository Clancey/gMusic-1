// 
//  Copyright 2012  Xamarin Inc  (http://www.xamarin.com)
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

namespace gMusic
{
	public class Artist : web.artists
	{
		[Ignore]
		public string IndexChar {get{return indexChar ?? getIndex(name);} }
		public int Order {get;set;}
		
		
		static string getIndex (out string name)
		{
			name = name.Trim ().ToLower();
			if (name.StartsWith ("the "))
				name = name.Replace ("the ", "");
			var firstLetter = (string.IsNullOrEmpty (name)) ? " ".First () : name.First ();
			if (char.IsLetter (firstLetter))
				return firstLetter.ToString ().ToUpper ();
			return "#";
		}
	}
}

