// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections.Generic;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Declares public contract for operations container.
  /// </summary>
  public interface IOperationSet : IEnumerable<IOperation>
  {
    /// <summary>
    /// Gets the keys for remap.
    /// </summary>
    /// <returns></returns>
    HashSet<Key> GetKeysForRemap();

    /// <summary>
    /// Registers the key for remap.
    /// </summary>
    /// <param name="key">The key.</param>
    void RegisterKeyForRemap(Key key);

    /// <summary>
    /// Registers the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    void Register(IOperation operation);

    /// <summary>
    /// Registers the specified <see cref="IOperationSet"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    void Register(IOperationSet source);

    /// <summary>
    /// Applies current operation set using specified session.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>Key mapping.</returns>
    KeyMapping Apply(Session session);
  }
}