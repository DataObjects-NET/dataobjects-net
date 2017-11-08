// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests
{
  public static class StorageTestHelper
  {
    public static bool IsFetched(Session session, Key key)
    {
      EntityState dummy;
      return session.EntityStateCache.TryGetItem(key, false, out dummy);
    }

    public static object GetNativeTransaction()
    {
      var handler = Session.Demand().Handler;
      var sqlHandler = handler as SqlSessionHandler;
      if (sqlHandler!=null)
        return sqlHandler.Connection.ActiveTransaction;
      throw new NotSupportedException();
    }

    public static ModelMapping GetDefaultModelMapping(Domain domain)
    {
      return domain.Handlers.StorageNodeRegistry.Get(WellKnown.DefaultNodeId).Mapping;
    }

    public static Schema GetDefaultSchema(Domain domain)
    {
      var mapping = GetDefaultModelMapping(domain);
      return mapping[domain.Model.Types[typeof (Metadata.Assembly)]].Schema;
    }

    public static void DemandSchemas(ConnectionInfo connectionInfo, params string[] schemas)
    {
      var driver = TestSqlDriver.Create(connectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();

        var extractionTask = new SqlExtractionTask(driver.CoreServerInfo.DatabaseName);
        var extractionResult = driver.Extract(connection, new[] {extractionTask});
        var catalog = extractionResult.Catalogs.Single();
        var existingSchemas = catalog.Schemas.Select(s => s.Name);
        var schemasToCreate = schemas.Except(existingSchemas, StringComparer.OrdinalIgnoreCase);

        // Oracle does not support creating schemas, user should be created instead.
        if (connectionInfo.Provider==WellKnown.Provider.Oracle)
          CreateUsers(connection, schemasToCreate);
        else
          CreateSchemas(connection, catalog, schemasToCreate);

        connection.Close();
      }
    }

    private static void CreateUsers(SqlConnection connection, IEnumerable<string> schemasToCreate)
    {
      var translator = connection.Driver.Translator;
      foreach (var schema in schemasToCreate) {
        var userName = translator.QuoteIdentifier(schema);
        var password = schema;
        ExecuteQuery(connection, string.Format("create user {0} identified by {1}", userName, password));
        ExecuteQuery(connection, string.Format("alter user {0} quota unlimited on system", userName));
      }
    }

    private static void CreateSchemas(SqlConnection connection, Catalog catalog, IEnumerable<string> schemasToCreate)
    {
      foreach (var schema in schemasToCreate) {
        ExecuteQuery(connection, SqlDdl.Create(catalog.CreateSchema(schema)));
      }
    }

    private static void ExecuteQuery(SqlConnection connection, ISqlCompileUnit query)
    {
      using (var command = connection.CreateCommand(query))
        command.ExecuteNonQuery();
    }

    private static void ExecuteQuery(SqlConnection connection, string query)
    {
      using (var command = connection.CreateCommand(query))
        command.ExecuteNonQuery();
    }
  }
}