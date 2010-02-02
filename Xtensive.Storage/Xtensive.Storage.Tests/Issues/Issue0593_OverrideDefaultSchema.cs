// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.02

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Storage.Configuration;
using SqlDomainHandler = Xtensive.Storage.Providers.Sql.DomainHandler;

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0593_OverrideDefaultSchema : AutoBuildTest
  {
    private const string SchemaName = "MyFancySchema";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "DomainWithCustomSchema");
      EnsureSchemaExists(configuration.ConnectionInfo);
      return configuration;
    }

    [Test]
    public void CheckSchemaTest()
    {
      var handler = (SqlDomainHandler) Domain.Handler;
      Assert.AreEqual(handler.Schema.Name, SchemaName);
    }

    private static void EnsureSchemaExists(UrlInfo url)
    {
      var driver = SqlDriver.Create(url);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        string checkSchemaQuery = string.Format("select object_id('{0}')", SchemaName);
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