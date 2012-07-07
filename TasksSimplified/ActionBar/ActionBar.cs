/*
 * Copyright (C) 2010 Johan Nilsson <http://markupartist.com>
 *
 * Original (https://github.com/johannilsson/android-actionbar) Ported to Mono for Android
 * Copyright (C) 2012 Tomasz Cielecki <tomasz@ostebaronen.dk>
 * 
 * Modified by James Montemagno Copyright 2012 http://www.montemagno.com
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Content.Res;

namespace TasksSimplified.ActionBar
{
    public sealed class ActionBar : RelativeLayout, View.IOnClickListener, View.IOnLongClickListener
    {
        private readonly LayoutInflater m_Inflater;
        private readonly RelativeLayout m_BarView;
        private readonly ImageView m_LogoView;
        private readonly View m_BackIndicator;
        private readonly TextView m_TitleView;
        private readonly LinearLayout m_ActionsView;
        private readonly ImageButton m_HomeBtn;
        private readonly RelativeLayout m_HomeLayout;
        private readonly ProgressBar m_Progress;
        private readonly RelativeLayout m_TitleLayout;
        private readonly Context m_Context;
        private readonly OverflowActionBarAction m_OverflowAction;
        private readonly bool m_HasMenuButton;


        //Used to track what we need to hide in the pop up menu.
        public List<int> MenuItemsToHide = new List<int>();


        public Activity CurrentActivity { get; set; }

        public ActionBar(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            m_Context = context;
            m_Inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

            m_BarView = (RelativeLayout)m_Inflater.Inflate(Resource.Layout.ActionBar, null);
            AddView(m_BarView);

            m_LogoView = m_BarView.FindViewById<ImageView>(Resource.Id.actionbar_home_logo);
            m_HomeLayout = m_BarView.FindViewById<RelativeLayout>(Resource.Id.actionbar_home_bg);
            m_HomeBtn = m_BarView.FindViewById<ImageButton>(Resource.Id.actionbar_home_btn);
            m_BackIndicator = m_BarView.FindViewById(Resource.Id.actionbar_home_is_back);

            m_TitleView = m_BarView.FindViewById<TextView>(Resource.Id.actionbar_title);
            m_ActionsView = m_BarView.FindViewById<LinearLayout>(Resource.Id.actionbar_actions);

            m_Progress = m_BarView.FindViewById<ProgressBar>(Resource.Id.actionbar_progress);
            m_TitleLayout = m_BarView.FindViewById<RelativeLayout>(Resource.Id.actionbar_title_layout);
            TypedArray a = context.ObtainStyledAttributes(attrs,
                    Resource.Styleable.ActionBar);

            m_OverflowAction = new OverflowActionBarAction(context);
            string title = a.GetString(Resource.Styleable.ActionBar_title);

            //check if pre-honeycomb. Ideally here you would actually want to check if a menu button exists.
            //however on all pre-honeycomb phones they basically did.
            var currentapiVersion = (int)Build.VERSION.SdkInt;
            m_HasMenuButton = currentapiVersion <= 10;

            if (title != null)
            {
                SetTitle(title);
            }
            a.Recycle();
        }

        public void SetHomeAction(ActionBarAction action)
        {
            m_HomeBtn.SetOnClickListener(this);
            m_HomeBtn.Tag = action;
            m_HomeBtn.SetImageResource(action.GetDrawable());
            m_HomeLayout.Visibility = ViewStates.Visible;
            ((LayoutParams)m_TitleLayout.LayoutParameters).AddRule(LayoutRules.RightOf, Resource.Id.actionbar_home_bg);
        }

        public void ClearHomeAction()
        {
            m_HomeLayout.Visibility = ViewStates.Gone;
        }

        /**
         * Shows the provided logo to the left in the action bar.
         * 
         * This is ment to be used instead of the setHomeAction and does not draw
         * a divider to the left of the provided logo.
         * 
         * @param resId The drawable resource id
         */
        public void SetHomeLogo(int resId)
        {
            // TODO: Add possibility to add an IntentAction as well.
            m_LogoView.SetImageResource(resId);
            m_LogoView.Visibility = ViewStates.Visible;
            m_HomeLayout.Visibility = ViewStates.Gone;
            ((LayoutParams)m_TitleLayout.LayoutParameters).AddRule(LayoutRules.RightOf, Resource.Id.actionbar_home_logo);
        }

        /* Emulating Honeycomb, setdisplayHomeAsUpEnabled takes a boolean
         * and toggles whether the "home" view should have a little triangle
         * indicating "up" */
        public void SetDisplayHomeAsUpEnabled(bool show)
        {
            m_BackIndicator.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        public void SetTitle(string title)
        {
            m_TitleView.Text = title;
        }

        public void SetTitle(int resid)
        {
            m_TitleView.SetText(resid);
        }

        /**
         * Set the enabled state of the progress bar.
         * 
         * @param One of {@link View#VISIBLE}, {@link View#INVISIBLE},
         *   or {@link View#GONE}.
         */
        public void SetProgressBarVisibility(ViewStates visibility)
        {
            m_Progress.Visibility = visibility;
        }

        /**
         * Returns the visibility status for the progress bar.
         * 
         * @param One of {@link View#VISIBLE}, {@link View#INVISIBLE},
         *   or {@link View#GONE}.
         */
        public ViewStates GetProgressBarVisibility()
        {
            return m_Progress.Visibility;
        }

        /**
         * Function to set a click listener for Title TextView
         * 
         * @param listener the onClickListener
         */
        public void SetOnTitleClickListener(IOnClickListener listener)
        {
            m_TitleView.SetOnClickListener(listener);
        }

        public void OnClick(View v)
        {
            var tag = v.Tag;
            var action = tag as ActionBarAction;
            if (action != null)
            {
                action.PerformAction(v);
            }
        }

        /**
         * Adds a list of {@link Action}s.
         * @param actionList the actions to add
         */
        public void AddActions(ActionList actionList)
        {
            for (var i = 0; i < actionList.Count; i++)
            {
                AddAction(actionList.ElementAt(i));
            }
        }

        /**
         * Adds a new {@link Action}.
         * @param action the action to add
         */
        public void AddAction(ActionBarAction action) 
        {
            AddAction(action, m_ActionsView.ChildCount);
        }

        /**
      * Adds a new {@link Action}.
      * @param action the action to add
      */
        public void AddOverflowAction(ActionBarAction action)
        {
            var index = m_ActionsView.ChildCount;
            m_ActionsView.AddView(InflateOverflowAction(action), index);
            m_OverflowAction.Index = index;
        }

        /**
         * Adds a new {@link Action} at the specified index.
         * @param action the action to add
         * @param index the position at which to add the action
         */
        public void AddAction(ActionBarAction action, int index)
        {
            var addActionBar = false;

            var hideAction = false;
            if (!ActionBarUtils.ActionFits(CurrentActivity, index, m_HasMenuButton, action.ActionType))
            {
                if(!m_HasMenuButton)
                {
                    addActionBar = m_OverflowAction.ActionList.Count == 0;
                    m_OverflowAction.AddAction(action);
                    hideAction = true;
                }
            }
            else
            {
                if (m_OverflowAction.ActionList.Count != 0)//exists
                    index = m_OverflowAction.Index;//bring it inside

                hideAction = true;

                m_ActionsView.AddView(InflateAction(action), index);
            }

            //simply put it in the menu items to hide if we are a menu item.
            var taskAction = action as MenuItemActionBarAction;
            if (taskAction != null && hideAction)
                MenuItemsToHide.Add(taskAction.MenuItemId);

            if (addActionBar)
                AddOverflowAction(m_OverflowAction);
        }

        /**
     * Removes all action views from this action bar
     */
        public void RemoveAllActions()
        {
            m_ActionsView.RemoveAllViews();
            MenuItemsToHide.Clear();
        }

        /**
         * Remove a action from the action bar.
         * @param index position of action to remove
         */
        public void RemoveActionAt(int index)
        {
            if (index < 1) return;

            var menuItemAction = m_ActionsView.GetChildAt(index).Tag as MenuItemActionBarAction;
            if (menuItemAction != null)
                MenuItemsToHide.Remove(menuItemAction.MenuItemId);

            m_ActionsView.RemoveViewAt(index);
        }

        /**
       * Remove a action from the action bar.
       * @param index position of action to remove
       */
        public void RemoveActionAtMenuId(int id)
        {
            for (var i = 0; i < m_ActionsView.ChildCount; i++)
            {
                var view = m_ActionsView.GetChildAt(i);
                
                if (view == null) continue;

                var tag = view.Tag;
                var actionBarAction = tag as MenuItemActionBarAction;
                
                if (actionBarAction == null || id != actionBarAction.MenuItemId) continue;

                MenuItemsToHide.Remove(actionBarAction.MenuItemId);

                m_ActionsView.RemoveView(view);
            }
        }

        public int ActionCount
        {
            get
            {
                return m_ActionsView.ChildCount;
            }
        }

        /**
         * Remove a action from the action bar.
         * @param action The action to remove
         */
        public void RemoveAction(ActionBarAction action)
        {
            for (var i = 0; i < m_ActionsView.ChildCount; i++)
            {
                var view = m_ActionsView.GetChildAt(i);

                if (view == null) continue;

                var tag = view.Tag;
                var actionBarAction = tag as ActionBarAction;

                if (actionBarAction == null || !actionBarAction.Equals(action)) continue;

                var menuItemAction = tag as MenuItemActionBarAction;
                if (menuItemAction != null)
                    MenuItemsToHide.Remove(menuItemAction.MenuItemId);

                m_ActionsView.RemoveView(view);
            }
        }

        /**
         * A {@link LinkedList} that holds a list of {@link Action}s.
         */
        public class ActionList : LinkedList<ActionBarAction>
        {
        }


        /**
         * Inflates a {@link View} with the given {@link Action}.
         * @param action the action to inflate
         * @return a view
         */
        private View InflateAction(ActionBarAction action)
        {
            var view = m_Inflater.Inflate(Resource.Layout.ActionBar_Item, m_ActionsView, false);

            var labelView =
                view.FindViewById<ImageButton>(Resource.Id.actionbar_item);
            labelView.SetImageResource(action.GetDrawable());

            view.Tag = action;
            view.SetOnClickListener(this);
            view.SetOnLongClickListener(this);
            return view;
        }

        private View InflateOverflowAction(ActionBarAction action)
        {
            var view = m_Inflater.Inflate(Resource.Layout.OverflowActionBar_Item, m_ActionsView, false);

            var labelView =
                view.FindViewById<ImageButton>(Resource.Id.actionbar_item);
            labelView.SetImageResource(action.GetDrawable());

            var spinner = view.FindViewById<Spinner>(Resource.Id.overflow_spinner);
            m_OverflowAction.OverflowSpinner = spinner;

            labelView.Tag = action;
            labelView.SetOnClickListener(this);
            //view.SetOnLongClickListener(this);

            m_OverflowAction.Activity = CurrentActivity;
            return view;
        }

        public bool OnLongClick(View v)
        {
            var tag = v.Tag;
            var action = tag as ActionBarAction;
            if (action != null)
            {
                if (action.PopUpMessage == 0)
                    return true;

                if (CurrentActivity == null)
                    return false;

                Toast.MakeText(m_Context, action.PopUpMessage, ToastLength.Short).Show();

                return false;
            }

            return false;
        }
    }
}