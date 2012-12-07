Installation
=======================

Installation: Extract the .zip file and copy the *.dll file into your project 
directory. Then simply "Add Reference" the *.dll into your Mono
for Android project.

You should also copy the Resources/Layout/*.axml files into the same directory
of your Mono for Android project, and add those existing files into the project
as AndroidResource build type.


Using the Add-on
================

In your ListActivity, you can use the following code:

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
			

Contact / Discuss
=================

- Email: jon@altusapps.com
- Twitter: http://twitter.com/redth
