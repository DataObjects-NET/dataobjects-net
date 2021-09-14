// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="CompilableProvider"/> related extension methods.
  /// </summary>
  public static class CompilableProviderExtensions
  {
    public static CompilableProvider Calculate(this CompilableProvider source,
      params CalculatedColumnDescriptor[] columns)
    {
      return new CalculateProvider(source, columns);
    }

    public static CompilableProvider Calculate(this CompilableProvider source, bool isInlined,
      params CalculatedColumnDescriptor[] columns)
    {
      return new CalculateProvider(source, isInlined, columns);
    }

    public static CompilableProvider RowNumber(this CompilableProvider source, string columnName)
    {
      return new RowNumberProvider(source, columnName);
    }

    public static CompilableProvider Join(this CompilableProvider left, CompilableProvider right,
      params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left, right, JoinType.Inner, joinedColumnIndexes);
    }

    public static CompilableProvider Join(this CompilableProvider left, CompilableProvider right,
      params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left, right, JoinType.Inner, joinedColumnIndexes);
    }

    public static CompilableProvider LeftJoin(this CompilableProvider left, CompilableProvider right,
      params Pair<int>[] joinedColumnIndexes)
    {
      return new JoinProvider(left, right, JoinType.LeftOuter, joinedColumnIndexes);
    }

    public static CompilableProvider LeftJoin(this CompilableProvider left, CompilableProvider right,
      params int[] joinedColumnIndexes)
    {
      return new JoinProvider(left, right, JoinType.LeftOuter, joinedColumnIndexes);
    }

    public static CompilableProvider OrderBy(this CompilableProvider source, DirectionCollection<int> columnIndexes)
    {
      return new SortProvider(source, columnIndexes);
    }

    public static CompilableProvider Alias(this CompilableProvider source, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new AliasProvider(source, alias);
    }

    public static CompilableProvider Filter(this CompilableProvider source, Expression<Func<Tuple, bool>> predicate)
    {
      return new FilterProvider(source, predicate);
    }

    public static CompilableProvider Select(this CompilableProvider source, params int[] columnIndexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, "columnIndexes");
      return new SelectProvider(source, columnIndexes);
    }

    public static CompilableProvider Seek(this CompilableProvider source, Func<ParameterContext, Tuple> key)
    {
      return new SeekProvider(source, key);
    }

    public static CompilableProvider Seek(this CompilableProvider source, Tuple key)
    {
      return new SeekProvider(source, key);
    }

    public static CompilableProvider Aggregate(this CompilableProvider recordQuery,
      int[] groupIndexes, params AggregateColumnDescriptor[] descriptors)
    {
      return new AggregateProvider(recordQuery, groupIndexes, descriptors);
    }

    public static CompilableProvider Skip(this CompilableProvider source, Func<ParameterContext, int> count)
    {
      return new SkipProvider(source, count);
    }

    public static CompilableProvider Skip(this CompilableProvider source, int count)
    {
      return new SkipProvider(source, count);
    }

    public static CompilableProvider Take(this CompilableProvider source, Func<ParameterContext, int> count)
    {
      return new TakeProvider(source, count);
    }

    public static CompilableProvider Take(this CompilableProvider source, int count)
    {
      return new TakeProvider(source, count);
    }

    public static CompilableProvider Save(this CompilableProvider source)
    {
      return new StoreProvider(source);
    }

    public static CompilableProvider Save(this CompilableProvider source, string name)
    {
      return new StoreProvider(source, name);
    }

    public static CompilableProvider Distinct(this CompilableProvider source)
    {
      return new DistinctProvider(source);
    }

    public static CompilableProvider Apply(this CompilableProvider source,
      ApplyParameter applyParameter, CompilableProvider right)
    {
      return new ApplyProvider(applyParameter, source, right);
    }

    public static CompilableProvider Apply(this CompilableProvider source,
      ApplyParameter applyParameter, CompilableProvider right,
      bool isInlined, ApplySequenceType sequenceType, JoinType applyType)
    {
      return new ApplyProvider(applyParameter, source, right, isInlined, sequenceType, applyType);
    }

    public static CompilableProvider Existence(this CompilableProvider source, string existenceColumnName)
    {
      return new ExistenceProvider(source, existenceColumnName);
    }

    public static CompilableProvider Include(this CompilableProvider source,
      Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(
        source, IncludeAlgorithm.Auto, false, filterDataSource, resultColumnName, filteredColumns);
    }

    public static CompilableProvider Include(this CompilableProvider source,
      IncludeAlgorithm algorithm, Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource,
      string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(source, algorithm, false, filterDataSource, resultColumnName, filteredColumns);
    }

    public static CompilableProvider Include(this CompilableProvider source,
      IncludeAlgorithm algorithm, bool isInlined, Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource,
      string resultColumnName, int[] filteredColumns)
    {
      return new IncludeProvider(source, algorithm, isInlined, filterDataSource, resultColumnName, filteredColumns);
    }

    public static CompilableProvider Intersect(this CompilableProvider left, CompilableProvider right)
    {
      return new IntersectProvider(left, right);
    }

    public static CompilableProvider Except(this CompilableProvider left, CompilableProvider right)
    {
      return new ExceptProvider(left, right);
    }

    public static CompilableProvider Concat(this CompilableProvider left, CompilableProvider right)
    {
      return new ConcatProvider(left, right);
    }

    public static CompilableProvider Union(this CompilableProvider left, CompilableProvider right)
    {
      return new UnionProvider(left, right);
    }

    public static CompilableProvider Lock(this CompilableProvider source,
      LockMode lockMode, LockBehavior lockBehavior)
    {
      return new LockProvider(source, lockMode, lockBehavior);
    }

    public static CompilableProvider Lock(this CompilableProvider source,
      Func<LockMode> lockMode, Func<LockBehavior> lockBehavior)
    {
      return new LockProvider(source, lockMode, lockBehavior);
    }

    public static CompilableProvider Tag(this CompilableProvider source,
      string tag)
    {
      return new TagProvider(source, tag);
    }

    public static CompilableProvider MakeVoid(this CompilableProvider source)
    {
      return new VoidProvider(source.Header);
    }
  }
}
