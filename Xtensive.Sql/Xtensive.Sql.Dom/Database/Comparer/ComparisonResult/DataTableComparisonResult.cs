// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class DataTableComparisonResult : NodeComparisonResult,
    IComparisonResult<DataTable>
  {
    /// <inheritdoc/>
    public DataTable NewValue
    {
      get { return (DataTable)base.NewValue; }
    }

    /// <inheritdoc/>
    public DataTable OriginalValue
    {
      get { return (DataTable)base.OriginalValue; }
    }

    public DataTableComparisonResult(DataTable originalValue, DataTable newValue)
      : base(originalValue, newValue)
    {
    }
  }
}