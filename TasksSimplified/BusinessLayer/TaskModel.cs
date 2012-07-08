using System;
using System.Xml.Serialization;

namespace TasksSimplified.BusinessLayer
{
    public class TaskModel : Contracts.BusinessEntityBase
    {

        public TaskModel()
        {
            DateCreated = DateTime.UtcNow;
            DueDate = DateTime.UtcNow;
        }

        [XmlElement("t")]
        public string Task { get; set; }

        [XmlElement("c")]
        public DateTime DateCreated { get; set; }

        [XmlElement("d")]
        public DateTime DueDate { get; set; }

        [XmlElement("p")]
        public string PhoneNumber { get; set; }

        [XmlElement("ch")]
        public bool Checked { get; set; }

    }
}