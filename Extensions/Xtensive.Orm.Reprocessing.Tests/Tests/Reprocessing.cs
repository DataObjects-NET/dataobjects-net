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

namespace Xtensive.Orm.Reprocessing.Tests
{
  public class Reprocessing : ReprocessingBaseTest
  {
    private bool treatNullAsUniqueValue;

    private int Bar2Count()
    {
      return Domain.Execute(session => Queryable.Count(session.Query.All<Bar2>()));
    }

    private class Context
    {
      private readonly Domain domain;
      public int Count;
      private AutoResetEvent wait1 = new AutoResetEvent(false);
      private AutoResetEvent wait2 = new AutoResetEvent(false);

      public void Deadlock(bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
      {
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
      }

      public void External(
        bool first,
        IsolationLevel? isolationLevel,
        TransactionOpenMode? transactionOpenMode,
        Action<Session, bool, IsolationLevel?, TransactionOpenMode?> action)
      {
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
      }

      public void Parent(
        bool first,
        IsolationLevel? isolationLevel,
        TransactionOpenMode? transactionOpenMode,
        IExecuteActionStrategy strategy,
        Action<bool, IsolationLevel?, TransactionOpenMode?> action)
      {
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
      }

      public void Parent(
        Session session,
        bool first,
        IsolationLevel? isolationLevel,
        TransactionOpenMode? transactionOpenMode,
        IExecuteActionStrategy strategy,
        Action<bool, IsolationLevel?, TransactionOpenMode?> action)
      {
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
      }

      public void Run(
        IsolationLevel? isolationLevel,
        TransactionOpenMode? transactionOpenMode,
        Action<bool, IsolationLevel?, TransactionOpenMode?> action)
      {
        domain.Execute(
          session => {
            session.Remove(session.Query.All<Foo>());
            session.Remove(session.Query.All<Bar>());
            session.Remove(session.Query.All<Bar2>());
            _ = new Bar(session);
            _ = new Foo(session);
          });
        Parallel.Invoke(
          () => action(true, isolationLevel, transactionOpenMode),
          () => action(false, isolationLevel, transactionOpenMode));
      }

      public void UniqueConstraintViolation(
        bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
      {
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
      }

      public void UniqueConstraintViolationPrimaryKey(
        bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
      {
        domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead)).WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New)).Execute(
          session => {
            _ = Interlocked.Increment(ref Count);
            session.EnsureTransactionIsStarted();
            _ = new Bar2(session, DateTime.Now, Guid.NewGuid()) { Name = Guid.NewGuid().ToString() };
            var id = 10;
            if (session.Query.SingleOrDefault<Foo>(id)==null) {
              var w1 = wait1;
              var w2 = wait2;
              if (first) {
                if (w1!=null && w2!=null) {
                  _ = w1.Set();
                  _ = w2.WaitOne();
                  wait1 = null;
                }
              }
              else if (w1!=null && w2!=null) {
                _ = w2.Set();
                _ = w1.WaitOne();
                wait2 = null;
              }
              _ = new Foo(session, id) { Name = Guid.NewGuid().ToString() };
            }
            session.SaveChanges();
          });
      }

      public Context(Domain domain)
      {
        this.domain = domain;
      }
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      treatNullAsUniqueValue = Domain.StorageProviderInfo.ProviderName == WellKnown.Provider.SqlServer;
    }

    [Test]
    public void Test()
    {
      var context = new Context(Domain);
      context.Run(null, null, context.UniqueConstraintViolation);
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(2));

      context = new Context(Domain);
      context.Run(IsolationLevel.Serializable, null, context.Deadlock);
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(2));


      //nested serializable deadlock
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Serializable,
        null,
        (b, level, open) => context.Parent(b, level, open, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));

      //nested snapshot deadlock
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Snapshot,
        null,
        (b, level, open) => context.Parent(b, level, open, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));

      //nested nested serializable deadlock
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Serializable,
        null,
        (b, level, open) =>
          context.Parent(
            b,
            level,
            open,
            ExecuteActionStrategy.HandleReprocessableException,
            (b1, level1, open1) =>
              context.Parent(b1, level1, open1, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock)));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(6));

      //nested snapshot UniqueConstraint PrimaryKey
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Snapshot,
        null,
        (b, level, open) =>
          context.Parent(
            b, level, open, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolationPrimaryKey));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));

      //nested snapshot UniqueConstraint UniqueIndex
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Snapshot,
        null,
        (b, level, open) =>
          context.Parent(
            b, level, null, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolation));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));

      //ExternalWithoutTransaction nested snapshot UniqueConstraint
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Snapshot,
        null,
        (b, level, open) =>
          context.External(
            b,
            null,
            open,
            (s, b1, level1, open1) =>
              context.Parent(
                s,
                b1,
                IsolationLevel.Snapshot,
                open1,
                ExecuteActionStrategy.HandleUniqueConstraintViolation,
                context.UniqueConstraintViolation)));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));

      //ExternalWithTransaction nested snapshot UniqueConstraint
      context = new Context(Domain);
      context.Run(
        IsolationLevel.Snapshot,
        null,
        (b, level, open) =>
          context.External(
            b,
            level,
            open,
            (s, b1, level1, open1) =>
              context.Parent(
                s,
                b1,
                level1,
                open1,
                ExecuteActionStrategy.HandleUniqueConstraintViolation,
                context.UniqueConstraintViolation)));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(6));

      //nested UniqueConstraint with auto transaction
      context = new Context(Domain);
      context.Run(
        IsolationLevel.ReadUncommitted,
        TransactionOpenMode.Auto,
        (b, l, o) =>
          context.Parent(b, l, o, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolation));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));
    }

    [Test]
    public void UniqueConstraintViolationExceptionPrimary()
    {
      var i = 0;
      Domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).Execute(
        session => {
          _ = new Foo(session, i);
          i++;
          if (i < 5) {
            _ = new Foo(session, i);
          }
        });
      if (treatNullAsUniqueValue) {
        Assert.That(i, Is.EqualTo(5));
      }
      else {
        Assert.That(i, Is.EqualTo(1));
      }
    }

    [Test]
    public void UniqueConstraintViolationExceptionUnique()
    {
      var i = 0;
      var errorNotified = false;
      ExecuteActionStrategy.HandleUniqueConstraintViolation.Error += (sender, args) => errorNotified = true;
      Domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).Execute(
        session => {
          _ = new Foo(session) { Name = "test" };
          i++;
          if (i < 5) {
            _ = new Foo(session) { Name = "test" };
          }
        });
      Assert.That(i, Is.EqualTo(5));
      Assert.That(errorNotified, Is.True);
    }
  }
}

// ReSharper restore ReturnValueOfPureMethodIsNotUsed
// ReSharper restore ConvertToConstant.Local
// ReSharper restore AccessToModifiedClosure
