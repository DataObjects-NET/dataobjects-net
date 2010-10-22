// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.StorageModel;
using Xtensive.Comparison;
using Xtensive.Collections;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Providers.Index.Memory
{
  /// <summary>
  /// In memory index storage.
  /// </summary>
  public class MemoryIndexStorage : IndexStorage
  {
    private readonly Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> realIndexes = 
      new Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>>();

    private readonly Dictionary<IndexInfo, MapTransform> indexTransforms =
      new Dictionary<IndexInfo, MapTransform>();

    /// <inheritdoc/>
    public override IStorageView CreateView(Providers.SessionHandler sessionHandler, IsolationLevel isolationLevel)
    {
      return new MemoryIndexStorageView(this, Model, sessionHandler, isolationLevel);
    }

    /// <inheritdoc/>
    public override IStorageView GetView(Providers.SessionHandler sessionHandler, Guid transactionId)
    {
      // TODO: Complete this
      return new MemoryIndexStorageView(this, Model, sessionHandler, IsolationLevel.RepeatableRead);
    }

    /// <inheritdoc/>
    public override IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo)
    {
      return realIndexes[indexInfo];
    }

    /// <inheritdoc/>
    public override MapTransform GetTransform(IndexInfo indexInfo)
    {
      return indexTransforms[indexInfo];
    }

    #region Private / internal methods

    internal void ClearSchema()
    {
      realIndexes.Clear();
      Model = new StorageInfo();
      Model.Lock(true);
    }

    internal void CreateNewSchema(StorageInfo model)
    {
      realIndexes.Clear();
      Model = model;
      if (!Model.IsLocked)
        Model.Lock(true);

      foreach (var table in model.Tables) {
        realIndexes.Add(table.PrimaryIndex, BuildIndex(table.PrimaryIndex));
        indexTransforms.Add(table.PrimaryIndex, BuildIndexTransform(table.PrimaryIndex));
        foreach (var indexInfo in table.SecondaryIndexes) {
          realIndexes.Add(indexInfo, BuildIndex(indexInfo));
          indexTransforms.Add(indexInfo, BuildIndexTransform(indexInfo));
        }
      }
    }

    private static IUniqueOrderedIndex<Tuple, Tuple> BuildIndex(IndexInfo indexInfo)
    {
      var orderingRule = new DirectionCollection<ColumnInfo>();
      if (indexInfo.IsUnique | indexInfo.IsPrimary)
        foreach (var keyColumn in indexInfo.KeyColumns)
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
      else {
        foreach (var keyColumn in indexInfo.KeyColumns)
          orderingRule.Add(keyColumn.Value, keyColumn.Direction);
        foreach (var keyColumn in indexInfo.Parent.PrimaryIndex.KeyColumns)
          if (!indexInfo.KeyColumns.Any(cr => cr.Value==keyColumn.Value))
            orderingRule.Add(keyColumn.Value, keyColumn.Direction);
      }

      var indexConfig = new IndexConfiguration<Tuple, Tuple> {
        KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(
          new ComparisonRules(ComparisonRule.Positive,
            orderingRule
              .Select(pair => (ComparisonRules) new ComparisonRule(pair.Value, CultureInfo.InvariantCulture))
              .ToArray(),
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
      var indexColumns = indexInfo.IsPrimary
        ? indexInfo.KeyColumns
          .Select(columnRef => columnRef.Value)
          .Union(((PrimaryIndexInfo) indexInfo).ValueColumns
            .Select(columnRef => columnRef.Value)).ToArray()
        : indexInfo.KeyColumns
          .Select(columnRef => columnRef.Value)
          .Union(((SecondaryIndexInfo) indexInfo).PrimaryKeyColumns
            .Select(columnRef => columnRef.Value))
          .Union(((SecondaryIndexInfo) indexInfo).IncludedColumns
            .Select(columnRef => columnRef.Value)).ToArray();

      var map = indexColumns
        .Select(columnInfo => primaryIndexColums.IndexOf(columnInfo))
        .ToArray();
      var descriptor = TupleDescriptor.Create(
        indexColumns.Select(columnInfo => columnInfo.Type.Type));
      
      return new MapTransform(true, descriptor, map);
    }

    #endregion


    // Constructors

    /// <inheritdoc/>
    public MemoryIndexStorage(string name)
      : base(name)
    {
      Model = new StorageInfo();
      Model.Lock(true);
    }
  }
}