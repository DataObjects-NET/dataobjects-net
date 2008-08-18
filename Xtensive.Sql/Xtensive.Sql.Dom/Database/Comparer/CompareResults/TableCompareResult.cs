// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using System;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Table compare result.
  /// </summary>
  [Serializable]
  public class TableCompareResult : CompareResult<Table>
  {
    private readonly CollectionBaseSlim<TableColumnCompareResult> columns = new CollectionBaseSlim<TableColumnCompareResult>();
    private readonly CollectionBaseSlim<IndexCompareResult> indexes = new CollectionBaseSlim<IndexCompareResult>();
    private readonly CollectionBaseSlim<ConstraintCompareResult> constraints = new CollectionBaseSlim<ConstraintCompareResult>();

    /// <summary>
    /// Gets column compare results.
    /// </summary>
    public CollectionBaseSlim<TableColumnCompareResult> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets index compare results.
    /// </summary>
    public CollectionBaseSlim<IndexCompareResult> Indexes
    {
      get { return indexes; }
    }

    /// <summary>
    /// Gets constraint compare results.
    /// </summary>
    public CollectionBaseSlim<ConstraintCompareResult> Constraints
    {
      get { return constraints; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(true);
        indexes.Lock(true);
        constraints.Lock(true);
      }
    }
  }
}