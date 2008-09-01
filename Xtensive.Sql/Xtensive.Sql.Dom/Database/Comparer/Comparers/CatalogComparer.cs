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
  internal class CatalogComparer : WrappingNodeComparer<Catalog, Schema> //TODO: PartitionFunction, PartitionSchema
  {
    public override IComparisonResult<Catalog> Compare(Catalog originalNode, Catalog newNode)
    {
      var result = new CatalogComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.DefaultSchema = (SchemaComparisonResult) BaseNodeComparer1.Compare(originalNode==null ? null : originalNode.DefaultSchema, newNode==null ? null : newNode.DefaultSchema);
      hasChanges |= result.DefaultSchema.HasChanges;
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Schemas, newNode==null ? null : newNode.Schemas, BaseNodeComparer1, result.Schemas);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public CatalogComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}