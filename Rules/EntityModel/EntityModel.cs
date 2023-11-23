using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rules._EntityModel
{
    [Serializable]
    public class EntityModel
    {
        public string Name { get; set; }

        public EntityModel Base { get; set; }

        public bool DatabaseEntity { get; set; }

        public Dictionary<string, PropertyModel> Properties { get; protected set; }

        public Dictionary<string, PropertyModel> PrimaryKeys { get; protected set; }

        public EntityModel(string Name, EntityModel Base, bool DatabaseEntity = true)
        {
            this.Name = Name;
            this.Base = Base;
            this.DatabaseEntity = DatabaseEntity;

            this.Properties = new Dictionary<string, PropertyModel>();
            this.PrimaryKeys = new Dictionary<string, PropertyModel>();
        }
    }
}
