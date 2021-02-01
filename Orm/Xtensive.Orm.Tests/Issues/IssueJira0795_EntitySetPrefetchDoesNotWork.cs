// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.02.19

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0795_EntitySetPrefetchDoesNotWorkModel;
using Xtensive.Orm.Validation;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Caching;
using System.Transactions;

namespace Xtensive.Orm.Tests.Issues.IssueJira0795_EntitySetPrefetchDoesNotWorkModel
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }

    [Field(LazyLoad = true)]
    [Association(PairTo = nameof(IssueJira0795_EntitySetPrefetchDoesNotWorkModel.Sports.Person),
      OnOwnerRemove = OnRemoveAction.Cascade,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Sports> Sports { get; private set; }

    public Person(Session session)
        : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class Sports : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Status Status { get; set; }

    [Field]
    public double Valor { get; set; }

    [Field]
    public Person Person { get; private set; }

    public Sports(Session session)
      : base(session)
    {
    }
  }

  public enum Status
  {
    Active = 0,
    Inactive = 1
  }

  [HierarchyRoot]
  public class SomeEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
    [Field]
    public EntitySetContainer Ref { get; set; }
  }

  [HierarchyRoot]
  public class EntitySetContainer : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
    [Field]
    public EntitySet<EntitySetItem> Items { get; private set; }
  }

  [HierarchyRoot]
  public class EntitySetItem : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0795_EntitySetPrefetchDoesNotWork : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [TearDown]
    public void TearDown()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var sports in session.Query.All<Sports>()) {
          sports.Remove();
        }
        foreach (var person in session.Query.All<Person>()) {
          person.Remove();
        }
        transaction.Complete();
      }
    }

    [Test]
    public void ClientProfileTest01()
    {
      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var outerSessionPersonId = 0;

      using (var outerSession = Domain.OpenSession(configuration)) {
        var person = new Person(outerSession) { Name = "Jordi" };
        var sport1 = new Sports(outerSession) { Valor = 10 };
        var sport2 = new Sports(outerSession) { Valor = 20 };

        _ = person.Sports.Add(sport2);
        _ = person.Sports.Add(sport1);

        outerSession.SaveChanges();
        outerSessionPersonId = person.Id;

        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        var actualCountInStorage = outerSession.Query.All<Sports>().Count();

        Assert.That(actualCountInStorage, Is.EqualTo(2));
        Assert.That(person, Is.Not.Null);
        Assert.That(person.Id, Is.EqualTo(outerSessionPersonId));

        var countOuter = person.Sports.AsEnumerable().Count();
        Assert.That(countOuter, Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration)) {
          var person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          var countInner = person1.Sports.AsEnumerable().Count();
          Assert.That(countInner, Is.EqualTo(3));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          actualCountInStorage = innerSession.Query.All<Sports>().Count();

          Assert.That(actualCountInStorage, Is.EqualTo(3));

          countInner = person1.Sports.AsEnumerable().Count();
          Assert.That(countInner, Is.EqualTo(actualCountInStorage));
        }

        // autoprefetch
        countOuter = person.Sports.AsEnumerable().Count();
        Assert.That(countOuter, Is.EqualTo(2));

        // force manual prefetch
        person.Sports.Prefetch();
        actualCountInStorage = outerSession.Query.All<Sports>().Count();

        Assert.That(actualCountInStorage, Is.EqualTo(3));

        countOuter = person.Sports.AsEnumerable().Count();
        Assert.That(countOuter, Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileTest02()
    {
      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var outerSessionPersonId = 0;

      using (var outerSession = Domain.OpenSession(configuration)) {
        var person = new Person(outerSession) { Name = "Jordi" };

        var sport1 = new Sports(outerSession) { Valor = 10 };
        var sport2 = new Sports(outerSession) { Valor = 20 };

        _ = person.Sports.Add(sport2);
        _ = person.Sports.Add(sport1);

        outerSession.SaveChanges();
        outerSessionPersonId = person.Id;

        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        var actualCountInStorage = outerSession.Query.All<Sports>().Count();

        Assert.That(actualCountInStorage, Is.EqualTo(2));
        Assert.That(person, Is.Not.Null);
        Assert.That(person.Id, Is.EqualTo(outerSessionPersonId));

        var countOuter = person.Sports.AsEnumerable().Count();
        Assert.That(countOuter, Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration)) {
          var person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          var countInner = person1.Sports.AsEnumerable().Count();
          Assert.That(countInner, Is.EqualTo(3));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          actualCountInStorage = outerSession.Query.All<Sports>().Count();

          Assert.That(actualCountInStorage, Is.EqualTo(3));

          countInner = person1.Sports.AsEnumerable().Count();
          Assert.That(countInner, Is.EqualTo(actualCountInStorage));
        }

        // autoprefetch
        countOuter = person.Sports.AsEnumerable().Count();
        Assert.That(countOuter, Is.EqualTo(2));

        //force manual prefetch
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        actualCountInStorage = outerSession.Query.All<Sports>().Count();

        Assert.That(actualCountInStorage, Is.EqualTo(3));

        countOuter = person.Sports.AsEnumerable().Count();
        Assert.That(countOuter, Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadUncommitedTest1()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));
        person.Sports.Prefetch();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadUncommitedTest2()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          //rollback
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(0));
        person.Sports.Prefetch();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadUncommitedTest3()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadUncommitedTest4()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          //rollback
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(0));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadCommitedTest1()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));
        person.Sports.Prefetch();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadCommitedTest2()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          //rollback
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(0));
        person.Sports.Prefetch();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadCommitedTest3()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithReadCommitedTest4()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          //rollback
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(0));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithRepeatableReadTest1()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        if (StorageProviderInfo.Instance.Provider == StorageProvider.SqlServer) {
          Assert.That(actualCountInStorage, Is.EqualTo(1));
        }
        else {
          Assert.That(actualCountInStorage, Is.EqualTo(0));
        }

        person.Sports.Prefetch();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithRepeatableReadTest2()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          //rollback
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(0));
        person.Sports.Prefetch();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithRepeatableReadTest3()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        if (StorageProviderInfo.Instance.Provider == StorageProvider.SqlServer) {
          Assert.That(actualCountInStorage, Is.EqualTo(1));
        }
        else {
          Assert.That(actualCountInStorage, Is.EqualTo(0));
        }

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ClientProfileWithRepeatableReadTest4()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var configuration = new SessionConfiguration(SessionOptions.ClientProfile);

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession(configuration))
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession(configuration))
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession);
          sport3.Valor = 30;
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          //rollback
        }

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(0));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ReadUncommitedTest1()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        // autoprefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));
        person.Sports.Prefetch();// force manual prefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ReadUncommitedTest2()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        // autoprefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();// force manual prefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ReadUncommitedTest3()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadUncommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        using (var innerTx = outerSession.OpenTransaction(TransactionOpenMode.New, IsolationLevel.RepeatableRead)) {
          Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));// autoprefetch

          actualCountInStorage = outerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();// force manual prefetch
          Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
        }
      }
    }

    [Test]
    public void ReadCommittedTest1()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        // autoprefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));
        person.Sports.Prefetch();// force manual prefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ReadCommittedTest2()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        // autoprefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        Assert.That(actualCountInStorage, Is.EqualTo(1));

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();// force manual prefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void ReadCommittedTest3()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.ReadCommitted)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        using (var innerTx = outerSession.OpenTransaction(TransactionOpenMode.New, IsolationLevel.RepeatableRead)) {
          Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));// autoprefetch

          actualCountInStorage = outerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();// force manual prefetch
          Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
        }
      }
    }

    [Test]
    public void RepeatableReadTest1()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite | StorageProvider.MySql);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>()
          .Prefetch(p => p.Sports)
          .FirstOrDefault();

        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        // autoprefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        if (StorageProviderInfo.Instance.Provider == StorageProvider.SqlServer) {
          Assert.That(actualCountInStorage, Is.EqualTo(1));
        }
        else {
          Assert.That(actualCountInStorage, Is.EqualTo(0));
        }
        person.Sports.Prefetch();// force manual prefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void RepeatableReadTest2()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite | StorageProvider.MySql);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        // autoprefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));

        actualCountInStorage = outerSession.Query.All<Sports>().Count();
        if (StorageProviderInfo.Instance.Provider == StorageProvider.SqlServer) {
          Assert.That(actualCountInStorage, Is.EqualTo(1));
        }
        else {
          Assert.That(actualCountInStorage, Is.EqualTo(0));
        }

        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));
        person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();// force manual prefetch
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
      }
    }

    [Test]
    public void RepeatableReadTest3()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Sqlite | StorageProvider.MySql);

      using (var outerSession = Domain.OpenSession())
      using (var tx = outerSession.OpenTransaction()) {
        var person = new Person(outerSession) { Name = "Jordi" };
        tx.Complete();
      }

      var actualCountInStorage = 0;
      using (var outerSession = Domain.OpenSession())
      using (var outerSessionTx = outerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
        Assert.That(outerSession.Query.All<Sports>().Count(), Is.EqualTo(actualCountInStorage));

        var person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
        Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

        using (var innerSession = Domain.OpenSession())
        using (var innerSessionTx = innerSession.OpenTransaction(IsolationLevel.RepeatableRead)) {
          var person1 = innerSession.Query.All<Person>()
            .Prefetch(p => p.Sports)
            .FirstOrDefault();
          var sport3 = new Sports(innerSession) { Valor = 30 };
          _ = person1.Sports.Add(sport3);
          innerSession.SaveChanges();

          actualCountInStorage = innerSession.Query.All<Sports>().Count();
          Assert.That(actualCountInStorage, Is.EqualTo(1));
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));

          person1 = innerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();
          Assert.That(person1.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
          innerSessionTx.Complete();
        }

        using (var innerTx = outerSession.OpenTransaction(TransactionOpenMode.New, IsolationLevel.RepeatableRead)) {
          Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(0));// autoprefetch

          actualCountInStorage = outerSession.Query.All<Sports>().Count();
          if (StorageProviderInfo.Instance.Provider == StorageProvider.SqlServer) {
            Assert.That(actualCountInStorage, Is.EqualTo(1));
          }
          else {
            Assert.That(actualCountInStorage, Is.EqualTo(0));
          }

          person = outerSession.Query.All<Person>().Prefetch(p => p.Sports).FirstOrDefault();// force manual prefetch
          Assert.That(person.Sports.AsEnumerable().Count(), Is.EqualTo(actualCountInStorage));
        }
      }
    }
  }
}