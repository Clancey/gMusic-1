using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace GoogleMusic
{
	public class EditSongViewController : BaseDialogViewController
	{
		Song Song;
		EntryElement titleElement;
		EntryElement albumElement;
		EntryElement genreElement;
		EntryElement artistElement;
		EntryElement albumArtistElement;
		EntryElement discElement;
		EntryElement trackElement;
		public EditSongViewController(IntPtr handle):base(handle){}
		public EditSongViewController (Song song)
		{
			Song = song;
			titleElement = new EntryElement("Title".Translate(),"",song.Title);
			artistElement = new EntryElement("Artist".Translate(),"",song.Artist);
			albumArtistElement = new EntryElement("Album Artist".Translate(),"",song.AlbumArtist);
			albumElement = new EntryElement("Album".Translate(),"",song.Album);
			genreElement = new EntryElement("Genre".Translate(),"",song.Genre);
			discElement = new EntryElement("Disc".Translate(),"",song.Disc.ToString());
			discElement.KeyboardType = MonoTouch.UIKit.UIKeyboardType.NumberPad;
			trackElement = new EntryElement("Track".Translate(),"",song.Track.ToString());
			trackElement.KeyboardType = MonoTouch.UIKit.UIKeyboardType.NumberPad;
			
			Root = new RootElement("Edit Track".Translate())
			{
				new Section(){
					titleElement,
					artistElement,
					albumArtistElement,
					albumElement,
					genreElement,
					discElement,
					trackElement,					
				}
			};
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.NavigationItem.LeftBarButtonItem = new MonoTouch.UIKit.UIBarButtonItem("Cancel".Translate(),UIBarButtonItemStyle.Plain,delegate{
				this.NavigationController.PopViewControllerAnimated(true);
			});
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem("Save".Translate(),UIBarButtonItemStyle.Done,delegate{
				titleElement.FetchValue();
				artistElement.FetchValue();
				albumElement.FetchValue();
				albumArtistElement.FetchValue();
				genreElement.FetchValue();
				discElement.FetchValue();
				trackElement.FetchValue();
				Song.Title = titleElement.Value;
				Song.Album = albumElement.Value;
				Song.AlbumArtist = albumArtistElement.Value;
				//Song.Genre = genreElement.Value;
				Song.Artist = artistElement.Value;
				
				try
				{
					Song.Disc = int.Parse(discElement.Value);
				}
				catch(Exception ex)
				{
					
				}
				try
				{
					Song.Track = int.Parse(trackElement.Value);
				}
				catch(Exception ex)
				{
					
				}
				
				Util.Api.EditSong(Song,(success)=> {
					Console.WriteLine(success);
					this.BeginInvokeOnMainThread(delegate{
						if(!success)
						{
							var failedAlert = new BlockAlertView("Error".Translate(),"There was an error editing the track. Please try again.".Translate());
							failedAlert.AddButton("Ok".Translate(),null);
							failedAlert.Show();
						}
						else
							this.NavigationController.PopViewControllerAnimated(true);
					});
				});
			});
		}
	}
}

