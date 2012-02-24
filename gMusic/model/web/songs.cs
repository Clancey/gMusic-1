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

namespace gMusic.web
{
	public class songs
	{
		public string id {get;set;}
		public string name {get;set;}
		public int artist_id {get;set;}
		public int album_id {get;set;}
		public int genre_id {get;set;}
		public string albumArtUrl {get;set;}
		public int disc {get;set;}
		public int track{get;set;}
		public int durationMillis{get;set;}
	}
}

