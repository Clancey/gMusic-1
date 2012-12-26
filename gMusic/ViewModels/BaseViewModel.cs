using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Tables;


namespace GoogleMusic
{
	public abstract class BaseViewModel <T> : TableViewModel<T> where T : new ()
	{
			#if iOS
			
			public BaseViewModel (IBaseViewController parent)
			{
				tableView = parent;
			}
			
			#elif Droid
		public BaseViewModel (Android.Content.Context context, Android.Widget.ListView list,IBaseViewController parent ) : base (context, list)
			{
				tableView = parent;
			}
			#endif
			
			
			protected IBaseViewController tableView;
			public bool IsSearching {get;set;}
			public List<T> SearchResults = new List<T>();

		
		public void PrecachData()
		{
			Database.Main.Precache<T> ();
		}
		#region implemented abstract members of TableViewModel

		public override int RowsInSection (int section)
		{
			return Database.Main.RowsInSection<T> (section);
		}

		public override int NumberOfSections ()
		{
			return Database.Main.NumberOfSections<T> ();
		}

		public override int GetItemViewType (int section, int row)
		{
			throw new NotImplementedException ();
		}


		public override string HeaderForSection (int section)
		{
			return Database.Main.SectionHeader<T> (section);
		}

		public override string[] SectionIndexTitles ()
		{
			return Database.Main.QuickJump<T> ();
		}

		public override T ItemFor (int section, int row)
		{
			return Database.Main.ObjectForRow<T>(section,row);
		}

		#endregion
	}
}

