using System;
using System.Linq.Expressions;

namespace Xtensive.Orm
{
    [Serializable]
    public class QueryEventArgs : EventArgs
    {
        public QueryEventArgs(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; set; }
    }
}
