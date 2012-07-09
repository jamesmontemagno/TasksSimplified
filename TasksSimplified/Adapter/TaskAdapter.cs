using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TasksSimplified.BusinessLayer;
using TasksSimplified.Helpers;

namespace TasksSimplified.Adapter
{
    internal class TaskAdapterWrapper : Java.Lang.Object
    {
        public CheckedTextView Title { get; set; }
    }

    class TaskAdapter : BaseAdapter
    {


        private readonly Activity m_Context;
        private readonly JavaList<TaskModel> m_Tasks;

        private int m_FontColor;

        public TaskAdapter(Activity context, JavaList<TaskModel> tasks)
        {
            m_Context = context;
            m_Tasks = tasks;
            m_FontColor = Settings.ThemeSetting == 0 ? Resource.Color.gray : Resource.Color.white;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position < 0)
                return null;

            var view = (convertView
                            ?? m_Context.LayoutInflater.Inflate(
                                    Resource.Layout.CheckedListItem, parent, false)
                        );

            if (view == null)
                return null;

            var wrapper = view.Tag as TaskAdapterWrapper;
            if (wrapper == null)
            {
                wrapper = new TaskAdapterWrapper();
                wrapper.Title = view.FindViewById<CheckedTextView>(Android.Resource.Id.Text1);
                view.Tag = wrapper;
            }

            var task = m_Tasks[position];

            wrapper.Title.Text = task.Task;

            if (task.Checked)
            {
                wrapper.Title.SetTextColor(m_Context.Resources.GetColor(Resource.Color.pop));
                wrapper.Title.PaintFlags |= Android.Graphics.PaintFlags.StrikeThruText;
            }
            else
            {

                wrapper.Title.SetTextColor(m_Context.Resources.GetColor(m_FontColor));
                wrapper.Title.PaintFlags &= ~Android.Graphics.PaintFlags.StrikeThruText;
            }


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