// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;

namespace Xtensive.Core.ObjectMapping
{
  public sealed class MapperAdapter<TSource, TTarget> : IMapper
  {
    private readonly MapperBase realMapper;

    public MapperAdapter<TSource, TTarget> Map<TValue>(Expression<Func<TSource, TValue>> source,
      Expression<Func<TTarget, TValue>> target)
    {
      PropertyInfo sourceProperty;
      MappingHelper.TryExtractProperty(source, "source", out sourceProperty);
      var targetProperty = MappingHelper.ExtractProperty(target, "target");
      var compliedSource = source.CachingCompile();
      realMapper.MappingBuilder.Register(sourceProperty, obj => compliedSource.Invoke((TSource) obj),
        targetProperty);
      return this;
    }

    /// <inheritdoc/>
    public MapperAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Func<TTarget, TKey> targetKeyExtractor)
    {
      return realMapper.MapType(sourceKeyExtractor, targetKeyExtractor);
    }

    public void Complete()
    {
      realMapper.Complete();
    }

    // Constructors

    internal MapperAdapter(MapperBase realMapper)
    {
      this.realMapper = realMapper;
    }
  }
}