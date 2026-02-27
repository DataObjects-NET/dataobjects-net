// Copyright (C) 2008-2025 Xtensive LLC.
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
    /// <summary>
    /// Applies <see cref="CalculateProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="columns"> Calculated columns' descriptors.</param>
    /// <returns><see cref="CalculateProvider"/> instance.</returns>
    public static CalculateProvider Calculate(this CompilableProvider source, params CalculatedColumnDescriptor[] columns)
      => new CalculateProvider(source, columns, false);

    /// <summary>
    /// Applies <see cref="CalculateProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="isInlined">Columns should be inlined or not.</param>
    /// <param name="columns"> Calculated columns' descriptors.</param>
    /// <returns><see cref="CalculateProvider"/> instance.</returns>
    public static CalculateProvider Calculate(this CompilableProvider source, bool isInlined, params CalculatedColumnDescriptor[] columns)
      => new CalculateProvider(source, columns, isInlined);

    /// <summary>
    /// Applies <see cref="CalculateProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="columns"> Calculated columns' descriptors.</param>
    /// <returns><see cref="CalculateProvider"/> instance.</returns>
    public static CalculateProvider Calculate(this CompilableProvider source, IReadOnlyList<CalculatedColumnDescriptor> columns)
      => new CalculateProvider(source, columns);

    /// <summary>
    /// Applies <see cref="CalculateProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="isInlined">Columns should be inlined or not.</param>
    /// <param name="columns"> Calculated columns' descriptors.</param>
    /// <returns><see cref="CalculateProvider"/> instance.</returns>
    public static CalculateProvider Calculate(this CompilableProvider source, bool isInlined, IReadOnlyList<CalculatedColumnDescriptor> columns)
      => new CalculateProvider(source, columns, isInlined);

    /// <summary>
    /// Applies <see cref="RowNumberProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="columnName">Name of column.</param>
    /// <returns><see cref="RowNumberProvider"/> instance.</returns>
    public static CompilableProvider RowNumber(this CompilableProvider source, string columnName) => new RowNumberProvider(source, columnName);

    /// <summary>
    /// Applies <see cref="PredicateJoinProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <param name="predicate">Predicate to join.</param>
    /// <param name="joinType">Join Type</param>
    /// <returns><see cref="PredicateJoinProvider"/> instance.</returns>
    public static CompilableProvider Join(this CompilableProvider left, CompilableProvider right, Expression<Func<Tuple, Tuple, bool>> predicate, JoinType joinType)
      => new PredicateJoinProvider(left, right, predicate, joinType);

    /// <summary>
    /// Applies <see cref="JoinProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <param name="joinedColumnIndexes">Pair of column indexes to join.</param>
    /// <returns><see cref="JoinProvider"/> instance.</returns>
    public static CompilableProvider Join(this CompilableProvider left, CompilableProvider right, params Pair<int>[] joinedColumnIndexes)
      => new JoinProvider(left, right, JoinType.Inner, joinedColumnIndexes);

    /// <summary>
    /// Applies <see cref="JoinProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <param name="joinedColumnIndexes">Pair of column indexes to join.</param>
    /// <returns><see cref="JoinProvider"/> instance.</returns>
    public static CompilableProvider Join(this CompilableProvider left, CompilableProvider right, params int[] joinedColumnIndexes)
      => new JoinProvider(left, right, JoinType.Inner, joinedColumnIndexes);

    /// <summary>
    /// Applies <see cref="JoinProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <param name="joinedColumnIndexes">Pair of column indexes to join.</param>
    /// <returns><see cref="JoinProvider"/> instance.</returns>
    public static CompilableProvider LeftJoin(this CompilableProvider left, CompilableProvider right, params Pair<int>[] joinedColumnIndexes)
      => new JoinProvider(left, right, JoinType.LeftOuter, joinedColumnIndexes);

    /// <summary>
    /// Applies <see cref="JoinProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <param name="joinedColumnIndexes">Pair of column indexes to join.</param>
    /// <returns><see cref="JoinProvider"/> instance.</returns>
    public static CompilableProvider LeftJoin(this CompilableProvider left, CompilableProvider right, params int[] joinedColumnIndexes)
      => new JoinProvider(left, right, JoinType.LeftOuter, joinedColumnIndexes);

    /// <summary>
    /// Applies <see cref="SortProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="columnIndexes">Column indexes to order.</param>
    /// <returns><see cref="SortProvider"/> instance.</returns>
    public static CompilableProvider OrderBy(this CompilableProvider source, DirectionCollection<int> columnIndexes) => new SortProvider(source, columnIndexes);

    /// <summary>
    /// Applies <see cref="AliasProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="alias">Alias.</param>
    /// <returns><see cref="AliasProvider"/> instance.</returns>
    public static CompilableProvider Alias(this CompilableProvider source, string alias) => new AliasProvider(source, alias);

    /// <summary>
    /// Applies <see cref="FilterProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="predicate">Filtration predicate.</param>
    /// <returns><see cref="FilterProvider"/> instance.</returns>
    public static CompilableProvider Filter(this CompilableProvider source, Expression<Func<Tuple, bool>> predicate) => new FilterProvider(source, predicate);

    /// <summary>
    /// Applies <see cref="SelectProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="columnIndexes">Column indexes to select from the source.</param>
    /// <returns><see cref="SelectProvider"/> instance.</returns>
    public static CompilableProvider Select(this CompilableProvider source, IReadOnlyList<int> columnIndexes) => new SelectProvider(source, columnIndexes);

    /// <summary>
    /// Applies <see cref="SeekProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="key">Seek parameter.</param>
    /// <returns><see cref="SeekProvider"/> instance.</returns>
    public static CompilableProvider Seek(this CompilableProvider source, Func<ParameterContext, Tuple> key) => new SeekProvider(source, key);

    /// <summary>
    /// Applies <see cref="SeekProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="key">Seek parameter.</param>
    /// <returns><see cref="SeekProvider"/> instance.</returns>
    public static CompilableProvider Seek(this CompilableProvider source, Tuple key) => new SeekProvider(source, key);

    /// <summary>
    /// Applies <see cref="AggregateProvider"/> to the given source.
    /// </summary>
    /// <param name="recordQuery">Compilable provider.</param>
    /// <param name="groupIndexes">Column indexes to group by.</param>
    /// <param name="descriptors">Descriptors of aggregate columns.</param>
    /// <returns><see cref="AggregateProvider"/> instance.</returns>
    public static CompilableProvider Aggregate(this CompilableProvider recordQuery,
      int[] groupIndexes, params AggregateColumnDescriptor[] descriptors)
      => new AggregateProvider(recordQuery, groupIndexes, descriptors);

    /// <summary>
    /// Applies <see cref="AggregateProvider"/> to the given source.
    /// </summary>
    /// <param name="recordQuery">Compilable provider.</param>
    /// <param name="groupIndexes">Column indexes to group by.</param>
    /// <param name="descriptors">Descriptors of aggregate columns.</param>
    /// <returns><see cref="AggregateProvider"/> instance.</returns>
    public static CompilableProvider Aggregate(this CompilableProvider recordQuery,
      int[] groupIndexes, IReadOnlyList<AggregateColumnDescriptor> descriptors)
      => new AggregateProvider(recordQuery, groupIndexes, descriptors);

    /// <summary>
    /// Applies <see cref="SkipProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="count">Skip amout function.</param>
    /// <returns><see cref="SkipProvider"/> instance.</returns>
    public static CompilableProvider Skip(this CompilableProvider source, Func<ParameterContext, int> count) => new SkipProvider(source, count);

    /// <summary>
    /// Applies <see cref="SkipProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="count">Number of skippable items.</param>
    /// <returns><see cref="SkipProvider"/> instance.</returns>
    public static CompilableProvider Skip(this CompilableProvider source, int count) => new SkipProvider(source, count);

    /// <summary>
    /// Applies <see cref="TakeProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="count">Take amount function.</param>
    /// <returns><see cref="TakeProvider"/> instance.</returns>
    public static TakeProvider Take(this CompilableProvider source, Func<ParameterContext, int> count) => new TakeProvider(source, count);

    /// <summary>
    /// Applies <see cref="TakeProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="count">Number of items to take.</param>
    /// <returns><see cref="TakeProvider"/> instance.</returns>
    public static CompilableProvider Take(this CompilableProvider source, int count) => new TakeProvider(source, count);

    /// <summary>
    /// Applies <see cref="StoreProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <returns><see cref="StoreProvider"/> instance.</returns>
    public static CompilableProvider Save(this CompilableProvider source) => new StoreProvider(source);

    /// <summary>
    /// Applies <see cref="StoreProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="name">Name of saved data.</param>
    /// <returns><see cref="StoreProvider"/> instance.</returns>
    public static CompilableProvider Save(this CompilableProvider source, string name) => new StoreProvider(source, name);

    /// <summary>
    /// Applies <see cref="DistinctProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <returns><see cref="DistinctProvider"/> instance.</returns>
    public static CompilableProvider Distinct(this CompilableProvider source) => new DistinctProvider(source);

    /// <summary>
    /// Applies <see cref="ApplyProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Left source.</param>
    /// <param name="applyParameter">Apply parameter.</param>
    /// <param name="right">Compilable provider for right source (on each itteration of source).</param>
    /// <returns><see cref="ApplyProvider"/> instance.</returns>
    public static CompilableProvider Apply(this CompilableProvider source, ApplyParameter applyParameter, CompilableProvider right)
      => new ApplyProvider(applyParameter, source, right);

    /// <summary>
    /// Applies <see cref="ApplyProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Left source.</param>
    /// <param name="applyParameter">Apply parameter.</param>
    /// <param name="right">Compilable provider for right source (on each itteration of source).</param>
    /// <param name="isInlined">Inline column of apply provider or not.</param>
    /// <param name="sequenceType">Sequence type.</param>
    /// <param name="applyType">Apply type.</param>
    /// <returns><see cref="ApplyProvider"/> instance.</returns>
    public static CompilableProvider Apply(this CompilableProvider source,
      ApplyParameter applyParameter, CompilableProvider right,
      bool isInlined, ApplySequenceType sequenceType, JoinType applyType)
      => new ApplyProvider(applyParameter, source, right, isInlined, sequenceType, applyType);

    /// <summary>
    /// Applies <see cref="ExistenceProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="existenceColumnName">Name of the existance column.</param>
    /// <returns><see cref="ExistenceProvider"/> instance.</returns>
    public static CompilableProvider Existence(this CompilableProvider source, string existenceColumnName) => new ExistenceProvider(source, existenceColumnName);

    /// <summary>
    /// Applies <see cref="IncludeProvider"/> to the given source.
    /// </summary>
    /// /// <param name="source">Compilable provider.</param>
    /// <param name="filterDataSource">Filter data.</param>
    /// <param name="resultColumnName">Result column name.</param>
    /// <param name="filteredColumns">Filtered columns</param>
    /// <returns><see cref="IncludeProvider"/> instance.</returns>
    public static CompilableProvider Include(this CompilableProvider source,
      Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource, string resultColumnName, int[] filteredColumns)
      => new IncludeProvider(source, IncludeAlgorithm.Auto, false, filterDataSource, resultColumnName, filteredColumns);

    /// <summary>
    /// Applies <see cref="VoidProvider"/> to the given source.
    /// </summary>
    /// /// <param name="source">Compilable provider.</param>
    /// <param name="algorithm">Include algorithm.</param>
    /// <param name="filterDataSource">Filter data.</param>
    /// <param name="resultColumnName">Result column name.</param>
    /// <param name="filteredColumns">Filtered columns</param>
    /// <returns><see cref="VoidProvider"/> instance.</returns>
    public static CompilableProvider Include(this CompilableProvider source,
      IncludeAlgorithm algorithm, Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource,
      string resultColumnName, int[] filteredColumns)
      => new IncludeProvider(source, algorithm, false, filterDataSource, resultColumnName, filteredColumns);

    /// <summary>
    /// Applies <see cref="IncludeProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="algorithm">Include algorithm.</param>
    /// <param name="isInlined">Result column should be inlined or not.</param>
    /// <param name="filterDataSource">Filter data.</param>
    /// <param name="resultColumnName">Result column name.</param>
    /// <param name="filteredColumns">Filtered columns</param>
    /// <returns><see cref="IncludeProvider"/> instance.</returns>
    public static CompilableProvider Include(this CompilableProvider source,
      IncludeAlgorithm algorithm, bool isInlined, Expression<Func<ParameterContext, IEnumerable<Tuple>>> filterDataSource,
      string resultColumnName, int[] filteredColumns)
      => new IncludeProvider(source, algorithm, isInlined, filterDataSource, resultColumnName, filteredColumns);

    /// <summary>
    /// Applies <see cref="IntersectProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <returns><see cref="IntersectProvider"/> instance.</returns>
    public static CompilableProvider Intersect(this CompilableProvider left, CompilableProvider right) => new IntersectProvider(left, right);

    /// <summary>
    /// Applies <see cref="ExceptProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <returns><see cref="ExceptProvider"/> instance.</returns>
    public static CompilableProvider Except(this CompilableProvider left, CompilableProvider right) => new ExceptProvider(left, right);

    /// <summary>
    /// Applies <see cref="ConcatProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <returns><see cref="ConcatProvider"/> instance.</returns>
    public static CompilableProvider Concat(this CompilableProvider left, CompilableProvider right) => new ConcatProvider(left, right);

    /// <summary>
    /// Applies <see cref="UnionProvider"/> to the given source.
    /// </summary>
    /// <param name="left">Compilable provider.</param>
    /// <param name="right">Compilable provider.</param>
    /// <returns><see cref="UnionProvider"/> instance.</returns>
    public static CompilableProvider Union(this CompilableProvider left, CompilableProvider right) => new UnionProvider(left, right);

    /// <summary>
    /// Applies <see cref="LockProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="lockMode">Lock mode.</param>
    /// <param name="lockBehavior">Lock behavior</param>
    /// <returns><see cref="LockProvider"/> instance.</returns>
    public static CompilableProvider Lock(this CompilableProvider source, LockMode lockMode, LockBehavior lockBehavior)
      => new LockProvider(source, lockMode, lockBehavior);

    /// <summary>
    /// Applies <see cref="LockProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="lockMode">Lock mode.</param>
    /// <param name="lockBehavior">Lock behavior</param>
    /// <returns><see cref="LockProvider"/> instance.</returns>
    public static CompilableProvider Lock(this CompilableProvider source, Func<LockMode> lockMode, Func<LockBehavior> lockBehavior)
      => new LockProvider(source, lockMode, lockBehavior);

    /// <summary>
    /// Applies <see cref="TagProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <param name="tag">Tag string</param>
    /// <returns><see cref="TagProvider"/> instance.</returns>
    public static CompilableProvider Tag(this CompilableProvider source, string tag) => new TagProvider(source, tag);

    /// <summary>
    /// Applies <see cref="VoidProvider"/> to the given source.
    /// </summary>
    /// <param name="source">Compilable provider.</param>
    /// <returns><see cref="VoidProvider"/> instance.</returns>
    public static CompilableProvider MakeVoid(this CompilableProvider source) => new VoidProvider(source.Header);

    internal static bool CheckIfLeftJoinPrefered(this CompilableProvider provider)
    {
      var sourceToCheck = (provider is FilterProvider filterProvider) ? filterProvider.Source : provider;
      return (sourceToCheck is ApplyProvider applyProvider && applyProvider.ApplyType == JoinType.LeftOuter) ||
        (sourceToCheck is JoinProvider joinProvider && joinProvider.JoinType == JoinType.LeftOuter);
    }
  }
}
