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
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = "MyFancySchema";
      return configuration;
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
      Assert.That(configuration.DefaultSchema, Is.EqualTo(SchemaName));
      Assert.That(configuration.Clone().DefaultSchema, Is.EqualTo(SchemaName));
      Assert.That(SchemaName, Is.EqualTo(StorageTestHelper.GetDefaultSchema(Domain).Name));
    }

    private static void EnsureSchemaExists(ConnectionInfo connectionInfo)
    {
      var driver = TestSqlDriver.Create(connectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        string checkSchemaQuery = $"select schema_id('{SchemaName}')";
        using (var command = connection.CreateCommand(checkSchemaQuery))
          if (command.ExecuteScalar()!=DBNull.Value)
            return;
        var createSchemaQuery = $"create schema {SchemaName}";
        using (var command = connection.CreateCommand(createSchemaQuery))
          command.ExecuteNonQuery();
      }
    }
  }
}