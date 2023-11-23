using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace Rules
{
    class Program
    {
        static void Main(string[] args)
        {
            DbAnalyzer a = new DbAnalyzer("MySql.Data.MySqlClient", "Server=localhost;Database=mermaids_flow;Uid=root;Pwd=78b8af26f;");
            DbAnalyseArgs aa = new DbAnalyseArgs();
            aa.ClassesForm = NamesWritingForm.PascalCase;
            a.AnalyseTheDatabase(aa);
            a.BuildInheritedEntitiesForest();
            
            Console.Read();
        }
    }
}
