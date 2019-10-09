namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Reprocess task when <see cref="ReprocessableException"/> is thrown.
  /// </summary>
  public class HandleReprocessableExceptionStrategy : ExecuteActionStrategy
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
      if (eventArgs.Exception is ReprocessableException)
        return OnError(eventArgs);
      return base.HandleException(eventArgs);
    }

    #endregion
  }
}