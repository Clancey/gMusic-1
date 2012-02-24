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
using System.Runtime.Serialization;

namespace gMusic
{
	public class Track
	{
		
		public Track ()
		{
		}
		//[DataMember(Name="title")]
		//public string Name {get;set;}
		//public string Album {get;set;}
		public string  genre{get;set;}
		public int beatsPerMinute{get;set;}
		public string albumArtistNorm{get;set;} 
		public string artistNorm {get;set;}
		public string album {get;set;} 
		public DateTime lastPlayed {get;set;}
		public int type{get;set;}
		public int disc {get;set;}
		public string id {get;set;}
		public string composer{get;set;}
		public string title{get;set;}
		public string albumArtist{get;set;}
		public int totalTracks{get;set;}
		public string name{get;set;}
		public string totalDiscs{get;set;}
		public int year{get;set;}
		public string titleNorm{get;set;}
		public string artist{get;set;}
		public string albumNorm{get;set;}
		public int track{get;set;}
		public int durationMillis{get;set;}
		public string albumArtUrl{get;set;}
		public bool deleted {get;set;}
		public string url{get;set;}
		public DateTime creationDate{get;set;}
		public int playCount{get;set;}
		public int rating {get;set;}
		public string comment{get;set;}
	}
}

