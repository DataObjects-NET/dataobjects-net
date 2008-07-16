// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.13

using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  public sealed class IndexingProvider : ExecutableProvider
  {
    private readonly Provider source;
    private IOrderedIndex<Tuple, Tuple> index;
    private MapTransform transform;

    public override T GetService<T>()
    {
      EnsureIsCalculated();
      return index as T;
    }

    protected override IEnumerable<Tuple> Calculate()
    {
      var configuration = new IndexConfiguration<Tuple, Tuple>();
      var rules = new ComparisonRules[Header.OrderInfo.OrderedBy.Count];
      var columnIndexes = new int[Header.OrderInfo.OrderedBy.Count];
      for (int i = 0; i < Header.OrderInfo.OrderedBy.Count; i++) {
        KeyValuePair<int, Direction> sortItem = Header.OrderInfo.OrderedBy[i];
        CultureInfo culture = Header.RecordColumnCollection[sortItem.Key].ColumnInfoRef != null
                                ? Header.RecordColumnCollection[sortItem.Key].ColumnInfoRef.CultureInfo
                                : CultureInfo.InvariantCulture;
        rules[i] = new ComparisonRule(sortItem.Value, culture);
        columnIndexes[i] = sortItem.Key;
      }

      configuration.KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(ComparisonRule.Positive, rules, ComparisonRules.None));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(column => Header.TupleDescriptor[column]));
      transform = new MapTransform(true, keyDescriptor, columnIndexes);
      configuration.KeyExtractor = ExtractKey;

      index = new Index<Tuple, Tuple>(configuration);
      foreach (Tuple tuple in source)
        index.Add(tuple);

      return index;
    }

    private Tuple ExtractKey(Tuple input)
    {
      return transform.Apply(TupleTransformType.Auto, input); ;
    }


    // Constructors

    public IndexingProvider(RecordHeader header, Provider source)
      : base(header, source)
    {
      this.source = source;
    }
  }
}