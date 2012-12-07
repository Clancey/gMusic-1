
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

namespace GoogleMusic
{
	[Activity (Label = "BaseActivity")]			
	public class BaseActivity : ListActivity
	{
		Intent serviceIntent;
		BaseReceiver receiver;
		BaseServiceConnection connection;
		internal string ActionFilter;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!MainService.IsRunning) {
				serviceIntent = new Intent ("com.iis.musicService");
				connection = new BaseServiceConnection(this);
			}
			receiver = new BaseReceiver ();
		}
		protected override void OnStart ()
		{
			base.OnStart ();
			var intentFilter = new IntentFilter (ActionFilter){Priority = (int)IntentFilterPriority.HighPriority};
			RegisterReceiver (receiver, intentFilter);

			if (!MainService.IsRunning) {
				BindService (serviceIntent, connection, Bind.AutoCreate);
				var pendingServiceIntent = PendingIntent.GetService (this, 0, serviceIntent, PendingIntentFlags.CancelCurrent);
				pendingServiceIntent.Send();
			}
		}
		public virtual void HandleRefresh()
		{

		}

		class BaseReceiver : BroadcastReceiver
		{
			public override void OnReceive (Context context, Android.Content.Intent intent)
			{
				((BaseActivity)context).HandleRefresh ();
				
				InvokeAbortBroadcast ();
			}
		}
		class BaseServiceConnection : Java.Lang.Object, IServiceConnection
		{
			BaseActivity activity;
			
			public BaseServiceConnection (BaseActivity activity)
			{
				this.activity = activity;
			}
			
			public void OnServiceConnected (ComponentName name, IBinder service)
			{
//				var stockServiceBinder = service as StockServiceBinder;
//				if (stockServiceBinder != null) {
//					var binder = (StockServiceBinder)service;
//					activity.binder = binder;
//					activity.isBound = true;
//				}
			}
			
			public void OnServiceDisconnected (ComponentName name)
			{
				//activity.isBound = false;
			}
		}
	}
}

