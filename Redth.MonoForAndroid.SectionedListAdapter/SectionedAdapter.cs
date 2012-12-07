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
	public abstract class SectionedAdapter<T> : BaseAdapter<object>
	{
		const int TYPE_SECTION_HEADER = 0;

		Context context;
		LayoutInflater inflater;
		int sectionedListSeparator = 0;
		
		public SectionedAdapter(Context context, int sectionedListSeparatorLayout = Android.Resource.Layout.SimpleListItem1)
		{
			this.context = context;
			this.inflater = LayoutInflater.From(context);
			
			this.sectionedListSeparator = sectionedListSeparatorLayout;
		}

		public override int Count
		{
			get 
			{
				int count = 0;

				//Get each adapter's count + 1 for the header
				var section = NumberOfSections();
				for(int i = 0; i < section; i++)
					count += RowsInSection(i) + 1;

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
				var section = NumberOfSections();
				for(int i = 0; i < section; i++)
					viewTypeCount += RowsInSection(i);

				return viewTypeCount;
			}
		}
		public abstract int RowsInSection (int section);

		public abstract int NumberOfSections ();

		public abstract int GetItemViewType (int section, int row);

		public abstract ICell GetICell (int section, int position);

		public abstract string HeaderForSection(int section);
		
		public abstract string[] SectionIndexTitles ();
		
		public abstract void RowSelected(T item);
		
		public abstract T ItemFor (int section, int row);

		public override bool AreAllItemsEnabled()
		{
			return false;
		}
//
//		public override int GetItemViewType(int position)
//		{
//			if (position == 0)
//				return (TYPE_SECTION_HEADER);
//			// start counting from here
//			int typeOffset = TYPE_SECTION_HEADER + 1;
//			
//			var section = NumberOfSections();
//			for (int i = 0; i < section; i++) {
//				int size = RowsInSection(i) + 1;
//				
//				if (position < size)
//					return (typeOffset + GetItemViewType(i,position - 1));
//				
//				position -= size;
//				typeOffset += s.Adapter.ViewTypeCount;
//			}
//
//			foreach (var s in Sections)
//			{
//				if (position == 0)
//					return (TYPE_SECTION_HEADER);
//	
//				int size = s.Adapter.Count + 1;
//
//				if (position < size)
//					return (typeOffset + s.Adapter.GetItemViewType(position - 1));
//			
//				position -= size;
//				typeOffset += s.Adapter.ViewTypeCount;
//			}
//
//			return -1;
//		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = this[position];
			if(item is ICell)
				return ((ICell)item).GetCell(convertView, parent, inflater);
			var sectionCount = NumberOfSections();
			TextView separator = convertView as TextView;
			
			if (separator == null)
			{
				
				separator = (inflater.Inflate(sectionedListSeparator, null) as TextView);
			}
			
			separator.Text = item.ToString ();
			return separator;
		}

		public override object this [int position] {
			get {
				var sectionCount = NumberOfSections();
				for(int sectionIndex = 0; sectionIndex < sectionCount; sectionIndex ++)
				{
					if (position == 0) 
					{
						return HeaderForSection(sectionIndex);
					}
					
					int size = RowsInSection(sectionIndex) + 1;
					
					if (position < size)
						return GetICell(sectionIndex,position - 1);
					
					position -= size;
					sectionIndex++;
				}
				
				return null;
			}
		}


	}
}