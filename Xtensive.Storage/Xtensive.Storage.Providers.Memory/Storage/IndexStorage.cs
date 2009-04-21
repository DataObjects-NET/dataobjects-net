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
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Core.Comparison;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Providers.Memory
{
  /// <summary>
  /// In memory implementation of index storage.
  /// </summary>
  public class IndexStorage : Index.IndexStorage
  {
    private readonly Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> realIndexes = 
      new Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>>();
    private readonly Dictionary<IndexInfo, MapTransform> secondaryIndexTransforms = 
      new Dictionary<IndexInfo, MapTransform>();

    /// <inheritdoc/>
    public override IStorageView CreateView(IsolationLevel isolationLevel)
    {
      return new IndexStorageView(this, Model, isolationLevel);
    }

    /// <inheritdoc/>
    public override IStorageView GetView(Guid transactionId)
    {
      // ToDo: Complete this.
      return new IndexStorageView(this, Model, IsolationLevel.ReadCommitted);
    }

    /// <inheritdoc/>
    public override IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo)
    {
      return realIndexes[indexInfo];
    }

    /// <inheritdoc/>
    public override MapTransform GetTransform(IndexInfo indexInfo)
    {
      return secondaryIndexTransforms[indexInfo];
    }

    internal void ClearSchema()
    {
      realIndexes.Clear();
      Model = new StorageInfo(Name);
    }

    internal void CreateNewSchema(StorageInfo model)
    {
      ClearSchema();
      Model = model;

      foreach (TableInfo table in model.Tables) {
        realIndexes.Add(table.PrimaryIndex, BuildIndex(table.PrimaryIndex));
        secondaryIndexTransforms.Add(table.PrimaryIndex, BuildIndexTransform(table.PrimaryIndex));
        foreach (SecondaryIndexInfo indexInfo in table.SecondaryIndexes) {
          realIndexes.Add(indexInfo, BuildIndex(indexInfo));
          secondaryIndexTransforms.Add(indexInfo, BuildIndexTransform(indexInfo));
        }
      }
    }

    private static IUniqueOrderedIndex<Tuple, Tuple> BuildIndex(IndexInfo indexInfo)
    {
      var orderingRule = new DirectionCollection<ColumnInfo>();
      if (indexInfo.IsUnique | indexInfo.IsPrimary)
        foreach (var keyColumn in indexInfo.KeyColumns) {
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        }
      else {
        foreach (var keyColumn in indexInfo.KeyColumns) {
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        }
        foreach (var keyColumn in indexInfo.Parent.PrimaryIndex.KeyColumns) {
          if (indexInfo.KeyColumns.SingleOrDefault(cr => cr.Value == keyColumn.Value) == null)
            orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        }
      }

      var indexConfig = new IndexConfiguration<Tuple, Tuple>
        {
          KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(
            new ComparisonRules(ComparisonRule.Positive,
              orderingRule.Select(
                pair => (ComparisonRules) new ComparisonRule(pair.Value, CultureInfo.InvariantCulture)).ToArray(),
              ComparisonRules.None)),
          KeyExtractor = (input => input)
        };
      return IndexFactory.CreateUniqueOrdered<Tuple, Tuple, Index<Tuple, Tuple>>(indexConfig);
    }

    private static MapTransform BuildIndexTransform(IndexInfo indexInfo)
    {
      var primaryIndex = indexInfo.Parent.PrimaryIndex;
      var primaryIndexColums = primaryIndex.KeyColumns.Select(columnRef => columnRef.Value)
        .Union(primaryIndex.ValueColumns.Select(columnRef => columnRef.Value)).ToArray();
      var indexColumns = indexInfo.KeyColumns.Select(columnRef => columnRef.Value)
        .Union(indexInfo.ValueColumns.Select(columnRef => columnRef.Value)).ToArray();
      var map = indexColumns.Select(columnInfo => primaryIndexColums.IndexOf(columnInfo)).ToArray();
      var descriptor = TupleDescriptor.Create(indexColumns.Select(columnInfo => columnInfo.ColumnType.DataType));
      return new MapTransform(true, descriptor, map);
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