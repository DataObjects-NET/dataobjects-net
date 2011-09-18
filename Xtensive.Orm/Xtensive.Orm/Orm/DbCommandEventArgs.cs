using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Xtensive.Orm
{
    [Serializable]
    public class DbCommandEventArgs : EventArgs
    {
        public DbCommandEventArgs(DbCommand command)
        {
            Command = command;
        }

        public DbCommand Command { get; private set; }
    }
}
