using System;
using System.Transactions;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Configuration for executing reprocessable operation.
  /// </summary>
  public interface IExecuteConfiguration
  {
    /// <summary>
    /// Specifies <see cref="IExecuteActionStrategy"/> to use for this configuration.
    /// </summary>
    /// <param name="strategy">Strategy to use.</param>
    /// <returns>This instance.</returns>
    IExecuteConfiguration WithStrategy(IExecuteActionStrategy strategy);

    /// <summary>
    /// Specifies <see cref="IsolationLevel"/> to use for this configuration.
    /// </summary>
    /// <param name="isolationLevel">Isolation level to use.</param>
    /// <returns>This instance.</returns>
    IExecuteConfiguration WithIsolationLevel(IsolationLevel isolationLevel);

    /// <summary>
    /// Specifies <see cref="TransactionOpenMode"/> to use for this configuration.
    /// </summary>
    /// <param name="transactionOpenMode">Transction open mode to use.</param>
    /// <returns>This instance.</returns>
    IExecuteConfiguration WithTransactionOpenMode(TransactionOpenMode transactionOpenMode);

    /// <summary>
    /// Executes specified reprocessable <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    void Execute(Action<Session> action);

    /// <summary>
    /// Executes specified reprocessable <paramref name="func"/>.
    /// </summary>
    /// <typeparam name="T">Type of the result.</typeparam>
    /// <param name="func">Function to execute.</param>
    /// <returns>Result of <paramref name="func"/> execution.</returns>
    T Execute<T>(Func<Session, T> func);
  }
}
