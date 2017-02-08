using System;
using System.Linq.Expressions;


namespace Xtensive.Orm
{
  /// <summary>
  /// Event args for <see cref="SessionEventAccessor.QueryExecuting"/>
  /// and <see cref="SessionEventAccessor.QueryExecuted"/>.
  /// </summary>
  public class QueryEventArgs : EventArgs
  {
    /// <summary>
    /// Gets executed expression.
    /// </summary>
    public Expression Expression { get; set; }

    /// <summary>
    /// Gets exception, if any, thrown durring query execution
    /// </summary>
    public Exception Exception { get; private set; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// <param name="expression">Executed expression.</param>
    /// </summary>
    public QueryEventArgs(Expression expression, Exception exception=null)
    {
      Expression = expression;
      Exception = exception;
    }
  }
}
