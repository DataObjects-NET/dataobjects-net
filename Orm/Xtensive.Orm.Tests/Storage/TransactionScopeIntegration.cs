// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.05.12

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TransactionScopeIntegrationTestModel;
using SystemTransactionScope = System.Transactions.TransactionScope;

namespace Xtensive.Orm.Tests.Storage
{
  namespace TransactionScopeIntegrationTestModel
  {
    [HierarchyRoot]
    public class DataRecord : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public string Value { get; set; }
    }
  }

  [TestFixture]
  public class TransactionScopeIntegrationTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "uses System.Transactions.TransactionScope");
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (DataRecord));
      return configuration;
    }

    [Test]
    public void CommitTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new DataRecord {Name = "CommitTest", Value = "False"};
        tx.Complete();
      }

      //this behavior is not supported by netcore2.0 
      using (var ts = new SystemTransactionScope()) {
        using (var session = Domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var record = session.Query.All<DataRecord>().Single(e => e.Name=="CommitTest");
          record.Value = "True";
          tx.Complete();
        }
        ts.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var record = session.Query.All<DataRecord>().Single(e => e.Name=="CommitTest");
        Assert.That(record.Value, Is.EqualTo("True"));
        tx.Complete();
      }
    }

    [Test]
    public void RollbackTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new DataRecord {Name = "RollbackTest", Value = "False"};
        tx.Complete();
      }

      using (var ts = new SystemTransactionScope()) {
        using (var session = Domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var record = session.Query.All<DataRecord>().Single(e => e.Name=="RollbackTest");
          record.Value = "True";
          tx.Complete();
        }
        // Rollback
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var record = session.Query.All<DataRecord>().Single(e => e.Name=="RollbackTest");
        Assert.That(record.Value, Is.EqualTo("False"));
        tx.Complete();
      }
    }
  }
}