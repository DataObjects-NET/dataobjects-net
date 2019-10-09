using System;
using System.Transactions;

namespace Xtensive.Orm.Reprocessing
{
  internal class ExecuteConfiguration : IExecuteConfiguration
  {
    public ExecuteConfiguration(Domain domain)
    {
      Domain = domain;
      IsolationLevel = IsolationLevel.Unspecified;
    }

    protected Domain Domain { get; private set; }

    public IExecuteActionStrategy Strategy { get; set; }

    public IsolationLevel IsolationLevel { get; set; }

    public TransactionOpenMode? TransactionOpenMode { get; set; }

    public IExecuteConfiguration WithIsolationLevel(IsolationLevel isolationLevel)
    {
      IsolationLevel = isolationLevel;
      return this;
    }

    public IExecuteConfiguration WithStrategy(IExecuteActionStrategy strategy)
    {
      Strategy = strategy;
      return this;
    }

    public IExecuteConfiguration WithTransactionOpenMode(TransactionOpenMode transactionOpenMode)
    {
      TransactionOpenMode = transactionOpenMode;
      return this;
    }

    public void Execute(Action<Session> action)
    {
      Domain.ExecuteInternal(IsolationLevel, TransactionOpenMode, Strategy, action);
    }

    public T Execute<T>(Func<Session, T> func)
    {
      return Domain.ExecuteInternal(IsolationLevel, TransactionOpenMode, Strategy, func);
    }
  }
}