// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Define <see cref="IOperation"/> types.
  /// </summary>
  [Serializable]
  public enum OperationType
  {
    /// <summary>
    /// User defined.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Creating an <see cref="Entity"/>.
    /// </summary>
    CreateEntity,
    /// <summary>
    /// Setting an <see cref="Entity"/> field value.
    /// </summary>
    SetEntityField,
    /// <summary>
    /// Removing an <see cref="Entity"/>.
    /// </summary>
    RemoveEntity,
    /// <summary>
    /// Clearing an <see cref="Entity"/>.
    /// </summary>
    ClearEntitySet,
    /// <summary>
    /// Adding an item to <see cref="EntitySet{TItem}"/>.
    /// </summary>
    AddEntitySetItem,
    /// <summary>
    /// Removing an item from <see cref="EntitySet{TItem}"/>.
    /// </summary>
    RemoveEntitySetItem,
    /// <summary>
    /// Calling a method.
    /// </summary>
    MethodCall,
    /// <summary>
    /// Key generation.
    /// </summary>
    GenerateKey
  }
}