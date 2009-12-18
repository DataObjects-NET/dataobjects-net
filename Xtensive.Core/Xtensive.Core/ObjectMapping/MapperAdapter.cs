// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class MapperAdapter<TSource, TTarget> : IMappingBuilderAdapter<TSource, TTarget>
  {
    private readonly MapperBase realMapper;

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> Map<TValue>(Expression<Func<TSource, TValue>> source,
      Expression<Func<TTarget, TValue>> target)
    {
      PropertyInfo sourceProperty;
      MappingHelper.TryExtractProperty(source, "source", out sourceProperty);
      var targetProperty = MappingHelper.ExtractProperty(target, "target");
      var compliedSource = source.CachingCompile();
      realMapper.ModelBuilder.Register(sourceProperty, obj => compliedSource.Invoke((TSource) obj),
        targetProperty);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> Ignore<TValue>(Expression<Func<TTarget, TValue>> target)
    {
      var propertyInfo = MappingHelper.ExtractProperty(target, "target");
      realMapper.ModelBuilder.MarkPropertyAsIgnored(propertyInfo);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> Immutable<TValue>(Expression<Func<TTarget, TValue>> target)
    {
      var propertyInfo = MappingHelper.ExtractProperty(target, "target");
      realMapper.ModelBuilder.MarkPropertyAsImmutable(propertyInfo);
      return this;
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor)
    {
      return realMapper.MapType(sourceKeyExtractor, targetKeyExtractor);
    }

    /// <inheritdoc/>
    public void Complete()
    {
      realMapper.Complete();
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="realMapper">The real mapper.</param>
    public MapperAdapter(MapperBase realMapper)
    {
      this.realMapper = realMapper;
    }
  }
}