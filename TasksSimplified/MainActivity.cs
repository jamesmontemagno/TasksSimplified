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

using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
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
    internal class SaveState :Java.Lang.Object
    {
        public TaskModel[] Tasks { get; set; }
        public string NewTaskText { get; set; }
        public int LastPosition { get; set; }
        public int EditIndex { get; set; }
        public bool Editing { get; set; }
    }

    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/ic_launcher")]
    public class MainActivity : ActionBarListActivity, TextToSpeech.IOnInitListener
    {
        private TextToSpeech m_TextToSpeech;

        private JavaList<TaskModel> m_AllTasks;
        private int m_EditTaskPosition;

        private EditText m_TaskEditText;
        private ImageButton m_AddButton;
        private ImageButton m_MicrophoneButton;
        private bool m_Editing;
        protected override void OnCreate(Bundle bundle) 
        {
            SetTheme(Settings.ThemeSetting == 0 ? Resource.Style.MyTheme : Resource.Style.MyThemeDark);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            m_AllTasks = new JavaList<TaskModel>();
            m_EditTaskPosition = 0;

            m_AddButton = FindViewById<ImageButton>(Resource.Id.button_add_task);
            m_MicrophoneButton = FindViewById<ImageButton>(Resource.Id.button_microphone);
            m_TaskEditText = FindViewById<EditText>(Resource.Id.edit_text_new_task);

            ActionBar = FindViewById<ActionBar.ActionBar>(Resource.Id.actionbar);
            ActionBar.SetTitle("Tasks");
            ActionBar.CurrentActivity = this;
            ActionBar.SetHomeLogo(Resource.Drawable.ic_launcher);
            RegisterForContextMenu(ListView);


            SetupMainActionBar();

            ListView.ChoiceMode = ChoiceMode.Multiple;

            

            m_AddButton.Click += (sender, args) =>
                             {

                                 var task = m_TaskEditText.Text.Trim();

                                 if (string.IsNullOrWhiteSpace(task))
                                     return;

                                 var newTask = new TaskModel {Task = task};

                                 try
                                 {
                                     DataManager.SaveTask(newTask);
                                     m_AllTasks.Add(newTask);
                                     m_TaskEditText.Text = string.Empty;
                                     
                                     RunOnUiThread(() =>
                                     {
                                         ((TaskAdapter)ListAdapter).NotifyDataSetChanged();
                                         ListView.SetSelection(m_AllTasks.Count - 1);
                                     });
                                 }
                                 catch (Exception)
                                 {
                                     RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_save,
                                                                        ToastLength.Short).Show());
                                      
                                 }
                             };

            m_AddButton.SetImageResource(Settings.ThemeSetting == 0 ? Resource.Drawable.ic_action_add : Resource.Drawable.ic_action_add_dark);
            m_MicrophoneButton.SetImageResource(Settings.ThemeSetting == 0 ? Resource.Drawable.ic_action_microphone : Resource.Drawable.ic_action_microphone_dark);

            // remove speech if it doesn't exist
            var activities =PackageManager.QueryIntentActivities(new Intent(RecognizerIntent.ActionRecognizeSpeech), 0);
            if (activities.Count == 0)
            {
                m_MicrophoneButton.Visibility = ViewStates.Gone;
            }
            else
            {
                m_MicrophoneButton.Click += (sender, args) => StartVoiceRecognitionActivity();
            }

            m_TextToSpeech = new TextToSpeech(this, this);

            var saveState = LastNonConfigurationInstance as SaveState;
            if (saveState != null)
            {
                m_AllTasks = new JavaList<TaskModel>(saveState.Tasks);
                RunOnUiThread(() => ListAdapter = new TaskAdapter(this, m_AllTasks));
                RunOnUiThread(() => ListView.SetSelection(saveState.LastPosition));
                m_TaskEditText.Text = saveState.NewTaskText;

                if(saveState.Editing)
                {
                    m_EditTaskPosition = saveState.EditIndex;
                    SetupEditActionBar();
                }
            }
            else
            {
                FlurryAgent.OnPageView();
                FlurryAgent.LogEvent("MainActivity");
                ReloadData(0);
            }

            if (Intent.ActionSend != Intent.Action || Intent.Type == null)
                return;

            if ("text/plain" != Intent.Type)
                return;

            var sharedText = Intent.GetStringExtra(Intent.ExtraText);
            if (!string.IsNullOrEmpty(sharedText))
            {
                m_TaskEditText.Text = sharedText;
                if(ListView.GetCheckItemIds().Length == 0)
                    SetupMainActionBar();
                else
                    SetupDeleteActionBar();
            }
        }

        public override Java.Lang.Object OnRetainNonConfigurationInstance()
        {
            return new SaveState
                       {
                           NewTaskText = m_TaskEditText.Text,
                           Tasks = m_AllTasks.ToArray(),
                           LastPosition = ListView.SelectedItemPosition, 
                           EditIndex = m_EditTaskPosition,
                           Editing = m_Editing
                       };
        }

        private void StartVoiceRecognitionActivity()
        {
            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel,RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak new task...");
            StartActivityForResult(intent, 0);
        }
        
        private void Speak(string message)
        {
            if (m_TextToSpeech == null || !Settings.TalkBackEnabled)
                return;

            m_TextToSpeech.Speak(message, QueueMode.Flush, null);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 0 && resultCode == Result.Ok)
            {
                // Populate the wordsList with the String values the recognition engine thought it heard
                var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);

                if(matches.Count == 0)
                {
                    Speak("Heard nothing.");
                }
                else
                {
                    var newTask = matches[0];
                   
                    m_TaskEditText.Text = newTask;

                    Speak(newTask);
                }
            }
            
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void SetupMainActionBar()
        {
            if (DarkMenuId == Resource.Menu.MainMenu)
                return;

            ActionBar.SetTitle("Tasks");

            ActionBar.RemoveAllActions();
            
            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete_all, Resource.Drawable.ic_menu_delete_all,
                                                     Resource.String.menu_string_delete_all)
                             {ActionType = ActionType.Never};

            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_about, Resource.Drawable.ic_menu_settings,
                                                     Resource.String.menu_string_about) {ActionType = ActionType.Never};
            ActionBar.AddAction(action);

            DarkMenuId = Resource.Menu.MainMenu;
            MenuId = Resource.Menu.MainMenu;

            m_AddButton.Visibility = ViewStates.Visible;
            ListView.Enabled = true;
            m_Editing = false;
        }

        private void SetupDeleteActionBar()
        {
            if (DarkMenuId == Resource.Menu.MainMenuDelete)
                return;

            ActionBar.SetTitle("Delete Tasks");
            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete, Resource.Drawable.ic_action_delete_dark,
                                                    Resource.String.menu_string_delete) {ActionType = ActionType.Always};
            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_cancel, Resource.Drawable.ic_action_cancel_dark,
                                                     Resource.String.menu_string_cancel)
                         {ActionType = ActionType.Always};
            ActionBar.AddAction(action);

            DarkMenuId = Resource.Menu.MainMenuDelete;
            MenuId = Resource.Menu.MainMenuDelete;

            m_AddButton.Visibility = ViewStates.Visible;
            ListView.Enabled = true;
            m_Editing = false;
        }

        private void SetupEditActionBar()
        {
            if (DarkMenuId == Resource.Menu.MainMenuEdit)
                return;

            ActionBar.SetTitle("Edit Task");
            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_save, Resource.Drawable.ic_action_save_dark,
                                                   Resource.String.menu_string_save) { ActionType = ActionType.Always };
            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_cancel_save, Resource.Drawable.ic_action_cancel_dark,
                                                     Resource.String.menu_string_cancel) { ActionType = ActionType.Always };
            ActionBar.AddAction(action);

            DarkMenuId = Resource.Menu.MainMenuEdit;
            MenuId = Resource.Menu.MainMenuEdit;

            m_AddButton.Visibility = ViewStates.Gone;
            ListView.Enabled = false;
            m_Editing = true;
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);

            if (ListView.GetCheckItemIds().Length > 0)
            {
                SetupDeleteActionBar();
            }
            else
            {
                SetupMainActionBar();
            }

            m_AllTasks[position].Checked = l.IsItemChecked(position);
            RunOnUiThread(() => ((TaskAdapter) ListAdapter).NotifyDataSetChanged());

            try
            {
                DataManager.SaveTask(m_AllTasks[position]);
            }
            catch (Exception)
            {
                RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_save,
                                                   ToastLength.Short).Show());

            }

        }

        private void ReloadData(int startId)
        {
            m_AllTasks.Clear();
            foreach(var task in DataManager.GetTasks())
            {
                m_AllTasks.Add(task);
            }


#if DEBUG2
            if(m_AllTasks.Count == 0)
            {
                m_AllTasks = new JavaList<TaskModel>();
                foreach (var item in m_FakeData)
                {
                    var task = new TaskModel {Task = item};
                    task.ID = DataManager.SaveTask(task);
                    m_AllTasks.Add(task);
                }
            }
#endif
            RunOnUiThread(() => ListAdapter = new TaskAdapter(this, m_AllTasks));

            for (int i = 0; i < m_AllTasks.Count; i++ )
            {
                ListView.SetItemChecked(i, m_AllTasks[i].Checked);
            }

            if(ListView.GetCheckItemIds().Length > 0)
                SetupDeleteActionBar();
            else
                SetupMainActionBar();



            if (startId == 0)
                    return;

            var firstTask = m_AllTasks.FirstOrDefault(t => t.ID == startId);
            if(firstTask == null)
                return;

            var itemIndex = m_AllTasks.IndexOf(firstTask);
            RunOnUiThread(()=> ListView.SetSelection(itemIndex));

        }

        private void DeleteAll()
        {

            if (m_AllTasks.Count == 0)
                return;

            Util.ShowOkCancelPopup(this, Resource.String.confirm_delete_title, Resource.String.confirm_delete, delete =>
            {
                if (!delete)
                    return;

                try
                {
                    DataManager.DeleteTasks();
                    ReloadData(0);
                    RunOnUiThread(() => Toast.MakeText(this, Resource.String.nice_work_long, ToastLength.Long).Show());
                }
                catch
                {
                    RunOnUiThread(()=>Toast.MakeText(this, Resource.String.unable_to_delete, ToastLength.Short).Show());
                }
            });



        }

        private void ReallyDeleteSelected()
        {
            var startIndex = m_AllTasks[ListView.FirstVisiblePosition].ID;
            try
            {
                for (int i = 0; i < m_AllTasks.Count; i++)
                {
                    if (!ListView.IsItemChecked(i))
                        continue;


                    DataManager.DeleteTask(m_AllTasks[i].ID);
                }

                RunOnUiThread(() => Toast.MakeText(this, Resource.String.nice_work_short, ToastLength.Long).Show());
            }
            catch (Exception)
            {

                RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_delete, ToastLength.Short).Show());
            }



            ReloadData(startIndex);
        }

        private void DeleteSelected()
        {
            
            if(ListView.GetCheckItemIds().Length > 10)
            {
                Util.ShowOkCancelPopup(this, Resource.String.confirm_delete_title, Resource.String.confirm_delete,
                                       delete =>
                                           {
                                               if (!delete)
                                                   return;

                                               ReallyDeleteSelected();
                                           });
            }
            else
            {
                ReallyDeleteSelected();
            }

        }

        private void Cancel()
        {
            bool error = false;
            for(var i = 0; i < m_AllTasks.Count; i++)
            {
                m_AllTasks[i].Checked = false;
                if (ListView.IsItemChecked(i))
                {
                    ListView.SetItemChecked(i, false);
                    try
                    {
                        DataManager.SaveTask(m_AllTasks[i]);
                    }
                    catch (Exception)
                    {
                        error = true;
                    }
                    
                }

            }

            if(error)
            {
                RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_save,
                                                ToastLength.Short).Show());
                                      
            }

            RunOnUiThread(()=>((TaskAdapter)ListAdapter).NotifyDataSetChanged());
            SetupMainActionBar();
        }

        private void CancelSave()
        {
            m_TaskEditText.Text = string.Empty;
            if(ListView.GetCheckItemIds().Length > 0)
                SetupDeleteActionBar();
            else
                SetupMainActionBar();
           
        }

        private void Save()
        {
            m_AllTasks[m_EditTaskPosition].Task = m_TaskEditText.Text.Trim();

            try
            {
                DataManager.SaveTask(m_AllTasks[m_EditTaskPosition]);
            }
            catch (Exception)
            {
                RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_save,
                                                   ToastLength.Short).Show());

            }

            m_TaskEditText.Text = string.Empty;

            RunOnUiThread(() => ((TaskAdapter)ListAdapter).NotifyDataSetChanged());

            if (ListView.GetCheckItemIds().Length > 0)
                SetupDeleteActionBar();
            else
                SetupMainActionBar();
            
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);

            m_EditTaskPosition = ((AdapterView.AdapterContextMenuInfo)menuInfo).Position;

            menu.SetHeaderTitle(m_AllTasks[m_EditTaskPosition].Task);
            
            MenuInflater.Inflate(Resource.Menu.ContextMenuTask, menu);
           
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Resource.Id.menu_edit_task:
                    SetupEditActionBar();
                    m_TaskEditText.Text = m_AllTasks[m_EditTaskPosition].Task;
                    m_TaskEditText.RequestFocus();
                    return true;
            }

            return base.OnContextItemSelected(item);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Resource.Id.menu_about:
                    var intent = new Intent(this, typeof(AboutActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.menu_cancel:
                    Cancel();
                    break;
                case Resource.Id.menu_delete:
                    DeleteSelected();
                    break;
                case Resource.Id.menu_delete_all:
                    DeleteAll();
                    break;
                case Resource.Id.menu_cancel_save:
                    CancelSave();
                    break;
                case Resource.Id.menu_save:
                    Save();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void OnInit(OperationResult status)
        {
            if(status != OperationResult.Success)
            {
                m_TextToSpeech = null;
            }
        }
    }
}

