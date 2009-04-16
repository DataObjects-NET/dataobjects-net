// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Core.Comparison;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers.Memory
{
  public class IndexStorage : Index.IndexStorage
  {
    private readonly Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> realIndexes = 
      new Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>>();

    /// <inheritdoc/>
    public override IStorageView CreateView(IsolationLevel isolationLevel)
    {
      return new IndexStorageView(this, Model);
    }

    /// <inheritdoc/>
    public override IStorageView GetView(Guid transactionId)
    {
      // ToDo: Complete this.
      return new IndexStorageView(this, Model);
    }
    
    internal void ClearSchema()
    {
      Model = null;
      realIndexes.Clear();
    }

    internal void CreateNewSchema(StorageInfo model)
    {
      Model = model;
      realIndexes.Clear();
      foreach (TableInfo table in model.Tables)
      {
        realIndexes.Add(table.PrimaryIndex, BuildIndex(table.PrimaryIndex));
        foreach (SecondaryIndexInfo indexInfo in table.SecondaryIndexes)
        {
          realIndexes.Add(indexInfo, BuildIndex(indexInfo));
        }
      }
    }

    private static IUniqueOrderedIndex<Tuple, Tuple> BuildIndex(IndexInfo indexInfo)
    {
      var orderingRule = new DirectionCollection<ColumnInfo>();
      if (indexInfo.IsUnique | indexInfo.IsPrimary)
        foreach (KeyColumnRef keyColumn in indexInfo.KeyColumns) {
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        }
      else {
        foreach (KeyColumnRef keyColumn in indexInfo.KeyColumns) {
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        }
        foreach (KeyColumnRef keyColumn in indexInfo.Parent.PrimaryIndex.KeyColumns) {
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        }
      }

      var indexConfig = new IndexConfiguration<Tuple, Tuple>();
      indexConfig.KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new Xtensive.Core.Comparison.ComparisonRules(
        ComparisonRule.Positive,
        orderingRule.Select(pair => (ComparisonRules) new ComparisonRule(pair.Value, CultureInfo.InvariantCulture)).ToArray(),
        ComparisonRules.None));
      indexConfig.KeyExtractor = input => input;
      return IndexFactory.CreateUniqueOrdered<Tuple, Tuple, Index<Tuple, Tuple>>(indexConfig);
    }

    public override IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo)
    {
      return realIndexes[indexInfo];
    }
    

    // Constructor

    /// <inheritdoc/>
    public IndexStorage(string name)
      : base(name)
    {
      Model = new StorageInfo(name);
    }
  }
}