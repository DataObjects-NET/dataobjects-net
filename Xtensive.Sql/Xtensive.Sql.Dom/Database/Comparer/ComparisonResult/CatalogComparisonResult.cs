// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Catalog"/> comparison result.
  /// </summary>
  [Serializable]
  public class CatalogComparisonResult : NodeComparisonResult, 
    IComparisonResult<Catalog>
  {
    private SchemaComparisonResult defaultSchema;
    private readonly ComparisonResultCollection<SchemaComparisonResult> schemas = new ComparisonResultCollection<SchemaComparisonResult>();

    /// <inheritdoc/>
    public Catalog NewValue
    {
      get { return (Catalog)base.NewValue; }
    }

    /// <inheritdoc/>
    public Catalog OriginalValue
    {
      get { return (Catalog) base.OriginalValue; }
    }

    /// <summary>
    /// Gets comparison result of default <see cref="Schema"/>.
    /// </summary>
    public SchemaComparisonResult DefaultSchema
    {
      get { return defaultSchema; }
      internal set
      {
        this.EnsureNotLocked();
        defaultSchema = value;
      }
    }

    /// <summary>
    /// Gets comparison results of nested <see cref="Schema"/>s.
    /// </summary>
    public ComparisonResultCollection<SchemaComparisonResult> Schemas
    {
      get { return schemas; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        defaultSchema.Lock(recursive);
        schemas.Lock(recursive);
      }
    }

    public CatalogComparisonResult(Catalog originalValue, Catalog newValue)
      : base(originalValue, newValue)
    {
    }
  }
}