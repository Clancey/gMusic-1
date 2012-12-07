using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Redth.MonoForAndroid;

namespace Redth.MonoForAndroid.Sample
{
	[Activity (Label = "Sectioned List", MainLauncher = true)]
	public class SectionedListActivity : ListActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			var adapter1 = new ArrayAdapter<string>(this,
				Android.Resource.Layout.SimpleListItem1,
				new string[] {
					"First Item",
					"Second Item",
					"Third Item"
				});

			var adapter2 = new ArrayAdapter<string>(this,
				Android.Resource.Layout.SimpleListItem1,
				new string[] {
					"Fourth Item",
					"Fifth Item",
					"Sixth Item",
					"Seventh Item",
					"Eighth Item",
					"Ninth Item",
					"Tenth Item",
					"Eleventh Item"
				});
			
			var adapter3 = new ArrayAdapter<string>(this,
				Android.Resource.Layout.SimpleListItem1,
				new string[] {
					"Twelvth Item",
					"Thirteenth Item",
					"Fourteenth Item",
					"Fifteenth Item",
					"Sixteenth Item",
					"Seventeenth Item"
				});
			
			var sectionedAdapter = new Redth.MonoForAndroid.SectionedAdapter(this, Resource.Layout.SectionedListSeparator);
			sectionedAdapter.AddSection("Section 1", adapter1);
			sectionedAdapter.AddSection("Section 2", adapter2);
			sectionedAdapter.AddSection("Section 3", adapter3);

			this.ListAdapter = sectionedAdapter;
		}
	}
}


