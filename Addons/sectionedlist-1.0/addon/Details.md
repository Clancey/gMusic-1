A simple, abstracted list adapter to create a Sectioned list (with group headers) in your Mono for Android application, similar to Grouped lists in iOS.

You use it by creating a List Adapter of your choosing for each section you want in your list.  The Sectioned List Adapter then automatically displays each section with its items for you!

In your ListActivity, you can use the following code:
[code:csharp] 
    //Create multiple adapters for each 'section'
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
    
    var sectionedAdapter = new Redth.MonoForAndroid.SectionedAdapter(this, 
        Resource.Layout.SectionedListSeparator);
    sectionedAdapter.AddSection("Section 1", adapter1);
    sectionedAdapter.AddSection("Section 2", adapter2);
    sectionedAdapter.AddSection("Section 3", adapter3);
    
    //Set the ListView's ListAdapter to our newly created adapter
    this.ListAdapter = sectionedAdapter;
[code]