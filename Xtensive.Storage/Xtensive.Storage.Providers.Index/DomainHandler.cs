// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Optimization;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Index
{
  public class DomainHandler : Providers.DomainHandler
  {
    private readonly Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> realIndexes = new Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>>();
    private readonly Dictionary<Pair<IndexInfo, TypeInfo>, MapTransform> indexTransforms = new Dictionary<Pair<IndexInfo, TypeInfo>, MapTransform>();

    protected override IEnumerable<Type> GetProviderCompilerExtensionTypes()
    {
      return Type.EmptyTypes;
    }

    protected override ICompiler BuildCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      return new IndexCompiler(Handlers, compiledSources);
    }

    protected override IOptimizer BuildOptimizer()
    {
      return new CompositeOptimizer(new OrderbyOptimizer(),
        new IndexOptimizer(Handlers.Domain.Model, new StatisticsProviderResolver(this)));
    }

    /// <inheritdoc/>
    public override void BuildMappingSchema()
    {
      BuildRealIndexes();
      foreach (var pair in Handlers.Domain.Model.Types.SelectMany(type => type.Indexes.Where(i => i.ReflectedType==type).Union(type.AffectedIndexes).Distinct().Select(i => new Pair<IndexInfo, TypeInfo>(i, type)))) {
        MapTransform transform = BuildIndexTransform(pair.First, pair.Second);
        indexTransforms.Add(pair, transform);
      }
    }

    #region Private / internal methods

    internal IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfoRef indexInfoRef)
    {
      var index = indexInfoRef.Resolve(Handlers.Domain.Model);
      return realIndexes[index];
    }

    internal IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo)
    {
      return realIndexes[indexInfo];
    }

    internal MapTransform GetIndexTransform(IndexInfo indexInfo, TypeInfo type)
    {
      return indexTransforms[new Pair<IndexInfo, TypeInfo>(indexInfo, type)];
    }

    private static MapTransform BuildIndexTransform(IndexInfo indexInfo, TypeInfo type)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(indexInfo.Columns.Select(c => c.ValueType));
      int[] map = indexInfo.Columns
        .Select(c =>
        {
          ColumnInfo column;
          return type.Columns.TryGetValue(c.Field.Column.Name, out column) ?
            column.Field.MappingInfo.Offset :
            MapTransform.NoMapping;
        }).ToArray();
      return new MapTransform(true, descriptor, map);
    }

    private void BuildRealIndexes()
    {
      foreach (IndexInfo indexInfo in Handlers.Domain.Model.RealIndexes)
      {
        DirectionCollection<ColumnInfo> orderingRule;
        if (indexInfo.IsUnique | indexInfo.IsPrimary)
          orderingRule = new DirectionCollection<ColumnInfo>(indexInfo.KeyColumns);
        else
        {
          orderingRule = new DirectionCollection<ColumnInfo>(indexInfo.KeyColumns);
          for (int i = 0; i < indexInfo.ValueColumns.Count; i++)
          {
            var column = indexInfo.ValueColumns[i];
            if (indexInfo.IncludedColumns.Contains(column))
              break;
            orderingRule.Add(column, Direction.Positive);
          }
        }

        var indexConfig = new IndexConfiguration<Tuple, Tuple>();
        indexConfig.KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(
          ComparisonRule.Positive,
          orderingRule.Select(pair => (ComparisonRules)new ComparisonRule(pair.Value, CultureInfo.InvariantCulture)).ToArray(),
          ComparisonRules.None));
        indexConfig.KeyExtractor = input => input;
        IUniqueOrderedIndex<Tuple, Tuple> index = IndexFactory.CreateUniqueOrdered<Tuple, Tuple, Index<Tuple, Tuple>>(indexConfig);
        realIndexes[indexInfo] = index;
      }
    }

    #endregion
  }
}