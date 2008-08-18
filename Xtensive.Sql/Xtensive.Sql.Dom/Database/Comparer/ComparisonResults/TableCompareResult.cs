// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class TableCompareResult : CompareResult
  {
    private CollectionBaseSlim<ColumnCompareResult> columns = new CollectionBaseSlim<ColumnCompareResult>();
    private CollectionBaseSlim<IndexCompareResult> indexes = new CollectionBaseSlim<IndexCompareResult>();
    private CollectionBaseSlim<ConstraintCompareResult> constraints = new CollectionBaseSlim<ConstraintCompareResult>();

    public CollectionBaseSlim<ColumnCompareResult> Columns
    {
      get { return columns; }
    }

    public CollectionBaseSlim<IndexCompareResult> Indexes
    {
      get { return indexes; }
    }

    public CollectionBaseSlim<ConstraintCompareResult> Constraints
    {
      get { return constraints; }
    }


    public override bool HasChanges
    {
      get { throw new System.NotImplementedException(); }
    }

    public override CompareResultType Result
    {
      get { throw new System.NotImplementedException(); }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock();
      }
    }
  }
}