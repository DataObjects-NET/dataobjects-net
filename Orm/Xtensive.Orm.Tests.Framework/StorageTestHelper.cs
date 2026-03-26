// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests
{
  public static class StorageTestHelper
  {
    public static bool IsFetched(Session session, Key key)
    {
      return session.EntityStateCache.TryGetItem(key, false, out var _);
    }

    public static object GetNativeTransaction(Session session)
    {
      var handler = session.Handler;
      if (handler is SqlSessionHandler sqlHandler)
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

        // Oracle does not support creating schemas, user should be created instead.
        if (connectionInfo.Provider == WellKnown.Provider.Oracle) {
          var schemasToCreate = schemas.Except(existingSchemas, StringComparer.Ordinal);
          CreateUsers(connection, schemasToCreate);
        }
        else {
          var schemasToCreate = schemas.Except(existingSchemas, StringComparer.OrdinalIgnoreCase);
          CreateSchemas(connection, catalog, schemasToCreate);
        }

        connection.Close();
      }
    }

    /// <summary>
    /// Waits for full-text indexes of MS SQL to be populated.
    /// Every now and then it gets state of them from database or waits timeout to be reached.
    /// </summary>
    /// <param name="domain"></param>
    public static void WaitFullTextIndexesPopulated(Domain domain, TimeSpan timeout)
    {
      if (StorageProviderInfo.Instance.Provider == StorageProvider.SqlServer) {
        var driver = TestSqlDriver.Create(domain.Configuration.ConnectionInfo);
        using (var connection = driver.CreateConnection()) {

          var date = DateTime.UtcNow.Add(timeout);
          while (!CheckFtIndexesPopulated(connection) && DateTime.UtcNow < date) {
            Thread.Sleep(TimeSpan.FromSeconds(2));
          }
        }
      }

      static bool CheckFtIndexesPopulated(SqlConnection connection)
      {
        connection.Open();
        try {
          using (var command = connection.CreateCommand()) {
            command.CommandText = $"SELECT COUNT(*) FROM sys.fulltext_indexes WHERE has_crawl_completed = 0";
            return ((int) command.ExecuteScalar()) == 0;
          }
        }
        finally {
          connection.Close();
        }
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
      using var command = connection.CreateCommand(query);
      _ = command.ExecuteNonQuery();
    }

    private static void ExecuteQuery(SqlConnection connection, string query)
    {
      using var command = connection.CreateCommand(query);
      _ = command.ExecuteNonQuery();
    }
  }
}