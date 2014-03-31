// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.03.03

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.EntitySetRollbackTestModel;

namespace Xtensive.Orm.Tests.Storage.EntitySetRollbackTestModel
{
  [HierarchyRoot]
  public class Employee : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, Association(PairTo = "Employees")]
    public EntitySet<SomeAccountingDocument> Documents { get; set; }

    [Field]
    public EntitySet<AccountingDocument> AccountingDocuments { get; set; } 

  }

  [HierarchyRoot]
  public class SomeAccountingDocument : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Employee> Employees { get; set; }
  }

  [HierarchyRoot]
  public class AccountingDocument : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Employee> Employees { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class EntitySetRollbackTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration =  base.BuildConfiguration();
      configuration.Types.Register(typeof (AccountingDocument).Assembly, typeof (AccountingDocument).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void PairedEntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        Key documentKey;
        using (var transaction = session.OpenTransaction()) {
          documentKey = new SomeAccountingDocument().Key;
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<SomeAccountingDocument>(documentKey);
          document.Employees.Add(new Employee());
          document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<SomeAccountingDocument>(documentKey);
          document.Employees.Add(new Employee());
          document.Employees.Add(new Employee());
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<SomeAccountingDocument>(documentKey);
          document.Employees.Add(new Employee());
          document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<SomeAccountingDocument>(documentKey);
          for (var i = 0; i < 30; i++)
            document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<SomeAccountingDocument>(documentKey);
          var employees = document.Employees.ToList();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void NonPairedEntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        Key documentKey;
        using (var transaction = session.OpenTransaction()) {
          documentKey = new AccountingDocument().Key;
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          document.Employees.Add(new Employee());
          document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          document.Employees.Add(new Employee());
          document.Employees.Add(new Employee());
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          document.Employees.Add(new Employee());
          document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          for (var i = 0; i < 30; i++)
            document.Employees.Add(new Employee());
          transaction.Complete();
        }
        
        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          var employees = document.Employees.ToList();
          transaction.Complete();
        }
      }
    }

    [Test]
    public void RemoveFromEntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        Key documentKey;
        using (var transaction = session.OpenTransaction()) {
          var document = new AccountingDocument();
          documentKey = document.Key;
          for (var i = 0; i < 40; i++)
            document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          var documentEntityes = document.Employees.ToList();
          foreach (var documentEntity in documentEntityes) {
            document.Employees.Remove(documentEntity);
          }
          transaction.Complete();
        }
      }
    }

    [Test]
    public void RemoveFromPairedEntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        Key documentKey;
        using (var transaction = session.OpenTransaction()) {
          var document = new SomeAccountingDocument();
          documentKey = document.Key;
          for (var i = 0; i < 40; i++)
            document.Employees.Add(new Employee());
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<SomeAccountingDocument>(documentKey);
          var documentEntityes = document.Employees.ToList();
          foreach (var documentEntity in documentEntityes)
            document.Employees.Remove(documentEntity);
          transaction.Complete();
        }
      }
    }

    [Test]
    public void EntitySetStateCountsTest()
    {
      int addedItemsCount = 0;
      int removedItemsCount = 0;
      using (var session = Domain.OpenSession()) {
        Key documentKey;
        Key employeeToRemoveKey;
        using (var transaction = session.OpenTransaction()) {
          var document = new AccountingDocument();
          documentKey = document.Key;
          for (var i = 0; i < 40; i++) {
            document.Employees.Add(new Employee());
            addedItemsCount++;
          }
          var employee = new Employee();
          employeeToRemoveKey = employee.Key;
          document.Employees.Add(employee);
          addedItemsCount++;
          var entitySetState = session.EntitySetChangeRegistry.GetItems().First();
          Assert.AreEqual(addedItemsCount, entitySetState.CachedItemCount);
          Assert.AreEqual(0, entitySetState.FetchedItemsCount);
          Assert.AreEqual(addedItemsCount, entitySetState.AddedItemsCount);
          Assert.AreEqual(removedItemsCount, entitySetState.RemovedItemsCount);
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          var employee = session.Query.Single<Employee>(employeeToRemoveKey);
          document.Employees.Remove(employee);
          removedItemsCount++;
          var entitySetState = session.EntitySetChangeRegistry.GetItems().First();
          Assert.AreEqual(WellKnown.EntitySetPreloadCount + 1 - removedItemsCount, entitySetState.CachedItemCount);
          Assert.AreEqual(WellKnown.EntitySetPreloadCount + 1, entitySetState.FetchedItemsCount);
          Assert.AreEqual(0, entitySetState.AddedItemsCount);
          Assert.AreEqual(removedItemsCount, entitySetState.RemovedItemsCount);

          document.Employees.Prefetch(35);
          Assert.AreEqual(35, entitySetState.CachedItemCount);
          Assert.AreEqual(35, entitySetState.FetchedItemsCount);
          Assert.AreEqual(0, entitySetState.AddedItemsCount);
          Assert.AreEqual(0, entitySetState.RemovedItemsCount);
          transaction.Complete();
        }
        
        using (var transaction = session.OpenTransaction()) {
          var document = session.Query.Single<AccountingDocument>(documentKey);
          var employees = document.Employees.ToList();
          Assert.AreEqual(addedItemsCount - removedItemsCount, employees.Count);
        }
      }
    }

    [Test]
    public void NonPairedEntitySetInDisconnectedSessionTest()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(AccountingDocument).Assembly, typeof(AccountingDocument).Namespace);
      var defaultConfiguration = configuration.Sessions.Default;
      defaultConfiguration.Options = SessionOptions.Disconnected | SessionOptions.AutoActivation;
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var domain = BuildDomain(configuration);

      using (var session = domain.OpenSession()) {
        var document = new AccountingDocument();
        session.SaveChanges();

        document.Employees.Add(new Employee());
        document.Employees.Add(new Employee());
        session.SaveChanges();

        document.Employees.Add(new Employee());
        document.Employees.Add(new Employee());
        session.CancelChanges();

        document.Employees.Add(new Employee());
        document.Employees.Add(new Employee());
        session.SaveChanges();

        for (var i = 0; i < 40; i++)
          document.Employees.Add(new Employee());
        session.SaveChanges();

        var employees = document.Employees.ToList();
        session.SaveChanges();
      }
    }

    [Test]
    public void PairedEnitySetInDisconnectedSessionTest()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(AccountingDocument).Assembly, typeof(AccountingDocument).Namespace);
      var defaultConfiguration = configuration.Sessions.Default;
      defaultConfiguration.Options = SessionOptions.Disconnected | SessionOptions.AutoActivation;
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var domain = BuildDomain(configuration);

      using (var session = domain.OpenSession()) {
        var document = new SomeAccountingDocument();
        session.SaveChanges();

        document.Employees.Add(new Employee());
        document.Employees.Add(new Employee());
        session.SaveChanges();

        document.Employees.Add(new Employee());
        document.Employees.Add(new Employee());
        session.CancelChanges();

        document.Employees.Add(new Employee());
        document.Employees.Add(new Employee());
        session.SaveChanges();

        for (var i = 0; i < 40; i++)
          document.Employees.Add(new Employee());
        session.SaveChanges();

        var employees = document.Employees.ToList();
        session.SaveChanges();
      }
    }

    [Test]
    public void RemoveFromEntitySetInDisconnectedSessionTest()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (AccountingDocument).Assembly, typeof (AccountingDocument).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var defaultConfiguration = configuration.Sessions.Default;
      defaultConfiguration.Options = SessionOptions.Disconnected | SessionOptions.AutoActivation;

      var domain = BuildDomain(configuration);
      
      using (var session = domain.OpenSession()) {
        var document = new AccountingDocument();
        for (var i = 0; i < 40; i++)
          document.Employees.Add(new Employee());
        session.SaveChanges();
        
        var documentEntityes = document.Employees.ToList();
        foreach (var documentEntity in documentEntityes)
          document.Employees.Remove(documentEntity);
        session.SaveChanges();
      }
    }

    [Test]
    public void RemoveFromPairedEntitySetInDisconnectedSessionTest()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(AccountingDocument).Assembly, typeof(AccountingDocument).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var defaultConfiguration = configuration.Sessions.Default;
      defaultConfiguration.Options = SessionOptions.Disconnected | SessionOptions.AutoActivation;

      var domain = BuildDomain(configuration);

      using (var session = domain.OpenSession()) {
        var document = new AccountingDocument();
        for (var i = 0; i < 40; i++)
          document.Employees.Add(new Employee());
        session.SaveChanges();

        var documentEntityes = document.Employees.ToList();
        foreach (var documentEntity in documentEntityes)
          document.Employees.Remove(documentEntity);
        session.SaveChanges();
      }
    }
  }
}
