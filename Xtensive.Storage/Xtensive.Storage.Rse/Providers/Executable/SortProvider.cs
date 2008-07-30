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
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class SortProvider : ExecutableProvider,
    IListProvider
  {
    private readonly Provider source;
    private MapTransform transform;
    private AdvancedComparer<Tuple> keyComparer;


    long ICountable.Count
    {
      get
      {
        var context = EnumerationScope.CurrentContext;
        var list = Enumerate(context) as ICountable;
        if (list != null)
          return list.Count;
        return -1;
      }
    }

    public Tuple GetItem(int index)
    {
      var context = EnumerationScope.CurrentContext;
      var list = Enumerate(context) as IListProvider;
      if (list!=null)
        return list.GetItem(index);
      return null;
    }


    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var list = source.ToList();

      var rules = new ComparisonRules[Header.OrderDescriptor.Order.Count];
      var columnIndexes = new int[Header.OrderDescriptor.Order.Count];
      for (int i = 0; i < Header.OrderDescriptor.Order.Count; i++)
      {
        KeyValuePair<int, Direction> sortItem = Header.OrderDescriptor.Order[i];
        CultureInfo culture = Header.Columns[sortItem.Key].ColumnInfoRef != null
                                ? Header.Columns[sortItem.Key].ColumnInfoRef.CultureInfo
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

    protected override void Initialize()
    {
    }


    // Constructors

    public SortProvider(Provider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<IListProvider>();
      this.source = source;
    }
  }
}