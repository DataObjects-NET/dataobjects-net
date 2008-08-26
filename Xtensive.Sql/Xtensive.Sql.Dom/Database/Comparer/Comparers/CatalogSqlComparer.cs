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
  internal class CatalogSqlComparer : WrappingSqlComparer<Catalog, Schema> //TODO: PartitionFunction, PartitionSchema
  {
    public override ComparisonResult<Catalog> Compare(Catalog originalNode, Catalog newNode, IEnumerable<ComparisonHintBase> hints)
    {
      CatalogComparisonResult result = InitializeResult<Catalog, CatalogComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.DefaultSchema = (SchemaComparisonResult)BaseSqlComparer1.Compare(originalNode == null ? null : originalNode.DefaultSchema, newNode == null ? null : newNode.DefaultSchema, hints);
      hasChanges |= result.DefaultSchema.HasChanges;
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Schemas, newNode == null ? null : newNode.Schemas, hints, BaseSqlComparer1, result.Schemas);
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public CatalogSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}