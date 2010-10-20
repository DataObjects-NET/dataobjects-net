// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping
{
  internal sealed class MappingBuilderAdapter<TSource, TTarget>
    : IMappingBuilderAdapter<TSource, TTarget>
  {
    private readonly MappingBuilder realBuilder;

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapProperty<TValue>(Expression<Func<TSource, TValue>> source,
      Expression<Func<TTarget, TValue>> target)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var targetProperty = MappingHelper.ExtractProperty(target, "target");
      var compliedSource = source.Compile();
      realBuilder.RegisterProperty(null, obj => compliedSource.Invoke((TSource) obj), targetProperty);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> IgnoreProperty<TValue>(Expression<Func<TTarget, TValue>> target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var propertyInfo = MappingHelper.ExtractProperty(target, "target");
      realBuilder.IgnoreProperty(propertyInfo);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> TrackChanges<TValue>(Expression<Func<TTarget, TValue>> target,
      bool isEnabled)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var propertyInfo = MappingHelper.ExtractProperty(target, "target");
      realBuilder.TrackChanges(propertyInfo, isEnabled);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor)
    {
      return realBuilder.MapType(sourceKeyExtractor, targetKeyExtractor);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor,
      Func<TTarget, object[]> generatorArgumentsProvider)
    {
      return realBuilder.MapType(sourceKeyExtractor, targetKeyExtractor, generatorArgumentsProvider);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapStructure<TSource, TTarget>()
      where TTarget : struct
    {
      return realBuilder.MapStructure<TSource, TTarget>();
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TDescendantSource, TDescendantTarget> Inherit<TTargetBase, TDescendantSource, TDescendantTarget>()
      where TDescendantTarget: TTargetBase
    {
      return RegisterDescendant<TTargetBase, TDescendantSource, TDescendantTarget>(null);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TDescendantSource, TDescendantTarget> Inherit<TTargetBase, TDescendantSource, TDescendantTarget>(
      Func<TDescendantTarget, object[]> generatorArgumentsProvider)
      where TDescendantTarget: TTargetBase
    {
      ArgumentValidator.EnsureArgumentNotNull(generatorArgumentsProvider, "generatorArgumentsProvider");
      return RegisterDescendant<TTargetBase, TDescendantSource, TDescendantTarget>(generatorArgumentsProvider);
    }

    /// <inheritdoc/>
    public MappingDescription Build()
    {
      return realBuilder.Build();
    }

    private IMappingBuilderAdapter<TDescendantSource, TDescendantTarget> RegisterDescendant<TTargetBase, TDescendantSource, TDescendantTarget>(
      Func<TDescendantTarget, object[]> generatorArgumentsProvider)
      where TDescendantTarget: TTargetBase
    {
      var adaptedArgumentsProvider = generatorArgumentsProvider!=null
        ? (Func<object, object[]>) (target => generatorArgumentsProvider.Invoke((TDescendantTarget) target))
        : null;
      realBuilder.RegisterDescendant(typeof (TTargetBase), typeof (TDescendantSource), typeof (TDescendantTarget),
        adaptedArgumentsProvider);
      return new MappingBuilderAdapter<TDescendantSource, TDescendantTarget>(realBuilder);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="realBuilder">The real mapper.</param>
    public MappingBuilderAdapter(MappingBuilder realBuilder)
    {
      this.realBuilder = realBuilder;
    }
  }
}