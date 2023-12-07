// ReSharper disable ConvertToConstant.Local
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable AccessToModifiedClosure

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using TestCommon.Model;

namespace Xtensive.Orm.Reprocessing.Tests.ReprocessingContext
{
  public class Context
  {
    private readonly Domain domain;
    public int Count;
    private AutoResetEvent wait1 = new AutoResetEvent(false);
    private AutoResetEvent wait2 = new AutoResetEvent(false);

    public void Deadlock(bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
    {
      TestContext.WriteLine("Context.DeadLock entered");
      domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead)).WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New)).Execute(
        session => {
          _ = Interlocked.Increment(ref Count);
          _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
          if (first) {
            _ = session.Query.All<Foo>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
            if (wait1 != null) {
              _ = wait1.Set();
              _ = wait2.WaitOne();
              wait1 = null;
            }
            _ = session.Query.All<Bar>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
          }
          else {
            _ = session.Query.All<Bar>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
            if (wait2 != null) {
              _ = wait2.Set();
              _ = wait1.WaitOne();
              wait2 = null;
            }
            _ = session.Query.All<Foo>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
          }
        });
      TestContext.WriteLine("Context.DeadLock left");
    }

    public void External(
      bool first,
      IsolationLevel? isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      Action<Session, bool, IsolationLevel?, TransactionOpenMode?> action)
    {
      TestContext.WriteLine("Context.External entered");
      using (var session = domain.OpenSession())
      using (var tran = isolationLevel == null ? null : session.OpenTransaction()) {
        if (tran != null) {
          session.EnsureTransactionIsStarted();
          _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
        }
        if (first) {
          if (wait1 != null && wait2 != null) {
            _ = wait1.Set();
            _ = wait2.WaitOne();
          }
        }
        else if (wait1 != null && wait2 != null) {
          _ = wait2.Set();
          _ = wait1.WaitOne();
        }
        action(session, first, isolationLevel, transactionOpenMode);
        if (tran != null) {
          tran.Complete();
        }
      }
      TestContext.WriteLine("Context.External left");
    }

    public void Parent(
      bool first,
      IsolationLevel? isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      IExecuteActionStrategy strategy,
      Action<bool, IsolationLevel?, TransactionOpenMode?> action)
    {
      TestContext.WriteLine("Context.Parent1 entered");
      domain.WithStrategy(strategy)
        .WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead))
        .WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New))
        .Execute(
          session => {
            session.EnsureTransactionIsStarted();
            _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
            if (first) {
              if (wait1 != null && wait2 != null) {
                _ = wait1.Set();
                _ = wait2.WaitOne();
              }
            }
            else if (wait1 != null && wait2 != null) {
              _ = wait2.Set();
              _ = wait1.WaitOne();
            }
            action(first, isolationLevel, transactionOpenMode);
          });
      TestContext.WriteLine("Context.Parent1 left");
    }

    public void Parent(
      Session session,
      bool first,
      IsolationLevel? isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      IExecuteActionStrategy strategy,
      Action<bool, IsolationLevel?, TransactionOpenMode?> action)
    {
      TestContext.WriteLine("Context.Parent2 entered");
      domain.WithStrategy(strategy)
        .WithSession(session)
        .WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead))
        .WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New))
        .Execute(
          session => {
            session.EnsureTransactionIsStarted();
            _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
            if (first) {
              if (wait1 != null && wait2 != null) {
                _ = wait1.Set();
                _ = wait2.WaitOne();
              }
            }
            else if (wait1 != null && wait2 != null) {
              _ = wait2.Set();
              _ = wait1.WaitOne();
            }
            action(first, isolationLevel, transactionOpenMode);
          });
      TestContext.WriteLine("Context.Parent2 left");
    }

    public void Run(
      IsolationLevel? isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      Action<bool, IsolationLevel?, TransactionOpenMode?> action)
    {
      TestContext.WriteLine("Context.Run entered");
      domain.Execute(
        session => {
          session.Remove(session.Query.All<Foo>());
          session.Remove(session.Query.All<Bar>());
          session.Remove(session.Query.All<Bar2>());
          _ = new Bar(session);
          _ = new Foo(session);
        });
      TestContext.WriteLine("Context.Run executed Domain.Execute");
      TestContext.WriteLine("Context.Run Parallel.Invoke started");
      Parallel.Invoke(
        () => action(true, isolationLevel, transactionOpenMode),
        () => action(false, isolationLevel, transactionOpenMode));
      TestContext.WriteLine("Context.Run Parallel.Invoke ended");
      TestContext.WriteLine("Context.Run left");
    }

    public void UniqueConstraintViolation(
      bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
    {
      TestContext.WriteLine("Context.UniqueConstraintViolation entered");
      domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead)).WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New)).Execute(
        session => {
          _ = Interlocked.Increment(ref Count);
          session.EnsureTransactionIsStarted();
          _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
          var name = "test";
          if (!session.Query.All<Foo>().Any(a => a.Name == name)) {
            if (first) {
              if (wait1 != null && wait2 != null) {
                _ = wait1.Set();
                _ = wait2.WaitOne();
                wait1 = null;
              }
            }
            else if (wait2 != null && wait2 != null) {
              _ = wait2.Set();
              _ = wait1.WaitOne();
              wait2 = null;
            }
            _ = new Foo(session) { Name = name };
          }
          session.SaveChanges();
        });
      TestContext.WriteLine("Context.UniqueConstraintViolation left");
    }

    public void UniqueConstraintViolationPrimaryKey(
      bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
    {
      TestContext.WriteLine("Context.UniqueConstraintViolationPrimaryKey entered");
      domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead)).WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New)).Execute(
        session => {
          _ = Interlocked.Increment(ref Count);
          session.EnsureTransactionIsStarted();
          _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
          var id = 10;
          if (session.Query.SingleOrDefault<Foo>(id) == null) {
            var w1 = wait1;
            var w2 = wait2;
            if (first) {
              if (w1 != null && w2 != null) {
                _ = w1.Set();
                _ = w2.WaitOne();
                wait1 = null;
              }
            }
            else if (w1 != null && w2 != null) {
              _ = w2.Set();
              _ = w1.WaitOne();
              wait2 = null;
            }
            _ = new Foo(session, id) { Name = Guid.NewGuid().ToString() };
          }
          session.SaveChanges();
        });
      TestContext.WriteLine("Context.UniqueConstraintViolationPrimaryKey left");
    }

    public Context(Domain domain)
    {
      this.domain = domain;
    }
  }
}

// ReSharper restore ReturnValueOfPureMethodIsNotUsed
// ReSharper restore ConvertToConstant.Local
// ReSharper restore AccessToModifiedClosure
