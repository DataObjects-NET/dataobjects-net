// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Reflection;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node nesting information.
  /// </summary>
  public interface INesting
  {
    /// <summary>
    /// Gets the node this object belongs to.
    /// </summary>
    Node Node { get; }

    /// <summary>
    /// Gets the name of the parent property, to which the node can be nested.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the escaped <see cref="PropertyName"/>.
    /// </summary>
    string EscapedPropertyName { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="PropertyName"/> property is a collection property.
    /// </summary>
    bool IsNestedToCollection { get; }

    /// <summary>
    /// Gets the property info for <see cref="PropertyName"/> property.
    /// </summary>
    PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// Gets the property getter for <see cref="PropertyName"/> property.
    /// </summary>
    Func<Node, IPathNode> PropertyGetter { get; }

    /// <summary>
    /// Gets the property value for <see cref="PropertyName"/> property.
    /// </summary>
    IPathNode PropertyValue { get; }
  }
}