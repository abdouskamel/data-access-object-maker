using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace Rules
{
    using _EntityModel;
    using System.Collections;
    using System.IO;

    [Serializable]
    public struct DbAnalyseArgs
    {
        public NamesWritingForm PropertiesForm, MethodsForm,
                                ClassesForm, NamespacesForm;
    }

    [Serializable]
    public class DbAnalyzer
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }

        public string DatabaseName { get; protected set; }

        public List<EntityNode> EntitiesForest { get; protected set; }

        public List<EntityNode> InheritedEntitiesForest { get; protected set; }

        DbAnalyseArgs _DbAnalyseArgs;

        public DbAnalyzer(string Provider, string ConnectionString)
        {
            this.Provider = Provider;
            this.ConnectionString = ConnectionString;

            EntitiesForest = new List<EntityNode>();
        }

        public DbAnalyzer(string BddPath, string BddName, string Passwd)
        {
            switch(Path.GetExtension(BddPath))
            {
                case ".mdf":
                    if(BddName.Length == 0)
                        throw new DbIdentifiersException("Une connexion à une base de données Sql Server nécessite un nom de base de données.");

                    Provider = "System.Data.SqlClient";
                    ConnectionString = string.Format("Server=.\\SQLExpress; AttachDbFilename = {0}; Database = {1}; Password = {2};", BddPath, BddName, Passwd);
                    break;

                case ".accdb":
                    Provider = "System.Data.OleDb";
                    ConnectionString = string.Format("Provider = Microsoft.ACE.OLEDB.12.0; Data Source = {0}; Jet OLEDB:Database Password = {1}", BddPath, Passwd);
                    break;

                default:
                    throw new DbIdentifiersException("Extension de fichier de base de données érronée.");
            }

            EntitiesForest = new List<EntityNode>();
        }

        public void AddAnEntity(EntityModel entity)
        {
            int i = 0;
            while(i < EntitiesForest.Count && entity.Properties.Count >= EntitiesForest[i].Value.Properties.Count)
                i++;

            EntitiesForest.Insert(i, new EntityNode(entity));
        }

        public void AnalyseTheDatabase(DbAnalyseArgs args)
        {
            // Initialisation de la connexion
            if (!Helper.IsAvailableProvider(Provider))
                throw new DbConnectionException(String.Format("Pilote de base de données indisponible : {0}.", Provider));

            DbProviderFactory dbpf = DbProviderFactories.GetFactory(Provider);
            DbConnection con = dbpf.CreateConnection();

            try
            {
                con.ConnectionString = ConnectionString;
                con.Open();
            }

            catch (Exception e)
            {
                throw new DbConnectionException(String.Format("Impossible de se connecter à votre base de données. Message d'erreur :\n\n{0}.", e.Message));
            }

            // Construction de notre objet à partir du schéma
            using (con)
            {
                DataTable Tables = con.GetSchema("Tables"),
                          Fields;

                string[] getfieldsargs = new string[] { null, null, null, null };
                string table_name, field_name;
                EntityModel mEntity;
                PropertyModel mProperty;

                // Parcours des tables
                foreach (DataRow row in Tables.Rows)
                {
                    if (Helper.IsUserTable(row["TABLE_TYPE"].ToString()))
                    {
                        table_name = row["TABLE_NAME"].ToString();
                        mEntity = new EntityModel(Helper.GetWantedWritingForm(table_name, args.ClassesForm, true), null);

                        getfieldsargs[2] = table_name;
                        Fields = con.GetSchema("Columns", getfieldsargs);

                        // Parcours des champs de chaque table
                        foreach (DataRow col in Fields.Rows)
                        {
                            field_name = Helper.GetWantedWritingForm(col["COLUMN_NAME"].ToString(), args.PropertiesForm);

                            mProperty = new PropertyModel(field_name, false, col);
                            mEntity.Properties.Add(field_name, mProperty);

                            if (col["COLUMN_KEY"].ToString() == "PRI")
                                mEntity.PrimaryKeys.Add(field_name, mProperty);
                        }

                        EntitiesForest.Add(new EntityNode { Value = mEntity });
                    }
                }

                DatabaseName = con.Database;
            }

            EntitiesForest.Sort(new DefaultEntityNodeComparer());
            _DbAnalyseArgs = args;
        }

        public void BuildInheritedEntitiesForest()
        {
            InheritedEntitiesForest = new List<EntityNode>();
            List<PropertyModel> props_list = null, tmp_list;
            EntityNode baseentity;
            int last_count, j;

            foreach(EntityNode node in EntitiesForest)
            {
                if (node.Children != null)
                    node.Children.Clear();
            }

            for (int i = EntitiesForest.Count - 1; i >= 0; --i)
            {
                baseentity = null;
                last_count = j = 0;

                while(j < EntitiesForest.Count && EntitiesForest[i].Value.Properties.Count >= EntitiesForest[j].Value.Properties.Count)
                {
                    if(i != j && EntitiesForest[i] != EntitiesForest[j].Father)
                    {
                        tmp_list = new List<PropertyModel>();
                        Helper.GetInheritedProperties(EntitiesForest[j].Value, EntitiesForest[i].Value, tmp_list);

                        if(tmp_list.Count > last_count)
                        {
                            props_list = tmp_list;
                            last_count = tmp_list.Count;
                            baseentity = EntitiesForest[j];
                        }
                    }

                    ++j;
                }

                if (baseentity != null)
                    baseentity.AddChild(EntitiesForest[i], props_list);

                else
                    InheritedEntitiesForest.Add(EntitiesForest[i]);
            }
        }

        public void GenerateTheDao(string GenPath)
        {
            DAOGenerator.AnalyseArgs = _DbAnalyseArgs;

            string RootFolder = string.Format("{0}/{1}_DAO", GenPath, DatabaseName);
            int i = 1;

            while(Directory.Exists(RootFolder))
            {
                RootFolder = string.Format("{0}/{1}_DAO_{2}", GenPath, DatabaseName, i);
                i++;
            }

            Directory.CreateDirectory(RootFolder);
            Directory.SetCurrentDirectory(RootFolder);

            Directory.CreateDirectory("Entities");
            Directory.CreateDirectory("Managers");

            Directory.SetCurrentDirectory("Entities");
            DAOGenerator.GenerateEntities(EntitiesForest);

            Directory.SetCurrentDirectory("../Managers");
            DAOGenerator.GenerateManagers(EntitiesForest);
        }
    }

    public class DefaultEntityNodeComparer : IComparer<EntityNode>
    {
        public int Compare(EntityNode x, EntityNode y)
        {
            return x.Value.Properties.Count - y.Value.Properties.Count;
        }
    }
}
