// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Provides data for the <see cref="IExecuteActionStrategy.Error"/> event
  /// </summary>
  public readonly struct ExecuteErrorEventArgs
  {
    /// <summary>
    /// Gets the attempt number of this task.
    /// </summary>
    public int Attempt { get; }

    /// <summary>
    /// Gets the exception of this task.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the session of this task. Session will have outer transaction.
    /// </summary>
    public Session Session { get; }

    /// <summary>
    /// Gets the transaction of this task.
    /// </summary>
    public Transaction Transaction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteErrorEventArgs"/> class.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="session">The session with outer transaction.</param>
    /// <param name="transaction">The transaction of this task. Transaction will be in <see cref="TransactionState.RolledBack"/> state.</param>
    /// <param name="attempt">The attempt number.</param>
    public ExecuteErrorEventArgs(Exception exception, Session session, Transaction transaction, int attempt)
    {
      Attempt = attempt;
      Exception = exception;
      Session = session;
      Transaction = transaction;
    }
  }
}
