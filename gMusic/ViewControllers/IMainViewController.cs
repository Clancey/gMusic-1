using System;

namespace GoogleMusic
{
	public interface IMainViewController
	{
		void ShowStatus(string message);
		void HideStatus();
		void UpdateStatus(float percent);

		void UpdateSong (Song currentSong);
		
		void SetState (bool state);
		
		void PlaylistChanged ();
		
		void UpdateStatus (string currentTime, string remainingTime, float percent);
		
		void SetPlayCount ();
		
		void ShowNowPlaying ();
		
		void RefreshSongs ();
		
		void RefreshArtists ();
		
		void RefreshGenre ();
		
		void RefreshAlbum ();
		
		void RefreshPlaylist ();
		
		void UpdateSongProgress (float percent);
		
		void UpdatePlaylistProgress (float percent);
		
		void UpdateCurrentSongDownloadProgress (float percent);
		
		void UpdateMeter ();
		void GoToArtist(int artistId);
		void GoToAlbum(int albumId);
		void GoToGenre(int genreId);
		
		void ToggleMenu ();
		void DownloaderUpdated();
	}
}

