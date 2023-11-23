using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Rules._EntityModel
{
    [Serializable]
    public class PropertyModel
    {
        public string Name { get; set; }

        public string DbName { get; set; }

        public bool Inherited { get; set; }

        public DbType DbType { get; set; }

        public string CLRType { get; set; }

        public PropertyModel() { }

        public PropertyModel(string Name, bool Inherited, DataRow Column) 
            : this(Name, Inherited, Helper.GetDbTypeFromString(Column["DATA_TYPE"].ToString()))
        {
            this.DbName = Column["COLUMN_NAME"].ToString();
        }

        public PropertyModel(string Name, bool Inherited, DbType DbType, Type CLRType) : this(Name, Inherited)
        {
            this.DbType = DbType;
            this.CLRType = CLRType.ToString();
        }

        public PropertyModel(string Name, bool Inherited, DbType DbType) : this(Name, Inherited)
        {
            this.DbType = DbType;
            CLRType = Helper.GetCLRTypeFromDbType(DbType).ToString();
        }

        public PropertyModel(string Name, bool Inherited, Type CLRType) : this(Name, Inherited)
        {
            this.CLRType = CLRType.ToString();
        }

        private PropertyModel(string Name, bool Inherited)
        {
            this.Name = Name;
            this.Inherited = Inherited;
        }
    }
}
