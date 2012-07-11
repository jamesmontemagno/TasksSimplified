using System.Collections.Generic;
using System.Linq;
using TasksSimplified.BusinessLayer;

namespace TasksSimplified.DataAccessLayer
{
    public enum SortOption
    {
        Newest = 0,
        Oldest = 1,
        TitleAscending = 2,
        TitleDescending =3
    }

    public static class DataManager
    {
        #region Tsks
        public static IEnumerable<TaskModel> GetTasks(SortOption sortOption)
        {
            switch (sortOption)
            {
                case SortOption.Newest:
                    return DataLayer.TaskDatabase.GetItems<TaskModel>().OrderByDescending(t=>t.DateCreated);
                case SortOption.Oldest:
                    return DataLayer.TaskDatabase.GetItems<TaskModel>().OrderBy(t => t.DateCreated);
                case SortOption.TitleAscending:
                    return DataLayer.TaskDatabase.GetItems<TaskModel>().OrderBy(t => t.Task);
                case SortOption.TitleDescending:
                    return DataLayer.TaskDatabase.GetItems<TaskModel>().OrderByDescending(t => t.Task);
            }

            return DataLayer.TaskDatabase.GetItems<TaskModel>().OrderBy(t => t.DateCreated);
        }

        public static void DeleteTasks()
        {
            DataLayer.TaskDatabase.ClearTable<TaskModel>();
        }

        public static void DeleteTask(int id)
        {
            DataLayer.TaskDatabase.DeleteItem<TaskModel>(id);
        }

        public static int SaveTask(TaskModel item)
        {
            return DataLayer.TaskDatabase.SaveItem<TaskModel>(item);
        }

        #endregion
    }
}