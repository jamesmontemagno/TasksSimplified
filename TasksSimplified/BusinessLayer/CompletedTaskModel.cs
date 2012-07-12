using System;
using System.Xml.Serialization;

namespace TasksSimplified.BusinessLayer
{
    public class ClearedTaskModel : TaskModel
    {

        public ClearedTaskModel(TaskModel taskModel)
        {
            Checked = taskModel.Checked;
            DateCompleted = DateTime.UtcNow;
            DateCreated = taskModel.DateCreated;
            DueDate = taskModel.DueDate;
            PhoneNumber = taskModel.PhoneNumber;
            Task = taskModel.Task;
        }

        public ClearedTaskModel()
        {
        }

        [XmlElement("co")]
        public DateTime DateCompleted { get; set; }
    }
}