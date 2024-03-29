using System;
using System.Collections.Concurrent;
using System.Transactions;
using Xtensive.Core;
using SystemTransaction = System.Transactions.Transaction;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Base class for the standard execute action strategies.
  /// </summary>
  public abstract class ExecuteActionStrategy : IExecuteActionStrategy
  {
    private static readonly ConcurrentDictionary<Type, Lazy<IExecuteActionStrategy>> Singletons =
      new ConcurrentDictionary<Type, Lazy<IExecuteActionStrategy>>();

    /// <summary>
    /// Gets singleton of the <see cref="HandleReprocessableExceptionStrategy"/>.
    /// </summary>
    public static readonly HandleReprocessableExceptionStrategy HandleReprocessableException =
      new HandleReprocessableExceptionStrategy();

    /// <summary>
    /// Gets singleton of the <see cref="HandleUniqueConstraintViolationStrategy"/>.
    /// </summary>
    public static readonly HandleUniqueConstraintViolationStrategy HandleUniqueConstraintViolation =
      new HandleUniqueConstraintViolationStrategy();

    /// <summary>
    /// Gets singleton of the <see cref="NoReprocessStrategy"/>.
    /// </summary>
    public static readonly NoReprocessStrategy NoReprocess = new NoReprocessStrategy();

    /// <summary>
    /// Gets or sets attempts limit.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Execute task.
    /// </summary>
    /// <typeparam name="T">Task return type.</typeparam>
    /// <param name="context">The context of this task.</param>
    /// <returns>
    /// Task result.
    /// </returns>
    public virtual T Execute<T>(ExecutionContext<T> context)
    {
      var i = 0;
      while (true) {
        Session session = null;
        TransactionScope tran = null;
        var needBreak = false;
        Transaction transaction = null;
        try {
          T result;
          var disposeSession = false;
          try {
            try {
              var isolationLevel = context.IsolationLevel;
              var currentTransaction = SystemTransaction.Current;
              if (isolationLevel == IsolationLevel.Unspecified && currentTransaction != null) {
                isolationLevel = currentTransaction.IsolationLevel;
              }

              session = context.ExternalSession;
              if (session == null) {
                session = context.Domain.OpenSession();
                disposeSession = true;
              }
              if (currentTransaction != null && session.Transaction != null) {
                session.EnsureTransactionIsStarted();
              }

              tran = (currentTransaction != null && currentTransaction.TransactionInformation.DistributedIdentifier != Guid.Empty) ||
                     (session.Transaction != null && session.Transaction.IsDisconnected)
                       ? session.OpenTransaction(isolationLevel)
                       : session.OpenTransaction(context.TransactionOpenMode, isolationLevel);
              transaction = session.Transaction;
              result = context.Function(session);
              tran.Complete();
            }
            finally {
              try {
                tran.DisposeSafely();
              }
              catch (StorageException e) {
                if (e.InnerException == null || !(e.InnerException is InvalidOperationException) ||
                    !e.InnerException.Source.Equals("System.Data", StringComparison.Ordinal)) {
                  throw;
                }

                if (tran.Transaction.IsNested) {
                  needBreak = true;
                }
              }
            }
          }
          finally {
            if (disposeSession) {
              session.DisposeSafely();
            }
          }
          return result;
        }
        catch (Exception e) {
          if ((SystemTransaction.Current != null &&
               SystemTransaction.Current.TransactionInformation.DistributedIdentifier != Guid.Empty) ||
              (transaction != null && (transaction.State == TransactionState.Active || transaction.IsDisconnected))) {
            throw;
          }
          if (e is RollbackTransactionException) {
            return default(T);
          }

          i++;
          if (needBreak || !HandleException(new ExecuteErrorEventArgs(e, session, transaction, i))) {
            throw;
          }
        }
      }
    }

    /// <summary>
    /// Gets the singleton of the specified strategy
    /// </summary>
    /// <param name="type">The type of the strategy.</param>
    /// <returns>The singleton.</returns>
    public static IExecuteActionStrategy GetSingleton(Type type)
    {
      if (type == typeof(HandleReprocessableExceptionStrategy)) {
        return HandleReprocessableException;
      }
      if (type == typeof(HandleUniqueConstraintViolationStrategy)) {
        return HandleUniqueConstraintViolation;
      }
      if (type == typeof(NoReprocessStrategy)) {
        return NoReprocess;
      }

      return Singletons.GetOrAdd(type, ActionStrategyFactory).Value;

      static Lazy<IExecuteActionStrategy> ActionStrategyFactory(Type t)
      {
        return new Lazy<IExecuteActionStrategy>(() => (IExecuteActionStrategy) Activator.CreateInstance(t));
      }
    }

    #region Non-public methods

    /// <summary>
    /// Handles the exception.
    /// </summary>
    /// <param name="eventArgs">The <see cref="Xtensive.Orm.Reprocessing.ExecuteErrorEventArgs"/> instance containing the exception data.</param>
    /// <returns>True if needs to reprocess the task, otherwise false.</returns>
    protected virtual bool HandleException(ExecuteErrorEventArgs eventArgs) => false;

    /// <summary>
    /// Raises the <see cref="E:Error"/> event.
    /// </summary>
    /// <param name="eventArgs">The <see cref="Xtensive.Orm.Reprocessing.ExecuteErrorEventArgs"/> instance containing the event data.</param>
    /// <returns>True if needs to reprocess the task.</returns>
    protected virtual bool OnError(ExecuteErrorEventArgs eventArgs)
    {
      if (Error != null) {
        Error(this, eventArgs);
      }

      return eventArgs.Attempt < Attempts;
    }

    #endregion

    #region IExecuteActionStrategy Members

    /// <summary>
    /// Occurs when unhandled exception is thrown
    /// </summary>
    public event EventHandler<ExecuteErrorEventArgs> Error;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteActionStrategy"/> class.
    /// </summary>
    protected ExecuteActionStrategy()
    {
      Attempts = 5;
    }
  }
}