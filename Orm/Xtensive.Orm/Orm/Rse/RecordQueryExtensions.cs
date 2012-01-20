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
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="RecordQuery"/> related extension methods.
  /// </summary>
  public static class RecordQueryExtensions
  {
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

    public static RecordQuery Lock(this RecordQuery source, LockMode lockMode, LockBehavior lockBehavior)
    {
      return new LockProvider(source.Provider, lockMode, lockBehavior).Result;
    }

    public static RecordQuery Lock(this RecordQuery source, Func<LockMode> lockMode, Func<LockBehavior> lockBehavior)
    {
      return new LockProvider(source.Provider, lockMode, lockBehavior).Result;
    }
  }
}
