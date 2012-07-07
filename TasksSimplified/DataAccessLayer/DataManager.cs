using System.Collections.Generic;
using TasksSimplified.BusinessLayer;

namespace TasksSimplified.DataAccessLayer
{
    public static class DataManager
    {
        #region Devices
        public static IEnumerable<TaskModel> GetTasks()
        {
            return DataLayer.TaskDatabase.GetItems<TaskModel>();
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