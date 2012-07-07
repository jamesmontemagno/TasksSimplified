/*
 * Copyright (C) 2012 James Montemagno <http://www.montemagno.com>
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
 */

using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace TasksSimplified.ActionBar
{
    public sealed class BottomActionBar : RelativeLayout, View.IOnClickListener, View.IOnLongClickListener
    {
        private readonly LayoutInflater m_Inflater;
        private readonly RelativeLayout m_BarView;
        private readonly TextView m_TitleView;
        private readonly LinearLayout m_ActionsView;
        private readonly Context m_Context;
        public Activity CurrentActivity { get; set; }

        public BottomActionBar(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            m_Context = context;
            m_Inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

            m_BarView = (RelativeLayout)m_Inflater.Inflate(Resource.Layout.BottomActionBar, null);
            AddView(m_BarView);

            m_BarView.FindViewById<ImageView>(Resource.Id.actionbar_home_logo);
            m_BarView.FindViewById<RelativeLayout>(Resource.Id.actionbar_home_bg);
            m_BarView.FindViewById<ImageButton>(Resource.Id.actionbar_home_btn);
            m_BarView.FindViewById(Resource.Id.actionbar_home_is_back);

            m_TitleView = m_BarView.FindViewById<TextView>(Resource.Id.actionbar_title);
            m_ActionsView = m_BarView.FindViewById<LinearLayout>(Resource.Id.actionbar_actions);
            m_BarView.FindViewById<RelativeLayout>(Resource.Id.actionbar_title_layout);
            m_BarView.FindViewById<ProgressBar>(Resource.Id.actionbar_progress);
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
         * Adds a new {@link Action} at the specified index.
         * @param action the action to add
         * @param index the position at which to add the action
         */
        public void AddAction(ActionBarAction action, int index)
        {
            var newAction = InflateAction(action);
            m_ActionsView.AddView(newAction, index);
        }

        public int ActionCount
        {
            get
            {
                return m_ActionsView.ChildCount;
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
            var view = m_Inflater.Inflate(Resource.Layout.BottomActionBar_Item, m_ActionsView, false);

            var labelView =
                view.FindViewById<ImageButton>(Resource.Id.bottomactionbar_item);
            labelView.SetImageResource(action.GetDrawable());

            view.Tag = action;
            view.SetOnClickListener(this);
            view.SetOnLongClickListener(this);
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