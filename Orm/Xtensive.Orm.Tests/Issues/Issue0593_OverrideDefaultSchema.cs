// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.02

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Orm.Configuration;
using Xtensive.Sql.Tests;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0593_OverrideDefaultSchema : AutoBuildTest
  {
    private const string SchemaName = "MyFancySchema";

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      return DomainConfiguration.Load("AppConfigTest", "DomainWithCustomSchema");
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      EnsureSchemaExists(configuration.ConnectionInfo);
      return base.BuildDomain(configuration);
    }

    [Test]
    public void CheckSchemaTest()
    {
      var configuration = Domain.Configuration;
      Assert.AreEqual(SchemaName, configuration.DefaultSchema);
      Assert.AreEqual(SchemaName, configuration.Clone().DefaultSchema);
      Assert.AreEqual(StorageTestHelper.GetDefaultSchema(Domain).Name, SchemaName);
    }

    private static void EnsureSchemaExists(ConnectionInfo connectionInfo)
    {
      var driver = TestSqlDriver.Create(connectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        string checkSchemaQuery = string.Format("select schema_id('{0}')", SchemaName);
        using (var command = connection.CreateCommand(checkSchemaQuery))
          if (command.ExecuteScalar()!=DBNull.Value)
            return;
        var createSchemaQuery = string.Format("create schema {0}", SchemaName);
        using (var command = connection.CreateCommand(createSchemaQuery))
          command.ExecuteNonQuery();
      }
    }
  }
}