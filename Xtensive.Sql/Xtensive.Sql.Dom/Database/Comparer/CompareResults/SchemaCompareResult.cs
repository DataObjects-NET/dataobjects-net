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
  public class SchemaCompareResult : CompareResult<Schema>
  {
    private readonly CollectionBaseSlim<TableCompareResult> tables = new CollectionBaseSlim<TableCompareResult>();
    private readonly CollectionBaseSlim<ViewCompareResult> views = new CollectionBaseSlim<ViewCompareResult>();

    /// <summary>
    /// Gets table compare result.
    /// </summary>
    public CollectionBaseSlim<TableCompareResult> Tables
    {
      get { return tables; }
    }

    /// <summary>
    /// Gets view compare results.
    /// </summary>
    public CollectionBaseSlim<ViewCompareResult> Views
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