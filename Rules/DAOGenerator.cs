using Rules._EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Rules
{
    public class DAOGenerator
    {
        private static DbAnalyseArgs mAnalyseArgs;
        public static DbAnalyseArgs AnalyseArgs
        {
            get 
            {
                return mAnalyseArgs;
            }

            set
            {
                mAnalyseArgs = value;
                if(mAnalyseArgs.NamespacesForm == NamesWritingForm.PascalCase)
                {
                    EntitiesNamespace = "Entities";
                    ManagersNamespace = "Managers";
                }

                else
                {
                    EntitiesNamespace = "entities";
                    ManagersNamespace = "managers";
                }
            }
        }

        private static string EntitiesNamespace, ManagersNamespace;

        public static void GenerateEntities(List<EntityNode> Entities)
        {
            StreamWriter writer;
            using(writer = new StreamWriter("ICtorDataReader.cs"))
            {
                string ictordatareader = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "TemplateFiles/ictordatareader.cs.tp", Encoding.Default);
                writer.Write(string.Format("namespace {0}\r\n{{\r\n{1}\r\n}}", EntitiesNamespace, ictordatareader));
            }

            foreach(EntityNode node in Entities)
            {
                using (writer = new StreamWriter(string.Format("{0}.cs", node.Value.Name)))
                {
                    writer.WriteLine(string.Format("namespace {0}\r\n{{", EntitiesNamespace));
                    GenerateOneEntity(node.Value, writer);
                    writer.WriteLine("}");
                }
            }
        }

        public static void GenerateOneEntity(EntityModel entity, StreamWriter writer)
        {
            writer.Write(string.Format("\tpublic class {0} : ", entity.Name));
            if (entity.Base != null)
                writer.Write(string.Format("{0}", entity.Base.Name));

            else
                writer.Write("ICtorDataReader");

            writer.WriteLine("\r\n\t{");

            foreach(PropertyModel prop in entity.Properties.Values)
            {
                if(!prop.Inherited)
                    writer.WriteLine(string.Format("\t\tpublic {0} {1} {{get; set;}}", prop.CLRType.ToString(), prop.Name));
            }

            writer.Write(string.Format("\r\n\t\tpublic {0}() ", entity.Name));
            if (entity.Base != null)
                writer.Write(": base() ");

            writer.WriteLine("{}\r\n");

            string virtOrOver = "virtual";
            if (entity.Base != null)
                virtOrOver = "override";

            writer.WriteLine(string.Format("\t\tpublic {0} void CtorDataReader(System.Data.IDataReader reader)\r\n\t\t{{", virtOrOver));
            if (entity.Base != null)
                writer.WriteLine("\t\t\tbase.CtorDataReader(reader);");

            foreach(PropertyModel prop in entity.Properties.Values)
            {
                if(!prop.Inherited && !string.IsNullOrEmpty(prop.DbName))
                {
                    writer.WriteLine(string.Format("\t\t\t{0} = ({1})reader[\"{2}\"];", prop.Name, prop.CLRType, prop.DbName));
                }
            }

            writer.WriteLine("\t\t}");

            writer.WriteLine("\t}");
        }

        public static void GenerateManagers(List<EntityNode> Entities)
        {
            StreamWriter writer;
            using(writer = new StreamWriter("GenericManager.cs"))
            {
                writer.WriteLine(string.Format("using {0};\r\n\r\nnamespace {1}\r\n{{", EntitiesNamespace, ManagersNamespace));
                GenerateGenericManager(writer);
                writer.WriteLine("}");
            }

            string ManagerName;
            foreach (EntityNode node in Entities)
            {
                if (node.Value.DatabaseEntity)
                {
                    ManagerName = node.Value.Name.Substring(0, node.Value.Name.Length - "Entity".Length) + "Manager";

                    using (writer = new StreamWriter(string.Format("{0}.cs", ManagerName)))
                    {
                        writer.WriteLine(string.Format("using {0};\r\n\r\nnamespace {1}\r\n{{", EntitiesNamespace, ManagersNamespace));
                        GenerateOneManager(node.Value, ManagerName, writer);
                        writer.WriteLine("}");
                    }
                }
            }
        }

        public static void GenerateGenericManager(StreamWriter writer)
        {
            string genericmanager = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/TemplateFiles/genericmanager.cs.tp");
            writer.WriteLine(genericmanager);
        }

        public static void GenerateOneManager(EntityModel entity, string ManagerName, StreamWriter writer)
        {
            writer.WriteLine(string.Format("\tpublic class {0} : GenericManager<{1}>\t\r\n\t{{\r\n", ManagerName, entity.Name));

            writer.WriteLine("\t}");
        }
    }
}