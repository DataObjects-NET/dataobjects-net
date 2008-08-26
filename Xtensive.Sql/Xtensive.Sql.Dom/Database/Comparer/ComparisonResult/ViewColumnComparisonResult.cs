// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.26

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class ViewColumnComparisonResult : DataTableColumnComparisonResult, IComparisonResult<ViewColumn>
  {
    /// <inheritdoc/>
    public ViewColumn NewValue
    {
      get { return (ViewColumn)base.NewValue; }
    }
    /// <inheritdoc/>
    public ViewColumn OriginalValue
    {
      get { return (ViewColumn)base.OriginalValue; }
    }
  }
}