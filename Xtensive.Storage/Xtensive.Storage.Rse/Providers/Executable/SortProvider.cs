// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.16

using System;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class SortProvider : ExecutableProvider,
    ISupportRandomAccess<Tuple>,
    ICountable
  {
    private readonly Provider source;
    private List<Tuple> list;
    private MapTransform transform;
    private AdvancedComparer<Tuple> keyComparer;


    long ICountable.Count
    {
      get
      {
        EnsureIsCalculated();
        return list.Count;
      }
    }


    Tuple ISupportRandomAccess<Tuple>.this[int index]
    {
      get
      {
        EnsureIsCalculated();
        return list[index];
      }
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      list = source.ToList();

      var rules = new ComparisonRules[Header.OrderInfo.OrderedBy.Count];
      var columnIndexes = new int[Header.OrderInfo.OrderedBy.Count];
      for (int i = 0; i < Header.OrderInfo.OrderedBy.Count; i++)
      {
        KeyValuePair<int, Direction> sortItem = Header.OrderInfo.OrderedBy[i];
        CultureInfo culture = Header.RecordColumnCollection[sortItem.Key].ColumnInfoRef != null
                                ? Header.RecordColumnCollection[sortItem.Key].ColumnInfoRef.CultureInfo
                                : CultureInfo.InvariantCulture;
        rules[i] = new ComparisonRule(sortItem.Value, culture);
        columnIndexes[i] = sortItem.Key;
      }
      keyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(ComparisonRule.Positive, rules, ComparisonRules.None));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => Header.TupleDescriptor[i]));

      transform = new MapTransform(true, keyDescriptor, columnIndexes);

      list.Sort(CompareStep);
      return list;
    }

    private int CompareStep(Tuple x, Tuple y)
    {
      var xKey = transform.Apply(TupleTransformType.TransformedTuple, x);
      var yKey = transform.Apply(TupleTransformType.TransformedTuple, y);
      return keyComparer.Compare(xKey, yKey);
    }


    // Constructors

    public SortProvider(CompilableProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      this.source = source;
    }
  }
}