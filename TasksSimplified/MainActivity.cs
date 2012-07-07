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

using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Views;
using Android.Widget;
using TasksSimplified.ActionBarBase;
using TasksSimplified.ActionBar;
using TasksSimplified.Adapter;
using TasksSimplified.BusinessLayer;
using System.Collections.ObjectModel;

namespace TasksSimplified
{
    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/icon", Theme = "@style/MyTheme")]
    public class MainActivity : ActionBarListActivity
    {
        string[] items = new string[]{"lorem","ipsum", "dolor", "sit", "amet",
        "consectetuer", "adipisc", "jklfe", "morbi", "vel",
        "ligula", "vitae", "carcu", "aliequet", "this is a crazy crazy long entry into the list so i can check this out to see if it wraps."};

        private JavaList<TaskModel> m_Items;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Window.SetSoftInputMode(Android.Views.SoftInput.StateAlwaysHidden);

            DarkMenuId = Resource.Menu.MainMenu;
            MenuId = Resource.Menu.MainMenu;

            ActionBar = FindViewById<ActionBar.ActionBar>(Resource.Id.actionbar);
            ActionBar.SetTitle("Tasks");
            ActionBar.CurrentActivity = this;
            ActionBar.SetHomeLogo(Resource.Drawable.Icon);
            RegisterForContextMenu(ListView);

            SetupMainActionBar();

            m_Items = new JavaList<TaskModel>();
            foreach(var item in items)
                m_Items.Add(new TaskModel() { Task = item });

            
         
            ListView.ChoiceMode = ChoiceMode.Multiple;

            var add = FindViewById<ImageButton>(Resource.Id.button_add_task);
            var microphone = FindViewById<ImageButton>(Resource.Id.button_microphone);

            //add.SetBackgroundResource(Android.Resource.Color.Transparent);
            //microphone.SetBackgroundResource(Android.Resource.Color.Transparent);

            add.Click += (sender, args) =>
                             {
                                 var text = FindViewById<EditText>(Resource.Id.edit_text_new_task);
                                 var newTask = text.Text.Trim();

                                 text.Text = string.Empty;

                                 if (string.IsNullOrWhiteSpace(newTask))
                                     return;



                                 m_Items.Add(new TaskModel() {Task = newTask});
                                 RunOnUiThread(() =>
                                 {
                                     ((TaskAdapter)ListAdapter).NotifyDataSetChanged();
                                     ListView.SetSelection(m_Items.Count - 1);
                                 });
                             };

            // remove speech if it doesn't exist
            var activities =PackageManager.QueryIntentActivities(new Intent(RecognizerIntent.ActionRecognizeSpeech), 0);
            if (activities.Count == 0)
            {
                microphone.Visibility = ViewStates.Gone;
            }
            else
            {
                microphone.Click += (sender, args) =>
                                        {
                                            StartVoiceRecognitionActivity();
                                        };
            }

            RunOnUiThread(()=>ListAdapter = new TaskAdapter(this, m_Items));

        }

        private void StartVoiceRecognitionActivity()
        {
            Intent intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel,
                    RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak new task...");
            StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 0 && resultCode == Result.Ok)
            {
                // Populate the wordsList with the String values the recognition engine thought it heard
                var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);

                string newTask = string.Empty;
                foreach(var match in matches)
                {
                    newTask += match + " ";
                }
                var text = FindViewById<EditText>(Resource.Id.edit_text_new_task);
                text.Text = newTask;

            }
            
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void SetupMainActionBar()
        {
            if (ActionBar.HasId(Resource.Id.menu_about))
                return;
            ActionBar.SetTitle("Tasks");

            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete_all, Resource.Drawable.ic_menu_delete_all,
                                                     Resource.String.menu_string_delete_all);
            action.ActionType = ActionType.Never;
            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_about, Resource.Drawable.ic_menu_about,
                                                     Resource.String.menu_string_about);
            action.ActionType = ActionType.Never;
            ActionBar.AddAction(action);

            DarkMenuId = Resource.Menu.MainMenu;
            MenuId = Resource.Menu.MainMenu;
        }

        private void SetupEditActionBar()
        {
            if(ActionBar.HasId(Resource.Id.menu_delete))
                return;

            ActionBar.SetTitle("Delete Tasks");
            ActionBar.RemoveAllActions();

            var action = new MenuItemActionBarAction(this, this, Resource.Id.menu_delete, Resource.Drawable.ic_action_delete_dark,
                                                    Resource.String.menu_string_delete);
            action.ActionType = ActionType.Always;
            ActionBar.AddAction(action);

            action = new MenuItemActionBarAction(this, this, Resource.Id.menu_cancel, Resource.Drawable.ic_action_cancel,
                                                     Resource.String.menu_string_cancel);
            action.ActionType = ActionType.Always;
            ActionBar.AddAction(action);

            DarkMenuId = Resource.Menu.MainMenuEdit;
            MenuId = Resource.Menu.MainMenuEdit;

        }

        protected override void OnListItemClick(ListView l, Android.Views.View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);

            if (ListView.GetCheckItemIds().Length > 0)
            {
                SetupEditActionBar();
            }
            else
            {
                SetupMainActionBar();
            }

            m_Items[position].Checked = l.IsItemChecked(position);
            RunOnUiThread(() =>
                              {
                                  ((TaskAdapter) ListAdapter).NotifyDataSetChanged();
                              });

        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Resource.Id.menu_about:
                    break;
                case Resource.Id.menu_cancel:
                    break;
                case Resource.Id.menu_delete:
                    break;
                case Resource.Id.menu_delete_all:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}

