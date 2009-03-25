// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System.ComponentModel;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Comparison;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node interface.
  /// </summary>
  public interface INode : IPathNode,
    IDifferentiable<Node, NodeDifference>,
    INotifyPropertyChanged
  {
    /// <summary>
    /// Gets the state of the node.
    /// </summary>
    NodeState State { get; }
    
    /// <summary>
    /// Gets or sets the index of the node in the parent collection, if applicable;
    /// otherwise, <see langword="0" />.
    /// </summary>
    int Index { get; set; }

    /// <summary>
    /// Gets the node nesting information.
    /// </summary>
    Nesting Nesting { get; }

    /// <summary>
    /// Gets the property accessors for this node.
    /// </summary>
    ReadOnlyDictionary<string, PropertyAccessor> PropertyAccessors { get; }

    /// <summary>
    /// Gets the value of the property with specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Property value.</returns>
    object GetProperty(string propertyName);

    /// <summary>
    /// Sets the value of the property with specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">The value to set.</param>
    void SetProperty(string propertyName, object value);

    /// <summary>
    /// Tries the get nested property value.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Nested property value;
    /// otherwise, <see langword="null" />.</returns>
    IPathNode GetNestedProperty(string propertyName);

    /// <summary>
    /// Moves the node.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    void Move(Node newParent, string newName, int newIndex);

    /// <summary>
    /// Removes the node.
    /// </summary>
    void Remove();
  }
}