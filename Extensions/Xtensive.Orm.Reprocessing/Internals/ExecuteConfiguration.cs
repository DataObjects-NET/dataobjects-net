using System;
using System.Transactions;

namespace Xtensive.Orm.Reprocessing
{
  internal class ExecuteConfiguration : IExecuteConfiguration
  {
    protected Domain Domain { get; private set; }

    public Session ExternalSession { get; set; }

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

    public IExecuteConfiguration WithExternalSession(Session session)
    {
      ExternalSession = session;
      return this;
    }

    public void Execute(Action<Session> action)
    {
      if (ExternalSession != null) {
        Domain.ExecuteInternal(ExternalSession, IsolationLevel, TransactionOpenMode, Strategy, action);
      }
      else {
        Domain.ExecuteInternal(IsolationLevel, TransactionOpenMode, Strategy, action);
      }
    }

    public T Execute<T>(Func<Session, T> func)
    {
      return (ExternalSession != null)
        ? Domain.ExecuteInternal(ExternalSession, IsolationLevel, TransactionOpenMode, Strategy, func)
        : Domain.ExecuteInternal(IsolationLevel, TransactionOpenMode, Strategy, func);
    }

    public ExecuteConfiguration(Domain domain)
    {
      Domain = domain;
      IsolationLevel = IsolationLevel.Unspecified;
    }
  }
}