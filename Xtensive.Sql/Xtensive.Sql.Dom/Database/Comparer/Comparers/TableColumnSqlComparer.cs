// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class TableColumnSqlComparer : WrappingSqlComparer<TableColumn, Collation, SequenceDescriptor>
  {
    public override IComparisonResult<TableColumn> Compare(TableColumn originalNode, TableColumn newNode, IEnumerable<ComparisonHintBase> hints)
    {
      TableColumnComparisonResult result = InitializeResult<TableColumn, TableColumnComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      throw new NotImplementedException();
//      result.Owner = (NodeComparisonResult<User>)userComparer.Compare(originalNode == null ? null : originalNode.Owner, newNode == null ? null : newNode.Owner, hints);
//      hasChanges |= result.Owner.HasChanges;
//      result.DefaultCharacterSet = (NodeComparisonResult<CharacterSet>)characterSetComparer.Compare(originalNode == null ? null : originalNode.DefaultCharacterSet, newNode == null ? null : newNode.DefaultCharacterSet, hints);
//      hasChanges |= result.DefaultCharacterSet.HasChanges;
//      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Tables, newNode == null ? null : newNode.Tables, hints, tableComparer, result.Tables);
//      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
//        result.ResultType = ComparisonResultType.Modified;
//      result.Lock(true);
//      return result;
    }

    public TableColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}