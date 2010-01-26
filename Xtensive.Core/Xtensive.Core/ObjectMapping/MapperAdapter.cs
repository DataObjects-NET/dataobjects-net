// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class MapperAdapter<TSource, TTarget, TComparisonResult>
    : IMappingBuilderAdapter<TSource, TTarget>
    where TComparisonResult : GraphComparisonResult
  {
    private readonly MapperBase<TComparisonResult> realMapper;

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapProperty<TValue>(Expression<Func<TSource, TValue>> source,
      Expression<Func<TTarget, TValue>> target)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var targetProperty = MappingHelper.ExtractProperty(target, "target");
      var compliedSource = source.Compile();
      realMapper.ModelBuilder.RegisterProperty(null, obj => compliedSource.Invoke((TSource) obj),
        targetProperty);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> IgnoreProperty<TValue>(Expression<Func<TTarget, TValue>> target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var propertyInfo = MappingHelper.ExtractProperty(target, "target");
      realMapper.ModelBuilder.Ignore(propertyInfo);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> TrackChanges<TValue>(Expression<Func<TTarget, TValue>> target,
      bool isEnabled)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var propertyInfo = MappingHelper.ExtractProperty(target, "target");
      realMapper.ModelBuilder.TrackChanges(propertyInfo, isEnabled);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor)
    {
      return realMapper.MapType(sourceKeyExtractor, targetKeyExtractor);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor,
      Func<TTarget, object[]> generatorArgumentsProvider)
    {
      return realMapper.MapType(sourceKeyExtractor, targetKeyExtractor, generatorArgumentsProvider);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapStructure<TSource, TTarget>()
      where TTarget : struct
    {
      return realMapper.MapStructure<TSource, TTarget>();
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<THeirSource, THeirTarget> Inherit<TTargetBase, THeirSource, THeirTarget>()
      where THeirTarget: TTargetBase
    {
      return RegisterHeir<TTargetBase, THeirSource, THeirTarget>(null);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<THeirSource, THeirTarget> Inherit<TTargetBase, THeirSource, THeirTarget>(
      Func<THeirTarget, object[]> generatorArgumentsProvider)
      where THeirTarget: TTargetBase
    {
      ArgumentValidator.EnsureArgumentNotNull(generatorArgumentsProvider, "generatorArgumentsProvider");
      return RegisterHeir<TTargetBase, THeirSource, THeirTarget>(generatorArgumentsProvider);
    }

    /// <inheritdoc/>
    public void Complete()
    {
      realMapper.Complete();
    }

    private IMappingBuilderAdapter<THeirSource, THeirTarget> RegisterHeir<TTargetBase, THeirSource, THeirTarget>(
      Func<THeirTarget, object[]> generatorArgumentsProvider)
      where THeirTarget: TTargetBase
    {
      var adaptedArgumentsProvider = generatorArgumentsProvider!=null
        ? (Func<object, object[]>) (target => generatorArgumentsProvider.Invoke((THeirTarget) target))
        : null;
      realMapper.ModelBuilder.RegisterHeir(typeof (TTargetBase), typeof (THeirSource), typeof (THeirTarget),
        adaptedArgumentsProvider);
      return new MapperAdapter<THeirSource, THeirTarget, TComparisonResult>(realMapper);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="realMapper">The real mapper.</param>
    public MapperAdapter(MapperBase<TComparisonResult> realMapper)
    {
      this.realMapper = realMapper;
    }
  }
}