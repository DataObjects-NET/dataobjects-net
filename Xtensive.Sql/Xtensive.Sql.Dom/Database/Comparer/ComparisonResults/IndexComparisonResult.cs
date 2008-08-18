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
  /// Index compare result.
  /// </summary>
  [Serializable]
  public class IndexComparisonResult : ComparisonResult<Index>
  {
    private readonly CollectionBaseSlim<IndexColumnComparisonResult> columns = new CollectionBaseSlim<IndexColumnComparisonResult>();

    /// <summary>
    /// Gets column compare results.
    /// </summary>
    public CollectionBaseSlim<IndexColumnComparisonResult> Columns
    {
      get { return columns; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(recursive);
      }
    }
  }
}