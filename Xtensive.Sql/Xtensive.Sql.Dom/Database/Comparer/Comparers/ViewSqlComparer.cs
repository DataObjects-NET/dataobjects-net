// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class ViewSqlComparer : WrappingSqlComparer<View, DataTableColumn, Index>
  {
    public ViewSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}