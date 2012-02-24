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
using System.Collections.Generic;

namespace gMusic
{
	public static class Util
	{
		public static List<Song> Songs = new List<Song>();
		public static List<SongGroup> SongGroups = new List<SongGroup>();
		public static Dictionary<string,Song> SongsDict = new Dictionary<string, Song>();
		
		public static List<Artist> Artists = new List<Artist>();
		public static List<ArtistGroup> ArtistGroups = new List<ArtistGroup>();
		public static Dictionary<int,Artist> ArtistsDict = new Dictionary<int, Artist>();
	}
}

