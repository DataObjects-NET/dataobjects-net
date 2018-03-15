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
    /// Gets exception, thrown during expression execution. <see langword="null" /> if expression executed successfully.
    /// </summary>
    public Exception Exception { get; private set; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// <param name="expression">Executed expression.</param>
    /// <param name="exception">Exception, appeared during expression execution or <see langword="null"/>.</param>
    /// </summary>
    public QueryEventArgs(Expression expression, Exception exception = null)
    {
      Expression = expression;
      Exception = exception;
    }
  }
}
