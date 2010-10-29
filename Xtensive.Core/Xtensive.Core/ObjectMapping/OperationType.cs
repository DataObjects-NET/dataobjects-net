// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Type of modification detected in an object graph.
  /// </summary>
  public enum OperationType
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
    /// Setting a property value.
    /// </summary>
    SetProperty,

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