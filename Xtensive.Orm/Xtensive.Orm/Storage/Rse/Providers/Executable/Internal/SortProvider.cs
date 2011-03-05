// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.16

using System;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class SortProvider : ExecutableProvider,
    IListProvider
  {
    private const string SortKey = "SortKey";
    private readonly ExecutableProvider source;
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
      return GetCachedValue<List<Tuple>>(context, SortKey) ?? EnumerableUtils<Tuple>.Empty;
    }

    protected override void Initialize()
    {
      base.Initialize();

      var rules = new ComparisonRules[Header.Order.Count];
      var columnIndexes = new int[Header.Order.Count];
      for (int i = 0; i < Header.Order.Count; i++) {
        KeyValuePair<int, Direction> sortItem = Header.Order[i];
        var culture = CultureInfo.InvariantCulture;
        var mappedColumn = Header.Columns[sortItem.Key] as MappedColumn;
        if (mappedColumn != null && mappedColumn.ColumnInfoRef != null)
          culture = mappedColumn.ColumnInfoRef.CultureInfo;
        rules[i] = new ComparisonRule(sortItem.Value, culture);
        columnIndexes[i] = sortItem.Key;
      }

      keyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(ComparisonRule.Positive, rules, ComparisonRules.None));
      TupleDescriptor keyDescriptor = TupleDescriptor.Create(columnIndexes.Select(i => Header.TupleDescriptor[i]));
      transform = new MapTransform(true, keyDescriptor, columnIndexes);
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