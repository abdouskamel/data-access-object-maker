using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using Rules._EntityModel;

namespace Rules
{
    public enum NamesWritingForm
    {
        LowerCamelCase, PascalCase, C_Case, Default
    }

    public static class Helper
    {
        // 
        public static void GetInheritedProperties(EntityModel _base, EntityModel child, List<PropertyModel> list)
        {
            var bprops = _base.Properties.Values;
            var cprops = child.Properties.Values.ToList();

            foreach(PropertyModel prop in bprops)
            {
                for(int i = 0; i < cprops.Count; ++i)
                {
                    if(prop.Name == cprops[i].Name && prop.CLRType == cprops[i].CLRType)
                    {
                        list.Add(cprops[i]);
                        cprops.RemoveAt(i);
                    }
                }
            }

            if (list.Count != bprops.Count)
                list.Clear();
        }

        public static bool AreBaseChild(EntityModel _base, EntityModel child)
        {
            List<PropertyModel> list = new List<PropertyModel>();
            GetInheritedProperties(_base, child, list);

            return list.Count != 0;
        }

        // ------------------------------------------------------------------------------------- //
        public static bool IsAvailableProvider(string Provider)
        {
            DataTable dt = DbProviderFactories.GetFactoryClasses();

            foreach(DataRow row in dt.Rows)
            {
                if (row["InvariantName"].ToString() == Provider)
                    return true;
            }

            return false;
        }

        public static bool IsUserTable(string table_type)
        {
            return table_type == "TABLE" || table_type == "BASE TABLE";
        }

        public static DbType GetDbTypeFromString(string type)
        {
            switch(type.ToUpper())
            {
                case "TINYINT":
                    return DbType.SByte;

                case "SMALLINT":
                    return DbType.Int16;

                case "TIMESTAMP":
                case "INT":
                    return DbType.Int32;

                case "BIGINT":
                    return DbType.Int64;

                case "FLOAT":
                    return DbType.Single;

                case "DOUBLE":
                    return DbType.Double;

                case "DECIMAL":
                    return DbType.Decimal;

                case "DATE":
                    return DbType.Date;

                case "DATETIME":
                    return DbType.DateTime;

                case "CHAR":
                case "VARCHAR":
                case "TINYTEXT":
                case "TEXT":
                case "MEDIUMTEXT":
                case "LONGTEXT":
                    return DbType.String;

                default:
                    return DbType.Object;
            }
        }

        public static Type GetCLRTypeFromDbType(DbType type)
        {
            switch(type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return typeof(string);

                case DbType.Boolean:
                    return typeof(bool);

                case DbType.Byte:
                    return typeof(byte);

                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return typeof(DateTime);

                case DbType.DateTimeOffset:
                    return typeof(DateTimeOffset);

                case DbType.Decimal:
                    return typeof(decimal);

                case DbType.Int16:
                    return typeof(char);

                case DbType.Int32:
                    return typeof(int);

                case DbType.Int64:
                    return typeof(long);

                case DbType.SByte:
                    return typeof(sbyte);

                case DbType.Single:
                    return typeof(double);

                case DbType.UInt16:
                    return typeof(UInt16);

                case DbType.UInt32:
                    return typeof(UInt32);

                case DbType.UInt64:
                    return typeof(UInt64);

                default:
                    return typeof(object);
            }
        }

        // ------------------------------------------------------------------------------------- //
        public static string GetWantedWritingForm(string str, NamesWritingForm arg, bool entity = false)
        {
            switch (arg)
            {
                case NamesWritingForm.LowerCamelCase:
                    return Helper.GetLowerCamelCaseForm(str, entity);

                case NamesWritingForm.PascalCase:
                    return Helper.GetPascalCaseFrom(str, entity);

                case NamesWritingForm.C_Case:
                    return Helper.GetC_CaseForm(str, entity);

                default:
                    return str;
            }
        }

        public static string GetLowerCamelCaseForm(string str, bool entity)
        {
            StringBuilder strb = new StringBuilder(str.Length);
            bool maj_mode = false;

            foreach(char c in str)
            {
                if (c == '_')
                {
                    if (strb.Length != 0)
                        maj_mode = true;
                }

                else if (strb.Length == 0)
                    strb.Append(char.ToLower(c));

                else
                {
                    if (maj_mode)
                    {
                        strb.Append(char.ToUpper(c));
                        maj_mode = false;
                    }

                    else
                        strb.Append(c);
                }
            }

            if (entity)
                strb.Append("Entity");

            return strb.ToString();
        }

        public static string GetPascalCaseFrom(string str, bool entity)
        {
            StringBuilder strb = new StringBuilder(GetLowerCamelCaseForm(str, entity));
            strb[0] = char.ToUpper(strb[0]);

            return strb.ToString();
        }

        public static string GetC_CaseForm(string str, bool entity)
        {
            StringBuilder strb = new StringBuilder(str.Length);
            bool _mode = false;

            foreach(char c in str)
            {
                if(char.IsUpper(c))
                {
                    if (_mode)
                    {
                        strb.Append('_');
                        _mode = false;
                    }

                    strb.Append(char.ToLower(c)); 
                }

                else
                {
                    if(c != '_')
                        _mode = true;

                    strb.Append(c);
                }
            }

            if (entity)
                strb.Append("_entity");

            return strb.ToString();
        }
    }
}
