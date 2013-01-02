using System;
using Xamarin.Tables;

namespace GoogleMusic
{
	public class SongAlbumCell : Cell
	{
		public SongAlbumCell (Song song) : base(song.Title)
		{

		}
	}
}

