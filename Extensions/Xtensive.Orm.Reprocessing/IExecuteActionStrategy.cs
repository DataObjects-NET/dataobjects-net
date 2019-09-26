using System;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Defines action strategy contract.
  /// </summary>
  public interface IExecuteActionStrategy
  {
    /// <summary>
    /// Execute task.
    /// </summary>
    /// <param name="context">The context of this task.</param>
    /// <typeparam name="T">Task return type.</typeparam>
    /// <returns>Task result.</returns>
    T Execute<T>(ExecutionContext<T> context);

    /// <summary>
    /// Occurs when unhandled exception is thrown
    /// </summary>
    event EventHandler<ExecuteErrorEventArgs> Error;
  }
}