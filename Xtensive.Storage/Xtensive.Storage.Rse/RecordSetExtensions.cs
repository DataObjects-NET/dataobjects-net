// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Globalization;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Helpers;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse
{
  public static class RecordSetExtensions
  {
    public static Tuple GetRecord(this RecordSet recordSet, int index)
    {
      return recordSet.Provider.GetService<IListProvider>(true).GetItem(index);
    }

    public static RecordSet Range(this RecordSet recordSet, Func<Range<Entire<Tuple>>> range)
    {
      return new RangeProvider(recordSet.Provider, ()=>range.Invoke()){CompiledRange = range}.Result;
    }

    public static RecordSet Range(this RecordSet recordSet, Expression<Func<Range<Entire<Tuple>>>> range, bool isExpression)
    {
      return new RangeProvider(recordSet.Provider, range).Result;
    }

    public static RecordSet Range(this RecordSet recordSet, Range<Entire<Tuple>> range)
    {
      return new RangeProvider(recordSet.Provider, range).Result;
    }

    public static RecordSet Range(this RecordSet recordSet, Entire<Tuple> from, Entire<Tuple> to)
    {
      return Range(recordSet, new Range<Entire<Tuple>>(from, to));
    }

    public static RecordSet Range(this RecordSet recordSet, Tuple from, Tuple to)
    {
      Entire<Tuple> xPoint;
      Entire<Tuple> yPoint;
      Direction rangeDirection = new Range<Tuple>(from,to).GetDirection(AdvancedComparer<Tuple>.Default);
      DirectionCollection<int> directions = recordSet.Provider.Header.Order;

      if (directions.Count > from.Count) {
        Direction fromDirection = directions[from.Count].Value;
        xPoint = new Entire<Tuple>(from, (Direction)((int)rangeDirection * (int)fromDirection * -1));
      }
      else
        xPoint = new Entire<Tuple>(from);

      if (directions.Count > to.Count) {
        Direction toDirection = directions[to.Count].Value;
        yPoint = new Entire<Tuple>(to, (Direction)((int)rangeDirection * (int)toDirection));
      }
      else
        yPoint = new Entire<Tuple>(to);

      return Range(recordSet, new Range<Entire<Tuple>>(xPoint, yPoint));
    }

    public static RecordSet Like(this RecordSet recordSet, Tuple beginning)
    {
      var from = beginning.Clone();
      var to = beginning.Clone();
      for (int i = 0; i < beginning.Count; i++) {
        if (beginning.Descriptor[i]==typeof (string)) {
            from.SetValue(i, beginning.GetValue<string>(i).ToLower(CultureInfo.InvariantCulture));
            to.SetValue(i, beginning.GetValue<string>(i).ToUpper(CultureInfo.InvariantCulture) + '\u00FF');
        }
      }
      return Range(recordSet, from, to);
    }

    public static RecordSet Calculate(this RecordSet recordSet, params CalculatedColumnDescriptor[] columns)
    {
      return new CalculationProvider(recordSet.Provider, columns).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, JoinType.Default, joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, JoinType.Default, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, JoinType.Default, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, JoinType.Default, joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, JoinType joinType, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, joinType, joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, JoinType joinType, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, joinType, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, JoinType joinType, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, joinType, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, JoinType joinType, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, joinType, joinedColumnIndexes).Result;
    }

    public static RecordSet OrderBy(this RecordSet recordSet, DirectionCollection<int> columnIndexes)
    {
      return new SortProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet OrderBy(this RecordSet recordSet, DirectionCollection<int> columnIndexes, bool reindex)
    {
      if (reindex)
        return new ReindexProvider(recordSet.Provider, columnIndexes).Result;
      else
        return new SortProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet Alias(this RecordSet recordSet, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new AliasProvider(recordSet.Provider, alias).Result;
    }

    public static RecordSet Filter(this RecordSet recordSet, Expression<Func<Tuple,bool>> predicate)
    {
      return new FilterProvider(recordSet.Provider, predicate).Result;
    }

    public static RecordSet Select(this RecordSet recordSet, params int[] columnIndexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, "columnIndexes");
      ArgumentValidator.EnsureArgumentIsInRange(columnIndexes.Length, 1, int.MaxValue, "columnIndexes.Length");
      return new SelectProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet Seek(this RecordSet recordSet, Func<Tuple> key)
    {
      return new SeekProvider(recordSet.Provider, () => key.Invoke()) {CompiledKey = key}.Result;
    }

    public static RecordSet Seek(this RecordSet recordSet, Expression<Func<Tuple>> key, bool isExpression)
    {
      return new SeekProvider(recordSet.Provider, key).Result;
    }

    public static RecordSet Seek(this RecordSet recordSet, Tuple key)
    {
      return new SeekProvider(recordSet.Provider, key).Result;
    }

    public static RecordSet Aggregate(this RecordSet recordSet, int[] groupIndexes, params AggregateColumnDescriptor[] descriptors)
    {
      return new AggregateProvider(recordSet.Provider, groupIndexes, descriptors).Result;
    }

    public static long Count(this RecordSet recordSet)
    {
      var resultSet = recordSet.Aggregate(null, 
        new AggregateColumnDescriptor("$Count", 0, AggregateType.Count));
      return resultSet.First().GetValue<long>(0);
    }

    public static RecordSet Skip(this RecordSet recordSet, Func<int> count)
    {
      return new SkipProvider(recordSet.Provider, ()=>count.Invoke()){CompiledCount = count}.Result;
    }

    public static RecordSet Skip(this RecordSet recordSet, Expression<Func<int>> count, bool isExpression)
    {
      return new SkipProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Skip(this RecordSet recordSet, int count)
    {
      return new SkipProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Take(this RecordSet recordSet, Func<int> count)
    {
      return new TakeProvider(recordSet.Provider, ()=>count.Invoke()){CompiledCount = count}.Result;
    }

    public static RecordSet Take(this RecordSet recordSet, Expression<Func<int>> count, bool isExpression)
    {
      return new TakeProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Take(this RecordSet recordSet, int count)
    {
      return new TakeProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Save(this RecordSet recordSet)
    {
      return new StoredProvider(recordSet.Provider).Result;
    }

    public static RecordSet Save(this RecordSet recordSet, TemporaryDataScope scope, string name)
    {
      return new StoredProvider(recordSet.Provider, scope, name).Result;
    }

    public static RecordSet ExecuteAt(this RecordSet recordSet, ExecutionOptions options)
    {
      return new ExecutionSiteProvider(recordSet.Provider, options).Result;
    }

    public static RecordSet ToRecordSet(this Tuple[] tuples, RecordSetHeader header)
    {
      return new RawProvider(header, tuples).Result;
    }

    public static RecordSet Distinct(this RecordSet recordSet)
    {
      return new DistinctProvider(recordSet.Provider).Result;
    }

    public static RecordSet Subquery(this RecordSet recordSet, Parameter<Tuple> leftItemParameter, RecordSet right)
    {
      return new SubqueryProvider(leftItemParameter, recordSet.Provider, right.Provider).Result;
    }
  }
}
