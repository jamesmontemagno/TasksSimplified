using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using TasksSimplified.BusinessLayer;

namespace TasksSimplified.Adapter
{
    internal class TaskAdapterWrapper : Java.Lang.Object
    {
        public CheckedTextView Title { get; set; }
    }

    class TaskAdapter : BaseAdapter
    {


        private readonly Activity m_Context;
        private readonly IEnumerable<TaskModel> m_Tasks;



        public TaskAdapter(Activity context, IEnumerable<TaskModel> tasks)
        {
            m_Context = context;
            m_Tasks = tasks;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position < 0)
                return null;

            var task = m_Tasks.ElementAt(position);

            View view = null;

            if (task.Checked)
            {
                view = m_Context.LayoutInflater.Inflate(Resource.Layout.CheckedListItem, parent, false);
            }
            else
            {
                view = m_Context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemMultipleChoice, parent, false);
            }

            if (view == null)
                return null;

            var taskTitle = view.FindViewById<CheckedTextView>(Android.Resource.Id.Text1);

            taskTitle.Text = task.Task;


            return view;
        }

        public override int Count
        {
            get { return m_Tasks.Count(); }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            if (position < 0)
                return 0;

            return position;
            //return m_Tasks.ElementAt(position).ID;
        }

        public override bool HasStableIds
        {
            get
            {
                return true;
            }
        }

    }
}