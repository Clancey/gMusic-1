using System;

namespace GoogleMusic
{
	public interface ISearchable
	{
		void StartSearch ();

		void FinishSearch ();

		void PerformFilter (string text);
		
		void SearchButtonClicked (string text);
		
	}
}

