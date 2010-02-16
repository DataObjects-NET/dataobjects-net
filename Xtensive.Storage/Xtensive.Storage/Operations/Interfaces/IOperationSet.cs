// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using Xtensive.Core.Collections;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Declares public contract for operations container.
  /// </summary>
  public interface IOperationSet : ICountable<IOperation>
  {
    /// <summary>
    /// Registers the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    void Append(IOperation operation);

    /// <summary>
    /// Registers the specified <see cref="IOperationSet"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    void Append(IOperationSet source);

    /// <summary>
    /// Applies this operation set to the <see cref="Session.Current"/> session.
    /// </summary>
    /// <returns>Key mapping.</returns>
    KeyMapping Apply();

    /// <summary>
    /// Applies this operation set to the specified session.
    /// </summary>
    /// <param name="session">The session to apply operations to.</param>
    /// <returns>Key mapping.</returns>
    KeyMapping Apply(Session session);

    /// <summary>
    /// Clears the operation set - 
    /// "forgets" all the registered operations and new keys.
    /// </summary>
    void Clear();
  }
}