using System;
using System.Transactions;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Task execution context.
  /// </summary>
  /// <typeparam name="T">Return type of the task. <see cref="object"/> if the task is specified by <see cref="Action{T}"/></typeparam>
  public class ExecutionContext<T>
  {
    /// <summary>
    /// Gets the domain of this task.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the <see cref="System.Transactions.IsolationLevel"/> of this task.
    /// </summary>
    public IsolationLevel IsolationLevel { get; private set; }

    /// <summary>
    /// Gets the task.
    /// </summary>
    public Func<Session, T> Function { get; private set; }

    /// <summary>
    /// Gets the <see cref="Orm.TransactionOpenMode"/> of this task
    /// </summary>
    public TransactionOpenMode TransactionOpenMode { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionContext{T}"/> class.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="transactionOpenMode">The transaction open mode.</param>
    /// <param name="function">The task.</param>
    public ExecutionContext(
      Domain domain, IsolationLevel isolationLevel, TransactionOpenMode transactionOpenMode, Func<Session, T> function)
    {
      Domain = domain;
      IsolationLevel = isolationLevel;
      TransactionOpenMode = transactionOpenMode;
      Function = function;
    }
  }
}