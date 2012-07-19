/*
 * Copyright (C) 2012 James Montemagno (http://www.montemagno.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *          http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;
using TasksSimplified.ActionBarBase;
using TasksSimplified.ActionBar;
using TasksSimplified.Adapter;
using TasksSimplified.BusinessLayer;
using TasksSimplified.DataAccessLayer;
using TasksSimplified.Helpers;

namespace TasksSimplified
{
    internal class SaveStateHistory :Java.Lang.Object
    {
        public ClearedTaskModel[] Tasks { get; set; }
        public int LastPosition { get; set; }
    }

    [Activity(Label = "@string/completed_tasks", Icon = "@drawable/ic_launcher")]
    public class HistoryActivity : ActionBarListActivity
    {
        private TextToSpeech m_TextToSpeech;

        private JavaList<ClearedTaskModel> m_AllTasks;

        protected override void OnCreate(Bundle bundle) 
        {
            SetTheme(Settings.ThemeSetting == 0 ? Resource.Style.MyTheme : Resource.Style.MyThemeDark);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.History);


            m_AllTasks = new JavaList<ClearedTaskModel>();
            ActionBar = FindViewById<ActionBar.ActionBar>(Resource.Id.actionbar);
            ActionBar.TitleRaw = Resource.String.completed_tasks;
            ActionBar.CurrentActivity = this;
            AddHomeAction();
            RegisterForContextMenu(ListView);


            var saveState = LastNonConfigurationInstance as SaveStateHistory;
            if (saveState != null)
            {
                m_AllTasks = new JavaList<ClearedTaskModel>(saveState.Tasks);
                RunOnUiThread(() => ListAdapter = new ClearedTaskAdapter(this, m_AllTasks));
                RunOnUiThread(() => ListView.SetSelection(saveState.LastPosition));
            }
            else
            {
                FlurryAgent.OnPageView();
                FlurryAgent.LogEvent("HistoryActivity");
                ReloadData(0);
            }

            SetupMainActionBar();
        }

        

        public override Java.Lang.Object OnRetainNonConfigurationInstance()
        {
            return new SaveState
                       {
                           Tasks = m_AllTasks.ToArray(),
                           LastPosition = ListView.SelectedItemPosition
                       };
        }

        

        private void SetupMainActionBar()
        {
            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_share, Resource.Drawable.ic_action_share_dark,
                                                   Resource.String.menu_string_share_history) { ActionType = ActionType.Always };

            ActionBar.AddAction(action); 
            
            ActionBar.Title = ActionBar.Title + " - " + m_AllTasks.Count;

            DarkMenuId = Resource.Menu.HistoryMenu;
            MenuId = Resource.Menu.HistoryMenu;
        }

        private void ReloadData(int startId)
        {
            m_AllTasks.Clear();
            foreach(var task in DataManager.GetClearedTasks(SortOption.Newest))
            {
                m_AllTasks.Add(task);
            }


            RunOnUiThread(() => ListAdapter = new ClearedTaskAdapter(this, m_AllTasks));

            if (startId == 0)
                   return;

            var firstTask = m_AllTasks.FirstOrDefault(t => t.ID == startId);
            if(firstTask == null)
                return;

            var itemIndex = m_AllTasks.IndexOf(firstTask);
            RunOnUiThread(()=> ListView.SetSelection(itemIndex));

        }

        private int m_EditTaskPosition;
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);

            m_EditTaskPosition = ((AdapterView.AdapterContextMenuInfo)menuInfo).Position;

            menu.SetHeaderTitle(m_AllTasks[m_EditTaskPosition].Task);
            
            MenuInflater.Inflate(Resource.Menu.ContextMenuHistoryTask, menu);
           
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Resource.Id.menu_share_task:

                     var intent = new Intent(Intent.ActionSend);
                    intent.SetType("text/plain");

                    var shareMessage = string.Format(Resources.GetString(Resource.String.share_single_history_message), m_AllTasks[m_EditTaskPosition].Task,
                    m_AllTasks[m_EditTaskPosition].DateCompleted.ToLocalTime().ToShortDateString());

                    intent.PutExtra(Intent.ExtraText, shareMessage);
                    StartActivity(Intent.CreateChooser(intent, Resources.GetString(Resource.String.share)));

                    return true;
            }

            return base.OnContextItemSelected(item);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Resource.Id.menu_share:
                    var intent = new Intent(Intent.ActionSend);
                    intent.SetType("text/plain");

                    var shareMessage = string.Format(Resources.GetString(Resource.String.share_history_message), m_AllTasks.Count);

                    intent.PutExtra(Intent.ExtraText, shareMessage);
                    StartActivity(Intent.CreateChooser(intent, Resources.GetString(Resource.String.share)));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}

