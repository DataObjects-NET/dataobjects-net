// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse
{
  public static class RecordSetExtensions
  {
    public static RecordSet Range(this RecordSet recordSet, Func<Range<Entire<Tuple>>> range)
    {
      return new RangeProvider(recordSet.Provider, () => range.Invoke()) { CompiledRange = range }.Result;
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

    public static RecordSet RangeSet(this RecordSet recordSet, Func<RangeSet<Entire<Tuple>>> range)
    {
      return new RangeSetProvider(recordSet.Provider, () => range.Invoke()) { CompiledRange = range }.Result;
    }

    public static RecordSet RangeSet(this RecordSet recordSet, Expression<Func<RangeSet<Entire<Tuple>>>> range, bool isExpression)
    {
      return new RangeSetProvider(recordSet.Provider, range).Result;
    }

    public static RecordSet RangeSet(this RecordSet recordSet, RangeSet<Entire<Tuple>> range)
    {
      return new RangeSetProvider(recordSet.Provider, range).Result;
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
      return new CalculateProvider(recordSet.Provider, columns).Result;
    }

    public static RecordSet Calculate(this RecordSet recordSet, bool couldBeInlined, params CalculatedColumnDescriptor[] columns)
    {
      return new CalculateProvider(recordSet.Provider, couldBeInlined, columns).Result;
    }

    public static RecordSet RowNumber(this RecordSet recordSet, string columnName)
    {
      return new RowNumberProvider(recordSet.Provider, columnName).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, JoinAlgorithm joinAlgorithm, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordSet Join(this RecordSet left, RecordSet right, JoinAlgorithm joinAlgorithm, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, JoinAlgorithm joinAlgorithm, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordSet JoinLeft(this RecordSet left, RecordSet right, JoinAlgorithm joinAlgorithm, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, joinAlgorithm,
        joinedColumnIndexes).Result;
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
      return new SelectProvider(recordSet.Provider, columnIndexes).Result;
    }

    public static RecordSet Seek(this RecordSet recordSet, Func<Tuple> key)
    {
      return new SeekProvider(recordSet.Provider, () => key.Invoke()) { CompiledKey = key }.Result;
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
      return new SkipProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Skip(this RecordSet recordSet, int count)
    {
      return new SkipProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Take(this RecordSet recordSet, Func<int> count)
    {
      return new TakeProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Take(this RecordSet recordSet, int count)
    {
      return new TakeProvider(recordSet.Provider, count).Result;
    }

    public static RecordSet Save(this RecordSet recordSet)
    {
      return new StoreProvider(recordSet.Provider).Result;
    }

    public static RecordSet Save(this RecordSet recordSet, TemporaryDataScope scope, string name)
    {
      return new StoreProvider(recordSet.Provider, scope, name).Result;
    }

    public static RecordSet ExecuteAt(this RecordSet recordSet, TransferType options)
    {
      return new TransferProvider(recordSet.Provider, options).Result;
    }

    public static RecordSet ToRecordSet(this IEnumerable<Tuple> tuples, RecordSetHeader header)
    {
      return new RawProvider(header, tuples).Result;
    }

    public static RecordSet Distinct(this RecordSet recordSet)
    {
      return new DistinctProvider(recordSet.Provider).Result;
    }

    public static RecordSet Apply(this RecordSet recordSet, ApplyParameter applyParameter, RecordSet right)
    {
      return new ApplyProvider(applyParameter, recordSet.Provider, right.Provider).Result;
    }

    public static RecordSet Apply(this RecordSet recordSet, ApplyParameter applyParameter, RecordSet right, ApplySequenceType sequenceType, JoinType applyType)
    {
      return new ApplyProvider(applyParameter, recordSet.Provider, right.Provider, sequenceType, applyType).Result;
    }

    public static RecordSet Apply(this RecordSet recordSet, ApplyParameter applyParameter, RecordSet right, bool couldBeInlinied, ApplySequenceType sequenceType, JoinType applyType)
    {
      return new ApplyProvider(applyParameter, recordSet.Provider, right.Provider, couldBeInlinied, sequenceType, applyType).Result;
    }

    public static RecordSet Existence(this RecordSet recordSet, string existenceColumnName)
    {
      return new ExistenceProvider(recordSet.Provider, existenceColumnName).Result;
    }

    public static RecordSet Include(this RecordSet recordSet, Expression<Func<IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(recordSet.Provider, IncludeAlgorithm.Auto, filterDataSource, resultColumnName, filteredColumns).Result;
    }
    
    public static RecordSet Include(this RecordSet recordSet, IncludeAlgorithm algorithm, Expression<Func<IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(recordSet.Provider, algorithm, filterDataSource, resultColumnName, filteredColumns).Result;
    }
    
    public static RecordSet Intersect(this RecordSet left, RecordSet right)
    {
      return new IntersectProvider(left.Provider, right.Provider).Result;
    }

    public static RecordSet Except(this RecordSet left, RecordSet right)
    {
      return new ExceptProvider(left.Provider, right.Provider).Result;
    }

    public static RecordSet Concat(this RecordSet left, RecordSet right)
    {
      return new ConcatProvider(left.Provider, right.Provider).Result;
    }

    public static RecordSet Union(this RecordSet left, RecordSet right)
    {
      return new UnionProvider(left.Provider, right.Provider).Result;
    }

    /// <summary>
    /// Creates the <see cref="LockProvider"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="lockMode">The lock mode.</param>
    /// <param name="lockBehavior">The lock behavior.</param>
    /// <returns>The <see cref="RecordSet"/> which is the result of 
    /// the created <see cref="LockProvider"/>.</returns>
    public static RecordSet Lock(this RecordSet source, LockMode lockMode, LockBehavior lockBehavior)
    {
      return new LockProvider(source.Provider, lockMode, lockBehavior).Result;
    }

    /// <summary>
    /// Creates the <see cref="LockProvider"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="lockMode">The delegate returning the lock mode.</param>
    /// <param name="lockBehavior">The delegate returning the lock behavior.</param>
    /// <returns>The <see cref="RecordSet"/> which is the result of 
    /// the created <see cref="LockProvider"/>.</returns>
    public static RecordSet Lock(this RecordSet source, Func<LockMode> lockMode,
      Func<LockBehavior> lockBehavior)
    {
      return new LockProvider(source.Provider, lockMode, lockBehavior).Result;
    }
  }
}
