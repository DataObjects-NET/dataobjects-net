// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.14

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.MemoryProviderTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace MemoryProviderTestModel
  {
    [HierarchyRoot]
    public class TheEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }
  }

  [TestFixture]
  public class MemoryProviderTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite, "Only sqlite supports memory data source");
    }

    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.SharedConnection = true;
      configuration.Types.Register(typeof (TheEntity));
      configuration.ConnectionInfo = new ConnectionInfo(WellKnown.Provider.Sqlite, "Data Source=:memory:");
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new TheEntity {Value = "in-memory"};
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var theEntity = session.Query.All<TheEntity>().Single();
        Assert.That(theEntity.Value, Is.EqualTo("in-memory"));
        tx.Complete();
      }
    }
  }
}