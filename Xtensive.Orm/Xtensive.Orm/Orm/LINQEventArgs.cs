using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Xtensive.Orm
{
    [Serializable]
    public class LinqEventArgs : EventArgs
    {
        public LinqEventArgs(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; private set; }
    }
}
