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

namespace Redth.MonoForAndroid
{
	public class SectionedAdapter : BaseAdapter<Section>
	{
		const int TYPE_SECTION_HEADER = 0;

		Context context;
		LayoutInflater inflater;
		int sectionedListSeparator = 0;
		
		public SectionedAdapter(Context context, int sectionedListSeparatorLayout)
		{
			this.context = context;
			this.inflater = LayoutInflater.From(context);
			this.Sections = new List<Section>();
			
			this.sectionedListSeparator = sectionedListSeparatorLayout;
		}

		public List<Section> Sections
		{
			get;
			set;
		}

		public override int Count
		{
			get 
			{
				int count = 0;

				//Get each adapter's count + 1 for the header
				foreach (var s in Sections)
					count += s.Adapter.Count + 1;

				return count;
			}
		}

		public override int ViewTypeCount
		{
			get
			{
				//The headers count as a view type too
				int viewTypeCount = 1;

				//Get each adapter's ViewTypeCount
				foreach (var s in Sections)
					viewTypeCount += s.Adapter.ViewTypeCount;

				return viewTypeCount;
			}
		}

		public override Section this[int position]
		{
			get { return this.Sections[position]; }
		}

		public override bool AreAllItemsEnabled()
		{
			return false;
		}

		public override int GetItemViewType(int position)
		{
			// start counting from here
			int typeOffset = TYPE_SECTION_HEADER + 1;

			foreach (var s in Sections)
			{
				if (position == 0)
					return (TYPE_SECTION_HEADER);
	
				int size = s.Adapter.Count + 1;

				if (position < size)
					return (typeOffset + s.Adapter.GetItemViewType(position - 1));
			
				position -= size;
				typeOffset += s.Adapter.ViewTypeCount;
			}

			return -1;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public void AddSection(string caption, BaseAdapter adapter)
		{
			this.Sections.Add(new Section() { Caption = caption, Adapter = adapter });
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			int sectionIndex = 0;

			foreach (var s in Sections)
			{
				if (position == 0) 
				{
					TextView separator = convertView as TextView;

					if (separator == null)
					{
						//string xmlLayout = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><TextView xmlns:android=\"http://schemas.android.com/apk/res/android\" android:id=\"@+id/separator\"	android:layout_width=\"fill_parent\"	android:layout_height=\"wrap_content\"	android:gravity=\"center\"	android:text=\"\"	style=\"?android:attr/listSeparatorTextViewStyle\" />";

						//separator = new TextView(this.context, null, Android.Resource.Attribute.ListSeparatorTextViewStyle);
												
						//separator = inflater.Inflate(new System.Xml.XmlTextReader(new System.IO.StringReader(xmlLayout)), null) as TextView;
						
						separator = (inflater.Inflate(sectionedListSeparator, null) as TextView);
					}
					
					separator.Text = s.Caption;

					return separator;
				}

				int size = s.Adapter.Count + 1;

				if (position < size)
					return (s.Adapter.GetView(position - 1, convertView, parent));
			
				position -= size;
				sectionIndex++;
			}

			return null;
		}
	}
}