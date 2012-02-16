// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// <see cref="MetadataAccessor"/> for SQL storages.
  /// </summary>
  public class MetadataAccessor : Providers.MetadataAccessor
  {
    /// <inheritdoc/>
    public override IEnumerable<AssemblyMetadata> GetAssemblies(string database, string schema)
    {
      return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<TypeMetadata> GetTypes(string database, string schema)
    {
      return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<ExtensionMetadata> GetExtension(string database, string schema, string name)
    {
      return null;
    }

    private IQueryExecutor GetQueryExecutor()
    {
      return BuildingContext.Demand().SystemSessionHandler.GetService<IQueryExecutor>(true);
    }
  }
}