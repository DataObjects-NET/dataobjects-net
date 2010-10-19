// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.18

using System;
using System.Linq.Expressions;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping
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
    IMappingBuilderAdapter<TSource, TTarget> MapProperty<TValue>(Expression<Func<TSource, TValue>> source,
      Expression<Func<TTarget, TValue>> target);

    /// <summary>
    /// Specifies that the property won't be converted and its modifications won't be detected.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="target">The target property's expression.</param>
    /// <returns><see langword="this" /></returns>
    IMappingBuilderAdapter<TSource, TTarget> IgnoreProperty<TValue>(Expression<Func<TTarget, TValue>> target);

    /// <summary>
    /// Specifies whether modification of the given property will be tracked.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="target">The target property's expression.</param>
    /// <param name="isEnabled">if set to <see langword="true"/> then changes will be tracked.</param>
    /// <returns><see langword="this"/></returns>
    IMappingBuilderAdapter<TSource, TTarget> TrackChanges<TValue>(Expression<Func<TTarget, TValue>> target,
      bool isEnabled);

    /// <summary>
    /// Registers mapping from the <typeparamref name="TDescendantSource"/>
    /// to the <typeparamref name="TDescendantTarget"/>.
    /// </summary>
    /// <typeparam name="TTargetBase">The ancestor of the <typeparamref name="TDescendantTarget"/>.</typeparam>
    /// <typeparam name="TDescendantSource">The source type.</typeparam>
    /// <typeparam name="TDescendantTarget">The target type.</typeparam>
    /// <returns>A new instance of helper class.</returns>
    IMappingBuilderAdapter<TDescendantSource, TDescendantTarget> Inherit<TTargetBase, TDescendantSource, TDescendantTarget>()
      where TDescendantTarget: TTargetBase;

    /// <summary>
    /// Registers mapping from the <typeparamref name="TDescendantSource"/>
    /// to the <typeparamref name="TDescendantTarget"/>.
    /// </summary>
    /// <typeparam name="TTargetBase">The ancestor of the <typeparamref name="TDescendantTarget"/>.</typeparam>
    /// <typeparam name="TDescendantSource">The source type.</typeparam>
    /// <typeparam name="TDescendantTarget">The target type.</typeparam>
    /// <param name="generatorArgumentsProvider">The provider of arguments for an
    /// algorithm of a new source object creation. For example, it can provide arguments for a custom
    /// constructor or a key generator.</param>
    /// <returns>A new instance of helper class.</returns>
    IMappingBuilderAdapter<TDescendantSource, TDescendantTarget> Inherit<TTargetBase, TDescendantSource, TDescendantTarget>(
      Func<TDescendantTarget, object[]> generatorArgumentsProvider)
      where TDescendantTarget: TTargetBase;

    /// <summary>
    /// Completes creation of the mapping.
    /// </summary>
    MappingDescription Build();
  }
}