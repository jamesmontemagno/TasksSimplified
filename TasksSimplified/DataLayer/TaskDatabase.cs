using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TasksSimplified.BusinessLayer;
using TasksSimplified.BusinessLayer.Contracts;

namespace TasksSimplified.DataLayer
{

    public class TaskDatabase : SQLiteConnection
    {
        protected static TaskDatabase LocalDatabase;
        protected static string DbLocation;

        private static readonly object Locker = new object();

        protected TaskDatabase(string path) : base (path)
        {
            CreateTable<TaskModel>();
            CreateTable<ClearedTaskModel>();
        }


        static TaskDatabase()
		{
			// set the db location
#if WINDOWS_PHONE
            dbLocation = "TasksSimplifiedDB.db3";
#else
            DbLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "TasksSimplifiedDB.db3");
#endif
			
			// instantiate a new db
			LocalDatabase = new TaskDatabase(DbLocation);
		}



        public static IEnumerable<T> GetItems<T>() where T : IBusinessEntity, new()
        {
            lock (Locker)
            {
                return (from i in LocalDatabase.Table<T>() select i).ToList();
            }
        }

        public static T GetItem<T>(int id) where T : IBusinessEntity, new()
        {
            lock (Locker)
            {
                return (from i in LocalDatabase.Table<T>()
                        where i.ID == id
                        select i).FirstOrDefault();
            }
        }

        public static int SaveItem<T>(T item) where T : IBusinessEntity
        {
            lock (Locker)
            {
                if (item.ID != 0)
                {
                    LocalDatabase.Update(item);
                    return item.ID;
                }

                return LocalDatabase.Insert(item);
            }
        }

        public static void SaveItems<T>(IEnumerable<T> items) where T : IBusinessEntity
        {
            lock (Locker)
            {
                LocalDatabase.BeginTransaction();

                foreach (var item in items)
                {
                    SaveItem(item);
                }

                LocalDatabase.Commit();
            }
        }

        public static int DeleteItem<T>(int id) where T : IBusinessEntity, new()
        {
            lock (Locker)
            {
                return LocalDatabase.Delete(new T { ID = id });
            }
        }

        public static void ClearTable<T>() where T : IBusinessEntity, new()
        {
            lock (Locker)
            {
                LocalDatabase.Execute(string.Format("delete from \"{0}\"", typeof(T).Name));
            }
        }
    }
}