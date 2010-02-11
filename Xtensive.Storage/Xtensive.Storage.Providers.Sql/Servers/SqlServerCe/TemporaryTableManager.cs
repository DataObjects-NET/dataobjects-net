// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Storage.Providers.Sql.Servers.SqlServerCe
{
  /// <summary>
  /// A temporary table manager specific to SQL Server CE
  /// </summary>
  public class TemporaryTableManager : Sql.TemporaryTableManager
  {
    /// <inheritdoc/>
    protected override Table CreateTemporaryTable(Schema schema, string tableName)
    {
      return schema.CreateTable(tableName);
    }

    /// <inheritdoc/>
    protected override void Release(TemporaryTableDescriptor descriptor)
    {
      Handlers.SessionHandler.GetService<IQueryExecutor>(true).ExecuteNonQuery(descriptor.DropStatement);
    }
  }
}