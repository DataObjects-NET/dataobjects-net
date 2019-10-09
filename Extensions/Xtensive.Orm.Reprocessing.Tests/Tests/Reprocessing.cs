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
  public class Reprocessing : AutoBuildTest
  {
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
            Interlocked.Increment(ref Count);
            new Bar2(session, DateTime.Now, Guid.NewGuid()) {Name = Guid.NewGuid().ToString()};
            if (first) {
              session.Query.All<Foo>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
              if (wait1!=null) {
                wait1.Set();
                wait2.WaitOne();
                wait1 = null;
              }
              session.Query.All<Bar>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
            }
            else {
              session.Query.All<Bar>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
              if (wait2!=null) {
                wait2.Set();
                wait1.WaitOne();
                wait2 = null;
              }
              session.Query.All<Foo>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
            }
          });
      }

      public void External(
        bool first,
        IsolationLevel? isolationLevel,
        TransactionOpenMode? transactionOpenMode,
        Action<bool, IsolationLevel?, TransactionOpenMode?> action)
      {
        using (Session session = domain.OpenSession())
        using (Xtensive.Orm.TransactionScope tran = isolationLevel==null ? null : session.OpenTransaction())
        using (session.Activate()) {
          if (tran!=null) {
            session.EnsureTransactionIsStarted();
            new Bar2(session, DateTime.Now, Guid.NewGuid()) {Name = Guid.NewGuid().ToString()};
          }
          if (first) {
            if (wait1!=null && wait2!=null) {
              wait1.Set();
              wait2.WaitOne();
            }
          }
          else if (wait1!=null && wait2!=null) {
            wait2.Set();
            wait1.WaitOne();
          }
          action(first, isolationLevel, transactionOpenMode);
          if (tran!=null)
            tran.Complete();
        }
      }

      public void Parent(
        bool first,
        IsolationLevel? isolationLevel,
        TransactionOpenMode? transactionOpenMode,
        IExecuteActionStrategy strategy,
        Action<bool, IsolationLevel?, TransactionOpenMode?> action)
      {
        domain.WithStrategy(strategy).WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead)).WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New)).Execute(
          session => {
            session.EnsureTransactionIsStarted();
            new Bar2(session, DateTime.Now, Guid.NewGuid()) {Name = Guid.NewGuid().ToString()};
            if (first) {
              if (wait1!=null && wait2!=null) {
                wait1.Set();
                wait2.WaitOne();
              }
            }
            else if (wait1!=null && wait2!=null) {
              wait2.Set();
              wait1.WaitOne();
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
            new Bar(session);
            new Foo(session);
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
            Interlocked.Increment(ref Count);
            session.EnsureTransactionIsStarted();
            new Bar2(session, DateTime.Now, Guid.NewGuid()) {Name = Guid.NewGuid().ToString()};
            string name = "test";
            if (!session.Query.All<Foo>().Any(a => a.Name==name)) {
              if (first) {
                if (wait1!=null && wait2!=null) {
                  wait1.Set();
                  wait2.WaitOne();
                  wait1 = null;
                }
              }
              else if (wait2!=null && wait2!=null) {
                wait2.Set();
                wait1.WaitOne();
                wait2 = null;
              }
              new Foo(session) {Name = name};
            }
            session.SaveChanges();
          });
      }

      public void UniqueConstraintViolationPrimaryKey(
        bool first, IsolationLevel? isolationLevel, TransactionOpenMode? transactionOpenMode)
      {
        domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).WithIsolationLevel(isolationLevel.GetValueOrDefault(IsolationLevel.RepeatableRead)).WithTransactionOpenMode(transactionOpenMode.GetValueOrDefault(TransactionOpenMode.New)).Execute(
          session => {
            Interlocked.Increment(ref Count);
            session.EnsureTransactionIsStarted();
            new Bar2(session, DateTime.Now, Guid.NewGuid()) {Name = Guid.NewGuid().ToString()};
            int id = 10;
            if (session.Query.SingleOrDefault<Foo>(id)==null) {
              AutoResetEvent w1 = wait1;
              AutoResetEvent w2 = wait2;
              if (first) {
                if (w1!=null && w2!=null) {
                  w1.Set();
                  w2.WaitOne();
                  wait1 = null;
                }
              }
              else if (w1!=null && w2!=null) {
                w2.Set();
                w1.WaitOne();
                wait2 = null;
              }
              new Foo(session, id) {Name = Guid.NewGuid().ToString()};
            }
            session.SaveChanges();
          });
      }

      public Context(Domain domain)
      {
        this.domain = domain;
      }
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
            (b1, level1, open1) =>
              context.Parent(
                b1,
                IsolationLevel.Snapshot,
                open1,
                ExecuteActionStrategy.HandleUniqueConstraintViolation,
                context.UniqueConstraintViolation)));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));

      //ExternalWithTransaction nested snapshot UniqueConstraint
      context = new Context(Domain);
      var ex =
        Assert.Throws<AggregateException>(
          () =>
            context.Run(
              IsolationLevel.Snapshot,
              null,
              (b, level, open) =>
                context.External(
                  b,
                  level,
                  open,
                  (b1, level1, open1) =>
                    context.Parent(
                      b1,
                      level1,
                      open1,
                      ExecuteActionStrategy.HandleUniqueConstraintViolation,
                      context.UniqueConstraintViolation))));
      Assert.That(
        ex.InnerExceptions.Single(),
        Is.TypeOf<InvalidOperationException>().Or.TypeOf<StorageException>().With.Property("InnerException").TypeOf
          <InvalidOperationException>());
      Assert.That(context.Count, Is.EqualTo(2));
      Assert.That(Bar2Count(), Is.EqualTo(3));

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
      int i = 0;
      Domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).Execute(
        session => {
          new Foo(session, i);
          i++;
          if (i < 5)
            new Foo(session, i);
        }
        );
      Assert.That(i, Is.EqualTo(5));
    }

    [Test]
    public void UniqueConstraintViolationExceptionUnique()
    {
      int i = 0;
      bool b = false;
      ExecuteActionStrategy.HandleUniqueConstraintViolation.Error += (sender, args) => b = true;
      Domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).Execute(
        session => {
          new Foo(session) {Name = "test"};
          i++;
          if (i < 5)
            new Foo(session) {Name = "test"};
        });
      Assert.That(i, Is.EqualTo(5));
      Assert.That(b, Is.True);
    }
  }
}

// ReSharper restore ReturnValueOfPureMethodIsNotUsed
// ReSharper restore ConvertToConstant.Local
// ReSharper restore AccessToModifiedClosure