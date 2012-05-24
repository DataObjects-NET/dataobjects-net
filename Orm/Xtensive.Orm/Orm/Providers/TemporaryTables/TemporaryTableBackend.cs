// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.24

using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  public abstract class TemporaryTableBackEnd
  {
    /// <summary>
    /// Creates the temporary table with the specified name.
    /// </summary>
    /// <param name="schema">The schema to create table in.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <returns>Created table.</returns>
    public abstract Table CreateTemporaryTable(Schema schema, string tableName);

    /// <summary>
    /// Initializes the table. This is called once per session on a first acquire request.
    /// </summary>
    public abstract void InitializeTable(EnumerationContext context, TemporaryTableDescriptor descriptor);

    /// <summary>
    /// Gets the lock on a temporary table.
    /// </summary>
    public abstract void AcquireTable(EnumerationContext context, TemporaryTableDescriptor descriptor);

    /// <summary>
    /// Releases the lock on a temporary table.
    /// </summary>
    public abstract void ReleaseTable(EnumerationContext context, TemporaryTableDescriptor descriptor);

    protected void ExecuteNonQuery(EnumerationContext context, string statement)
    {
      var executor = context.Session.Services.Demand<ISqlExecutor>();
      executor.ExecuteNonQuery(statement);
    }
  }
}