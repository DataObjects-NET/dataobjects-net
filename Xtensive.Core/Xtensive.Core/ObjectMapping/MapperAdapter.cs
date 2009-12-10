// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  public sealed class MapperAdapter<TSource, TTarget> : IMapper
  {
    private readonly MapperBase realMapper;

    public MapperAdapter<TSource, TTarget> Map<TValue>(Func<TSource, TValue> source,
      Expression<Func<TTarget, TValue>> target)
    {
      var targetProperty = ExtractProperty(target, "target");
      realMapper.mappingInfo.Register(obj => source.Invoke((TSource) obj), targetProperty);
      return this;
    }

    /// <inheritdoc/>
    public MapperAdapter<TSource, TTarget> MapType<TSource, TSourceKey, TTarget, TTargetKey>(
      Func<TSource, TSourceKey> sourceKeyExtractor, Func<TTarget, TTargetKey> targetKeyExtractor)
    {
      return realMapper.MapType(sourceKeyExtractor, targetKeyExtractor);
    }

    public void Complete()
    {
      realMapper.Complete();
    }

    private static PropertyInfo ExtractProperty(LambdaExpression expression, string paramName)
    {
      var asMemberExpression = expression.Body as MemberExpression;
      if (asMemberExpression == null)
        throw new ArgumentException(Strings.ExSpecifiedExpressionIsNotMemberExpression, paramName);
      var result = asMemberExpression.Member as PropertyInfo;
      if (result == null)
        throw new ArgumentException(Strings.ExAccessedMemberIsNotProperty, paramName);
      return result;
    }

    // Constructors

    internal MapperAdapter(MapperBase realMapper)
    {
      this.realMapper = realMapper;
    }
  }
}