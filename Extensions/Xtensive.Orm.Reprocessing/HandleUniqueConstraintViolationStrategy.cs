using System.Data.SqlClient;
using System.Transactions;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Reprocess task when <see cref="ReprocessableException"/> or <see cref="UniqueConstraintViolationException"/> are thrown.
  /// </summary>
  public class HandleUniqueConstraintViolationStrategy : HandleReprocessableExceptionStrategy
  {
    #region Non-public methods

    /// <summary>
    /// Handles the exception.
    /// </summary>
    /// <param name="eventArgs">The <see cref="Xtensive.Orm.Reprocessing.ExecuteErrorEventArgs"/> instance containing the exception data.</param>
    /// <returns>
    /// True if needs to reprocess the task, otherwise false.
    /// </returns>
    protected override bool HandleException(ExecuteErrorEventArgs eventArgs)
    {
      if (eventArgs.Transaction!=null && eventArgs.Transaction.IsNested &&
        eventArgs.Transaction.IsolationLevel==IsolationLevel.Snapshot)
        return false;
      if (eventArgs.Exception is UniqueConstraintViolationException)
        return OnError(eventArgs);
      var ex = eventArgs.Exception as StorageException;
      if (ex!=null) {
        var ex1 = ex.InnerException as SqlException;
        if (ex1!=null && ex1.Number==2601)
          return OnError(eventArgs);
      }
      return base.HandleException(eventArgs);
    }

    #endregion
  }
}