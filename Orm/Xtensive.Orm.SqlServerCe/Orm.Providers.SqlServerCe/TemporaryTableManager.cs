// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers.SqlServerCe
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
    protected override void InitializeTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
    }
  
    /// <inheritdoc/>
    protected override void AcquireTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      ExecuteNonQuery(context, descriptor.CreateStatement);
    }

    /// <inheritdoc/>
    protected override void ReleaseTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      ExecuteNonQuery(context, descriptor.DropStatement);
    }
  }
}