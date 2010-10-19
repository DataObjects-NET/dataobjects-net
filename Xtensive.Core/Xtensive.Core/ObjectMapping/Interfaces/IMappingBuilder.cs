// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Contract for classes which build a mapping configuration for an O2O-mapper.
  /// </summary>
  public interface IMappingBuilder
  {
    /// <summary>
    /// Registers the mapping from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="sourceKeyExtractor">The source key extractor.</param>
    /// <param name="targetKeyExtractor">The target key extractor.</param>
    /// <returns>An instance of helper class.</returns>
    IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor);

    /// <summary>
    /// Registers the mapping from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="sourceKeyExtractor">The source key extractor.</param>
    /// <param name="targetKeyExtractor">The target key extractor.</param>
    /// <param name="generatorArgumentsProvider">The provider of arguments for an
    /// algorithm of a new source object creation. For example, it can provide arguments for a custom
    /// constructor or a key generator.</param>
    /// <returns>An instance of helper class.</returns>
    IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor,
      Func<TTarget, object[]> generatorArgumentsProvider);

    /// <summary>
    /// Registers the mapping from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <returns>An instance of helper class.</returns>
    IMappingBuilderAdapter<TSource, TTarget> MapStructure<TSource, TTarget>()
      where TTarget : struct;
  }
}