// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using System.Linq;
using Xtensive.Core;
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
        foreach (var schema in schemas.Except(existingSchemas)) {
          var query = SqlDdl.Create(catalog.CreateSchema(schema));
          using (var command = connection.CreateCommand(query)) {
            command.ExecuteNonQuery();
          }
        }
      }
    }
  }
}