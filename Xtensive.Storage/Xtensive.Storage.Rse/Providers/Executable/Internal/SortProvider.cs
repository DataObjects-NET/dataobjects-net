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
    private const string SortKey = "SortKey";
    private readonly Provider source;
    private MapTransform transform;
    private AdvancedComparer<Tuple> keyComparer;

    long ICountable.Count {
      get {
        var context = EnumerationScope.CurrentContext;
        var list = GetCachedValue<List<Tuple>>(context, SortKey);
        return list.Count;
      }
    }

    Tuple IListProvider.GetItem(int index)
    {
      var context = EnumerationScope.CurrentContext;
      var list = GetCachedValue<List<Tuple>>(context, SortKey);
      return list[index];
    }

    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);

      var list = source.ToList();
      list.Sort((x, y) => keyComparer.Compare(
        transform.Apply(TupleTransformType.TransformedTuple, x), 
        transform.Apply(TupleTransformType.TransformedTuple, y)));

      SetCachedValue(context, SortKey, list);
    }

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return GetCachedValue<List<Tuple>>(context, SortKey);
    }

    protected override void Initialize()
    {
      base.Initialize();

      var rules = new ComparisonRules[Header.Order.Count];
      var columnIndexes = new int[Header.Order.Count];
      for (int i = 0; i < Header.Order.Count; i++) {
        KeyValuePair<int, Direction> sortItem = Header.Order[i];
        CultureInfo culture = ((RawColumn)Header.Columns[sortItem.Key]).ColumnInfoRef != null
                                ? ((RawColumn)Header.Columns[sortItem.Key]).ColumnInfoRef.CultureInfo
                                : CultureInfo.InvariantCulture;
        rules[i] = new ComparisonRule(sortItem.Value, culture);
        columnIndexes[i] = sortItem.Key;
      }

      keyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(ComparisonRule.Positive, rules, ComparisonRules.None));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => Header.TupleDescriptor[i]));
      transform = new MapTransform(true, keyDescriptor, columnIndexes);

      var context = EnumerationScope.CurrentContext;
      SetCachedValue(context, SortKey, new List<Tuple>());
    }


    // Constructors

    public SortProvider(CompilableProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      AddService<IListProvider>();
      this.source = source;
    }
  }
}