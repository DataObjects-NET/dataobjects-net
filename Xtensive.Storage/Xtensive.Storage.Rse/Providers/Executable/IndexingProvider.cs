// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.13

using System;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public sealed class IndexingProvider : UnaryExecutableProvider
  {
    private MapTransform transform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var configuration = new IndexConfiguration<Tuple, Tuple>();
      var rules = new ComparisonRules[Header.OrderDescriptor.Order.Count];
      var columnIndexes = new int[Header.OrderDescriptor.Order.Count];
      for (int i = 0; i < Header.OrderDescriptor.Order.Count; i++) {
        KeyValuePair<int, Direction> sortItem = Header.OrderDescriptor.Order[i];
        CultureInfo culture = Header.Columns[sortItem.Key].ColumnInfoRef != null
                                ? Header.Columns[sortItem.Key].ColumnInfoRef.CultureInfo
                                : CultureInfo.InvariantCulture;
        rules[i] = new ComparisonRule(sortItem.Value, culture);
        columnIndexes[i] = sortItem.Key;
      }

      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(column => Header.TupleDescriptor[column]));
      transform = new MapTransform(true, keyDescriptor, columnIndexes);

      configuration.KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(ComparisonRule.Positive, rules, ComparisonRules.None));
      configuration.KeyExtractor = ExtractKey;

      var index = new Index<Tuple, Tuple>(configuration);
      foreach (Tuple tuple in Source.Enumerate(context))
        index.Add(tuple);

      return index;
    }

    private Tuple ExtractKey(Tuple input)
    {
      return transform.Apply(TupleTransformType.Auto, input); ;
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      if (typeof(T) == typeof(ICachingProvider))
        return base.GetService<T>();
      return Enumerate(EnumerationScope.CurrentContext) as T;
    }


    // Constructors

    public IndexingProvider(CompilableProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<ICachingProvider>();
    }
  }
}