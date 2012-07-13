using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TasksSimplified
{
    [BroadcastReceiver(Label = "@string/widget_name")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE", "com.refractored.tasks.simplified.UPDATE_WIDGET" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/task_widget_info")]
    public class TaskWidgetProvider : AppWidgetProvider
    {
        public const string UpdateIntent = "com.refractored.tasks.simplified.UPDATE_WIDGET";
        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == UpdateIntent)
                OnUpdate(context, null, null);
            else
                base.OnReceive(context, intent);

        }

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            // To prevent any ANR timeouts, we perform the update in a service
            context.StartService(new Intent(context, typeof(UpdateService)));
        }
    }


    [BroadcastReceiver(Label = "@string/widget_name_large")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE", "com.refractored.tasks.simplified.UPDATE_WIDGET" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/task_widget_info_large")]
    public class TaskWidgetProviderLarge : TaskWidgetProvider
    {
        
    }
}