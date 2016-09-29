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
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0675_EntityChangeRegistryDoesntClear : AutoBuildTest
  {
    private const int MaxEntities = 50;

    [Test]
    public void TestTransactionRollbackOnDeadLock()
    {
      System.Threading.Tasks.Task task1 = System.Threading.Tasks.Task.Factory.StartNew(() => UpdateEntities(1));
      System.Threading.Tasks.Task task2 = System.Threading.Tasks.Task.Factory.StartNew(() => UpdateEntities(2));
      System.Threading.Tasks.Task.WaitAll(task1, task2);
    }

    [Test]
    public void ChangeSavedEntityInsideTransactionTest01()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex a;
        TestEntityWithUniqueIndex b;
        using (var transaction = session.OpenTransaction()) {
          new TestEntityWithUniqueIndex {
            Index = 1,
            Value = 1,
          };
          new TestEntityWithUniqueIndex {
            Index = 2,
            Value = 2
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el=>el.Index==1);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el=>el.Index==2);

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
          new TestEntityWithUniqueIndex {
            Index = 3,
            Value = 3,
          };
          new TestEntityWithUniqueIndex {
            Index = 4,
            Value = 4
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el=>el.Index==3);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el=>el.Index==4);
        Assert.Throws<InvalidOperationException>(()=>a.Value = 5);
      }
    }

    [Test]
    public void DeleteOutsideTransactionTest()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex a;
        TestEntityWithUniqueIndex b;
        using (var transaction = session.OpenTransaction()) {
          new TestEntityWithUniqueIndex {
            Index = 5,
            Value = 5,
          };
          new TestEntityWithUniqueIndex {
            Index = 6,
            Value = 6
          };
          transaction.Complete();
        }

        a = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index==5);
        b = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index==6);

        Assert.Throws<InvalidOperationException>(()=>a.Remove());
      }
    }

    [Test]
    public void DeleteInsideTransaction()
    {
      using (var session = Domain.OpenSession()) {
        TestEntityWithUniqueIndex aaa;
        TestEntityWithUniqueIndex bbb;
        TestEntityWithUniqueIndex ccc;
        using (var transaction = session.OpenTransaction()) {
          new TestEntityWithUniqueIndex {
            Index = 7,
            Value = 7,
          };
          new TestEntityWithUniqueIndex {
            Index = 8,
            Value = 8
          };
          new TestEntityWithUniqueIndex {
            Index = 9,
            Value = 9
          };
          transaction.Complete();
        }

        aaa = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index==7);
        bbb = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index==8);
        ccc = session.Query.All<TestEntityWithUniqueIndex>().First(el => el.Index==9);

        try {
          using (var transaction = session.OpenTransaction()) {
            aaa.Value = 8;
            ccc.Remove();

            transaction.Complete();
          }
        }
        catch(Exception){}

        Assert.That(aaa.Index, Is.EqualTo(7));
        Assert.That(aaa.Value, Is.EqualTo(7));
        Assert.That(bbb.Index, Is.EqualTo(8));
        Assert.That(bbb.Value, Is.EqualTo(8));
        Assert.That(ccc.IsRemoved, Is.False);
        Assert.That(ccc.Index, Is.EqualTo(9));
        Assert.That(ccc.Value, Is.EqualTo(9));
        Assert.That(session.EntityChangeRegistry.Count, Is.EqualTo(0));
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (int i = 0; i < MaxEntities; i++) {
          new TestEntity() {Index = i, Value = 1};
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
      for (int i = 0; i < MaxEntities; i++) {
        using (Session session = Domain.OpenSession()) {
          int retryCount = 3;
          for (int retry = 0; ; retry++) {

            int initialValue = 0;
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
                  int currentValue = GetEntityValue(session, i);
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
      using (TransactionScope t = session.OpenTransaction())
      {
        TestEntity entity = Query.All<TestEntity>().Single(e => e.Index == i);
        initialValue = entity.Value;
        t.Complete();
      }
      return initialValue;
    }

    private void UpdateEntity(Session session, int i)
    {
      using (TransactionScope t = session.OpenTransaction()) {
        TestEntity entity = Query.All<TestEntity>().Single(e => e.Index == i);
        int initialValue = entity.Value;
        entity.Value++;
        t.Complete(); // rollback
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof (TestEntity).Namespace);

      SessionConfiguration defaultSessionConfig = configuration.Sessions.Default;
      if (defaultSessionConfig==null) {
        defaultSessionConfig = new SessionConfiguration(WellKnown.Sessions.Default);
        configuration.Sessions.Add(defaultSessionConfig);
      }

      defaultSessionConfig.Options = SessionOptions.ValidateEntities | SessionOptions.AutoActivation | SessionOptions.AutoSaveChanges | SessionOptions.NonTransactionalReads;
      return configuration;
    }
  }
}
