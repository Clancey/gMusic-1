
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace GoogleMusic
{
	public class AlbumCellView : LinearLayout
	{
		public AlbumCellView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize (context);
		}

		public AlbumCellView (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize (context);
		}
		public AlbumCellView (Context context) : base(context)
		{
			Initialize (context);
		}
		private void Initialize(Context context)
		{
			LayoutInflater inflater = LayoutInflater.From(context);
			View v = inflater.Inflate (Resource.Layout.albumCell, null, false);

			this.AddView(v);
		}
		Album currentAlbum;
		ImageView albumArt;
		public void Update (Album album)
		{
			var albumTv = this.FindViewById<TextView> (Resource.Id.albumName);
			albumTv.Text = album.Name;
			var artistTv = this.FindViewById<TextView> (Resource.Id.artistName);
			albumArt = this.FindViewById<ImageView> (Resource.Id.albumArt);
			artistTv.Text = album.Artist;
			try
			{
				if(currentAlbum != null)
					currentAlbum.ALbumArtUpdated -= HandleALbumArtUpdated;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			
			currentAlbum = album;
			currentAlbum.ALbumArtUpdated += HandleALbumArtUpdated;
			albumArt.SetImageDrawable (currentAlbum.AlbumArt (57) ??  Images.DefaultSmallAlbumImage);
		}

		void HandleALbumArtUpdated (object sender, EventArgs e)
		{
			this.Post (delegate {
				if (albumArt != null)				
					albumArt.SetImageDrawable (currentAlbum.AlbumArt (57) ?? Images.DefaultSmallAlbumImage);
			});
		}
	}
}

