// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse
{
  public static class RecordSetExtensions
  {
    public static Tuple GetRecord(this RecordSet recordSet, int index)
    {
      return recordSet.Provider.GetService<IListProvider>(true).GetItem(index);
    }

    public static RecordSet Range(this RecordSet recordSet, Expression<Func<Range<IEntire<Tuple>>>> range)
    {
      return new RangeProvider(recordSet.Provider, range).Result;
    }

    public static RecordSet Range(this RecordSet recordSet, Range<IEntire<Tuple>> range)
    {
      return new RangeProvider(recordSet.Provider, range).Result;
    }

    public static RecordSet Range(this RecordSet recordSet, IEntire<Tuple> from, IEntire<Tuple> to)
    {
      return Range(recordSet, new Range<IEntire<Tuple>>(from, to));
    }

    public static RecordSet Range(this RecordSet recordSet, Tuple from, Tuple to)
    {
      IEntire<Tuple> xPoint;
      IEntire<Tuple> yPoint;
      Direction rangeDirection = new Range<Tuple>(from,to).GetDirection(AdvancedComparer<Tuple>.Default);
      DirectionCollection<int> directions = recordSet.Provider.Header.Order;

      if (directions.Count > from.Count) {
        Direction fromDirection = directions[from.Count].Value;
        xPoint = Entire<Tuple>.Create(from, (Direction)((int)rangeDirection * (int)fromDirection * -1));
      }
      else
        xPoint = Entire<Tuple>.Create(from);

      if (directions.Count > to.Count) {
        Direction toDirection = directions[to.Count].Value;
        yPoint = Entire<Tuple>.Create(to, (Direction)((int)rangeDirection * (int)toDirection));
      }
      else
        yPoint = Entire<Tuple>.Create(to);

      return Range(recordSet, new Range<IEntire<Tuple>>(xPoint, yPoint));
    }

    public static RecordSet CalculateColumns(this RecordSet recordSet, params CalculatedColumnDescriptor[] columns)
    {
      return new CalculationProvider(recordSet.Provider, columns).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, joinedColumnIndexes).Result;
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

    public static RecordSet Seek(this RecordSet recordSet, Expression<Func<Tuple>> key)
    {
      return new SeekProvider(recordSet.Provider, key).Result;
    }

    public static RecordSet Seek(this RecordSet recordSet, Tuple key)
    {
      return new SeekProvider(recordSet.Provider, key).Result;
    }

    public static RecordSet Skip(this RecordSet recordSet, int count)
    {
      return new SkipProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Take(this RecordSet recordSet, int count)
    {
      return new TakeProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet ToRecordSet(this Tuple[] tuples, RecordSetHeader header)
    {
      return new RawProvider(header, tuples).Result;
    }

    public static int IndexOf(this RecordSet recordSet, string columnName)
    {
      MappedColumn column = (MappedColumn)recordSet.Header.Columns[columnName];
      if (column == null)
        throw new ArgumentException("columnName");
      return column.Index;
    }

    public static RecordSet Save(this RecordSet recordSet)
    {
      return new StoredProvider(recordSet.Provider).Result;
    }

    public static RecordSet Save(this RecordSet recordSet, TemporaryDataScope scope, string name)
    {
      return new StoredProvider(recordSet.Provider, scope, name).Result;
    }

    public static RecordSet CalculateAggregateFunction(this RecordSet recordSet, params AggregateColumnDescriptor[] descriptors)
    {
      return new AggregateProvider(recordSet.Provider, descriptors).Result;
    }

    public static RecordSet CalculateAggregateFunction(this RecordSet recordSet, AggregateColumnDescriptor[] descriptors, params int[] groupIndexes)
    {
      return new UnOrderedGroupProvider(recordSet.Provider, descriptors, groupIndexes).Result;
    }

    public static RecordSet ExecuteAt(this RecordSet recordSet, ExecutionOptions options)
    {
      return new ExecutionSiteProvider(recordSet.Provider, options).Result;
    }
  }
}
