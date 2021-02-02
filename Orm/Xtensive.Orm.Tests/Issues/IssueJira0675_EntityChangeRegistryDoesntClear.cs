// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Interfaces.Model;
using Xtensive.Orm.Tests.Issues.CustomerBug1Model;

namespace Xtensive.Orm.Tests.Issues.CustomerBug1Model
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public int Index { get; set; }

    [Field]
    public int Value { get; set; }
  }

  [HierarchyRoot]
  [Index("Index", Unique = true)]
  [Index("Value", Unique = true)]
  public class TestEntityWithUniqueIndex : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public int Index { get; set; }

    [Field]
    public int Value { get; set; }
  }

  [HierarchyRoot]
  public class Alert : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Data { get; set; }

    public Alert(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0675_EntityChangeRegistryDoesntClear : AutoBuildTest
  {
    private const int MaxEntities = 50;

    [Test]
    public void TestTransactionRollbackOnDeadLock()
    {
      var task1 = System.Threading.Tasks.Task.Factory.StartNew(() => UpdateEntities(1));
      var task2 = System.Threading.Tasks.Task.Factory.StartNew(() => UpdateEntities(2));
      System.Threading.Tasks.Task.WaitAll(task1, task2);
    }

    [Test]
    public void TestTransactionIsUnsuableAfterDeadlock()
    {
      var task1 = System.Threading.Tasks.Task.Factory.StartNew(() => UpdateEntities1(1));
      var task2 = System.Threading.Tasks.Task.Factory.StartNew(() => UpdateEntities1(2));
      System.Threading.Tasks.Task.WaitAll(task1, task2);
    }

    [Test]
    public void ChangeSavedEntityInsideTransactionTest01()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex a;
        TestEntityWithUniqueIndex b;
        using (var transaction = session.OpenTransaction()) {
          _ = new TestEntityWithUniqueIndex {
            Index = 1,
            Value = 1,
          };
          _ = new TestEntityWithUniqueIndex {
            Index = 2,
            Value = 2
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 1);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 2);

        try {
          using (var trasaction = session.OpenTransaction()) {
            a.Value = 2; //unique constraint violation
            trasaction.Complete();
          }
        }
        catch(Exception) {}

        Assert.That(a.Index, Is.EqualTo(1));
        Assert.That(a.Value, Is.EqualTo(1));
        Assert.That(b.Index, Is.EqualTo(2));
        Assert.That(b.Value, Is.EqualTo(2));
        Assert.That(session.EntityChangeRegistry.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void ChangeEntityOutsideTransaction()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex a;
        TestEntityWithUniqueIndex b;
        using (var transaction = session.OpenTransaction()) {
          _ = new TestEntityWithUniqueIndex {
            Index = 3,
            Value = 3,
          };
          _ = new TestEntityWithUniqueIndex {
            Index = 4,
            Value = 4
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 3);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 4);
        _ = Assert.Throws<InvalidOperationException>(() => a.Value = 5);
      }
    }

    [Test]
    public void DeleteOutsideTransactionTest()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex a;
        TestEntityWithUniqueIndex b;
        using (var transaction = session.OpenTransaction()) {
          _ = new TestEntityWithUniqueIndex {
            Index = 5,
            Value = 5,
          };
          _ = new TestEntityWithUniqueIndex {
            Index = 6,
            Value = 6
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 5);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 6);

        _ = Assert.Throws<InvalidOperationException>(()=>a.Remove());
      }
    }

    [Test]
    public void DeleteInsideTransaction()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex a;
        TestEntityWithUniqueIndex b;
        TestEntityWithUniqueIndex c;
        using (var transaction = session.OpenTransaction()) {
          _ = new TestEntityWithUniqueIndex {
            Index = 7,
            Value = 7,
          };
          _ = new TestEntityWithUniqueIndex {
            Index = 8,
            Value = 8
          };
          _ = new TestEntityWithUniqueIndex {
            Index = 9,
            Value = 9
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 7);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 8);
        c = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index == 9);

        try {
          using (var transaction = session.OpenTransaction()) {
            a.Value = 8;
            c.Remove();

            transaction.Complete();
          }
        }
        catch(Exception){}

        Assert.That(a.Index, Is.EqualTo(7));
        Assert.That(a.Value, Is.EqualTo(7));
        Assert.That(b.Index, Is.EqualTo(8));
        Assert.That(b.Value, Is.EqualTo(8));
        Assert.That(c.IsRemoved, Is.False);
        Assert.That(c.Index, Is.EqualTo(9));
        Assert.That(c.Value, Is.EqualTo(9));
        Assert.That(session.EntityChangeRegistry.Count, Is.EqualTo(0));
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (var i = 0; i < MaxEntities; i++) {
          _ = new TestEntity() { Index = i, Value = 1 };
        }
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Assert.That(session.Query.All<TestEntity>().Count(), Is.EqualTo(MaxEntities));
      }
    }

    private void UpdateEntities(int instanceId)
    {
      for (var i = 0; i < MaxEntities; i++) {
        using (var session = Domain.OpenSession()) {
          var retryCount = 3;
          for (var retry = 0; ; retry++) {
            var initialValue = 0;
            try {
              initialValue = GetEntityValue(session, i);
              UpdateEntity(session, i);
              break;
            }
            catch (Exception ex) {
              if (ex is DeadlockException ||
                  ex is TransactionSerializationFailureException ||
                  (ex is TargetInvocationException && (ex.InnerException is DeadlockException || ex.InnerException is TransactionSerializationFailureException)))
              {
                if (retry + 1 < retryCount) {
                  Console.WriteLine("Deadlock detected : retrying transactional method for UpdateEntities({0}, {1})", instanceId, i);
                  var currentValue = GetEntityValue(session, i);
                  if (currentValue != initialValue) {
                    Console.WriteLine("Deadlock detected : retrying transactional method for UpdateEntities({0}, {1})", instanceId, i);
                  }
                  continue;
                }
                else {
                  Console.WriteLine("Deadlock detected on last try : giving up on UpdateEntities2({0})", i);
                  throw;
                }
              }
              else {
                throw;
              }
            }
          }
        }
      }
    }

    private void UpdateEntities1(int instanceId)
    {
      for (var i = 0; i < MaxEntities; i++) {
        using (var session = Domain.OpenSession())
        using (session.Activate()) {
          var retryCount = 3;
          for (var retry = 0; ; retry++) {
            var initialValue = 0;

            try {
              initialValue = GetEntityValue1(session, i);
              UpdateEntity1(session, i);
              break;
            }
            catch (Exception ex) {
              if (ex is DeadlockException ||
                  ex is TransactionSerializationFailureException ||
                  (ex is TargetInvocationException && (ex.InnerException is DeadlockException || ex.InnerException is TransactionSerializationFailureException))) {
                if (retry + 1 < retryCount) {
                  Console.WriteLine("Deadlock detected : retrying transactional method for UpdateEntities({0}, {1})", instanceId, i);
                  var currentValue = GetEntityValue(session, i);
                  if (currentValue != initialValue) {
                    Console.WriteLine("Deadlock detected : retrying transactional method for UpdateEntities({0}, {1})", instanceId, i);
                  }
                  continue;
                }
                else {
                  Console.WriteLine("Deadlock detected on last try : giving up on UpdateEntities2({0})", i);
                  throw;
                }
              }
              else {
                throw;
              }
            }
          }
        }
      }
    }

    private int GetEntityValue(Session session, int i)
    {
      int initialValue;
      using (var t = session.OpenTransaction()) {
        var entity = Query.All<TestEntity>().Single(e => e.Index == i);
        initialValue = entity.Value;
        t.Complete();
      }
      return initialValue;
    }

    private void UpdateEntity(Session session, int i)
    {
      using (var t = session.OpenTransaction()) {
        var entity = Query.All<TestEntity>().Single(e => e.Index == i);
        var initialValue = entity.Value;
        entity.Value++;
        t.Complete(); // rollback
      }
    }

    private static int GetEntityValue1(Session session, int i)
    {
      var initialValue = 0;
      var deadlockDetected = false;
      var transactionScope = session.OpenTransaction();
      try {
        var entity = Query.All<TestEntity>().Single(e => e.Index == i);
        initialValue = entity.Value;
        session.SaveChanges();
      }
      catch (Exception ex) {
        if (ex is DeadlockException ||
            ex is TransactionSerializationFailureException ||
            (ex is TargetInvocationException && (ex.InnerException is DeadlockException || ex.InnerException is TransactionSerializationFailureException))) {
          Console.WriteLine("Deadlock detected : retrying transactional method for UpdateEntities({0}, {1})");
          deadlockDetected = true;
          _ = new Alert(session);
        }
        else {
          throw;
        }
      }

      transactionScope.Complete();
      if (deadlockDetected) {
        _ = Assert.Throws<InvalidOperationException>(() => transactionScope.Dispose());
      }
      else {
        Assert.DoesNotThrow(() => transactionScope.Dispose());
      }
      return initialValue;
    }

    private void UpdateEntity1(Session session, int i)
    {
      var deadlockDetected = false;

      var transactionScope = session.OpenTransaction();
      try {
        var entity = Query.All<TestEntity>().Single(e => e.Index == i);
        var initialValue = entity.Value;
        entity.Value++;
        session.SaveChanges();
      }
      catch (Exception ex) {
        if (ex is DeadlockException ||
            ex is TransactionSerializationFailureException ||
            (ex is TargetInvocationException && (ex.InnerException is DeadlockException || ex.InnerException is TransactionSerializationFailureException))) {
          Console.WriteLine("Deadlock detected : retrying transactional method for UpdateEntities({0}, {1})");
          deadlockDetected = true;
          _ = new Alert(session);
        }
        else {
          throw;
        }
      }

      transactionScope.Complete();
      if (deadlockDetected) {
        _ = Assert.Throws<InvalidOperationException>(() => transactionScope.Dispose());
      }
      else {
        Assert.DoesNotThrow(() => transactionScope.Dispose());
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);

      var defaultSessionConfig = configuration.Sessions.Default;
      if (defaultSessionConfig==null) {
        defaultSessionConfig = new SessionConfiguration(WellKnown.Sessions.Default);
        configuration.Sessions.Add(defaultSessionConfig);
      }

      defaultSessionConfig.Options = SessionOptions.ValidateEntities | SessionOptions.AutoActivation | SessionOptions.AutoSaveChanges | SessionOptions.NonTransactionalReads;
      return configuration;
    }
  }
}
