
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
using Android.Graphics;

namespace GoogleMusic
{

	public class MenuFragment : ListFragment
	{
		public MenuFragment ()
		{
			MenuItems = new MenuItem[]{
				new MenuItem{
					Title = "Playlists",
					Content = new PlaylistFragment(),
				},
				new MenuItem{
					Title = "Songs",
					Content = new SongFragment(),
				},
			};
		}
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.list, null);
		}
		MenuAdapater menuAdapater;
		public MenuItem[] MenuItems = new MenuItem[0];
		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);

			ListAdapter = menuAdapater = new MenuAdapater (this,MenuItems);
		}
		public override void OnListItemClick (ListView l, View v, int position, long id)
		{
			Fragment newContent = menuAdapater [position].Content;
			
			if (newContent != null)
				switchFragment(newContent);

		}
		private void switchFragment(Fragment fragment) {
			if (Activity == null)
				return;
			
			if (Activity is FragmentChangeActivity) {
				FragmentChangeActivity fca = (FragmentChangeActivity) Activity;
				fca.switchContent(fragment);
			}
		}

		class MenuAdapater : BaseAdapter<MenuItem>
		{
			#region implemented abstract members of BaseAdapter
			MenuItem[] Items = new MenuItem[0];
			MenuFragment Parent;
			public MenuAdapater(MenuFragment parent,MenuItem[] items)
			{
				Parent = parent;
				Items = items;
			}
			public override long GetItemId (int position)
			{
				return position;
			}

			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				View view = null; // re-use an existing view, if one is available
				int type = Android.Resource.Layout.SimpleListItem1;
				if (view == null) // otherwise create a new one
					view = LayoutInflater.From (Parent.Activity).Inflate(type , null);

				var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
				textView.Text = this[position].Title;
				textView.SetTextColor (Color.White);
				textView.SetBackgroundColor(Color.Transparent);
				return view;
			}

			public override int Count {
				get {
					return Items.Length;
				}
			}

			#endregion

			#region implemented abstract members of BaseAdapter

			public override MenuItem this [int position] {
				get {
					return Items[position];
				}
			}

			#endregion


		}
	}

	public class MenuItem {
		public string Title {get;set;}
		public Fragment Content {get;set;}
	}
}

