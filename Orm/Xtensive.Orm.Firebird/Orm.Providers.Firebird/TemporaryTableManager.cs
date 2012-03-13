// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using Xtensive.Orm.Providers;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers.Firebird
{
  /// <summary>
  /// A temporary table manager specific to Firebird
  /// </summary>
  public class TemporaryTableManager : Providers.TemporaryTableManager
  {
    /// <inheritdoc/>
    protected override Table CreateTemporaryTable(Schema schema, string tableName)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override void InitializeTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      throw new NotSupportedException();
    }
  
    /// <inheritdoc/>
    protected override void AcquireTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override void ReleaseTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      throw new NotSupportedException();
    }
  }
}