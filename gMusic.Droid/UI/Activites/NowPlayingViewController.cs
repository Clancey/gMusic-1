
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Tables;

namespace GoogleMusic
{

	public class NowPlayingViewController : Fragment, IViewController, INowPlayingViewController
	{
		public UINavigationController NavigationController { get; set; }
		public string Title {get;set;}
		public MediaController mediaController;
		public SeekBar progressBar;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View v = inflater.Inflate (Resource.Layout.nowplaying, null, false);
			var mediaController = new MediaController (this.Activity);
			mediaController.SetAnchorView (v);
			mediaController.NextClick += (object sender, EventArgs e) => {
				Console.WriteLine("next");
			};
			mediaController.PreviousClick += (object sender, EventArgs e) => {
				Console.WriteLine("previous");
			};
			var mp = new MyMediaPlayer ();

			mediaController.SetMediaPlayer (mp);
			//mediaController.SetAnchorView (v);

			progressBar = v.FindViewById<SeekBar> (Resource.Id.seekBar1);
			progressBar.StartTrackingTouch += (object sender, SeekBar.StartTrackingTouchEventArgs e) => {
				mediaController.Show(0);
			};
			return v;

		}
		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);


		}
		public override void OnResume ()
		{
			base.OnResume ();

		}
		#region INowPlayingViewController implementation
		public void Update ()
		{
			
		}
		#endregion
		private class MyMediaPlayer : Java.Lang.Object ,MediaController.IMediaPlayerControl
		{
			#region IMediaPlayerControl implementation

			public bool CanPause ()
			{
				return true;
			}

			public bool CanSeekBackward ()
			{
				return true;
			}

			public bool CanSeekForward ()
			{
				return true;
			}

			public void Pause ()
			{
				Util.Player.Pause ();
			}

			public void SeekTo (int pos)
			{

				//Util.Seek(
			}

			public void Start ()
			{
				Util.Player.Play ();
			}

			public int BufferPercentage {
				get {
					return (int)Util.Player.CurrentlyPlayingSong.DownloadPercent * 100;
				}
			}

			public int CurrentPosition {
				get {
					return (int)Util.Player.Progress  * 100;
				}
			}

			public int Duration {
				get {
					return Util.Player.CurrentlyPlayingSong.Duration;
				}
			}

			public bool IsPlaying {
				get {
					return Util.Player.CurrentState == StreamingPlayback.State.Playing;
				}
			}

			#endregion


		}
	}

}