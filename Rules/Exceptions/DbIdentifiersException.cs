using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rules
{
    public class DbIdentifiersException : Exception
    {
        public DbIdentifiersException(string Message) : base(Message)
        {

        }
    }
}
