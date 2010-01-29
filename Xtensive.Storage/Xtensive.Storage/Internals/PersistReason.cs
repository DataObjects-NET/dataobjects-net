// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// A reason of calling <see cref="Session.Persist(PersistReason)"/>.
  /// </summary>
  public enum PersistReason
  {
    /// <summary>
    /// Persist is not required.
    /// </summary>
    None = 0,
    /// <summary>
    /// Manual persist is requested.
    /// </summary>
    Manual,
    /// <summary>
    /// Query is to be executed.
    /// </summary>
    Query,
    /// <summary>
    /// Nested transaction is about to start.
    /// </summary>
    NestedTransaction,
    /// <summary>
    /// Commit is to be performed.
    /// </summary>
    Commit,
    /// <summary>
    /// <see cref="Session.EntityChangeRegistry"/> has reached its size limit.
    /// </summary>
    ChangeRegistrySizeLimit,
  }
}