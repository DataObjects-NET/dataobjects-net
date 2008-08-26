// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class TableColumnComparisonResult : DataTableColumnComparisonResult, IComparisonResult<TableColumn>
  {
    /// <inheritdoc/>
    public TableColumn NewValue
    {
      get { return (TableColumn) base.NewValue; }
    }
    /// <inheritdoc/>
    public TableColumn OriginalValue
    {
      get { return (TableColumn) base.OriginalValue; }
    }
  }
}