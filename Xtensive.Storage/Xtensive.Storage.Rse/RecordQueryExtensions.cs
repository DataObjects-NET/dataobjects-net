// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// <see cref="RecordQuery"/> related extension methods.
  /// </summary>
  public static class RecordQueryExtensions
  {
    public static RecordQuery Range(this RecordQuery recordQuery, Func<Range<Entire<Tuple>>> range)
    {
      return new RangeProvider(recordQuery.Provider, () => range.Invoke()) { CompiledRange = range }.Result;
    }

    public static RecordQuery Range(this RecordQuery recordQuery, Expression<Func<Range<Entire<Tuple>>>> range, bool isExpression)
    {
      return new RangeProvider(recordQuery.Provider, range).Result;
    }

    public static RecordQuery Range(this RecordQuery recordQuery, Range<Entire<Tuple>> range)
    {
      return new RangeProvider(recordQuery.Provider, range).Result;
    }

    public static RecordQuery Range(this RecordQuery recordQuery, Entire<Tuple> from, Entire<Tuple> to)
    {
      return Range(recordQuery, new Range<Entire<Tuple>>(from, to));
    }

    public static RecordQuery Range(this RecordQuery recordQuery, Tuple from, Tuple to)
    {
      Entire<Tuple> xPoint;
      Entire<Tuple> yPoint;
      Direction rangeDirection = new Range<Tuple>(from,to).GetDirection(AdvancedComparer<Tuple>.Default);
      DirectionCollection<int> directions = recordQuery.Provider.Header.Order;

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

      return Range(recordQuery, new Range<Entire<Tuple>>(xPoint, yPoint));
    }

    public static RecordQuery RangeSet(this RecordQuery recordQuery, Func<RangeSet<Entire<Tuple>>> range)
    {
      return new RangeSetProvider(recordQuery.Provider, () => range.Invoke()) { CompiledRange = range }.Result;
    }

    public static RecordQuery RangeSet(this RecordQuery recordQuery, Expression<Func<RangeSet<Entire<Tuple>>>> range, bool isExpression)
    {
      return new RangeSetProvider(recordQuery.Provider, range).Result;
    }

    public static RecordQuery RangeSet(this RecordQuery recordQuery, RangeSet<Entire<Tuple>> range)
    {
      return new RangeSetProvider(recordQuery.Provider, range).Result;
    }

    public static RecordQuery Like(this RecordQuery recordQuery, Tuple beginning)
    {
      var from = beginning.Clone();
      var to = beginning.Clone();
      for (int i = 0; i < beginning.Count; i++) {
        if (beginning.Descriptor[i]==typeof (string)) {
            from.SetValue(i, beginning.GetValue<string>(i).ToLower(CultureInfo.InvariantCulture));
            to.SetValue(i, beginning.GetValue<string>(i).ToUpper(CultureInfo.InvariantCulture) + '\u00FF');
        }
      }
      return Range(recordQuery, from, to);
    }

    public static RecordQuery Calculate(this RecordQuery recordQuery, params CalculatedColumnDescriptor[] columns)
    {
      return new CalculateProvider(recordQuery.Provider, columns).Result;
    }

    public static RecordQuery Calculate(this RecordQuery recordQuery, bool isInlined, params CalculatedColumnDescriptor[] columns)
    {
      return new CalculateProvider(recordQuery.Provider, isInlined, columns).Result;
    }

    public static RecordQuery RowNumber(this RecordQuery recordQuery, string columnName)
    {
      return new RowNumberProvider(recordQuery.Provider, columnName).Result;
    }

    public static RecordQuery Join(this RecordQuery left, RecordQuery right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery Join(this RecordQuery left, RecordQuery right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery LeftJoin(this RecordQuery left, RecordQuery right, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery LeftJoin(this RecordQuery left, RecordQuery right, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, JoinAlgorithm.Default,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery Join(this RecordQuery left, RecordQuery right, JoinAlgorithm joinAlgorithm, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery Join(this RecordQuery left, RecordQuery right, JoinAlgorithm joinAlgorithm, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.Inner, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery LeftJoin(this RecordQuery left, RecordQuery right, JoinAlgorithm joinAlgorithm, params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery LeftJoin(this RecordQuery left, RecordQuery right, JoinAlgorithm joinAlgorithm, params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left.Provider, right.Provider, JoinType.LeftOuter, joinAlgorithm,
        joinedColumnIndexes).Result;
    }

    public static RecordQuery OrderBy(this RecordQuery recordQuery, DirectionCollection<int> columnIndexes)
    {
      return new SortProvider(recordQuery.Provider, columnIndexes).Result;
    }

    public static RecordQuery OrderBy(this RecordQuery recordQuery, DirectionCollection<int> columnIndexes, bool reindex)
    {
      if (reindex)
        return new ReindexProvider(recordQuery.Provider, columnIndexes).Result;
      else
        return new SortProvider(recordQuery.Provider, columnIndexes).Result;
    }

    public static RecordQuery Alias(this RecordQuery recordQuery, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new AliasProvider(recordQuery.Provider, alias).Result;
    }

    public static RecordQuery Filter(this RecordQuery recordQuery, Expression<Func<Tuple,bool>> predicate)
    {
      return new FilterProvider(recordQuery.Provider, predicate).Result;
    }

    public static RecordQuery Select(this RecordQuery recordQuery, params int[] columnIndexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, "columnIndexes");
      return new SelectProvider(recordQuery.Provider, columnIndexes).Result;
    }

    public static RecordQuery Seek(this RecordQuery recordQuery, Func<Tuple> key)
    {
      return new SeekProvider(recordQuery.Provider, () => key.Invoke()) { CompiledKey = key }.Result;
    }

    public static RecordQuery Seek(this RecordQuery recordQuery, Expression<Func<Tuple>> key, bool isExpression)
    {
      return new SeekProvider(recordQuery.Provider, key).Result;
    }

    public static RecordQuery Seek(this RecordQuery recordQuery, Tuple key)
    {
      return new SeekProvider(recordQuery.Provider, key).Result;
    }

    public static RecordQuery Aggregate(this RecordQuery recordQuery, int[] groupIndexes, params AggregateColumnDescriptor[] descriptors)
    {
      return new AggregateProvider(recordQuery.Provider, groupIndexes, descriptors).Result;
    }

    public static long Count(this RecordQuery recordQuery, EnumerationContext context, CompilationService compilationService)
    {
      var resultQuery = recordQuery.Aggregate(null, 
        new AggregateColumnDescriptor("$Count", 0, AggregateType.Count));
      var recordSet = new RecordSet(context, compilationService.Compile(resultQuery.Provider));
      return recordSet.First().GetValue<long>(0);
    }

    public static RecordSet ToRecordSet(this RecordQuery recordQuery, EnumerationContext context, CompilationService compilationService)
    {
      return new RecordSet(context, compilationService.Compile(recordQuery.Provider));
    }

    public static RecordQuery Skip(this RecordQuery recordQuery, Func<int> count)
    {
      return new SkipProvider(recordQuery.Provider, count).Result;
    }

    public static RecordQuery Skip(this RecordQuery recordQuery, int count)
    {
      return new SkipProvider(recordQuery.Provider, count).Result;
    }

    public static RecordQuery Take(this RecordQuery recordQuery, Func<int> count)
    {
      return new TakeProvider(recordQuery.Provider, count).Result;
    }

    public static RecordQuery Take(this RecordQuery recordQuery, int count)
    {
      return new TakeProvider(recordQuery.Provider, count).Result;
    }

    public static RecordQuery Save(this RecordQuery recordQuery)
    {
      return new StoreProvider(recordQuery.Provider).Result;
    }

    public static RecordQuery Save(this RecordQuery recordQuery, TemporaryDataScope scope, string name)
    {
      return new StoreProvider(recordQuery.Provider, scope, name).Result;
    }

    public static RecordQuery ExecuteAt(this RecordQuery recordQuery, TransferType options)
    {
      return new TransferProvider(recordQuery.Provider, options).Result;
    }

    public static RecordQuery ToRecordSet(this IEnumerable<Tuple> tuples, RecordSetHeader header)
    {
      return new RawProvider(header, tuples).Result;
    }

    public static RecordQuery Distinct(this RecordQuery recordQuery)
    {
      return new DistinctProvider(recordQuery.Provider).Result;
    }

    public static RecordQuery Apply(this RecordQuery recordQuery, ApplyParameter applyParameter, RecordQuery right)
    {
      return new ApplyProvider(applyParameter, recordQuery.Provider, right.Provider).Result;
    }

    public static RecordQuery Apply(this RecordQuery recordQuery, ApplyParameter applyParameter, RecordQuery right, bool isInlined, ApplySequenceType sequenceType, JoinType applyType)
    {
      return new ApplyProvider(applyParameter, recordQuery.Provider, right.Provider, isInlined, sequenceType, applyType).Result;
    }

    public static RecordQuery Existence(this RecordQuery recordQuery, string existenceColumnName)
    {
      return new ExistenceProvider(recordQuery.Provider, existenceColumnName).Result;
    }

    public static RecordQuery Include(this RecordQuery recordQuery, Expression<Func<IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(recordQuery.Provider, IncludeAlgorithm.Auto, false, filterDataSource, resultColumnName, filteredColumns).Result;
    }
    
    public static RecordQuery Include(this RecordQuery recordQuery, IncludeAlgorithm algorithm, Expression<Func<IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(recordQuery.Provider, algorithm, false, filterDataSource, resultColumnName, filteredColumns).Result;
    }

    public static RecordQuery Include(this RecordQuery recordQuery, IncludeAlgorithm algorithm, bool isInlined, Expression<Func<IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(recordQuery.Provider, algorithm, isInlined, filterDataSource, resultColumnName, filteredColumns).Result;
    }
    
    public static RecordQuery Intersect(this RecordQuery left, RecordQuery right)
    {
      return new IntersectProvider(left.Provider, right.Provider).Result;
    }

    public static RecordQuery Except(this RecordQuery left, RecordQuery right)
    {
      return new ExceptProvider(left.Provider, right.Provider).Result;
    }

    public static RecordQuery Concat(this RecordQuery left, RecordQuery right)
    {
      return new ConcatProvider(left.Provider, right.Provider).Result;
    }

    public static RecordQuery Union(this RecordQuery left, RecordQuery right)
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
    public static RecordQuery Lock(this RecordQuery source, LockMode lockMode, LockBehavior lockBehavior)
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
    public static RecordQuery Lock(this RecordQuery source, Func<LockMode> lockMode,
      Func<LockBehavior> lockBehavior)
    {
      return new LockProvider(source.Provider, lockMode, lockBehavior).Result;
    }
  }
}
