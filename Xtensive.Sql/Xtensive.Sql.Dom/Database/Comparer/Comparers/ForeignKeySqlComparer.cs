// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class ForeignKeySqlComparer : SqlComparerBase<ForeignKey>
  {
    public override IComparisonResult<ForeignKey> Compare(ForeignKey originalNode, ForeignKey newNode, IEnumerable<ComparisonHintBase> hints)
    {
      ForeignKeyComparisonResult result = InitializeResult<ForeignKey, ForeignKeyComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
// TODO: Table, TableColumn (fix recursive loop)
//      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Columns, newNode==null ? null : newNode.Columns, hints, BaseSqlComparer2, result.Columns);
//      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.ReferencedColumns, newNode==null ? null : newNode.ReferencedColumns, hints, BaseSqlComparer2, result.ReferencedColumns);
      result.MatchType = CompareSimpleStruct(originalNode==null ? (SqlMatchType?) null : originalNode.MatchType, newNode==null ? (SqlMatchType?) null : newNode.MatchType, ref hasChanges);
      result.OnUpdate = CompareSimpleStruct(originalNode==null ? (ReferentialAction?) null : originalNode.OnUpdate, newNode==null ? (ReferentialAction?) null : newNode.OnUpdate, ref hasChanges);
      result.OnDelete = CompareSimpleStruct(originalNode==null ? (ReferentialAction?) null : originalNode.OnDelete, newNode==null ? (ReferentialAction?) null : newNode.OnDelete, ref hasChanges);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public ForeignKeySqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}