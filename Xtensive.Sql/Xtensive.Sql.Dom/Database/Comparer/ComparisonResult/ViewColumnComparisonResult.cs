// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.26

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class ViewColumnComparisonResult : ComparisonResult<ViewColumn>, 
    IComparisonResult<DataTableColumn>
  {
    /// <inheritdoc/>
    DataTableColumn IComparisonResult<DataTableColumn>.NewValue
    {
      get { return NewValue; }
    }
    /// <inheritdoc/>
    DataTableColumn IComparisonResult<DataTableColumn>.OriginalValue
    {
      get { return OriginalValue; }
    }
  }
}