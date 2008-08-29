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
  internal class TableColumnSqlComparer : WrappingSqlComparer<TableColumn, Collation, Domain, SequenceDescriptor>
  {
    public override IComparisonResult<TableColumn> Compare(TableColumn originalNode, TableColumn newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = new TableColumnComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.Collation = (CollationComparisonResult)BaseSqlComparer1.Compare(originalNode == null ? null : originalNode.Collation, newNode == null ? null : newNode.Collation, hints);
      hasChanges |= result.Collation.HasChanges;
      result.Domain = (DomainComparisonResult) BaseSqlComparer2.Compare(originalNode==null ? null : originalNode.Domain, newNode==null ? null : newNode.Domain, hints);
      hasChanges |= result.Domain.HasChanges;
      result.SequenceDescriptor = (SequenceDescriptorComparisonResult) BaseSqlComparer3.Compare(originalNode==null ? null : originalNode.SequenceDescriptor, newNode==null ? null : newNode.SequenceDescriptor, hints);
      hasChanges |= result.SequenceDescriptor.HasChanges;
      result.DataType = CompareSimpleNode(originalNode==null ? null : originalNode.DataType, newNode==null ? null : newNode.DataType, ref hasChanges);
      result.DefaultValue = CompareSimpleNode(originalNode==null ? null : originalNode.DefaultValue, newNode==null ? null : newNode.DefaultValue, ref hasChanges);
      result.Expression = CompareSimpleNode(originalNode==null ? null : originalNode.Expression, newNode==null ? null : newNode.Expression, ref hasChanges);
      result.IsPersisted = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.IsPersisted, newNode==null ? (bool?) null : newNode.IsPersisted, ref hasChanges);
      result.IsNullable = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.IsNullable, newNode==null ? (bool?) null : newNode.IsNullable, ref hasChanges);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public TableColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}