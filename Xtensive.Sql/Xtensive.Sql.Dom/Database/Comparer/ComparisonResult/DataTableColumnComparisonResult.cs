// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.22

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class DataTableColumnComparisonResult : NodeComparisonResult, 
    IComparisonResult<DataTableColumn>
  {
    public DataTableColumn NewValue
    {
      get { return (DataTableColumn) base.NewValue; }
    }

    public DataTableColumn OriginalValue
    {
      get { return (DataTableColumn) base.OriginalValue; }
    }

    public DataTableColumnComparisonResult(DataTableColumn originalValue, DataTableColumn newValue)
      : base(originalValue, newValue)
    {
    }
  }
}