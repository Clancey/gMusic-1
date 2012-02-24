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
using System.Runtime.Serialization;

namespace gMusic
{
	[DataContract()]
	public class loadAllResponse
	{
		[DataMember(Name="continuation")]
		public bool continuation {get;set;}
		[DataMember(Name="continuationToken")]
		public string continuationToken {get;set;}
		[DataMember(Name="playlistId")]
		public string playlistId {get;set;}
		[DataMember()]
		public DateTime requestTime {get;set;}
		[DataMember]
		public List<Track> playlist {get;set;}
	}
}

