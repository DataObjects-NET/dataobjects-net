// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using System.Linq;
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
      DirectionCollection<int> directions = recordSet.Provider.Header.OrderDescriptor.Order;

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

    public static RecordSet Join(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, false, joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, true, joinedColumnIndexes).Result;
    }

    public static RecordSet OrderBy(this RecordSet recordSet, DirectionCollection<int> columnIndexes)
    {
      return new SortProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet IndexBy(this RecordSet recordSet, DirectionCollection<int> columnIndexes)
    {
      return new IndexingProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet Alias(this RecordSet recordSet, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new AliasProvider(recordSet.Provider, alias).Result;
    }

    public static RecordSet Where(this RecordSet recordSet, Expression<Func<Tuple,bool>> predicate)
    {
      return new WhereProvider(recordSet.Provider, predicate).Result;
    }

    public static RecordSet Select(this RecordSet recordSet, params int[] columnIndexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, "columnIndexes");
      ArgumentValidator.EnsureArgumentIsInRange(columnIndexes.Length, 1, int.MaxValue, "columnIndexes.Length");
      return new SelectProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet Skip(this RecordSet recordSet, int count)
    {
      return new RawProvider(recordSet.Header, ((IEnumerable<Tuple>)recordSet).Skip(count).ToArray()).Result;
    }


    public static RecordSet Take(this RecordSet recordSet, int count)
    {
      return new RawProvider(recordSet.Header, ((IEnumerable<Tuple>)recordSet).Take(count).ToArray()).Result;
    }

    public static int IndexOf(this RecordSet recordSet, string columnName)
    {
      RecordColumn column = recordSet.Header.Columns[columnName];
      if (column == null)
        throw new ArgumentException("columnName");
      return column.Index;
    }
  }
}