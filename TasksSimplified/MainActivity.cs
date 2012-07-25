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
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Speech.Tts;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
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
    public class MainActivity : ActionBarListActivity, TextToSpeech.IOnInitListener, TextView.IOnEditorActionListener
    {
        private TextToSpeech m_TextToSpeech;

        private JavaList<TaskModel> m_AllTasks;
        private bool m_DataChanged;
        private int m_EditTaskPosition;

        private EditText m_TaskEditText;
        private ImageButton m_AddButton;
        private ImageButton m_MicrophoneButton;
        private bool m_Editing;
        private int m_OriginalTheme;
        private int m_OriginalAccent;

        private string[] m_FakeData = new string[]
                                          {
                                              "Tasks Simplified", "Beautiful", "Adorable", "Simple", "www.TasksSimplified.com",
                                              "1 List", "Tons of Themes", "The Only Task List You Need"
                                          };

        protected override void OnCreate(Bundle bundle) 
        {
            SetTheme(Settings.ThemeSetting == 0 ? Resource.Style.MyTheme : Resource.Style.MyThemeDark);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            m_OriginalTheme = Settings.ThemeSetting;
            m_OriginalAccent = Settings.ThemeAccent;

            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            m_AllTasks = new JavaList<TaskModel>();
            m_EditTaskPosition = 0;

            m_AddButton = FindViewById<ImageButton>(Resource.Id.button_add_task);
            m_MicrophoneButton = FindViewById<ImageButton>(Resource.Id.button_microphone);
            m_TaskEditText = FindViewById<EditText>(Resource.Id.edit_text_new_task);

            
            ActionBar = FindViewById<ActionBar.ActionBar>(Resource.Id.actionbar);

            //ActionBar.BackgroundDrawable = Resources.GetDrawable(Resource.Drawable.actionbar_background_blue);
            //ActionBar.ItemBackgroundDrawable = Resources.GetDrawable(Resource.Drawable.actionbar_btn_blue);
            //ActionBar.SeparatorColor = Resources.GetColor(Resource.Color.actionbar_separatorcolor_blue);
            ActionBar.Title = "Tasks";
            ActionBar.CurrentActivity = this;
            ActionBar.SetHomeLogo(Resource.Drawable.ic_launcher);
            RegisterForContextMenu(ListView);


            m_TaskEditText.SetOnEditorActionListener(this);
            

            ListView.ChoiceMode = ChoiceMode.Multiple;

            m_AddButton.Click += (sender, args) => AddNewTask();

            m_AddButton.SetImageResource(Settings.ThemeSetting == 0 ? Resource.Drawable.ic_action_add : Resource.Drawable.ic_action_add_dark);
            m_MicrophoneButton.SetImageResource(Settings.ThemeSetting == 0 ? Resource.Drawable.ic_action_microphone : Resource.Drawable.ic_action_microphone_dark);

            m_AddButton.SetBackgroundResource(Settings.ImageButtonDrawable);
            m_MicrophoneButton.SetBackgroundResource(Settings.ImageButtonDrawable);

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
                ListView.Visibility = ViewStates.Visible;
                RunOnUiThread(() => ListAdapter = new TaskAdapter(this, m_AllTasks));
                RunOnUiThread(() => ListView.SetSelection(saveState.LastPosition));
                SetChecks();
                m_TaskEditText.Text = saveState.NewTaskText;
                m_Editing = saveState.Editing;
                m_EditTaskPosition = saveState.EditIndex;
            }
            else
            {
                FlurryAgent.OnPageView();
                FlurryAgent.LogEvent("MainActivity");
                ReloadData(0);
            }

            SetActionBar();
            try
            {
                if(Intent.GetBooleanExtra("CameFromWidget", false))
                {
                    FocusMainText();
                    return;
                }

                if(Intent.Action == TaskWidgetProvider.UpdateIntent)

                if (Intent.ActionSend != Intent.Action || Intent.Type == null)
                    return;

                if ("text/plain" != Intent.Type)
                    return;

                var sharedText = Intent.GetStringExtra(Intent.ExtraText);
                if (!string.IsNullOrEmpty(sharedText))
                {
                    m_TaskEditText.Text = sharedText;
                    m_Editing = false;
                    SetActionBar();
                }
            }
            finally
            {
                var version = Resources.GetString(Resource.String.VersionNumber);
                if (Settings.CurrentVersionNumber != version)
                {
                    Settings.CurrentVersionNumber = version;
                    PopUpHelpers.ShowOKPopup(this, Resource.String.update_title, Resource.String.update_message, (ok)=>{ });
                }
            }
        }

        private void FocusMainText()
        {
            RunOnUiThread(() =>
                              {
                                  
                                  m_TaskEditText.RequestFocus();
                                  var inputService = GetSystemService(InputMethodService) as InputMethodManager;
                                  if (inputService != null)
                                      inputService.ToggleSoftInput(0,0);
                                      //inputService.ShowSoftInput(m_TaskEditText,ShowFlags.Implicit);
                              });

        }

        private void HideKeyboard()
        {
             RunOnUiThread(() =>
                               {

                                   var inputService = GetSystemService(InputMethodService) as InputMethodManager;
                                   if (inputService != null)
                                       inputService.HideSoftInputFromWindow(m_TaskEditText.WindowToken,
                                                                            HideSoftInputFlags.None);
                               });
        }

        private void AddNewTask()
        {
            var task = m_TaskEditText.Text.Trim();

            if (string.IsNullOrWhiteSpace(task))
                return;

            m_DataChanged = true;
            var newTask = new TaskModel {Task = task};

            try
            {
                DataManager.SaveTask(newTask);

                var selection = 0;
                switch(Settings.SortBy)
                {
                    case SortOption.Newest:
                        m_AllTasks.Insert(0, newTask);
                        SetChecks();
                        break;
                    case SortOption.Oldest:
                        m_AllTasks.Add(newTask);
                        selection = m_AllTasks.Count - 1;
                        break;
                }
               
                m_TaskEditText.Text = string.Empty;
                                     
                RunOnUiThread(() =>
                                  {
                                      ((TaskAdapter)ListAdapter).NotifyDataSetChanged();
                                      ListView.SetSelection(selection);
                                  });
            }
            catch (Exception)
            {
                RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_save,
                                                   ToastLength.Short).Show());
                                      
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
            intent.PutExtra(RecognizerIntent.ExtraPrompt, Resources.GetString(Resource.String.speak_new_task));
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

            ActionBar.TitleRaw = Resource.String.title_tasks;

            ActionBar.RemoveAllActions();

            var
            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_sort, Resource.Drawable.ic_menu_sort,
                                                   Resource.String.menu_string_sort) { ActionType = ActionType.Never };

            ActionBar.AddAction(action); 
            
            
            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete_all, Resource.Drawable.ic_menu_delete_all,
                                                     Resource.String.menu_string_delete_all)
                             {ActionType = ActionType.Never};

            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_history, Resource.Drawable.ic_menu_history,
                                                     Resource.String.menu_string_history) { ActionType = ActionType.Never };

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

            ActionBar.TitleRaw = Resource.String.title_delete_tasks;
            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete, Settings.UseLightIcons ? Resource.Drawable.ic_action_delete : Resource.Drawable.ic_action_delete_dark,
                                                    Resource.String.menu_string_delete) {ActionType = ActionType.Always};
            ActionBar.AddAction(action);


            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_sort, Resource.Drawable.ic_menu_sort,
                                                   Resource.String.menu_string_sort) { ActionType = ActionType.Never };

            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete_all, Resource.Drawable.ic_menu_delete_all,
                                                    Resource.String.menu_string_delete_all) { ActionType = ActionType.Never };

            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_history, Resource.Drawable.ic_menu_history,
                                                     Resource.String.menu_string_history) { ActionType = ActionType.Never };

            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_about, Resource.Drawable.ic_menu_settings,
                                                     Resource.String.menu_string_about) { ActionType = ActionType.Never };
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

            ActionBar.TitleRaw = Resource.String.title_edit_task;
            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_cancel_save, Settings.UseLightIcons ? Resource.Drawable.ic_action_cancel : Resource.Drawable.ic_action_cancel_dark,
                                                     Resource.String.menu_string_cancel) { ActionType = ActionType.Always };
            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_save, Settings.UseLightIcons ? Resource.Drawable.ic_action_save : Resource.Drawable.ic_action_save_dark,
                                                   Resource.String.menu_string_save) { ActionType = ActionType.Always };
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


            m_DataChanged = true;
            m_AllTasks[position].Checked = l.IsItemChecked(position);

            try
            {
                DataManager.SaveTask(m_AllTasks[position]);
            }
            catch (Exception)
            {
                RunOnUiThread(() => Toast.MakeText(this, Resource.String.unable_to_save,
                                                   ToastLength.Short).Show());

            }

           

            RunOnUiThread(() => ((TaskAdapter)ListAdapter).NotifyDataSetChanged());
            SetActionBar();
        }

        private void SetActionBar()
        {
            if (m_Editing)
            {
                SetupEditActionBar();
                return;
            }


    

            if (m_AllTasks.Where(t => t.Checked).Count() > 0)
            {
                SetupDeleteActionBar();
            }
            else
            {
                SetupMainActionBar();
            }
        }


        private void SetChecks()
        {
            for (int i = 0; i < m_AllTasks.Count; i++)
            {
                ListView.SetItemChecked(i, m_AllTasks[i].Checked);
            }
        }

        private void ReloadData(int startId)
        {
            m_AllTasks.Clear();
            foreach(var task in DataManager.GetTasks(Settings.SortBy))
            {
                m_AllTasks.Add(task);
            }


#if DEBUG
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
            RunOnUiThread(() =>
            {
                ListAdapter = new TaskAdapter(this, m_AllTasks);
                if (ListView.Visibility == ViewStates.Gone)
                {
                    ListView.Visibility = ViewStates.Visible;
                    ListView.StartAnimation(AnimationUtils.LoadAnimation(this, Resource.Animation.fadein));
                }
            });


            SetChecks();

            SetActionBar();

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
                m_DataChanged = true;
                try
                {
                    foreach (var task in m_AllTasks)
                        DataManager.SaveTask(new ClearedTaskModel(task));
                }
                catch{}

                try
                {
                    DataManager.DeleteTasks();
                    ReloadData(0);
                    RunOnUiThread(() => Toast.MakeText(this, Resource.String.nice_work_long, ToastLength.Short).Show());
                }
                catch
                {
                    RunOnUiThread(()=>Toast.MakeText(this, Resource.String.unable_to_delete, ToastLength.Short).Show());
                }
            });

        }

        private void ReallyDeleteSelected()
        {
            m_DataChanged = true;
            var startIndex = m_AllTasks[ListView.FirstVisiblePosition].ID;
            try
            {
                var allChecked = true;
                for (int i = 0; i < m_AllTasks.Count; i++)
                {
                    if (!ListView.IsItemChecked(i))
                    {
                        allChecked = false;
                        continue;
                    }

                    DataManager.SaveTask(new ClearedTaskModel(m_AllTasks[i]));

                    DataManager.DeleteTask(m_AllTasks[i].ID);
                }

                if(allChecked)
                    RunOnUiThread(() => Toast.MakeText(this, Resource.String.nice_work_long, ToastLength.Short).Show());
                else
                    RunOnUiThread(() => Toast.MakeText(this, Resource.String.nice_work_short, ToastLength.Short).Show());
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

        private void CancelSave()
        {
            m_TaskEditText.Text = string.Empty;
            m_Editing = false;
            SetActionBar();
            HideKeyboard();
        }

        private void Save()
        {
            var editedTask = m_TaskEditText.Text.Trim();

            if(string.IsNullOrWhiteSpace(editedTask))
            {
                CancelSave();
                return;
            }

            m_DataChanged = true;

            m_AllTasks[m_EditTaskPosition].Task = editedTask;

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

            m_Editing = false;
            SetActionBar();
        }

        private void Sort()
        {
            var oldSort = Settings.SortBy;

            PopUpHelpers.ShowListPopup(this, Resource.String.sort_title, Resource.Array.sort_by_options, (newSort) =>
                                                                                                             {
                                                                                                                 if(oldSort == (SortOption)newSort)
                                                                                                                     return;

                                                                                                                 Settings.SortBy = (SortOption)newSort;
                                                                                                                 ReloadData(0);
                                                                                                             });
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
                    FocusMainText();
                    return true;

                case Resource.Id.menu_share_task:

                     var intent = new Intent(Intent.ActionSend);
                    intent.SetType("text/plain");

                    var stringId = m_AllTasks[m_EditTaskPosition].Checked
                                       ? Resource.String.share_finished
                                       : Resource.String.share_not_finished;

                    var shareMessage = string.Format(Resources.GetString(stringId), m_AllTasks[m_EditTaskPosition].Task);

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
                case Resource.Id.menu_about:
                    var aboutIntent = new Intent(this, typeof(AboutActivity));
                    StartActivity(aboutIntent);
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
                case Resource.Id.menu_sort:
                    Sort();
                    break;
                case Resource.Id.menu_history:
                    var historyIntent = new Intent(this, typeof (HistoryActivity));
                    StartActivity(historyIntent);
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

        public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent e)
        {
            try
            {
                if (actionId == Android.Views.InputMethods.ImeAction.Done)
                {
                    if (m_Editing)
                    {
                        Save();
                        return false;
                    }

                    AddNewTask();

                    return Settings.KeepKeyboardUp;
                }
            }
            catch (Exception)
            {
                
                //throw;
            }
           
            return false;
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (!m_DataChanged)
                return;

            var intent = new Intent(TaskWidgetProvider.UpdateIntent);
            this.SendBroadcast(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if(m_OriginalTheme != Settings.ThemeSetting || m_OriginalAccent != Settings.ThemeAccent)
            {
                PopUpHelpers.ShowOKCancelPopup(this, Resource.String.theme_changed_title, 
                    Resource.String.theme_changed_message, (reload) =>
                                                               {
                                                                   if (!reload)
                                                                       return;

                                                                  
                                                                   OverridePendingTransition(0, 0);
                                                                   Intent.AddFlags(ActivityFlags.NoAnimation);
                                                                   Finish();

                                                                   OverridePendingTransition(0, 0);
                                                                   StartActivity(Intent);
                                                               });
            }
        }
    }
}

