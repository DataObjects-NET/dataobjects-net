// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Type of modification detected in an object graph.
  /// </summary>
  public enum ModificationType
  {
    /// <summary>
    /// Creating an object.
    /// </summary>
    CreateObject,

    /// <summary>
    /// Removing an object.
    /// </summary>
    RemoveObject,

    /// <summary>
    /// Changing a property value.
    /// </summary>
    ChangeProperty,

    /// <summary>
    /// Adding an item to a collection.
    /// </summary>
    AddItem,

    /// <summary>
    /// Removing an item from a collection.
    /// </summary>
    RemoveItem
  }
}