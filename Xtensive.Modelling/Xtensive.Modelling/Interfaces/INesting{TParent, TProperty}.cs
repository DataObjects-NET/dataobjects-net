// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Typed node nesting information.
  /// </summary>
  public interface INesting<TParent, TProperty> : INesting
    where TParent: Node
    where TProperty: IPathNode
  {
    /// <summary>
    /// Gets the property getter for <see cref="INesting.PropertyName"/> property.
    /// </summary>
    new Func<Node, TProperty> PropertyGetter { get; }

    /// <summary>
    /// Gets the property value for <see cref="INesting.PropertyName"/> property.
    /// </summary>
    new TProperty PropertyValue { get; }
  }
}