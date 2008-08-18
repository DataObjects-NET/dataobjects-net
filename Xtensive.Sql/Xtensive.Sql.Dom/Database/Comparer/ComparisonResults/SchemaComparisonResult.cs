// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Schema compare result.
  /// </summary>
  [Serializable]
  public class SchemaComparisonResult : ComparisonResult<Schema>
  {
    private readonly CollectionBaseSlim<TableComparisonResult> tables = new CollectionBaseSlim<TableComparisonResult>();
    private readonly CollectionBaseSlim<ViewComparisonResult> views = new CollectionBaseSlim<ViewComparisonResult>();

    /// <summary>
    /// Gets table compare result.
    /// </summary>
    public CollectionBaseSlim<TableComparisonResult> Tables
    {
      get { return tables; }
    }

    /// <summary>
    /// Gets view compare results.
    /// </summary>
    public CollectionBaseSlim<ViewComparisonResult> Views
    {
      get { return views; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        tables.Lock(recursive);
        views.Lock(recursive);
      }
    }
  }
}