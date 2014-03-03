// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.03

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.Multimapping;
using Xtensive.Orm.Tests.Storage.SessionInitializationSqlTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace SessionInitializationSqlTestModel
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }
  }

  [TestFixture]
  public class SessionInitializationSqlTest
  {
    [Test]
    public void MainTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "Uses multiple databases and native SQL");

      long id;

      var domain1 = BuildDomain(MultidatabaseTest.Database1Name);
      using (var session = domain1.OpenSession())
      using (var tx = session.OpenTransaction()) {
        id = new TestEntity().Id;
        tx.Complete();
      }

      var domain2 = BuildDomain(MultidatabaseTest.Database2Name);
      using (var session = domain2.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Services.Demand<DirectSqlAccessor>().RegisterInitializationSql(GetUseDatabaseScript(MultidatabaseTest.Database1Name));
        var entity = session.Query.Single<TestEntity>(id);
        Assert.That(entity.Id, Is.EqualTo(id));
      }
    }

    private Domain BuildDomain(string database)
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.Types.Register(typeof (TestEntity));
      domainConfiguration.ConnectionInitializationSql = GetUseDatabaseScript(database);
      return Domain.Build(domainConfiguration);
    }

    private static string GetUseDatabaseScript(string database)
    {
      return string.Format("use [{0}]", database);
    }
  }
}