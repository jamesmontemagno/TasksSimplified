using System;
using System.Xml.Serialization;

namespace TasksSimplified.BusinessLayer
{
    public class TaskModel : Contracts.BusinessEntityBase
    {
        [XmlElement("t")]
        public string Task { get; set; }

        [XmlElement("d")]
        public DateTime DateCreated { get; set; }
    }
}