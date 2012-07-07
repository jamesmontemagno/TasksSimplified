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
using Android.OS;
using Android.Widget;
using TasksSimplified.ActionBarBase;
using TasksSimplified.ActionBar;
using TasksSimplified.Adapter;
using TasksSimplified.BusinessLayer;

namespace TasksSimplified
{
    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/icon", Theme = "@style/MyTheme")]
    public class MainActivity : ActionBarListActivity
    {
        string[] items = new string[]{"lorem","ipsum", "dolor", "sit", "amet",
        "consectetuer", "adipisc", "jklfe", "morbi", "vel",
        "ligula", "vitae", "carcu", "aliequet"};
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

            List<TaskModel> tasks = new List<TaskModel>();
            foreach(var item in items)
                tasks.Add(new TaskModel(){Task = item});

            ListAdapter = new TaskAdapter(this, tasks);
         
            ListView.ChoiceMode = ChoiceMode.Multiple;
            // Get our button from the layout resource,
            // and attach an event to it

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

        }

        protected override void OnListItemClick(ListView l, Android.Views.View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);

            if(ListView.GetCheckItemIds().Length > 0)
                SetupEditActionBar();


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

