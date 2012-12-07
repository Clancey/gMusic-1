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
using SQLite;
using System.IO;

namespace GoogleMusic
{
	public class WebDatabase : SQLiteConnection
	{
		internal WebDatabase (string file) : base (file)
		{
		}
		public static readonly string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString () + "/Library/WebKit/Databases/";
		static public WebDatabase Main { get; private set; }
		public static void Setup()
		{	
			var dbPAth = BaseDir + "Databases.db";
			Main = new WebDatabase (dbPAth);
		}
		public static bool Exists()
		{
			return File.Exists(BaseDir + "Databases.db");
		}
	}
}

