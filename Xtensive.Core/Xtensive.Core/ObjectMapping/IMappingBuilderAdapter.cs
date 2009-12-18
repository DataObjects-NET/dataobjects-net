// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.18

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Contract of classes which helps to build a mapping configuration for an O2O-mapper.
  /// </summary>
  /// <typeparam name="TSource">The source type.</typeparam>
  /// <typeparam name="TTarget">The target type.</typeparam>
  public interface IMappingBuilderAdapter<TSource, TTarget> : IMappingBuilder
  {
    /// <summary>
    /// Registers mapping of a source property to a target property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property's value.</typeparam>
    /// <param name="source">The expression that calculates a value to be assigned to target property.
    /// It may contain the access to a target property or an arbitrary delegate.</param>
    /// <param name="target">The target property's expression.</param>
    /// <returns><see langword="this" /></returns>
    IMappingBuilderAdapter<TSource, TTarget> Map<TValue>(Expression<Func<TSource, TValue>> source,
      Expression<Func<TTarget, TValue>> target);

    /// <summary>
    /// Specifies that a modification of the given property shouldn't be detected.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="target">The target property's expression.</param>
    /// <returns><see langword="this" /></returns>
    IMappingBuilderAdapter<TSource, TTarget> Ignore<TValue>(Expression<Func<TTarget, TValue>> target);

    /// <summary>
    /// Specifies that the given property is immutable.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="target">The target property's expression.</param>
    /// <returns><see langword="this" /></returns>
    IMappingBuilderAdapter<TSource, TTarget> Immutable<TValue>(Expression<Func<TTarget, TValue>> target);

    /// <summary>
    /// Completes creation of the mapping.
    /// </summary>
    void Complete();
  }
}