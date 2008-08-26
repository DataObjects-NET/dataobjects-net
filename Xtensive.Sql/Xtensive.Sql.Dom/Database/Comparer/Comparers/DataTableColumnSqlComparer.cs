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
  internal class DataTableColumnSqlComparer : WrappingSqlComparer<DataTableColumn, TableColumn, ViewColumn>
  {
    public override ComparisonResult<DataTableColumn> Compare(DataTableColumn originalNode, DataTableColumn newNode, IEnumerable<ComparisonHintBase> hints)
    {
      DataTableColumnComparisonResult<DataTableColumn> result;
      if (originalNode==null && newNode==null) {
        result = new DataTableColumnComparisonResult<DataTableColumn>
          {
            OriginalValue = originalNode,
            NewValue = newNode,
            ResultType = ComparisonResultType.Unchanged
          };
      }
      else if (originalNode!=null && newNode!=null && originalNode.GetType()!=newNode.GetType()) {
        result = new DataTableColumnComparisonResult<DataTableColumn>
          {
            OriginalValue = originalNode,
            NewValue = newNode,
            ResultType = ComparisonResultType.Modified
          };
      }
      throw new NotImplementedException();
//      else if ((originalNode ?? newNode).GetType()==typeof (TableColumn)) {
//        result = BaseSqlComparer1.Compare(originalNode as TableColumn, newNode as TableColumn, hints);
//      } else if ((originalNode ?? newNode).GetType() == typeof(ViewColumn)) {
//        result = BaseSqlComparer2.Compare(originalNode as ViewColumn, newNode as ViewColumn, hints);
//      } else {
//        throw new NotSupportedException(String.Format(Resources.Strings.ExColumnTypeIsNotSupportedByComparer, (originalNode ?? newNode).GetType().FullName, GetType().FullName));
//      }
//      result.Lock();
//      return result;
    }

    public DataTableColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}