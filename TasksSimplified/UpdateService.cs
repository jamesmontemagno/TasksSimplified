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
using TasksSimplified.DataAccessLayer;
using TasksSimplified.Helpers;

namespace TasksSimplified
{
    [Service]
    public class UpdateService : Service
    {
        
        public override void OnStart(Intent intent, int startId)
        {
            // Build the widget update for today
            var updateViews = BuildUpdate(this);

            // Push update for this widget to the home screen
            var thisWidget = new ComponentName(this, "taskssimplified.TaskWidgetProvider");
            var manager = AppWidgetManager.GetInstance(this);
            manager.UpdateAppWidget(thisWidget, updateViews);
        }

        public override IBinder OnBind(Intent intent)
        {
            // We don't need to bind to this service
            return null;
        }

        

        private readonly int[] m_TextViews = new[]{Resource.Id.widget_task_1, Resource.Id.widget_task_2, Resource.Id.widget_task_3,
            Resource.Id.widget_task_4, Resource.Id.widget_task_5, Resource.Id.widget_task_6, Resource.Id.widget_task_7};
        // Build a widget update to show the current Wiktionary
        // "Word of the day." Will block until the online API returns.
        public RemoteViews BuildUpdate(Context context)
        {
           // var entry = WordEntry.GetWordOfTheDay();

            // Build an update that holds the updated widget contents
            var updateViews = new RemoteViews(context.PackageName, Resource.Layout.taskwidget);
            
            try
            {

           
            var launchAppIntent = new Intent(this, typeof (MainActivity));
            launchAppIntent.AddFlags(ActivityFlags.SingleTop);
            launchAppIntent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(context, 0, launchAppIntent, 0);
            updateViews.SetOnClickPendingIntent(Resource.Id.widget_full, pendingIntent);

            var newTasks = new[] {string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};
            var newTasksChecked = new[] { false, false, false, false, false, false, false };

            var tasks = DataManager.GetTasks(SortOption.Newest);
            var count = tasks.Count();
            count = count > m_TextViews.Length ? m_TextViews.Length : count;

            for (int i = 0; i < count; i++)
            {
                var task = tasks.ElementAt(i);
                newTasks[i] = task.Task;
                newTasksChecked[i] = task.Checked;
            }

            if (newTasks[0] == string.Empty)
                    newTasks[0] = "No Tasks Added.";

            for(int i = 0; i < m_TextViews.Length; i++)
            {
                updateViews.SetTextViewText(m_TextViews[i], newTasks[i]);
                updateViews.SetTextColor(m_TextViews[i], newTasksChecked[i] ? Resources.GetColor(Settings.ThemeAccentId) : Android.Graphics.Color.Black );
            }

            /*updateViews.SetTextViewText(Resource.Id.word_title, entry.Title);
            updateViews.SetTextViewText(Resource.Id.definition, entry.Description);*/
            }
            catch (Exception ex)
            {

                updateViews.SetTextViewText(Resource.Id.widget_task_1, "Error " + ex.ToString());
            }
          

            return updateViews;
        }
    }
}