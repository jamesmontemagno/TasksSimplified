using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TasksSimplified.BusinessLayer;
using TasksSimplified.Helpers;

namespace TasksSimplified.Adapter
{
    internal class ClearedTaskAdapterWrapper : Java.Lang.Object
    {
        public TextView Title { get; set; }
        public TextView Date { get; set; }
    }

    class ClearedTaskAdapter : BaseAdapter
    {


        private readonly Activity m_Context;
        private readonly JavaList<ClearedTaskModel> m_Tasks;
        public ClearedTaskAdapter(Activity context, JavaList<ClearedTaskModel> tasks)
        {
            m_Context = context;
            m_Tasks = tasks;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position < 0)
                return null;

            var view = (convertView
                            ?? m_Context.LayoutInflater.Inflate(
                                    Resource.Layout.HistoryItem, parent, false)
                        );

            if (view == null)
                return null;

            var wrapper = view.Tag as ClearedTaskAdapterWrapper;
            if (wrapper == null)
            {
                wrapper = new ClearedTaskAdapterWrapper();
                wrapper.Title = view.FindViewById<TextView>(Resource.Id.cleared_title);
                wrapper.Date = view.FindViewById<TextView>(Resource.Id.cleared_date);
                view.Tag = wrapper;
            }

            var task = m_Tasks[position];

            wrapper.Title.Text = task.Task;
            var date = task.DateCompleted.ToLocalTime();
            wrapper.Date.Text = date.ToShortDateString() + " " + date.ToShortTimeString();
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