﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rules
{
    public class DbConnectionException : Exception
    {
        public DbConnectionException(string Message) : base(Message)
        {

        }
    }
}
