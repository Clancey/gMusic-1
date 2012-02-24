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

namespace gMusic
{
	public class Database: SQLiteConnection
	{
		internal Database (string file) : base (file)
		{
			CreateTable<SongGroup>();
			CreateTable<ArtistGroup>();
			CreateTable<AlbumGroup>();
			CreateTable<GenreIndex>();
			CreateTable<Song>();
			CreateTable<Artist>();
		}
		public static readonly string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString () + "/Documents/";
		static public Database Main { get; private set; }
		public static void SetDatabase(string user)
		{
			var dbPAth = BaseDir + user;
			Main = new Database (dbPAth + "-Databases.db");
			WebDatabase = new WebDatabase (dbPAth + "-webDatabases.db");
		}
		public static bool DatabaseExists(string user)
		{
			
			var dbPAth = BaseDir + user + "-Databases.db";
			return File.Exists(dbPAth);
		}
		static public WebDatabase WebDatabase { get; private set; }
	}
}

