using TasksSimplified.DataLayer;

namespace TasksSimplified.BusinessLayer.Contracts
{
    public class BusinessEntityBase : IBusinessEntity
    {
        public BusinessEntityBase()
        {
        }

        /// <summary>
        /// Gets or sets the Database ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        [PrimaryKey, AutoIncrement]
        [System.Xml.Serialization.XmlIgnore]
        public int ID { get; set; }
    }
}