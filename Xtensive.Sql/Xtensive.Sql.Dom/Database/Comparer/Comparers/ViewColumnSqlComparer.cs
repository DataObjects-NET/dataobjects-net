// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27


namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class ViewColumnSqlComparer : NodeSqlComparer<ViewColumn, ViewColumnComparisonResult>
  {
    public ViewColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}