// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class ForeignKeyComparer : NodeComparerBase<ForeignKey>
  {
    private NodeComparerStruct<TableColumn> columnComparer; 

    public override IComparisonResult<ForeignKey> Compare(ForeignKey originalNode, ForeignKey newNode)
    {
      var result = ComparisonContext.Current.Factory.CreateComparisonResult<ForeignKey, ForeignKeyComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.MatchType = CompareSimpleStruct(originalNode==null ? (SqlMatchType?) null : originalNode.MatchType, newNode==null ? (SqlMatchType?) null : newNode.MatchType, ref hasChanges);
      result.OnUpdate = CompareSimpleStruct(originalNode==null ? (ReferentialAction?) null : originalNode.OnUpdate, newNode==null ? (ReferentialAction?) null : newNode.OnUpdate, ref hasChanges);
      result.OnDelete = CompareSimpleStruct(originalNode==null ? (ReferentialAction?) null : originalNode.OnDelete, newNode==null ? (ReferentialAction?) null : newNode.OnDelete, ref hasChanges);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Columns, newNode == null ? null : newNode.Columns, columnComparer, result.Columns);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.ReferencedColumns, newNode == null ? null : newNode.ReferencedColumns, columnComparer, result.ReferencedColumns);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      return result;
    }

    public ForeignKeyComparer(INodeComparerProvider provider)
      : base(provider)
    {
      ReferenceComparer<TableColumn> referenceComparer = new ReferenceComparer<TableColumn>(provider);
      INodeComparer<TableColumn> nodeComparer = (INodeComparer<TableColumn>)referenceComparer;
      columnComparer = new NodeComparer<TableColumn>(nodeComparer);
    }
  }
}